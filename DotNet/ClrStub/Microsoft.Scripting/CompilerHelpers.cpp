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
        // �� To type
        //�� From type
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
        //�v���~�e�B�u�^�̈ÖٓI�ϊ����\���H
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
            //�����^�Ȃ疳�����ŋ���0
            return 0;
        }
        else if (fromType->IsPrimitive)
        {
            //�ϊ���^��Object�^�Ȃ�K���ȑ傫������
            if (toType == Object::typeid)
            {
                return 99;
            }
            else
            {
                //�v���~�e�B�u�^�̈ÖٓI�ϊ��̋������v�Z����
                TypeCode fromTypeCode = Type::GetTypeCode(fromType);
                TypeCode toTypeCode = Type::GetTypeCode(toType);
                if (CanImplicitConvertFrom(fromTypeCode, toTypeCode))
                {
                    return (int)(toTypeCode - fromTypeCode);
                }

                //�ÖٓI�ȕϊ����o���Ȃ�����
                return -1;
            }
        }
        else if (toType->IsInterface)
        {
            //�ϊ���̌^���C���^�t�F�[�X�̏ꍇ

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
            //�ϊ���̌^���v���~�e�B�u�^�̏ꍇ�A
            //�ԊґO�̌^���I�u�W�F�N�g�^�Ȃ̂ŕϊ��ł��Ȃ�
            return -1;
        }
        else
        {
            //�I�u�W�F�N�g�^���m�̕ϊ��Ȃ̂ŁA�p�������ǂ��ċ����𑪂�

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
