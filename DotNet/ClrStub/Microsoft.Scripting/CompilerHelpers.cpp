/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/


#include "CompilerHelpers.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Collections::Generic;

namespace CompilerHelpers
{
    Type^ GetReturnType(MethodBase^ mi)
    {
        if (mi->IsConstructor) return mi->DeclaringType;
        else return ((MethodInfo^)mi)->ReturnType;
    }

    bool IsParamsMethod(MethodBase^ method)
    {
        return IsParamsMethod(method->GetParameters());
    }

    bool IsParamsMethod(array<ParameterInfo^>^ pis)
    {
        for each (ParameterInfo^ pi in pis)
        {
            if (IsParamArray(pi)) return true;
        }
        return false;
    }

    bool IsParamArray(ParameterInfo^ parameter)
    {
        return parameter->IsDefined(ParamArrayAttribute::typeid, false);
    }

    bool IsOutParameter(ParameterInfo^ pi)
    {
        // not using IsIn/IsOut properties as they are not available in Silverlight:
        return (pi->Attributes & (ParameterAttributes::Out | ParameterAttributes::In)) == ParameterAttributes::Out;
    }

    bool IsStatic(MethodBase^ mi)
    {
        return mi->IsConstructor || mi->IsStatic;
    }

    Type^ GetType(Object^ obj)
    {
        //return obj == nullptr ? NoneType : obj->GetType();
        return obj == nullptr ? nullptr : obj->GetType();
    }

    array<Type^>^ GetTypes(array<Object^>^ args)
    {
        array<Type^>^ types = gcnew array<Type^>(args->Length);
        for (int i = 0; i < args->Length; i++)
        {
            types[i] = GetType(args[i]);
        }
        return types;
    }

    static bool ImplicitConvertMatrix[10][9] = {
        // → To type
        //↓ From type
        //Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, Decimal
        {   false,  true,  true,  true, true, true, true,  true, true ,}, //Char
        {   true,   false, true, false, true, false, true, true,true ,}, //SByte
        {   true,   true,  true, true, true,  true,  true,  true, true ,}, //Byte
        {   false,  false, true, false, true,  false, true, true,true ,}, //Int16
        {   false,  false, true, true,  true, true,  true,   true, true ,}, //UInt16
        {   false,  false, false, false, true, false, true, true, true,}, //Int32
        {   false,  false, false, false, true, true,   true,  true, true ,}, //UInt32
        {   false,  false, false, false, false, false, true, true, true,}, //Int64
        {   false,  false, false, false, false, false, true, true, true,}, //UInt64
        {   false,  false, false, false, false, false, false,  true, true,}, //Single
    };

    bool CanImplicitConvertFrom(TypeCode fromTypeCode, TypeCode toTypeCode)
    {
        //プリミティブ型の暗黙的変換が可能か？
        return ((int)TypeCode::Char <= (int)fromTypeCode && (int)fromTypeCode <= (int)TypeCode::Single)
            && ((int)TypeCode::Int16 <= (int)toTypeCode && (int)toTypeCode <= (int)TypeCode::Decimal)
            && ImplicitConvertMatrix[(int)(fromTypeCode - TypeCode::Char), (int)(toTypeCode - TypeCode::Int16)];
    }

    bool CanConvertFrom(Type^ fromType, Type^ toType)
    {
        if (fromType == toType
            || toType->IsAssignableFrom(fromType)
            || (fromType->IsPrimitive && toType->IsPrimitive 
            && CanImplicitConvertFrom(Type::GetTypeCode(fromType), Type::GetTypeCode(toType)) )
            )
        {
            return true;
        }
        return false;
    }


    int DistanceBetweenType(Type^ fromType, Type^ toType)
    {
        if (fromType == toType)
        {
            //同じ型なら無条件で距離0
            return 0;
        }
        else if (fromType->IsPrimitive)
        {
            //変換後型がObject型なら適当な大きい数字
            if (toType == Object::typeid)
            {
                return 99;
            }
            else
            {
                //プリミティブ型の暗黙的変換の距離を計算する
                TypeCode fromTypeCode = Type::GetTypeCode(fromType);
                TypeCode toTypeCode = Type::GetTypeCode(toType);
                if (CanImplicitConvertFrom(fromTypeCode, toTypeCode))
                {
                    return (int)(toTypeCode - fromTypeCode);
                }

                //暗黙的な変換が出来なかった
                return -1;
            }
        }
        else if (toType->IsInterface)
        {
            //変換後の型がインタフェースの場合

            for each(Type^ t in fromType->GetInterfaces())
            {
                if(t == toType)
                {
                    return 1;
                }
            }
            return -1;
        }
        else if (toType->IsPrimitive)
        {
            //変換後の型がプリミティブ型の場合、
            //返還前の型がオブジェクト型なので変換できない
            return -1;
        }
        else
        {
            //オブジェクト型同士の変換なので、継承をたどって距離を測る

            int diff = 1;
            while (true)
            {
                fromType = fromType->BaseType;
                if (toType == fromType)
                {
                    return diff;
                }
                else if (fromType == Object::typeid)
                {
                    return -1;
                }
                ++diff;
            }
        }
    }



}
