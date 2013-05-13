/*
 * ClrBridge.cpp
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

#define DLL_EXPORT

#include "ClrBridge.h"
#include "ClrMethod.h"
#include "ClrStubConstant.h"
#include "ClrDelegate.h"

using namespace System;
using namespace System::Text;
using namespace System::Reflection;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;
using namespace GaucheDotNet;
using namespace GaucheDotNet::Native;

#ifdef __cplusplus
extern "C" {
#endif

DECDLL void ReleaseClrObject(void* obj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    gchObj.Free();
}

DECDLL void* ToClrObj(void* scmObj)
{
    Object^ hClrObj = gcnew GoshClrObject(IntPtr(scmObj));

    return (void*)(IntPtr) GCHandle::Alloc(hClrObj);
}

DECDLL void* ClrPrint(void* clrObj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(clrObj));
    Object^ target = gchObj.Target;

    String^ str;
    if(target == nullptr)
    {
        str = "null";
    }
    else
    {
        str = target->ToString();
    }
    Type^ t = target->GetType();
    if(String::typeid == target->GetType())
    {
        str = "\"" + str + "\"";
    }

    return (void*)GoshInvoke::Scm_MakeString(str, -1, -1, StringFlags::Copying);
}

static void ObjectNullCheck(Object^ obj)
{
    if(obj == nullptr)
    {
        ClrStubConstant::RaiseClrError(gcnew ArgumentNullException("obj can not be a null"));
        //does not reach
    }
}

DECDLL void* BooleanToClr(int boolean)
{
    return (void*)(IntPtr)GCHandle::Alloc((boolean == 1) ? true : false);
}

DECDLL int ClrToBoolean(void* obj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ target = gchObj.Target;
    ObjectNullCheck(target);

    if(target->GetType() == GoshBool::typeid)
    {
        return (target == GoshBool::True) ? 1 : 0;
    }
    else
    {
        try 
        {
            Boolean boolean = (Boolean)Convert::ChangeType(target, Boolean::typeid);
            return boolean ? 1 : 0;
        }
        catch(InvalidCastException^ e)
        {
            ClrStubConstant::RaiseClrError(e);
            //does not reach
            return 0;
        }
    }
}


DECDLL int ClrToInt(void* obj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ target = gchObj.Target;
    ObjectNullCheck(target);

    if(target->GetType() == GoshFixnum::typeid)
    {
        return ((GoshFixnum^)target)->Num;
    }
    else
    {
        try 
        {
            Object^ objNum = Convert::ChangeType(target, Int32::typeid);
            return (Int32)objNum;
        }
        catch(InvalidCastException^ e)
        {
            ClrStubConstant::RaiseClrError(e);
            //does not reach
            return 0;
        }
    }
}

DECDLL void* FixnumToClr(signed long int num)
{
    return (void*)(IntPtr)GCHandle::Alloc(num);
}

DECDLL void* ClrToGoshString(void* clrObj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(clrObj));
    Object^ target = gchObj.Target;
    ObjectNullCheck(target);

    if(target->GetType() == GoshString::typeid)
    {
        return (void*)((GoshString^)target)->Ptr;
    }
    else
    {
        return (void*)GoshInvoke::Scm_MakeString(
            target->ToString()
            , -1, -1, 
            StringFlags::Copying);
    }
}

DECDLL void* StringToClr(const char* str)
{
    Object^ obj = gcnew String(str);
    return (void*)(IntPtr) GCHandle::Alloc(obj);
}

static void InfoNullCheck(MemberInfo^ info, Object^ obj, String^ kind, String^ name)
{
    if(info == nullptr)
    {
        ClrStubConstant::RaiseClrError(gcnew ArgumentException(
            String::Format("{0} class doesn't have such {1}: {2}", obj->GetType()->FullName, kind, name)));
        //does not reach
    }
}

DECDLL void ClrPropSetClrObj(void* obj, const char* name
                            , ObjWrapper* indexer, int numIndexer
                            , void* clrObj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    String^ propName = (name == 0) ?
        propName = "Item" : //default indexer name;
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name)));

    PropertyInfo^ propInfo = hObj->GetType()->GetProperty(propName);
    InfoNullCheck(propInfo, hObj, "property", propName);

    GCHandle gchVal = GCHandle::FromIntPtr(IntPtr(clrObj));
    Object^ hVal = gchVal.Target;

    array<Object^>^ index = nullptr;
    if(numIndexer != 0)
    {
        index = gcnew array<Object^>(numIndexer);
        for(int i = 0;i < numIndexer;++i)
        {
            index[i] = ClrMethod::ToObject(&(indexer[i]));
        }
    }

    try
    {
        propInfo->SetValue(hObj, hVal, index);
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
}

DECDLL void ClrPropSetScmObj(void* obj, const char* name
                            , ObjWrapper* indexer, int numIndexer
                            , void* scmObj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    String^ propName = (name == 0) ?
        propName = "Item" : //default indexer name;
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name)));

    PropertyInfo^ propInfo = hObj->GetType()->GetProperty(propName);
    InfoNullCheck(propInfo, hObj, "property", propName);

    //ScmObj to .Net object(GoshObj instance)
    Object^ hVal = gcnew GoshClrObject(IntPtr(scmObj));

    array<Object^>^ index = nullptr;
    if(numIndexer != 0)
    {
        index = gcnew array<Object^>(numIndexer);
        for(int i = 0;i < numIndexer;++i)
        {
            index[i] = ClrMethod::ToObject(&(indexer[i]));
        }
    }

    try
    {
        propInfo->SetValue(hObj, hVal, index);
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
}

DECDLL void ClrPropSetInt(void* obj, const char* name
                         , ObjWrapper* indexer, int numIndexer
                         , int value)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    String^ propName = (name == 0) ?
        propName = "Item" : //default indexer name;
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name)));

    PropertyInfo^ propInfo = hObj->GetType()->GetProperty(propName);
    InfoNullCheck(propInfo, hObj, "property", propName);

    try 
    {
        Object^ objNum = Convert::ChangeType((Int32)value, propInfo->PropertyType);

        array<Object^>^ index = nullptr;
        if(numIndexer != 0)
        {
            index = gcnew array<Object^>(numIndexer);
            for(int i = 0;i < numIndexer;++i)
            {
                index[i] = ClrMethod::ToObject(&(indexer[i]));
            }
        }

        propInfo->SetValue(hObj, objNum, index);
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
}

DECDLL void ClrPropSetString(void* obj, const char* name
                            , ObjWrapper* indexer, int numIndexer
                            , const char* value)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    String^ propName = (name == 0) ?
        propName = "Item" : //default indexer name;
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name)));

    PropertyInfo^ propInfo = hObj->GetType()->GetProperty(propName);
    InfoNullCheck(propInfo, hObj, "property", propName);

    //string型を設定できるプロパティか?
    if(!propInfo->PropertyType->IsAssignableFrom(String::typeid))
    {
        ClrStubConstant::RaiseClrError(gcnew ArgumentException(
            String::Format("{0} property can not assign of String object", propName)));
    }

    array<Object^>^ index = nullptr;
    if(numIndexer != 0)
    {
        index = gcnew array<Object^>(numIndexer);
        for(int i = 0;i < numIndexer;++i)
        {
            index[i] = ClrMethod::ToObject(&(indexer[i]));
        }
    }

    try
    {
        propInfo->SetValue(hObj
            , Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(value)))
            , index);
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
}

DECDLL void* ClrPropGet(ObjWrapper* obj, const char* name
                        , ObjWrapper* indexer, int numIndexer)
{
    Object^ hObj = ClrMethod::ToObject(obj);
    ObjectNullCheck(hObj);

    String^ propName = (name == 0) ?
        propName = "Item" : //default indexer name;
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name)));

    PropertyInfo^ propInfo = hObj->GetType()->GetProperty(propName);
    InfoNullCheck(propInfo, hObj, "property", propName);

    array<Object^>^ index = nullptr;
    if(numIndexer != 0)
    {
        index = gcnew array<Object^>(numIndexer);
        for(int i = 0;i < numIndexer;++i)
        {
            index[i] = ClrMethod::ToObject(&(indexer[i]));
        }
    }

    try
    {
        Object^ ret = propInfo->GetValue(hObj, index);
        return (void*)(IntPtr) GCHandle::Alloc(ret);
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
        //does not reach
        return 0;
    }
}

#pragma region field setter / getter {

DECDLL void ClrFieldSetClrObj(void* obj, const char* name,  void* clrObj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    String^ fieldName = Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name)));
    FieldInfo^ fieldInfo = hObj->GetType()->GetField(fieldName);
    InfoNullCheck(fieldInfo, hObj, "field", fieldName);

    GCHandle gchVal = GCHandle::FromIntPtr(IntPtr(clrObj));
    Object^ hVal = gchVal.Target;

    try
    {
        fieldInfo->SetValue(hObj, hVal);
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
}

DECDLL void ClrFieldSetScmObj(void* obj, const char* name,  void* scmObj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    String^ fieldName = Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name)));
    FieldInfo^ fieldInfo = hObj->GetType()->GetField(fieldName);
    InfoNullCheck(fieldInfo, hObj, "field", fieldName);

    //ScmObj to .Net object(GoshObj instance)
    Object^ hVal = gcnew GoshClrObject(IntPtr(scmObj));

    try
    {
        fieldInfo->SetValue(hObj, hVal);
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
}

DECDLL void ClrFieldSetInt(void* obj, const char* name,  int value)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    String^ fieldName = Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name)));
    FieldInfo^ fieldInfo = hObj->GetType()->GetField(fieldName);
    InfoNullCheck(fieldInfo, hObj, "field", fieldName);

    try 
    {
        Object^ objNum = Convert::ChangeType((Int32)value, fieldInfo->FieldType);
        fieldInfo->SetValue(hObj, objNum);
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
}

DECDLL void ClrFieldSetString(void* obj, const char* name,  const char* value)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    String^ fieldName = Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name)));
    FieldInfo^ fieldInfo = hObj->GetType()->GetField(fieldName);
    InfoNullCheck(fieldInfo, hObj, "field", fieldName);

    //string型を設定できるフィールドか?
    if(!fieldInfo->FieldType->IsAssignableFrom(String::typeid))
    {
        ClrStubConstant::RaiseClrError(gcnew ArgumentException(
            String::Format("{0} field can not assign of String object", fieldName)));
    }

    try
    {
        fieldInfo->SetValue(hObj, 
            Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(value))));
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
}

DECDLL void* ClrFieldGet(void* obj, const char* name)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    String^ fieldName = Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name)));
    FieldInfo^ fieldInfo = hObj->GetType()->GetField(fieldName);
    InfoNullCheck(fieldInfo, hObj, "field", fieldName);

    try
    {
        Object^ ret = fieldInfo->GetValue(hObj);
        return (void*)(IntPtr) GCHandle::Alloc(ret);
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
        //does not reach
        return 0;
    }
}

#pragma endregion }

#pragma region event adder / remover {

static void ClrEventAddClrProc(void* obj, const char* name, GoshProc^ proc, IntPtr key)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    String^ eventName = Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name)));
    EventInfo^ eventInfo = hObj->GetType()->GetEvent(eventName);
    InfoNullCheck(eventInfo, hObj, "event", eventName);

    MethodInfo^ invokeInfo = eventInfo->EventHandlerType->GetMethod("Invoke");
    if(proc->Required != invokeInfo->GetParameters()->Length)
    {
        Exception^ e = gcnew ArgumentException(
            "wrong number of arguments." 
            + eventInfo->EventHandlerType->FullName 
            + "  requires " + invokeInfo->GetParameters()->Length
            + ", but got " + proc->Required + " arity funcion."
            );
        ClrStubConstant::RaiseClrError(e);
    }

    try
    {
        Delegate^ d = GetWrappedDelegate(eventInfo->EventHandlerType, proc, key);
        eventInfo->AddEventHandler(hObj, d);
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
}

DECDLL void ClrEventAddGoshProc(void* obj, const char* name, void* goshProc)
{
    GoshProc^ proc = gcnew Procedure::GoshProcedure((IntPtr)goshProc);

    ClrEventAddClrProc(obj, name, proc, (IntPtr)goshProc);
}

DECDLL void ClrEventAddClrObj(void* obj, const char* name, void* clrObj)
{
    Object^ hClrObj = GCHandle::FromIntPtr(IntPtr(clrObj)).Target;
    ObjectNullCheck(hClrObj);
    
    //GoshProcにキャストできる型でなければ終了
    if(!(GoshProc::typeid)->IsAssignableFrom(hClrObj->GetType()))
    {
        Exception^ e = gcnew ArgumentException("requires GoshProc object. but got " + hClrObj->GetType()->FullName);
        ClrStubConstant::RaiseClrError(e);
    }

    ClrEventAddClrProc(obj, name, (GoshProc^)hClrObj, (IntPtr)clrObj);
}

DECDLL void ClrEventRemove(void* obj, const char* name, void* proc)
{
    Delegate^ d = ClrStubConstant::UnregisterDelegate((IntPtr)proc);
    if(d == nullptr)
    {
        return;
    }

    //DelegateTableから削除する必要がないのでFinalieが呼ばれないようにする
    Object^ target = d->Target;
    if(target != nullptr)
    {
        GC::SuppressFinalize(target);
    }

    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    String^ eventName = Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name)));
    EventInfo^ eventInfo = hObj->GetType()->GetEvent(eventName);
    InfoNullCheck(eventInfo, hObj, "event", eventName);

    try
    {
        eventInfo->RemoveEventHandler(hObj, d);
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
}

#pragma endregion }

DECDLL int ClrReferenceAssembly(const char* assemblyName)
{
    String^ name = gcnew String(assemblyName);
    Assembly^ assembly = nullptr;

    try
    {
        AssemblyName^ asmName = AssemblyName::GetAssemblyName(name);
        assembly = Assembly::Load(asmName);
    }
    catch(System::IO::FileNotFoundException^)
    {
        try
        {
            assembly = Assembly::Load(name);
        }
        catch(System::IO::FileNotFoundException^) {}
    }

    if(assembly == nullptr)
    {
#pragma warning(push)
#pragma warning(disable :  4947)
        assembly = Assembly::LoadWithPartialName(name);
#pragma warning(pop)
    }

    return assembly == nullptr ?  0 : 1;
}

DECDLL void* ClrValidTypeName(const char* fullTypeName)
{
    String^ name = gcnew String(fullTypeName);

    Type^ ret = ClrMethod::GetType(name, true);
    if(ret == nullptr)
    {
        return 0;
    }
    else
    {
        return (void*)GaucheDotNet::Native::GoshInvoke::Scm_MakeString(
            ret->AssemblyQualifiedName
            , -1, -1, GaucheDotNet::StringFlags::Copying);
    }
}

DECDLL void* ClrNewArray(TypeSpec* typeSpec, int size)
{
    Type^ t = ClrMethod::TypeSpecToType(typeSpec);
    Array^ ary = Array::CreateInstance(t, size);

    return (void*)(IntPtr)GCHandle::Alloc(ary);
}

DECDLL void* ClrNew(
                    TypeSpec* methodSpec
                    , ObjWrapper* args, int numArg
                    )
{
    return ClrMethod::CallNew(methodSpec, args, numArg);
}

DECDLL void* ClrCallMethod(
                         TypeSpec* methodSpec
                         , void* obj, int isStatic
                         , ObjWrapper* args, int numArg
                         )
{
    return ClrMethod::CallMethod(methodSpec, obj, isStatic == 1, args, numArg);
}

DECDLL int ClrIs(TypeSpec* typeSpec, void* obj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ o = gchObj.Target;
    if(o == nullptr)
    {
        //FALSE
        return 0;
    }

    Type^ t = ClrMethod::TypeSpecToType(typeSpec);
    return t->IsAssignableFrom(o->GetType()) == true ? 1 : 0;
}

#ifdef __cplusplus
}
#endif

