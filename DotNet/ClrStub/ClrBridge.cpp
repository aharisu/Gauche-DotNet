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

#include "Microsoft.Scripting/CompilerHelpers.h"

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

DECDLL int ClrEqualP(void* x, void* y)
{
    Object^ objX = GCHandle::FromIntPtr(IntPtr(x)).Target;
    Object^ objY = GCHandle::FromIntPtr(IntPtr(y)).Target;
    if(objX == nullptr)
    {
        if(objY == nullptr)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
    if(objY == nullptr)
    {
        return 0;
    }

    return objX->Equals(objY) ? 1 : 0;
}

DECDLL int ClrCompare(void* x, void* y)
{
    Object^ objX = GCHandle::FromIntPtr(IntPtr(x)).Target;
    Object^ objY = GCHandle::FromIntPtr(IntPtr(y)).Target;
    if(objX == nullptr)
    {
        if(objY == nullptr)
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }
    if(objY == nullptr)
    {
        return 1;
    }

    if(objX->GetType()->IsAssignableFrom(IComparable::typeid))
    {
        return ((IComparable^)objX)->CompareTo(objY);
    }

    if(objY->GetType()->IsAssignableFrom(IComparable::typeid))
    {
        int cmp = ((IComparable^)objY)->CompareTo(objX);
        return  cmp == 0 ? 0 : cmp > 0 ? -1 : 1;
    } 

    ClrStubConstant::RaiseClrError(gcnew InvalidOperationException("can not be compared object"));
    return 0; //does not reach
}

DECDLL int ClrGetHash(void* obj)
{
    Object^ clrObj = GCHandle::FromIntPtr(IntPtr(obj)).Target;
    if(clrObj == nullptr)
    {
        return 0;
    }

    return clrObj->GetHashCode();
}

static void ObjectNullCheck(Object^ obj)
{
    if(obj == nullptr)
    {
        ClrStubConstant::RaiseClrError(gcnew ArgumentNullException("obj can not be a null"));
        //does not reach
    }
}

DECDLL void* ClrGetEnumerator(void* objPtr)
{
    Object^ obj = GCHandle::FromIntPtr((IntPtr)objPtr).Target;
    ObjectNullCheck(obj);

    System::Collections::IEnumerable^ e = dynamic_cast<System::Collections::IEnumerable^>(obj);
    if(e == nullptr)
    {
        ClrStubConstant::RaiseClrError(gcnew ArgumentException("object must be IEnumerable"));
    }

    return (void*)(IntPtr)GCHandle::Alloc(e->GetEnumerator());
}

DECDLL int ClrIsIterEnd(void* iter)
{
    GCHandle handle = GCHandle::FromIntPtr((IntPtr)iter);
    System::Collections::IEnumerator^ enumerator = (System::Collections::IEnumerator^) handle.Target;

    return enumerator->MoveNext() ? 0 : 1;
}

DECDLL void* ClrIterNext(void* iter)
{
    GCHandle handle = GCHandle::FromIntPtr((IntPtr)iter);
    System::Collections::IEnumerator^ enumerator = (System::Collections::IEnumerator^) handle.Target;

    return (void*)(IntPtr)GCHandle::Alloc(enumerator->Current);
}

DECDLL void ClrIterDispose(void* iter)
{
    GCHandle handle = GCHandle::FromIntPtr((IntPtr)iter);
    Object^ obj = handle.Target;

    IDisposable^ disposable = dynamic_cast<IDisposable^>(obj);
    if(disposable != nullptr)
    {
        delete disposable;
    }

    handle.Free();
}

DECDLL void* ClrToGoshObj(void* obj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ target = gchObj.Target;
    ObjectNullCheck(target);

    Type^ type = target->GetType();
    if(GoshObj::typeid->IsAssignableFrom(type))
    {
        return (void*)((GoshObj^)target)->Ptr;
    }
    else
    {
        if(type == Boolean::typeid)
        {
            return (void*)(((Boolean)target) ? GoshBool::True->Ptr : GoshBool::False->Ptr);
        }
        else if(type == String::typeid)
        {
            return (void*)GoshInvoke::Scm_MakeString((String^)target, -1, -1, StringFlags::Copying);
        }
        else if(type == double::typeid)
        {
            return (void*)GoshInvoke::Scm_MakeFlonum((double)target);
        }
        else if(type == Char::typeid)
        {
            return (void*)Cast::CharToScmChar((Char)target);
        }
        else if(type == float::typeid)
        {
            return (void*)GoshInvoke::Scm_MakeFlonum((double)(float)target);
        }
        else if(Array::typeid->IsAssignableFrom(type)) //TO UVector or Vector
        {
            IntPtr vec;
#pragma region Array {
            Array^ ary = (Array^)target;
            int len = ary->Length;

            switch(Type::GetTypeCode(type->GetElementType()))
            {
            case TypeCode::Byte:
                {
                    array<Byte>^ a = (array<Byte>^)ary;
                    vec = GoshInvoke::Scm_MakeU8Vector(len, 0);
                    for(int i = 0;i < len;++i)
                    {
                        GoshInvoke::Scm_U8VectorSet(vec, i, a[i]);
                    }
                }
                break;
            case TypeCode::SByte:
                {
                    array<SByte>^ a = (array<SByte>^)ary;
                    vec = GoshInvoke::Scm_MakeS8Vector(len, 0);
                    for(int i = 0;i < len;++i)
                    {
                        GoshInvoke::Scm_S8VectorSet(vec, i, a[i]);
                    }
                }
                break;
            case TypeCode::Int16:
                {
                    array<Int16>^ a = (array<Int16>^)ary;
                    vec = GoshInvoke::Scm_MakeS16Vector(len, 0);
                    for(int i = 0;i < len;++i)
                    {
                        GoshInvoke::Scm_S16VectorSet(vec, i, a[i]);
                    }
                }
                break;
            case TypeCode::UInt16:
                {
                    array<UInt16>^ a = (array<UInt16>^)ary;
                    vec = GoshInvoke::Scm_MakeU16Vector(len, 0);
                    for(int i = 0;i < len;++i)
                    {
                        GoshInvoke::Scm_U16VectorSet(vec, i, a[i]);
                    }
                }
                break;
            case TypeCode::Int32:
                {
                    array<Int32>^ a = (array<Int32>^)ary;
                    vec = GoshInvoke::Scm_MakeS32Vector(len, 0);
                    for(int i = 0;i < len;++i)
                    {
                        GoshInvoke::Scm_S32VectorSet(vec, i, a[i]);
                    }
                }
                break;
            case TypeCode::UInt32:
                {
                    array<UInt32>^ a = (array<UInt32>^)ary;
                    vec = GoshInvoke::Scm_MakeU32Vector(len, 0);
                    for(int i = 0;i < len;++i)
                    {
                        GoshInvoke::Scm_U32VectorSet(vec, i, a[i]);
                    }
                }
                break;
            case TypeCode::Int64:
                {
                    array<Int64>^ a = (array<Int64>^)ary;
                    vec = GoshInvoke::Scm_MakeS64Vector(len, 0);
                    for(int i = 0;i < len;++i)
                    {
                        GoshInvoke::Scm_S64VectorSet(vec, i, a[i]);
                    }
                }
                break;
            case TypeCode::UInt64:
                {
                    array<UInt64>^ a = (array<UInt64>^)ary;
                    vec = GoshInvoke::Scm_MakeU64Vector(len, 0);
                    for(int i = 0;i < len;++i)
                    {
                        GoshInvoke::Scm_U64VectorSet(vec, i, a[i]);
                    }
                }
                break;
            case TypeCode::Single:
                {
                    array<Single>^ a = (array<Single>^)ary;
                    vec = GoshInvoke::Scm_MakeF32Vector(len, 0);
                    for(int i = 0;i < len;++i)
                    {
                        GoshInvoke::Scm_F32VectorSet(vec, i, a[i]);
                    }
                }
                break;
            case TypeCode::Double:
                {
                    array<Double>^ a = (array<Double>^)ary;
                    vec = GoshInvoke::Scm_MakeF64Vector(len, 0);
                    for(int i = 0;i < len;++i)
                    {
                        GoshInvoke::Scm_F64VectorSet(vec, i, a[i]);
                    }
                }
                break;
            default:
                {
                    vec = GoshInvoke::Scm_MakeVector(len, GoshUndefined::Undefined->Ptr);
                    int i;
                    for each(Object^ o in ary)
                    {
                        GoshInvoke::Scm_VectorSet(vec, i, Cast::ToIntPtr(o));
                        ++i;
                    }
                }
                break;
            }
#pragma endregion }

            return (void*)vec;
        }
        else if(System::Collections::IList::typeid->IsAssignableFrom(type)) //To Vector
        {
            System::Collections::IList^ list = (System::Collections::IList^)target;
            IntPtr vec = GoshInvoke::Scm_MakeVector(list->Count, GoshUndefined::Undefined->Ptr);
            int i = 0;
            for each(Object^ o in list)
            {
                GoshInvoke::Scm_VectorSet(vec, i, Cast::ToIntPtr(o));
                ++i;
            }
            return (void*)vec;
        }
        else if(System::Collections::IDictionary::typeid->IsAssignableFrom(type)) //To HashTable
        {
            IntPtr hashtable = GoshInvoke::Scm_MakeHashTableSimple(HashType::Equal, 0);

            for each(System::Collections::DictionaryEntry^ entry in
                ((System::Collections::IDictionary^)target))
            {
                GoshInvoke::Scm_HashTableSet(hashtable, Cast::ToIntPtr(entry->Key), Cast::ToIntPtr(entry->Value), DictSetFlags::None);
            }

            return (void*)hashtable;
        }
        else if(System::Collections::IEnumerable::typeid->IsAssignableFrom(type)) // To Cons List
        {
            IntPtr c = GoshNIL::NIL->Ptr;
            for each(Object^ o in ((System::Collections::IEnumerable^)target))
            {
                c = GoshInvoke::Scm_Cons(Cast::ToIntPtr(o), c);
            }
            return (void*)GoshInvoke::Scm_ReverseX(c);
        }
        else
        {
            try 
            {
                Int64 num = (Int64)Convert::ChangeType(target, Int64::typeid);
                if(Int32::MinValue < num && num < Int32::MaxValue)
                {
                    return (void*)GoshInvoke::Scm_MakeInteger((Int32)num);
                }
                else
                {
                    return (void*)GoshInvoke::Scm_MakeInteger64(num);
                }
            }
            catch(InvalidCastException^ e)
            {
                ClrStubConstant::RaiseClrError(e);
                //does not reach
                return 0;
            }
        }
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

DECDLL void* ClrToNumber(void* obj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ target = gchObj.Target;
    ObjectNullCheck(target);

    Type^ type = target->GetType();
    if(GoshObj::typeid->IsAssignableFrom(type))
    {
        GoshObj^ goshObj = (GoshObj^)target;
        bool firstTry = true;
Retry:
        if(type == GoshFixnum::typeid
            || type == GoshInteger::typeid
            || type == GoshFlonum::typeid
            || type == GoshRatnum::typeid
            || type == GoshCompnum::typeid)
        {
            return (void*)goshObj->Ptr;
        }
        else
        {
            if(firstTry)
            {
                firstTry = false;

                goshObj = goshObj->Specify;
                Type^ retryType = goshObj->GetType();
                if(type != retryType)
                {
                    type = retryType;
                    goto Retry;
                }
            }

            ClrStubConstant::RaiseClrError(gcnew InvalidCastException("Invalid cast to number " + target));
            //does not reach
            return 0;
        }
    }
    else
    {
        if(type == double::typeid)
        {
            return (void*)GoshInvoke::Scm_MakeFlonum((double)target);
        }
        else if(type == float::typeid)
        {
            return (void*)GoshInvoke::Scm_MakeFlonum((double)(float)target);
        }
        else
        {
            try 
            {
                Int64 num = (Int64)Convert::ChangeType(target, Int64::typeid);
                if(Int32::MinValue < num && num < Int32::MaxValue)
                {
                    return (void*)GoshInvoke::Scm_MakeInteger((Int32)num);
                }
                else
                {
                    return (void*)GoshInvoke::Scm_MakeInteger64(num);
                }
            }
            catch(InvalidCastException^ e)
            {
                ClrStubConstant::RaiseClrError(e);
                //does not reach
                return 0;
            }
        }
    }
}

DECDLL void* FixnumToClr(signed long int num)
{
    return (void*)(IntPtr)GCHandle::Alloc(num);
}

DECDLL void* Int64ToClr(System::Int64 num)
{
    return (void*)(IntPtr)GCHandle::Alloc(num);
}

DECDLL void* DoubleToClr(double num)
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

DECDLL void* GetEnumObject(TypeSpec* enumTypeSpec, const char* enumObj)
{
    Type^ t = ClrMethod::TypeSpecToType(enumTypeSpec);
    String^ value = Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(enumObj)));

    try
    {
        Object^ ret = Enum::Parse(t, value, false);
        return (void*)(IntPtr)GCHandle::Alloc(ret);
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
        //does not reach
        return 0;
    }
}

// Util

DECDLL void* ClrPrint(void* clrObj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(clrObj));
    Object^ target = gchObj.Target;

    String^ str;
    //nullの場合は何も表示しない
    if(target == nullptr)
    {
        str = "";
    }
    else
    {
        Type^ t = target->GetType();
        //targetがStringの場合だけ、"で囲む
        if(String::typeid == target->GetType())
        {
            str = "\"" + target->ToString() + "\"";
        }
        else
        {
            //targetが実行するToStringがObjectが実装したものなら何も表示しない
            MethodInfo^ info = t->GetMethod("ToString", Type::EmptyTypes);
            if(info->DeclaringType == Object::typeid)
            {
                str = "";
            }
            else
            {
                //サブクラスがoverrideしている場合だけToStringを情報として取得する
                str = target->ToString();
            }
        }
    }

    return (void*)GoshInvoke::Scm_MakeString(str, -1, -1, StringFlags::Copying);
}

static String^ GetTypeName(Type^ t)
{
    if(t->IsGenericType)
    {
        StringBuilder builder;
        //ジェネリック型の名前から`より前(List`1のListの部分)だけ取得
        builder.Append(t->Name->Split('`')[0]);

        builder.Append("<");
        bool isFirst = true;
        //ジェネリック型の引数を型名称に含める
        for each(Type^ genericType in t->GetGenericArguments())
        {
            if(!isFirst)
            {
                builder.Append(",");
            }
            isFirst = false;

            builder.Append(genericType->Name);
        }
        builder.Append(">");

        return builder.ToString();
    }
    else
    {
        return t->Name;
    }
}

DECDLL void* ClrGetTypeName(void* obj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ o = gchObj.Target;

    String^ str;
    if(o == nullptr)
    {
        str = "Null";
    }
    else 
    {
        str = GetTypeName(o->GetType());
    }

    return (void*)GoshInvoke::Scm_MakeString(str, -1, -1, StringFlags::Copying);
}

DECDLL void* ClrMember(void* obj, int isStatic, const char* name)
{
    Type^ targetType = nullptr;
    if(isStatic)
    {
        targetType = ClrMethod::TypeSpecToType((TypeSpec*)obj);
    }
    else
    {
        GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
        Object^ o = gchObj.Target;

        if(o == nullptr)
        {
            return (void*)GaucheDotNet::GoshNIL::NIL->Ptr; 
        }

        targetType = o->GetType();
    }

    IntPtr ret = GaucheDotNet::GoshNIL::NIL->Ptr;
    String^ n;
    if(name == (const char*)0)
    {
        n = "*";
    }
    else
    {
        n = Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(name))) + "*";
    }

    for each(MemberInfo^ info in targetType->GetMember(n,
        BindingFlags::Public | (isStatic ? BindingFlags::Static : BindingFlags::Instance)
        ))
    {
        Type^ t = info->GetType();

        if(ConstructorInfo::typeid->IsAssignableFrom(t))
        {
            continue;
        }
        else if(MethodInfo::typeid->IsAssignableFrom(t))
        {
            MethodInfo^ method = (MethodInfo^)info;
            if(method->IsSpecialName)
            {
                continue;
            }

            StringBuilder builder;
#pragma region constract method name {
            //add return type
            builder.Append(GetTypeName(method->ReturnType));
            builder.Append(" ");

            //add method name
            builder.Append(info->Name);
            builder.Append("(");

            //add parameter type
            bool isFirst = true;
            for each(ParameterInfo^ param in method->GetParameters())
            {
                if(!isFirst)
                {
                    builder.Append(", ");
                }
                isFirst = false;

                if(CompilerHelpers::IsOutParameter(param))
                {
                    builder.Append("out ");
                    String^ paramTypeName = GetTypeName(param->ParameterType);
                    builder.Append(paramTypeName->Substring(0, paramTypeName->Length - 1));
                }
                else if(param->ParameterType->IsByRef)
                {
                    builder.Append("ref ");
                    String^ paramTypeName = GetTypeName(param->ParameterType);
                    builder.Append(paramTypeName->Substring(0, paramTypeName->Length - 1));
                }
                else if(CompilerHelpers::IsParamArray(param))
                {
                    builder.Append("params ");
                    builder.Append(GetTypeName(param->ParameterType));
                }
                else
                {
                    builder.Append(GetTypeName(param->ParameterType));
                }

                builder.Append(" ");
                builder.Append(param->Name);
            }
            builder.Append(")");
#pragma endregion }

            ret = GoshInvoke::Scm_Cons(
                GoshInvoke::Scm_Cons(ClrStubConstant::MemberKindMethod
                    , GoshInvoke::Scm_MakeString(builder.ToString(), -1, -1, StringFlags::Copying))
                , ret);
        }
        else
        {
            IntPtr kind;
            if(PropertyInfo::typeid->IsAssignableFrom(t))
            {
                kind = ClrStubConstant::MemberKindProperty;
            }
            else if(FieldInfo::typeid->IsAssignableFrom(t))
            {
                kind = ClrStubConstant::MemberKindField;
            }
            else if(EventInfo::typeid->IsAssignableFrom(t))
            {
                kind = ClrStubConstant::MemberKindEvent;
            }
            else
            {
                kind = ClrStubConstant::MemberKindType;
            }

            ret = GoshInvoke::Scm_Cons(
                GoshInvoke::Scm_Cons(kind,GoshInvoke::Scm_MakeString(info->Name, -1, -1, StringFlags::Copying))
                , ret);
        }
    }

    return (void*)ret;
}

#ifdef __cplusplus
}
#endif

