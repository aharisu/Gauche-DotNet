
#include "ClrDelegate.h"
#include "ClrStubConstant.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Reflection::Emit;
using namespace System::Collections::Generic;
using namespace GaucheDotNet;

static void EmitIntConst(ILGenerator^ ilGen, int num)
{
    switch(num)
    {
    case 0:
        ilGen->Emit(OpCodes::Ldc_I4_0);
        break;
    case 1:
        ilGen->Emit(OpCodes::Ldc_I4_1);
        break;
    case 2:
        ilGen->Emit(OpCodes::Ldc_I4_2);
        break;
    case 3:
        ilGen->Emit(OpCodes::Ldc_I4_3);
        break;
    case 4:
        ilGen->Emit(OpCodes::Ldc_I4_4);
        break;
    case 5:
        ilGen->Emit(OpCodes::Ldc_I4_5);
        break;
    case 6:
        ilGen->Emit(OpCodes::Ldc_I4_6);
        break;
    case 7:
        ilGen->Emit(OpCodes::Ldc_I4_7);
        break;
    case 8:
        ilGen->Emit(OpCodes::Ldc_I4_8);
        break;
    default:
        ilGen->Emit(OpCodes::Ldc_I4, num);
        break;
    }
}


static void EmitLoadArg(ILGenerator^ ilGen, int index)
{
    switch(index)
    {
    case 0:
        ilGen->Emit(OpCodes::Ldarg_0);
        break;
    case 1:
        ilGen->Emit(OpCodes::Ldarg_1);
        break;
    case 2:
        ilGen->Emit(OpCodes::Ldarg_2);
        break;
    case 3:
        ilGen->Emit(OpCodes::Ldarg_3);
        break;
    default:
        ilGen->Emit(OpCodes::Ldarg_S, index);
        break;
    }
}

static DelegateCreator^ DefineDelegateCreator(Type^ type, GoshProc^ proc)
{
    //以下の様なクラスとメソッドをランタイムに定義します
    //public class {DelegateTypeName}{Guid}
    //{
    //    public GoshProc proc;
    //
    //    public {DelegateReturnType} DelegateEventHandler({DelegateParameter} ... )
    //    {
    //        return proc.Apply({DelegateParameter}...);
    //    }
    //
    //    public IntPtr key;
    //
    //  ~{DelegateTypeName}{Guid}()
    //   {
    //       ClrStubConstant::UnregisterDelegate(this.key);
    //   }
    //}
    //
    ////GenType := {DelegateTypeName}{Guid}
    //public static Delegate Create_{GenType}(GoshProc proc, IntPtr key)
    //{
    //    {GenType} obj = new {GenType}();
    //    obj.proc = proc;
    //
    //  if(key == IntPtr.Zero)
    //  {
    //     GC.SuppressFinalize(obj);
    //  }
    //  else
    //  {
    //     obj.key = key;
    //  }
    //
    //    return (Delegate)({DelegateType})obj.DelegateEventHandler;
    //}

    ModuleBuilder^ modBuilder = ClrStubConstant::GetModuleBuilder();

    TypeBuilder^ typeBuilder = modBuilder->DefineType(
        type->Name + (Guid::NewGuid().ToString()->Replace('-', '_'))
        , TypeAttributes::Public |
            TypeAttributes::Class |
            TypeAttributes::AutoClass |
            TypeAttributes::AnsiClass |
            TypeAttributes::BeforeFieldInit |
            TypeAttributes::Sealed
        ,Object::typeid
        );

    //GoshProcを格納するprocフィールドを定義
    FieldBuilder^ fieldBuilder = typeBuilder->DefineField("proc", GoshProc::typeid
        , FieldAttributes::Public
        );

    //GoshProcに処理を委譲するためのメソッド定義
    MethodInfo^ invokeInfo = type->GetMethod("Invoke");
    array<ParameterInfo^>^ params =  invokeInfo->GetParameters();
    array<Type^>^ paramTypes = gcnew array<Type^>(params->Length);
    for(int i = 0;i < params->Length;++i)
    {
        paramTypes[i] = params[i]->ParameterType;
    }

    MethodBuilder^ delegateBuilder = typeBuilder->DefineMethod("DelegateEventHandler"
        , MethodAttributes::Public | MethodAttributes::Final
        , invokeInfo->ReturnType
        , paramTypes
        );
    ILGenerator^ ilGen = delegateBuilder->GetILGenerator();
    //object[]のローカル変数を一つ定義
    ilGen->DeclareLocal(Object::typeid->MakeArrayType());

    //パラメータの数と同数の配列を作成してローカル変数に設定
    EmitIntConst(ilGen, paramTypes->Length);
    ilGen->Emit(OpCodes::Newarr, Object::typeid);
    ilGen->Emit(OpCodes::Stloc_0);

    //ローカル変数の配列に引数の値を一つずつ設定していく
    for(int i = 0;i < paramTypes->Length;++i)
    {
        ilGen->Emit(OpCodes::Ldloc_0);
        EmitIntConst(ilGen, i);
        EmitLoadArg(ilGen, i + 1);
        if(paramTypes[i]->IsValueType)
        {
            ilGen->Emit(OpCodes::Box, paramTypes[i]);
        }
        ilGen->Emit(OpCodes::Stelem_Ref);
    }

    //フィールドからGoshProcオブジェクトを取得
    ilGen->Emit(OpCodes::Ldarg_0);
    ilGen->Emit(OpCodes::Ldfld, fieldBuilder);
    //Applyの引数に渡すオブジェクト配列をローカル変数から取得
    ilGen->Emit(OpCodes::Ldloc_0);
    //Applyの実行
    ilGen->Emit(OpCodes::Callvirt, ClrStubConstant::GoshProcMethodInfo);
    //Delegateがvoid型なら戻り値を捨てる
    Type^ returnType = invokeInfo->ReturnType;
    if(returnType == void::typeid)
    {
        ilGen->Emit(OpCodes::Pop);
    }
    //戻り値が値型ならunboxing
    else if(returnType->IsValueType)
    {
        //TODO 型チェックが必要か？
        ilGen->Emit(OpCodes::Unbox_Any, returnType);
    }
    ilGen->Emit(OpCodes::Ret);

    //GCされるタイミングでDelegateTableから削除するためのFinalize定義

    //DelegateMapのキーになるオブジェクト
    FieldBuilder^ keyFieldBuilder = typeBuilder->DefineField("key", IntPtr::typeid
        , FieldAttributes::Public
        );

    //Finalize定義
    MethodBuilder^ finlizeBuilder = typeBuilder->DefineMethod("Finalize"
        , MethodAttributes::Family | MethodAttributes::Virtual | MethodAttributes::HideBySig
        , CallingConventions::Standard
        , void::typeid
        , Type::EmptyTypes
        );
    ilGen = finlizeBuilder->GetILGenerator();

    //try
    //{
    ilGen->BeginExceptionBlock();
    //ClrStubConstant::UnregisterDelegate(this.key)を実行
    ilGen->Emit(OpCodes::Ldarg_0);
    ilGen->Emit(OpCodes::Ldfld, keyFieldBuilder);
    ilGen->Emit(OpCodes::Call, ClrStubConstant::UnregisterDelegateMethodInfo);
    ilGen->Emit(OpCodes::Pop);
    // }
    //finally
    // {
    ilGen->BeginFinallyBlock();
    ilGen->Emit(OpCodes::Ldarg_0);
    ilGen->Emit(OpCodes::Call, ClrStubConstant::ObjectFinalizeMethodInfo);
   ilGen->EndExceptionBlock();
    // }
    ilGen->Emit(OpCodes::Ret);


    //これまでに定義したクラスを作成
    Type^ eventHandlerType = typeBuilder->CreateType();

    //定義したクラスを生成しデリゲート取得するメソッド作成
    DynamicMethod createMethod("Create_" + type->Name
        , type
        , gcnew array<Type^>(2) { GoshProc::typeid, IntPtr::typeid }
        , modBuilder 
       ); 
    ilGen = createMethod.GetILGenerator();
    //定義したクラスを格納するローカル変数定義
    ilGen->DeclareLocal(eventHandlerType);
    Label elseLabel =  ilGen->DefineLabel();
    Label endifLabel =  ilGen->DefineLabel();

    //定義したクラスのインスタンスを作成
    ilGen->Emit(OpCodes::Newobj, eventHandlerType->GetConstructor(Type::EmptyTypes));
    ilGen->Emit(OpCodes::Stloc_0);
    //引数として渡されるGoshProcオブジェクトをフィールドに設定
    ilGen->Emit(OpCodes::Ldloc_0);
    ilGen->Emit(OpCodes::Ldarg_0);
    ilGen->Emit(OpCodes::Stfld, eventHandlerType->GetField("proc"));

    //keyが指定されていない場合オブジェクトのFinalizeを実行する必要がないので
    //あらかじめSuppressFinalizeで実行を抑制する
    // if(key == IntPtr.Zero)
    ilGen->Emit(OpCodes::Ldarg_1);
    ilGen->Emit(OpCodes::Ldsfld, IntPtr::typeid->GetField("Zero", BindingFlags::Static | BindingFlags::Public));
    ilGen->Emit(OpCodes::Call, IntPtr::typeid->GetMethod("op_Equality", gcnew array<Type^>(2) { IntPtr::typeid, IntPtr::typeid} ));
    ilGen->Emit(OpCodes::Brfalse_S, elseLabel);
    // { then clause
    ilGen->Emit(OpCodes::Ldloc_0);
    ilGen->Emit(OpCodes::Call, GC::typeid->GetMethod("SuppressFinalize", BindingFlags::Static | BindingFlags::Public));
    ilGen->Emit(OpCodes::Br_S, endifLabel);
    // } else {
    //keyが指定されている場合はFinlizerを実行するため、オブジェクトにkey情報を保存する
    ilGen->MarkLabel(elseLabel);
    ilGen->Emit(OpCodes::Ldloc_0);
    ilGen->Emit(OpCodes::Ldarg_1);
    ilGen->Emit(OpCodes::Stfld, eventHandlerType->GetField("key"));
    // }
    ilGen->MarkLabel(endifLabel);

    //定義したクラスと、処理委譲用メソッドから戻り値のデリゲート作成
    ilGen->Emit(OpCodes::Ldloc_0);
    ilGen->Emit(OpCodes::Ldftn, eventHandlerType->GetMethod("DelegateEventHandler"));
    ilGen->Emit(OpCodes::Newobj, type->GetConstructor(
        gcnew array<Type^>(2) { Object::typeid, IntPtr::typeid }));
    ilGen->Emit(OpCodes::Ret);

    //定義したデリゲート取得用メソッドを作成
    return (DelegateCreator^) createMethod.CreateDelegate(DelegateCreator::typeid);
}

Delegate^ GetWrappedDelegate(Type^ type, GoshProc^ proc, IntPtr delegateTableKey)
{
    DelegateCreator^ creator = ClrStubConstant::GetDelegateCreator(type);
    if(creator == nullptr)
    {
        creator = DefineDelegateCreator(type, proc);
        ClrStubConstant::AddDelegateCreator(type, creator);
    }

    Delegate^ d  = creator(proc, delegateTableKey);
    if(delegateTableKey != IntPtr::Zero)
    {
        ClrStubConstant::RegisterDelegate(delegateTableKey, d);
    }
    
    return d;
}