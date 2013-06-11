/*
 * ClrMethod.cpp
 *
 * MIT License
 * Copyright 2013 aharisu
 * All rights reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 *
 * aharisu
 * foo.yobina@gmail.com
 */


#include "ClrMethod.h"

#include "Microsoft.Scripting/MethodBinder.h"
#include "PrimitiveOpCall.h"
#include "ClrDelegate.h"

using namespace System;
using namespace System::Text;
using namespace System::Reflection;
using namespace System::Runtime::InteropServices;
using namespace GaucheDotNet;
using namespace GaucheDotNet::Native;

Object^ ClrMethod::ToObject(ObjWrapper* obj)
{
    switch(obj->kind)
    {
    case OBJWRAP_BOOL:
        return ((int)obj->v.value) != 0 ? true : false;
    case OBJWRAP_INT:
        return (int)obj->v.value;
    case OBJWRAP_FLONUM:
        return (double)obj->v.real;
    case OBJWRAP_STRING:
        return Marshal::PtrToStringAnsi((IntPtr)obj->v.value);
    case OBJWRAP_PROC:
        return gcnew Procedure::GoshProcedure((IntPtr)obj->ptr);
    default: //OBJWRAP_CLROBJECT:
        {
            GCHandle gchObj = GCHandle::FromIntPtr((IntPtr)obj->v.value);
            return gchObj.Target;
        }
    }
}

Type^ ClrMethod::GetType(String^ name, bool valid)
{
    Type^ t;
    if(!valid)
    {
        t = Type::GetType(name);
        if(t != nullptr)
        {
            return t;
        }
    }

    for each(Assembly^ a in AppDomain::CurrentDomain->GetAssemblies())
    {
        t = a->GetType(name);
        if(t != nullptr)
        {
            return t;
        }
    }
    return nullptr;
}

//TODO delegate type
static void TypeSpecToString(TypeSpec* spec, StringBuilder^ builder)
{
    //builder->Append(gcnew String(spec->name));
    builder->Append(Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(spec->name))));
}

Type^ ClrMethod::TypeSpecToType(TypeSpec* spec)
{
    StringBuilder builder;
    TypeSpecToString(spec, %builder);
    return GetType(builder.ToString(), false);
}

static Object^ ToArgumentObject(Type^ type, ObjWrapper* arg)
{
    switch(arg->kind)
    {
    case OBJWRAP_BOOL:
        if(type->IsAssignableFrom(GoshBool::typeid))
        {
            return ((int)arg->v.value) != 0 ? GoshBool::True : GoshBool::False;
        }
        else
        {
            return ((int)arg->v.value) != 0 ? true : false;
        }
    case OBJWRAP_INT:
        if(type->IsAssignableFrom(GoshFixnum::typeid))
        {
            return gcnew GoshFixnum((int)arg->v.value);
        }
        else
        {
            return Convert::ChangeType((int)arg->v.value, type);
        }
    case OBJWRAP_FLONUM:
        if(type->IsAssignableFrom(GoshFlonum::typeid))
        {
            return gcnew GoshFlonum(arg->v.real);
        }
        else
        {
            return Convert::ChangeType(arg->v.real, type);
        }
    case OBJWRAP_STRING:
        if(type->IsAssignableFrom(GoshString::typeid))
        {
            return gcnew GoshString((IntPtr)arg->ptr);
        }
        else
        {
            return Marshal::PtrToStringAnsi(IntPtr(arg->v.value));
        }
    case OBJWRAP_PROC:
        {
            Object^ ret = gcnew Procedure::GoshProcedure((IntPtr)arg->v.value);
            if(Delegate::typeid->IsAssignableFrom(type))
            {
                ret = GetWrappedDelegate(type, (GoshProc^)ret, IntPtr::Zero);
            }
            return ret;
        }
    case OBJWRAP_CLROBJECT:
    default:
        return GCHandle::FromIntPtr(IntPtr(arg->v.value)).Target;
    }
}

static Object^ ToArgumentObject(ObjWrapper* arg)
{
    switch(arg->kind)
    {
    case OBJWRAP_BOOL:
        return ((int)arg->v.value) != 0 ? true : false;
    case OBJWRAP_INT:
        return (int)arg->v.value;
    case OBJWRAP_FLONUM:
        return arg->v.real;
    case OBJWRAP_STRING:
        return Marshal::PtrToStringAnsi(IntPtr(arg->v.value));
    case OBJWRAP_PROC:
        return gcnew Procedure::GoshProcedure((IntPtr)arg->ptr);
    case OBJWRAP_CLROBJECT:
    default:
        return GCHandle::FromIntPtr(IntPtr(arg->v.value)).Target;
    }
}

array<Object^>^ ClrMethod::ConstractArguments(MethodCandidate^ callMethod, bool callExtensionMethod)
{
    int paramCount = callMethod->Parameters->Count;
    int paramStartIndex = _isStatic ? 0 : 1;
    int argumentOffset = callExtensionMethod ? 1 : 0;
    
    if(callMethod->ParamsArgumentIndex != -1)
    {
        int aryCount = (paramCount - callMethod->ParamsArgumentIndex);
        int actualParamCount = paramCount - aryCount + argumentOffset;

        array<Object^>^ actualArgs = gcnew array<Object^>(actualParamCount);
        int index = paramStartIndex;
        for(;index < callMethod->ParamsArgumentIndex; ++index)
        {
            actualArgs[argumentOffset + index - paramStartIndex] = 
                ToArgumentObject(callMethod->Parameters[index]->Type, &_args[index - paramStartIndex]);
        }

        Array^ paramsAry = Array::CreateInstance(callMethod->ParamsElementType, aryCount);
        actualArgs[argumentOffset + index - paramStartIndex] = paramsAry;
        for(int i = 0;index < paramCount; ++index, ++i)
        {
            paramsAry->SetValue(
                ToArgumentObject(callMethod->Parameters[index]->Type, &_args[index - paramStartIndex])
                , i);
        }

        return actualArgs;
    }
    else
    {
        array<Object^>^ arguments = gcnew array<Object^>(_numArg + argumentOffset);
        for(int i = 0;i < _numArg;++i)
        {
            arguments[argumentOffset + i] = ToArgumentObject(callMethod->Parameters[i + paramStartIndex]->Type, &_args[i]);
        }

        return arguments;
    }
}

bool ClrMethod::CreateArgTypes(StringBuilder^ builder, array<ArgType>^% argTypes)
{
    int startIndex = _isStatic ? 0 : 1;
    if(_methodSpec->numParamSpec >= 0)
    {
        argTypes = gcnew array<ArgType>(_methodSpec->numParamSpec + startIndex);
        for(int i = 0;i < _methodSpec->numParamSpec;++i)
        {
            TypeSpecToString(&(_methodSpec->paramSpec[i]), builder);
            argTypes[i + startIndex].type = ClrMethod::GetType(builder->ToString(), false);
            argTypes[i + startIndex].kind = OBJWRAP_CLROBJECT;
            argTypes[i + startIndex].attr = _methodSpec->paramSpec[i].attr;
            builder->Length = 0;
        }
        return true;
    }
    else
    {
        argTypes = gcnew array<ArgType>(_numArg + startIndex);
        for(int i = 0;i < _numArg;++i)
        {
            switch(_args[i].kind)
            {
            case OBJWRAP_CLROBJECT:
                {
                Object^ obj = GCHandle::FromIntPtr(IntPtr(_args[i].v.value)).Target;
                argTypes[i + startIndex].type = obj->GetType();
                argTypes[i + startIndex].kind = OBJWRAP_CLROBJECT;
                argTypes[i + startIndex].attr = TYPESPEC_ATTR_NORMAL;
                }
                break;
            case OBJWRAP_BOOL:
                argTypes[i + startIndex].type = Boolean::typeid;
                argTypes[i + startIndex].kind = OBJWRAP_BOOL;
                argTypes[i + startIndex].attr = TYPESPEC_ATTR_NORMAL;
            case OBJWRAP_INT:
                argTypes[i + startIndex].type = Int32::typeid;
                argTypes[i + startIndex].kind = OBJWRAP_INT;
                argTypes[i + startIndex].attr = TYPESPEC_ATTR_NORMAL;
                break;
            case OBJWRAP_FLONUM:
                argTypes[i + startIndex].type = Double::typeid;
                argTypes[i + startIndex].kind = OBJWRAP_FLONUM;
                argTypes[i + startIndex].attr = TYPESPEC_ATTR_NORMAL;
                break;
            case OBJWRAP_STRING:
                argTypes[i + startIndex].type = String::typeid;
                argTypes[i + startIndex].kind = OBJWRAP_STRING;
                argTypes[i + startIndex].attr = TYPESPEC_ATTR_NORMAL;
                break;
            case OBJWRAP_PROC:
                argTypes[i + startIndex].type = Delegate::typeid;
                argTypes[i + startIndex].delegateParameterCount = ((ScmProcedure*)_args[i].ptr)->required;
                argTypes[i + startIndex].kind = OBJWRAP_PROC;
                argTypes[i + startIndex].attr = TYPESPEC_ATTR_NORMAL;
                break;
            }
        }
        return false;
    }
}

void* ClrMethod::CallNew()
{
    StringBuilder builder;
    TypeSpecToString((TypeSpec*)_methodSpec, %builder);
    Type^ targetType = ClrMethod::GetType(builder.ToString(), false);
    if(targetType == nullptr)
    {
        throw gcnew GoshException("unknown type");
    }
    builder.Length = 0;

    //メソッド名と一緒にパラメータ型指定がある場合はtrue
    array<ArgType>^ argTypes;
    bool isSpecifyParamType = CreateArgTypes(%builder, argTypes);

    List<MethodBase^> candidates;
    for each(MethodBase^ info in targetType->GetConstructors(BindingFlags::Public | BindingFlags::Instance))
    {
        bool hasParams = false;
        array<ParameterInfo^>^ paramInfoAry = info->GetParameters();
        if(paramInfoAry->Length != 0)
        {
            hasParams = CompilerHelpers::IsParamArray(paramInfoAry[paramInfoAry->Length - 1]);
        }

        if(hasParams)
        {
            if(_numArg < paramInfoAry->Length - 1) continue;
        }
        else
        {
            if(_numArg != paramInfoAry->Length) continue;
        }
        candidates.Add(info);
    }

    if(candidates.Count == 0)
    {
        throw gcnew GoshException("Callable constructor can not be found");
    }

    MethodBinder mb(%candidates, BinderType::Constractor);
    MethodCandidate^ mc = mb.MakeBindingTarget(CallType::None, _numArg, argTypes, isSpecifyParamType);
    if(mc == nullptr)
    {
        throw gcnew GoshException("Callable constructor can not be found");
    }

    array<Object^>^ arguments = ConstractArguments(mc, false);
    Object^ result = ((ConstructorInfo^)mc->Target->Method)->Invoke(arguments);
    return (void*)(IntPtr)GCHandle::Alloc(result);
}

static array<Type^>^ ConvertToTypeArray(TypeSpec* typeSpec, int numTypeSpec)
{
    StringBuilder builder; 
    array<Type^>^ typeAry = gcnew array<Type^>(numTypeSpec);
    for(int i = 0;i < numTypeSpec;++i)
    {
        TypeSpecToString(&(typeSpec[i]), %builder);
        typeAry[i] = ClrMethod::GetType(builder.ToString(), false);
        builder.Length = 0;
    }

    return typeAry;
}

static Type^ GetHigherLevel(Type^ t1, Type^ t2)
{
    if (t1 == t2)
    {
        return t1;
    }
    else if (t1->IsPrimitive)
    {
        if (t2->IsPrimitive)
        {
            TypeCode t1TypeCode = Type::GetTypeCode(t1);
            TypeCode t2TypeCode = Type::GetTypeCode(t2);
            if (t1TypeCode == t2TypeCode)
            {
                return t1;
            }
            if (CompilerHelpers::CanImplicitConvertFrom(t1TypeCode, t2TypeCode))
            {
                return t2;
            }
            else if (CompilerHelpers::CanImplicitConvertFrom(t2TypeCode, t1TypeCode))
            {
                return t1;
            }
            else
            {
                return nullptr;
            }
        }
        else if (t2 == Object::typeid)
        {
            return t2;
        }
        else
        {
            return nullptr;
        }
    }
    else if (t2->IsPrimitive)
    {
        if (t1 == Object::typeid)
        {
            return t1;
        }
        else
        {
            return nullptr;
        }
    }
    else
    {
        if (t1->IsSubclassOf(t2))
        {
            return t2;
        }
        else if (t2->IsSubclassOf(t1))
        {
            return t1;
        }
        else
        {
            return nullptr;
        }
    }
}

static Type^ GetCompatiType(Type^ from, Type^ to)
{
    if(to->IsGenericType)
    {
        to = to->GetGenericTypeDefinition();
    }
    if(from->IsGenericType)
    {
        from = from->GetGenericTypeDefinition();
    }

    if(to->IsInterface)
    {
        for each(Type^ t in from->GetInterfaces())
        {
            Type^ tt = t->IsGenericType ? t->GetGenericTypeDefinition() : t;
            if(tt == to)
            {
                return t;
            }
        }
    }
    else
    {
        do
        {
            Type^ tt = from;
            if(tt->IsGenericType)
            {
                tt = tt->GetGenericTypeDefinition();
            }
            if(tt == to)
            {
                return from;
            }

            from = from->BaseType;
        }while(from != nullptr);
    }

    return nullptr;
}

static bool GenericTypeMatching(Type^ paramType, Type^ argType, bool isLambdaType
                                , array<Type^>^ genericType, array<Type^>^% inLambdaGenericType)
{
    if(paramType->IsGenericParameter)
    {
        array<Type^>^ target;
        if(isLambdaType)
        {
            //Gaucheのlambdaで記述された式からGenericの型(Object型)を取得している場合は
            //既存の型を上書きしない。
            if(inLambdaGenericType == nullptr)
            {
                inLambdaGenericType = gcnew array<Type^>(genericType->Length);
            }
            target = inLambdaGenericType;
        }
        else
        {
            target = genericType;
        }

        int index = paramType->GenericParameterPosition;
        if(target[index] == nullptr)
        {
            target[index] = argType;
        }
        else
        {
            Type^ actualType = GetHigherLevel(target[index], argType);
            if(actualType == nullptr)
            {
                return false;
            }
            target[index] = actualType;
        }
        return true;
    }
    else if(paramType->IsArray)
    {
        if(argType->IsArray)
        {
            return GenericTypeMatching(paramType->GetElementType(), argType->GetElementType(), isLambdaType
                , genericType, inLambdaGenericType);
        }
        else
        {
            return false;
        }
    }
    else if(GoshProc::typeid == argType && Delegate::typeid->IsAssignableFrom(paramType))
    {
        MethodInfo^ invokeInfo = paramType->GetMethod("Invoke");
        for each(ParameterInfo^ pi in invokeInfo->GetParameters())
        {
            if(pi->ParameterType->ContainsGenericParameters)
            {
                GenericTypeMatching(pi->ParameterType, Object::typeid, true
                    , genericType, inLambdaGenericType);
            }
        }
        if(invokeInfo->ReturnType->ContainsGenericParameters)
        {
            GenericTypeMatching(invokeInfo->ReturnType, Object::typeid, true
                , genericType, inLambdaGenericType);
        }

        return true;
    }
    else
    {
        Type^ compatiType = GetCompatiType(argType, paramType);
        if(compatiType == nullptr) return false;

        array<Type^>^ paramGenericArgs =  paramType->GetGenericArguments();
        array<Type^>^ argGenericArgs = argType->GetGenericArguments();
        array<Type^>^ compatiGenericArgs = compatiType->GetGenericArguments();

        for(int i = 0;i < paramGenericArgs->Length;++i)
        {
            Type^ paramGenericArg = paramGenericArgs[i];
            Type^ concreteType = compatiGenericArgs[i];
            if(concreteType->IsGenericParameter)
            {
                concreteType = argGenericArgs[concreteType->GenericParameterPosition];
            }

            if(paramGenericArg->ContainsGenericParameters)
            {
                if(!GenericTypeMatching(paramGenericArg, concreteType, isLambdaType
                    , genericType, inLambdaGenericType)) return false;
            }
            else
            {
                if(GetCompatiType(concreteType, paramGenericArg) == nullptr) return false;
            }
        }

        return true;
    }
}

static MethodInfo^ MakeGenericMethod(MethodInfo^ mi
                                     , TypeSpec* methodSpec, bool isStatic, array<ArgType>^ argTypes)
{
    array<Type^>^ genericArgs = mi->GetGenericArguments();
    if (methodSpec->numGenericSpec >= 0)
    {
        //ジェネリック型の指定がある場合は指定された型でメソッドを作成する

        if (genericArgs->Length != methodSpec->numGenericSpec)
        {
            return nullptr;
        }

        return mi->MakeGenericMethod(ConvertToTypeArray(methodSpec->genericSpec, methodSpec->numGenericSpec));
    }
    else
    {
        //ジェネリック型指定がない場合は実際の引数から型をジェネリック型を判別する
        array<ParameterInfo^>^ piAry = mi->GetParameters();

        array<Type^>^ paramTypes = nullptr;
        if (methodSpec->numParamSpec >= 0)
        {
            if (piAry->Length != methodSpec->numParamSpec)
            {
                return nullptr;
            }
            paramTypes = ConvertToTypeArray(methodSpec->paramSpec, methodSpec->numParamSpec);
        }
        else
        {
            int startIndex = (isStatic ? 0 : 1);
            paramTypes = gcnew array<Type^>(argTypes->Length - startIndex);
            for(int i = startIndex;i < argTypes->Length;++i)
            {
                switch(argTypes[i].kind)
                {
                case OBJWRAP_CLROBJECT:
                    paramTypes[i - startIndex] = argTypes[i].type;
                    break;
                case OBJWRAP_BOOL:
                    paramTypes[i - startIndex] = GoshBool::typeid;
                    break;
                case OBJWRAP_INT:
                    paramTypes[i - startIndex] = GoshFixnum::typeid;
                    break;
                case OBJWRAP_FLONUM:
                    paramTypes[i - startIndex] = GoshFlonum::typeid;
                    break;
                case OBJWRAP_STRING:
                    paramTypes[i - startIndex] = GoshString::typeid;
                    break;
                case OBJWRAP_PROC:
                    paramTypes[i - startIndex] = GoshProc::typeid;
                    break;
                //TODO more primitive type
                }
            }
        }

        array<Type^>^ genericType = gcnew array<Type^>(genericArgs->Length);
        array<Type^>^ inLambdaGenericType = nullptr;
        int index = 0;
        for each (ParameterInfo^ pi in piAry)
        {
            Type^ paramType = pi->ParameterType;
            if(paramType->ContainsGenericParameters)
            {
                if(!GenericTypeMatching(paramType, paramTypes[index], false, genericType, inLambdaGenericType))
                {
                    return nullptr;
                }
            }
            ++index;
        }

        for(int i = 0;i < genericType->Length;++i)
        {
            if(genericType[i] == nullptr)
            {
                if(inLambdaGenericType != nullptr 
                    && inLambdaGenericType[i] != nullptr)
                {
                    genericType[i] = inLambdaGenericType[i];
                }
                else
                {
                    //throw new InvalidOperationException("ジェネリック型を特定できませんでした。");
                    return nullptr;
                }
            }
        }

        return mi->MakeGenericMethod(genericType);
    }
}

static MethodBase^ CreateCandidate(MethodBase^ info, TypeSpec* methodSpec, bool isStatic, int numArg, array<ArgType>^ argTypes)
{
    bool hasParams = false;
    array<ParameterInfo^>^ paramInfoAry = info->GetParameters();
    if(paramInfoAry->Length != 0)
    {
        hasParams = CompilerHelpers::IsParamArray(paramInfoAry[paramInfoAry->Length - 1]);
    }

    if(hasParams)
    {
        if(numArg < paramInfoAry->Length - 1) return nullptr;
    }
    else
    {
        if(numArg != paramInfoAry->Length) return nullptr;
    }

    if(info->ContainsGenericParameters)
    {
        return MakeGenericMethod((MethodInfo^)info, methodSpec, isStatic, argTypes);
    }
    else
    {
        return info;
    }
}

static MethodCandidate^ GetApplicableMethod(System::Collections::IEnumerable^ methods
    , TypeSpec* methodSpec, bool isStatic, int numArg, array<ArgType>^ argTypes, bool isSpecifyParamType
    , List<MethodBase^>^ candidates)
{
    //実行メソッドの候補リストを作成する
    for each(MethodBase^ info in methods)
    {
        MethodBase^ candidate = CreateCandidate(info, methodSpec, isStatic, numArg, argTypes);
        if(candidate != nullptr)
        {
            candidates->Add(candidate);
        }
    }

    if(candidates->Count == 0)
    {
        return nullptr;
    }

    //パラメータ型配列から適用可能なメソッドを取得する
    MethodBinder mb(candidates, BinderType::Normal);
    return mb.MakeBindingTarget(
        isStatic ? CallType::None : CallType::ImplicitInstance
        ,  (isStatic ? 0 : 1) + numArg
        , argTypes
        , isSpecifyParamType
        );
}

void* ClrMethod::CallMethod(void* module)
{
    StringBuilder builder;
    Type^ targetType;
    Object^ instance;

    //実行メソッド検索に使用するパラメータの型配列
    array<ArgType>^ argTypes = nullptr;
    //メソッド名と一緒にパラメータ型指定がある場合はtrue
    bool isSpecifyParamType = _methodSpec->numParamSpec >= 0;
    //明示的なパラメータ型指定がある場合だけ、
    //プリミティブ型同士の演算で使用するためこのタイミングで作成する。
    if(isSpecifyParamType)
    {
        CreateArgTypes(%builder, argTypes);
    }

    bool isOperator = false;
    String^ method = gcnew String(_methodSpec->name);
    if(_isStatic)
    {
        TypeSpec* spec = (TypeSpec*)_obj;
        TypeSpecToString(spec, %builder);
        targetType = ClrMethod::GetType(builder.ToString(), false);
        if(targetType == nullptr)
        {
            throw gcnew GoshException("unknown type");
        }

        builder.Length = 0;
    }
    else
    {
        instance = ToObject((ObjWrapper*)_obj);
        targetType = instance->GetType();

#pragma region プリミティブ型同士の演算子を実行する {
        if(_numArg < 2 && method->Length <= 3)
        {
            Object^ secondArg = (_numArg == 1) ?  ToArgumentObject(&_args[0]) : nullptr;

            Object^ result = nullptr;
            if(method->Length == 1)
            {
                switch(method[0])
                {
                case '+':
                    if(targetType->IsPrimitive
                        && (_numArg == 0 || secondArg->GetType()->IsPrimitive))
                    {
                        result = PrimitiveAdd(argTypes, instance, secondArg);
                    }
                    else if(targetType == String::typeid && _numArg == 1)
                    {
                        result = String::Concat(instance, secondArg);
                    }
                    else
                    {
                        method = (_numArg == 0) ? "op_UnaryPlus" : "op_Addition";
                        isOperator = true;
                    }
                    break;
                case '-':
                    if(targetType->IsPrimitive
                        && (_numArg == 0 || secondArg->GetType()->IsPrimitive))
                    {
                        result = PrimitiveSub(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = (_numArg == 0) ? "op_UnaryNegation" : "op_Subtraction";
                        isOperator = true;
                    }
                    break;
                case '*':
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveMul(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_Multiply";
                        isOperator = true;
                    }
                    break;
                case '/':
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveDiv(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_Division";
                        isOperator = true;
                    }
                    break;
                case '%':
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveRemainder(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_Modulus";
                        isOperator = true;
                    }
                    break;
                case '&':
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveBitwiseAnd(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_BitwiseAnd";
                        isOperator = true;
                    }
                    break;
                case '|':
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveBitwiseOr(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_BitwiseOr";
                        isOperator = true;
                    }
                    break;
                case '^':
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveExclusiveOr(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_ExclusiveOr";
                        isOperator = true;
                    }
                    break;
                case '~':
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveOnesComplement(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_OnesComplement";
                        isOperator = true;
                    }
                    break;
                case '!':
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveNot(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_LogicalNot";
                        isOperator = true;
                    }
                    break;
                case '>':
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveGt(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_GreaterThan";
                        isOperator = true;
                    }
                    break;
                case '<':
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveLt(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_LessThan";
                        isOperator = true;
                    }
                    break;
                }
            }
            else if(method->Length == 2)
            {
                if(method == "==")
                {
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveEq(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_Equality";
                        isOperator = true;
                    }
                }
                else if(method == "&&")
                {
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveLogAnd(argTypes, instance, secondArg);
                    }

                    if(result == nullptr)
                    {
                        //無効な演算子
                        throw gcnew GoshException("invalid operation");
                    }
                }
                else if(method == "||")
                {
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveLogOr(argTypes, instance, secondArg);
                    }

                    if(result == nullptr)
                    {
                        //無効な演算子
                        throw gcnew GoshException("invalid operation");
                    }
                }
                else if(method == "!=")
                {
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveNotEq(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_Inequality";
                        isOperator = true;
                    }
                }
                else if(method == ">=")
                {
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveGtEq(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_GreaterThanOrEqual";
                        isOperator = true;
                    }
                }
                else if(method == "<=")
                {
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveLtEq(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_LessThanOrEqual";
                        isOperator = true;
                    }
                }
                else if(method == "<<")
                {
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveLeftShift(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_LeftShift";
                        isOperator = true;
                    }
                }
                else if(method == ">>")
                {
                    if(targetType->IsPrimitive)
                    {
                        result = PrimitiveRightShift(argTypes, instance, secondArg);
                    }
                    else
                    {
                        method = "op_RightShift";
                        isOperator = true;
                    }
                }
            }
            else //if(method->Length == 3)
            {
                int inc = 0;
                bool pre = true;
                if(method == "++x")
                {
                    inc = 1;
                    method = "op_Increment";
                    isOperator = true;
                }
                else if(method == "x++")
                {
                    inc = 1;
                    pre = false;
                    method = "op_Increment";
                    isOperator = true;
                }
                else if(method == "--x")
                {
                    inc = -1;
                    method = "op_Decrement";
                    isOperator = true;
                }
                else if(method == "x--")
                {
                    inc = -1;
                    pre = false;
                    method = "op_Decrement";
                    isOperator = true;
                }

                if(inc != 0)
                {
					//TODO increment and decrement
                }
            }

            if(result != nullptr)
            {
                return (void*)(IntPtr)GCHandle::Alloc(result);
            }
        }
#pragma endregion }
    }

    //明示的なパラメータ型指定がない場合はこのタイミングで
    //メソッド検索用引数型配列を作成する
    if(!isSpecifyParamType)
    {
        CreateArgTypes(%builder, argTypes);
    }
    if(!_isStatic)
    {
        argTypes[0].kind = OBJWRAP_CLROBJECT;
        argTypes[0].type = targetType;
        argTypes[0].attr = TYPESPEC_ATTR_NORMAL;
    }

    //実行メソッドの候補リストを作成し、パラメータ型配列から適用可能なメソッドを取得する
    bool callExtensionMethod = false;
    List<MethodBase^> candidates;
    MethodCandidate^ mc = GetApplicableMethod(
        targetType->GetMember(method, MemberTypes::Method, 
            BindingFlags::Public | ((_isStatic | isOperator) ? BindingFlags::Static : BindingFlags::Instance))
        , _methodSpec, _isStatic, _numArg, argTypes, isSpecifyParamType
        , %candidates);

    if(mc == nullptr && !_isStatic)
    {
        callExtensionMethod = true;

        Dictionary<String^, List<MethodInfo^>^>^ nameToMethods;
        if(_eachModuleExtensionMethods->TryGetValue((IntPtr)module, nameToMethods))
        {
            List<MethodInfo^>^ methods;
            if(nameToMethods->TryGetValue(method, methods))
            {
                candidates.Clear();
                mc = GetApplicableMethod(methods
                    ,_methodSpec, true, _numArg + 1, argTypes, isSpecifyParamType
                    , %candidates);
            }
        }
    }

    if(mc == nullptr)
    {
        throw gcnew GoshException("Applicable method can not be found");
    }

    //メソッド実行
    array<Object^>^ arguments = ConstractArguments(mc, callExtensionMethod);
    //拡張メソッドを呼ぶ場合は、引数の最初にインスタンスを設定する
    if(callExtensionMethod)
    {
      arguments[0] = instance;
    }
    Object^ result = mc->Target->Method->Invoke((_isStatic || callExtensionMethod) ? nullptr : instance, arguments);
    //TODO 戻り値がvoidのメソッドと本当にnullが返ってきたときの場合わけ
    if(result == nullptr)
    {
        return 0;
    }
    else
    {
        return (void*)(IntPtr)GCHandle::Alloc(result);
    }
}
