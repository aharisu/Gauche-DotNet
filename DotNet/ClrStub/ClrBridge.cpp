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

DECDLL int ClrToInt(void* obj, int* ret)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ target = gchObj.Target;
    if(target == nullptr)
    {
        return 0;
    }

    if(target->GetType() == GoshFixnum::typeid)
    {
        *ret = ((GoshFixnum^)target)->Num;
        return 1;
    }
    else
    {
        try 
        {
            Object^ objNum = Convert::ChangeType(target, Int32::typeid);
            *ret = *((Int32^)objNum);
            return 1;
        }
        catch(InvalidCastException^)
        {
            return 0;
        }
    }
}

DECDLL int FixnumToClr(signed long int num, void** ret)
{
    *ret = (void*)(IntPtr)GCHandle::Alloc(num);

    return 1;
}

DECDLL int ClrToGoshString(void* clrObj, void** ret)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(clrObj));
    Object^ target = gchObj.Target;
    if(target == nullptr)
    {
        return 0;
    }

    if(target->GetType() == GoshString::typeid)
    {
        *ret = (void*)((GoshString^)target)->Ptr;
    }
    else
    {
        *ret = (void*)GoshInvoke::Scm_MakeString(
            target->ToString()
            , -1, -1, 
            StringFlags::Copying);
    }

    return 1;
}

DECDLL int StringToClr(const char* str, void** ret)
{
    Object^ obj = gcnew String(str);
    *ret = (void*)(IntPtr) GCHandle::Alloc(obj);

    return 1;
}

DECDLL int ClrPropSetClrObj(void* obj, const char* name,  void* clrObj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;

    PropertyInfo^ propInfo = hObj->GetType()->GetProperty(
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name))));
    MethodInfo^ setter = propInfo->GetSetMethod();

    GCHandle gchVal = GCHandle::FromIntPtr(IntPtr(clrObj));
    Object^ hVal = gchVal.Target;

    //TODO catch error
    setter->Invoke(hObj, gcnew array<Object^>{hVal});

    return 1;
}

DECDLL int ClrPropSetScmObj(void* obj, const char* name,  void* scmObj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;

    PropertyInfo^ propInfo = hObj->GetType()->GetProperty(
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name))));
    MethodInfo^ setter = propInfo->GetSetMethod();

    //ScmObj to .Net object(GoshObj instance)
    Object^ hVal = gcnew GoshClrObject(IntPtr(scmObj));

    //TODO catch error
    setter->Invoke(hObj, gcnew array<Object^>{hVal});

    return 1;
}

DECDLL int ClrPropSetInt(void* obj, const char* name,  int value)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;

    PropertyInfo^ propInfo = hObj->GetType()->GetProperty(
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name))));
    MethodInfo^ setter = propInfo->GetSetMethod();

    try 
    {
        Object^ objNum = Convert::ChangeType((Int32)value, propInfo->PropertyType);
        setter->Invoke(hObj, gcnew array<Object^>{objNum});
        return 1;
    }
    catch(InvalidCastException^)
    {
        return 0;
    }
}

DECDLL int ClrPropSetString(void* obj, const char* name,  const char* value)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;

    PropertyInfo^ propInfo = hObj->GetType()->GetProperty(
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name))));
    //string型を設定できるプロパティか?
    if(!propInfo->PropertyType->IsAssignableFrom(String::typeid))
    {
        return 0;
    }

    MethodInfo^ setter = propInfo->GetSetMethod();
    setter->Invoke(hObj, gcnew array<Object^>{
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(value)))
    });
    return 1;
}

DECDLL void* ClrPropGet(void* obj, const char* name)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;

    PropertyInfo^ propInfo = hObj->GetType()->GetProperty(
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name))));
    MethodInfo^ getter = propInfo->GetGetMethod();

    Object^ ret = getter->Invoke(hObj, nullptr);

    return (void*)(IntPtr) GCHandle::Alloc(ret);
}

#pragma region field setter / getter {

DECDLL int ClrFieldSetClrObj(void* obj, const char* name,  void* clrObj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;

    //TODO catch error
    FieldInfo^ fieldInfo = hObj->GetType()->GetField(
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name))));

    GCHandle gchVal = GCHandle::FromIntPtr(IntPtr(clrObj));
    Object^ hVal = gchVal.Target;

    fieldInfo->SetValue(hObj, hVal);

    return 1;
}

DECDLL int ClrFieldSetScmObj(void* obj, const char* name,  void* scmObj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;

    FieldInfo^ fieldInfo = hObj->GetType()->GetField(
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name))));

    //ScmObj to .Net object(GoshObj instance)
    Object^ hVal = gcnew GoshClrObject(IntPtr(scmObj));

    fieldInfo->SetValue(hObj, hVal);

    return 1;
}

DECDLL int ClrFieldSetInt(void* obj, const char* name,  int value)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;

    FieldInfo^ fieldInfo = hObj->GetType()->GetField(
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name))));

    try 
    {
        Object^ objNum = Convert::ChangeType((Int32)value, fieldInfo->FieldType);
        fieldInfo->SetValue(hObj, objNum);

        return 1;
    }
    catch(InvalidCastException^)
    {
        return 0;
    }
}

DECDLL int ClrFieldSetString(void* obj, const char* name,  const char* value)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;

    FieldInfo^ fieldInfo = hObj->GetType()->GetField(
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name))));

    //string型を設定できるフィールドか?
    if(!fieldInfo->FieldType->IsAssignableFrom(String::typeid))
    {
        return 0;
    }

    fieldInfo->SetValue(hObj, 
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(value))));
    return 1;
}

DECDLL void* ClrFieldGet(void* obj, const char* name)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;

    FieldInfo^ fieldInfo = hObj->GetType()->GetField(
        Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name))));

    Object^ ret = fieldInfo->GetValue(hObj);

    return (void*)(IntPtr) GCHandle::Alloc(ret);
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

DECDLL int ClrValidTypeName(const char* fullTypeName)
{
    String^ name = gcnew String(fullTypeName);

    return ClrMethod::GetType(name) == nullptr ? 0 : 1;
}

DECDLL void* ClrNew(
                    TypeSpec* methodSpec
                    , MethodArg* args, int numArg
                    )
{
    return ClrMethod::CallNew(methodSpec, args, numArg);
}

DECDLL void* ClrCallMethod(
                         TypeSpec* methodSpec
                         , void* obj, int isStatic
                         , MethodArg* args, int numArg
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

