﻿/*
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

#pragma region cast of clr <-> Gauche {

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
            return (void*)GoshInvoke::Scm_MakeString((String^)target, StringFlags::Copying);
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
        return (void*)GoshInvoke::Scm_MakeString(target->ToString(), StringFlags::Copying);
    }
}

DECDLL void* StringToClr(const char* str)
{
    Object^ obj = Util::IntPtrToUTF8String((IntPtr)const_cast<char*>(str));
    return (void*)(IntPtr) GCHandle::Alloc(obj);
}

#pragma endregion }

static void InfoNullCheck(MemberInfo^ info, Object^ obj, String^ kind, String^ name)
{
    if(info == nullptr)
    {
        ClrStubConstant::RaiseClrError(gcnew ArgumentException(
            String::Format("{0} class doesn't have such {1}: {2}", obj->GetType()->FullName, kind, name)));
        //does not reach
    }
}

#pragma region field/property setter / getter {

static int ToInteger(ObjWrapper* objWrapper)
{
    Object^ o = ClrMethod::ToObject(objWrapper);
    switch(Type::GetTypeCode(o->GetType()))
    {
    case TypeCode::SByte:
        return (int)(SByte)o;
    case TypeCode::Byte:
        return (int)(Byte)o;
    case TypeCode::Int16:
        return (int)(Int16)o;
    case TypeCode::UInt16:
        return (int)(UInt16)o;
    case TypeCode::Int32:
        return (int)(Int32)o;
    case TypeCode::UInt32:
        {
            UInt32 num = (UInt32)o;
            if(num < Int32::MaxValue)
            {
                return (Int32)num;
            }
        }
    }

    ClrStubConstant::RaiseClrError(gcnew InvalidCastException(
        String::Format("required int object, but got {0}", o)));
    //does not reach
    return 0;
}

static bool IsArrayIndexAt(Object^ obj, const char* name, int numIndexer)
{
    return Array::typeid->IsAssignableFrom(obj->GetType())
        && name == 0
        && numIndexer != 0;
}

static void SetArrayObj(Object^ target
                        , ObjWrapper* indexer, int numIndexer
                        , Object^ value)
{
    try
    {
        switch(numIndexer)
        {
        case 1:
            ((Array^)target)->SetValue(value, ToInteger(&(indexer[0])));
            break;
        case 2:
            ((Array^)target)->SetValue(value, ToInteger(&(indexer[0])),
                ToInteger(&(indexer[1])));
            break;
        case 3:
            ((Array^)target)->SetValue(value, ToInteger(&(indexer[0])),
                ToInteger(&(indexer[1])), ToInteger(&(indexer[2])));
            break;
        default:
            {
                array<int>^ indices = gcnew array<int>(numIndexer);
                for(int i = 0;i < numIndexer;++i)
                {
                    indices[i] = ToInteger(&(indexer[i]));
                }
                ((Array^)target)->SetValue(value, indices);
                break;
            }
        }
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
}


DECDLL void ClrFieldPropSetClrObj(FieldPropKind kind
                            , void* obj, const char* name
                            , ObjWrapper* indexer, int numIndexer
                            , void* clrObj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    GCHandle gchVal = GCHandle::FromIntPtr(IntPtr(clrObj));
    Object^ hVal = gchVal.Target;

    String^ fpName = (name == 0) ?
        "Item" : //default indexer name;
        Util::IntPtrToUTF8String((IntPtr)const_cast<char*>(name));

    try
    {
        if(kind & KIND_PROP)
        {
            if(IsArrayIndexAt(hObj, name, numIndexer))
            {
                SetArrayObj(hObj, indexer, numIndexer, hVal);
                return;
            }

            PropertyInfo^ propInfo = hObj->GetType()->GetProperty(fpName);
            if(kind & KIND_FIELD)
            {
                if(propInfo == nullptr) goto END_PROP;
            }
            else
            {
                InfoNullCheck(propInfo, hObj, "property", fpName);
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
            propInfo->SetValue(hObj, hVal, index);
            return;
        }
END_PROP:

        if(kind & KIND_FIELD && name != 0)
        {
            FieldInfo^ fieldInfo = hObj->GetType()->GetField(fpName);
            if(kind & KIND_PROP)
            {
                if(fieldInfo == nullptr) goto END_FIELD;
            }
            else
            {
                InfoNullCheck(fieldInfo, hObj, "field", fpName);
            }

            fieldInfo->SetValue(hObj, hVal);
            return;
        }
END_FIELD:

        ClrStubConstant::RaiseClrError(gcnew ArgumentException(
            String::Format("{0} class dosen't have such property/field: {1}", hObj->GetType()->Name, fpName))); 
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }

}

DECDLL void ClrFieldPropSetScmObj(FieldPropKind kind
                             , void* obj, const char* name
                            , ObjWrapper* indexer, int numIndexer
                            , void* scmObj)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    //ScmObj to .Net object(GoshObj instance)
    Object^ hVal = gcnew GoshClrObject(IntPtr(scmObj));

    String^ fpName = (name == 0) ?
        "Item" : //default indexer name;
        Util::IntPtrToUTF8String((IntPtr)const_cast<char*>(name));

    try
    {
        if(kind & KIND_PROP)
        {
            if(IsArrayIndexAt(hObj, name, numIndexer))
            {
                SetArrayObj(hObj, indexer, numIndexer, gcnew GoshClrObject(IntPtr(scmObj)));
                return;
            }

            PropertyInfo^ propInfo = hObj->GetType()->GetProperty(fpName);
            if(kind & KIND_FIELD)
            {
                if(propInfo == nullptr) goto END_PROP;
            }
            else
            {
                InfoNullCheck(propInfo, hObj, "property", fpName);
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
            propInfo->SetValue(hObj, hVal, index);
            return;
        }
END_PROP:

        if(kind & KIND_FIELD && name != 0)
        {
            FieldInfo^ fieldInfo = hObj->GetType()->GetField(fpName);
            if(kind & KIND_PROP)
            {
                if(fieldInfo == nullptr) goto END_FIELD;
            }
            else
            {
                InfoNullCheck(fieldInfo, hObj, "field", fpName);
            }

            fieldInfo->SetValue(hObj, hVal);
            return;
        }
END_FIELD:

        ClrStubConstant::RaiseClrError(gcnew ArgumentException(
            String::Format("{0} class dosen't have such property/field: {1}", hObj->GetType()->Name, fpName))); 
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
}

DECDLL void ClrFieldPropSetInt(FieldPropKind kind
                         , void* obj, const char* name
                         , ObjWrapper* indexer, int numIndexer
                         , int value)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    String^ fpName = (name == 0) ?
        "Item" : //default indexer name;
        Util::IntPtrToUTF8String((IntPtr)const_cast<char*>(name));

    try 
    {
        if(kind & KIND_PROP)
        {
            if(IsArrayIndexAt(hObj, name, numIndexer))
            {
                SetArrayObj(hObj, indexer, numIndexer, value);
                return;
            }

            PropertyInfo^ propInfo = hObj->GetType()->GetProperty(fpName);
            if(kind & KIND_FIELD)
            {
                if(propInfo == nullptr) goto END_PROP;
            }
            else
            {
                InfoNullCheck(propInfo, hObj, "property", fpName);
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

            Object^ objNum = Convert::ChangeType((Int32)value, propInfo->PropertyType);
            propInfo->SetValue(hObj, objNum, index);
            return;
        }
END_PROP:

        if(kind & KIND_FIELD && name != 0)
        {
            FieldInfo^ fieldInfo = hObj->GetType()->GetField(fpName);
            if(kind & KIND_PROP)
            {
                if(fieldInfo == nullptr) goto END_FIELD;
            }
            else
            {
                InfoNullCheck(fieldInfo, hObj, "field", fpName);
            }

            Object^ objNum = Convert::ChangeType((Int32)value, fieldInfo->FieldType);
            fieldInfo->SetValue(hObj, objNum);
            return;
        }
END_FIELD:

        ClrStubConstant::RaiseClrError(gcnew ArgumentException(
            String::Format("{0} class dosen't have such property/field: {1}", hObj->GetType()->Name, fpName))); 
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
}

DECDLL void ClrFieldPropSetString(FieldPropKind kind
                             , void* obj, const char* name
                            , ObjWrapper* indexer, int numIndexer
                            , const char* value)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    String^ fpName = (name == 0) ?
        "Item" : //default indexer name;
        Util::IntPtrToUTF8String((IntPtr)const_cast<char*>(name));

    try
    {
        String^ str = Util::IntPtrToUTF8String((IntPtr)const_cast<char*>(value));

        if(kind & KIND_PROP)
        {
            if(IsArrayIndexAt(hObj, name, numIndexer))
            {
                SetArrayObj(hObj, indexer, numIndexer, str);
                return;
            } 

            PropertyInfo^ propInfo = hObj->GetType()->GetProperty(fpName);
            if(kind & KIND_FIELD)
            {
                if(propInfo == nullptr) goto END_PROP;
            }
            else
            {
                InfoNullCheck(propInfo, hObj, "property", fpName);
            }

            //string型を設定できるプロパティか?
            if(!propInfo->PropertyType->IsAssignableFrom(String::typeid))
            {
                ClrStubConstant::RaiseClrError(gcnew ArgumentException(
                    String::Format("{0} property can not assign of String object", fpName)));
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

            propInfo->SetValue(hObj, str, index);
            return;
        }
END_PROP:

        if(kind & KIND_FIELD && name != 0)
        {
            FieldInfo^ fieldInfo = hObj->GetType()->GetField(fpName);
            if(kind & KIND_PROP)
            {
                if(fieldInfo == nullptr) goto END_FIELD;
            }
            else
            {
                InfoNullCheck(fieldInfo, hObj, "field", fpName);
            }

            fieldInfo->SetValue(hObj, str);
            return;
        }
END_FIELD:

        ClrStubConstant::RaiseClrError(gcnew ArgumentException(
            String::Format("{0} class dosen't have such property/field: {1}", hObj->GetType()->Name, fpName))); 
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
}

DECDLL void* ClrFieldPropGet(FieldPropKind kind
                        , ObjWrapper* obj, const char* name
                        , ObjWrapper* indexer, int numIndexer)
{
    Object^ hObj = ClrMethod::ToObject(obj);
    ObjectNullCheck(hObj);

    String^ fpName = (name == 0) ?
        "Item" : //default indexer name;
        Util::IntPtrToUTF8String((IntPtr)const_cast<char*>(name));

    try
    {
        if(kind & KIND_PROP)
        {
            if(IsArrayIndexAt(hObj, name, numIndexer))
            {
                Object^ ret;

                try
                {
                    switch(numIndexer)
                    {
                    case 1:
                        ret = ((Array^)hObj)->GetValue(ToInteger(&(indexer[0])));
                        break;
                    case 2:
                        ret = ((Array^)hObj)->GetValue(ToInteger(&(indexer[0])),
                            ToInteger(&(indexer[1])));
                        break;
                    case 3:
                        ret = ((Array^)hObj)->GetValue(ToInteger(&(indexer[0])),
                            ToInteger(&(indexer[1])), ToInteger(&(indexer[2])));
                        break;
                    default:
                        {
                            array<int>^ indices = gcnew array<int>(numIndexer);
                            for(int i = 0;i < numIndexer;++i)
                            {
                                indices[i] = ToInteger(&(indexer[i]));
                            }
                            ret = ((Array^)hObj)->GetValue(indices);
                            break;
                        }
                    }
                    return (void*)(IntPtr) GCHandle::Alloc(ret);
                }
                catch(Exception^ e)
                {
                    ClrStubConstant::RaiseClrError(e);
                    //does not reach
                    return 0;
                }
            }
            
            PropertyInfo^ propInfo = hObj->GetType()->GetProperty(fpName);
            if(kind & KIND_FIELD)
            {
                if(propInfo == nullptr) goto END_PROP;
            }
            else
            {
                InfoNullCheck(propInfo, hObj, "property", fpName);
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

            return (void*)(IntPtr) GCHandle::Alloc(propInfo->GetValue(hObj, index));
        }
END_PROP:

        if(kind & KIND_FIELD && name != 0)
        {
            FieldInfo^ fieldInfo = hObj->GetType()->GetField(fpName);
            if(kind & KIND_PROP)
            {
                if(fieldInfo == nullptr) goto END_FIELD;
            }
            else
            {
                InfoNullCheck(fieldInfo, hObj, "field", fpName);
            }

            return (void*)(IntPtr) GCHandle::Alloc(fieldInfo->GetValue(hObj));
        }
END_FIELD:

        ClrStubConstant::RaiseClrError(gcnew ArgumentException(
            String::Format("{0} class dosen't have such property/field: {1}", hObj->GetType()->Name, fpName))); 
    }
    catch(Exception^ e)
    {
        ClrStubConstant::RaiseClrError(e);
    }
    //does not reach
    return 0;
}

#pragma endregion }

#pragma region event adder / remover {

static void ClrEventAddClrProc(void* obj, const char* name, GoshProc^ proc, IntPtr key)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(obj));
    Object^ hObj = gchObj.Target;
    ObjectNullCheck(hObj);

    String^ eventName = Util::IntPtrToUTF8String((IntPtr)const_cast<char*>(name));
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
    GoshProc^ proc;
    if(GoshInvoke::Scm_TypedClosureP((IntPtr)goshProc))
    {
        proc = gcnew Procedure::GoshTypedProcedure((IntPtr)goshProc);
    }
    else
    {
        proc = gcnew Procedure::GoshProcedure((IntPtr)goshProc);
    }

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

    String^ eventName = Util::IntPtrToUTF8String((IntPtr)const_cast<char*>(name));
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
    String^ name = Util::IntPtrToUTF8String((IntPtr)const_cast<char*>(assemblyName));
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

DECDLL void ClrUsingNamespace(const char* ns, void* module)
{
    ClrMethod::ModuleUsingNamespace((IntPtr)module
        , Util::IntPtrToUTF8String((IntPtr)const_cast<char*>(ns)));
}

DECDLL void* ClrValidTypeName(const char* fullTypeName)
{
    String^ name = Util::IntPtrToUTF8String((IntPtr)const_cast<char*>(fullTypeName));

    Type^ ret = ClrMethod::GetType(name, true);
    if(ret == nullptr)
    {
        return 0;
    }
    else
    {
        return (void*)GaucheDotNet::Native::GoshInvoke::Scm_MakeString(
            ret->AssemblyQualifiedName, GaucheDotNet::StringFlags::Copying);
    }
}

DECDLL void* ClrNewArray(TypeSpec* typeSpec, ObjWrapper* sizes, int numSizes)
{
    Type^ t = ClrMethod::TypeSpecToType(typeSpec);
    Array^ ary;
    switch(numSizes)
    {
    case 1:
        ary = Array::CreateInstance(t, ToInteger(&(sizes[0])));
        break;
    case 2:
        ary = Array::CreateInstance(t, ToInteger(&(sizes[0]))
            , ToInteger(&(sizes[1])));
        break;
    case 3:
        ary = Array::CreateInstance(t, ToInteger(&(sizes[0]))
            , ToInteger(&(sizes[1])), ToInteger(&(sizes[2])));
        break;
    default:
        {
            array<int>^ sizeAry = gcnew array<int>(numSizes);
            for(int i = 0;i < numSizes;++i)
            {
                sizeAry[i] = ToInteger(&(sizes[i]));
            }
            ary = Array::CreateInstance(t, sizeAry);
            break;
        }
    }

    return (void*)(IntPtr)GCHandle::Alloc(ary);
}

DECDLL void* ClrNew(
                    TypeSpec* methodSpec
                    , ObjWrapper* args, int numArg
                    )
{
    return ClrMethod::CallNew(methodSpec, args, numArg);
}

DECDLL void* ClrCallMethod(void* module
                         , TypeSpec* methodSpec
                         , void* obj, int isStatic
                         , ObjWrapper* args, int numArg
                         )
{
    return ClrMethod::CallMethod(module, methodSpec, obj, isStatic == 1, args, numArg);
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
    String^ value = Util::IntPtrToUTF8String((IntPtr)const_cast<char*>(enumObj));

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

static bool FindInterfacesFilter(Type^ m, Object^ type)
{
    return m->IsGenericType && m->GetGenericTypeDefinition() == type;
}

[System::Runtime::InteropServices::DllImport(GoshInvoke::GaucheLib, 
CallingConvention = CallingConvention::Cdecl)]
void Scm_Printf(void* port, const char* fmt, ...);

static int ClrWriteInternal(Object^ obj, void* port)
{
    Type^ t = obj->GetType();

    if(t->IsPrimitive)
    {
        switch (Type::GetTypeCode(t))
        {
        case TypeCode::Byte:
            Scm_Printf(port, "%u", (UInt32)(Byte)obj);
            break;
        case TypeCode::SByte:
            Scm_Printf(port, "%d", (Int32)(SByte)obj);
            break;
        case TypeCode::UInt16:
            Scm_Printf(port, "%u", (UInt32)(UInt16)obj);
            break;
        case TypeCode::Int16:
            Scm_Printf(port, "%d", (Int32)(Int16)obj);
            break;
        case TypeCode::UInt32:
            Scm_Printf(port, "%u", (UInt32)obj);
            break;
        case TypeCode::Int32:
            Scm_Printf(port, "%d", (Int32)obj);
            break;
        case TypeCode::UInt64:
            Scm_Printf(port, "%lu", (UInt64)obj);
            break;
        case TypeCode::Int64:
            Scm_Printf(port, "%ld", (Int64)obj);
            break;
        case TypeCode::Single:
            GoshInvoke::Scm_PrintDouble((IntPtr)port, (Double)(Single)obj, IntPtr::Zero);
            break;
        case TypeCode::Double:
            GoshInvoke::Scm_PrintDouble((IntPtr)port, (Double)obj, IntPtr::Zero);
            break;
        case TypeCode::Decimal:
            GoshInvoke::Scm_PrintDouble((IntPtr)port, (Double)(Decimal)obj, IntPtr::Zero);
            break;
        case TypeCode::Char:
            Scm_Printf(port, "#\\u%u", (UInt32)(Char)obj);
            break;
        case TypeCode::Boolean:
            Scm_Printf(port, "%s", ((Boolean)obj) ? "#t" : "#f");
            break;
        }

        return 1;
    }
    else if(String::typeid == t)
    {
        IntPtr str = GoshInvoke::Scm_MakeString((String^)obj
            , StringFlags::Immutable | StringFlags::Copying);
        GoshInvoke::Scm_Write(str, (IntPtr)port, GaucheDotNet::WriteMode::Write);

        return 1;
    }
    else if(dynamic_cast<System::Collections::IEnumerable^>(obj) != nullptr)
    {
        Type^ elemType = nullptr;
        if(t->IsArray)
        {
            elemType = t->GetElementType();
        }
        else
        {
            Type^ typeAbst = (System::Collections::Generic::IEnumerable<Object^>::typeid)->GetGenericTypeDefinition();
            array<Type^>^ typeConcrete = t->FindInterfaces(gcnew TypeFilter(FindInterfacesFilter), typeAbst);
            if(typeConcrete->Length != 0)
            {
                elemType = typeConcrete[0]->GetGenericArguments()[0];
            }
        }

        if(elemType != nullptr)
        {
            TypeCode code = Type::GetTypeCode(elemType);
            bool isFirst = true;
            switch (code)
            {
            case TypeCode::Byte:
                Scm_Printf(port, "#u8(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");
                    Scm_Printf(port, "%u", (UInt32)(Byte)elem);
                    isFirst = false;
                }
                break;
            case TypeCode::SByte:
                Scm_Printf(port, "#s8(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");
                    Scm_Printf(port, "%d", (Int32)(SByte)elem);
                    isFirst = false;
                }
                break;
            case TypeCode::UInt16:
                Scm_Printf(port, "#u16(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");
                    Scm_Printf(port, "%u", (UInt32)(UInt16)elem);
                    isFirst = false;
                }
                break;
            case TypeCode::Int16:
                Scm_Printf(port, "#s16(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");
                    Scm_Printf(port, "%d", (Int32)(Int16)elem);
                    isFirst = false;
                }
                break;
            case TypeCode::UInt32:
                Scm_Printf(port, "#u32(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");
                    Scm_Printf(port, "%u", (UInt32)elem);
                    isFirst = false;
                }
                break;
            case TypeCode::Int32:
                Scm_Printf(port, "#s32(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");
                    Scm_Printf(port, "%d", (Int32)elem);
                    isFirst = false;
                }
                break;
            case TypeCode::UInt64:
                Scm_Printf(port, "#u64(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");
                    Scm_Printf(port, "%lu", (UInt64)elem);
                    isFirst = false;
                }
                break;
            case TypeCode::Int64:
                Scm_Printf(port, "#s64(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");
                    Scm_Printf(port, "%ld", (Int64)elem);
                    isFirst = false;
                }
                break;
            case TypeCode::Single:
                Scm_Printf(port, "#f32(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");
                    GoshInvoke::Scm_PrintDouble((IntPtr)port, (Double)(Single)elem, IntPtr::Zero);
                    isFirst = false;
                }
                break;
            case TypeCode::Double:
                Scm_Printf(port, "#f64(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");
                    GoshInvoke::Scm_PrintDouble((IntPtr)port, (Double)elem, IntPtr::Zero);
                    isFirst = false;
                }
                break;
            case TypeCode::Decimal:
                Scm_Printf(port, "#f64(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");
                    GoshInvoke::Scm_PrintDouble((IntPtr)port, (Double)(Decimal)elem, IntPtr::Zero);
                    isFirst = false;
                }
                break;
            case TypeCode::Char:
                Scm_Printf(port, "#(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");
                    Scm_Printf(port, "#\\u%u", (UInt32)(Char)elem);
                    isFirst = false;
                }
                break;
            case TypeCode::Boolean:
                Scm_Printf(port, "#(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");
                    Scm_Printf(port, "%s", ((Boolean)elem) ? "#t" : "#f");
                    isFirst = false;
                }
                break;
            case TypeCode::String:
                Scm_Printf(port, "#(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");

                    IntPtr str = GoshInvoke::Scm_MakeString((String^)elem
                        , StringFlags::Immutable | StringFlags::Copying);
                    GoshInvoke::Scm_Write(str, (IntPtr)port, GaucheDotNet::WriteMode::Write);

                    isFirst = false;
                }
                break;
            default:
                Scm_Printf(port, "#(");
                for each(Object^ elem in (System::Collections::IEnumerable^)obj)
                {
                    if(!isFirst) Scm_Printf(port, " ");

                    GoshObj^ goshObj = dynamic_cast<GoshObj^>(elem);
                    if(goshObj != nullptr)
                    {
                        GoshInvoke::Scm_Write(goshObj->Ptr, (IntPtr)port, GaucheDotNet::WriteMode::Write);
                    }
                    else
                    {
                        ClrWriteInternal(elem, port);
                    }

                    isFirst = false;
                }
                break;
            }

            Scm_Printf(port, ")");
        }

        return 1;
    }

    return 0;
}

DECDLL int ClrWrite(void* clrObj, void* port)
{
    GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(clrObj));
    Object^ obj = gchObj.Target;

    return ClrWriteInternal(obj, port);
}

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

    return (void*)GoshInvoke::Scm_MakeString(str, StringFlags::Copying);
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

    return (void*)GoshInvoke::Scm_MakeString(str, StringFlags::Copying);
}

DECDLL void* ClrTypeSpecToTypeHandle(TypeSpec* spec)
{
    Type^ t = ClrMethod::TypeSpecToType(spec);
    if(t == nullptr) 
    {
        return 0;
    }
    else
    {
        return (void*)(IntPtr)GCHandle::Alloc(t);
    }
}

DECDLL void ClrFreeTypeHandle(void* type)
{
    if(type == 0) return;

    GCHandle handle = (GCHandle)(IntPtr)type;
    handle.Free();
}

DECDLL void* ClrTypeHandleToString(void* type)
{
    GCHandle handle = (GCHandle)(IntPtr)type;
    Type^ t = (Type^) handle.Target;
    String^ str;
    if(t == nullptr)
    {
        str = "unknown";
    }
    else
    {
        str = GetTypeName(t);
    }

    return (void*)GoshInvoke::Scm_MakeString(str, StringFlags::Copying);
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
        n = Util::IntPtrToUTF8String((IntPtr)const_cast<char*>(name)) + "*";
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
                    , GoshInvoke::Scm_MakeString(builder.ToString(), StringFlags::Copying))
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
                GoshInvoke::Scm_Cons(kind,GoshInvoke::Scm_MakeString(info->Name, StringFlags::Copying))
                , ret);
        }
    }

    return (void*)ret;
}

#ifdef __cplusplus
}
#endif

