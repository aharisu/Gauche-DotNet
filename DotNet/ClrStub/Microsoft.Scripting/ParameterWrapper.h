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

#pragma once

#include "CompilerHelpers.h"
#include "None.h"
#include "ArgType.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Collections::Generic;

ref class ParameterWrapper
{
public:
    ParameterWrapper(Type^ type)
        :Type(type)
    {
    }

    ParameterWrapper(Type^ type, String^ name)
        :Type(type)
        , Name(name)
    {
    }

    ParameterWrapper(Type^ type, bool prohibitNull)
        :Type(type)
        , _prohibitNull(prohibitNull)
    {
    }

    ParameterWrapper(Type^ type, bool prohibitNull, String^ name)
        :Type(type)
        , _prohibitNull(prohibitNull)
        , Name(name)
    {
    }

    ParameterWrapper(ParameterInfo^ info)
        :Type(info->ParameterType)
    {
        this->Name = info->Name == nullptr ? "<unknown>" : info->Name;
        this->IsParamsArray = info->IsDefined(ParamArrayAttribute::typeid, false);
    }

    static Nullable<int> CompareParameters(
        IList<ParameterWrapper^>^ parameters1
        , IList<ParameterWrapper^>^ parameters2
        , array<ArgType>^ actualTypes
        )
    {
        Nullable<int> ret = Nullable<int>(0);
        for(int i = 0;i < actualTypes->Length;++i)
        {
            ParameterWrapper^ p1 = parameters1[i];
            ParameterWrapper^ p2 = parameters2[i];
            Nullable<int> cmp = CompareTo(p1, p2, %actualTypes[i]);

            if(!ret.HasValue)
            {
                if(!cmp.HasValue || cmp.Value != 0)
                {
                    ret = cmp;
                }
            }
            else
            {
                switch(ret.Value)
                {
                case 0:
                    ret = cmp;
                    break;
                case 1:
                    if(cmp.HasValue && cmp.Value == -1) 
                    {
                        return Nullable<int>();
                    }
                    break;
                case -1:
                    if(cmp.HasValue && cmp.Value == 1) 
                    {
                        return Nullable<int>();
                    }
                    break;
                }
            }
        }

        return ret;
    }

    bool HasConversionFrom(ArgType^ t)
    {
        if(t->type == Type)
        {
            return true;
        }

        if(t->type == None::Type)
        {
            if(_prohibitNull)
            {
                return false;
            }

            if(Type->IsGenericType && Type->GetGenericTypeDefinition() == Nullable::typeid)
            {
                return true;
            }
            return !Type->IsValueType;
        }
        else
        {
            switch(t->kind)
            {
            //TODO GoshFixnum���󂯎��P�[�X���l����
            case OBJWRAP_INT:
                //Gauche���fixnum�ň������w�肳��Ă���ꍇ�́A
                //���\�b�h�̃p�����[�^�����l�ł���Ή��ł��}�b�`����悤�ɔ��肳����
                return CompilerHelpers::CanConvertFrom(Byte::typeid, Type);
            case OBJWRAP_STRING:
                if(Type->IsAssignableFrom(String::typeid))
                {
                    return true;
                }
                return Type->IsAssignableFrom(GaucheDotNet::GoshString::typeid);
            case OBJWRAP_PROC:
                if(Delegate::typeid->IsAssignableFrom(Type))
                {
                    return true;
                }
                return Type->IsAssignableFrom(GaucheDotNet::GoshProc::typeid);
            case OBJWRAP_CLROBJECT:
            default:
                return CompilerHelpers::CanConvertFrom(t->type, Type);
            }
        }

    }

private:

    ///<summary>
    ///�^��Int32�Ƃ̋����𑪂�B
    ///Byte��Int16�Ȃǖ{���͈ÖٓI�ȃL���X�g�s�Ȍ^�Ƃ̋������͂���A
    ///�����̏ꍇ�����̓}�C�i�X�ɂȂ�B
    ///</summary>
    static int DistanceBetweenInt32(System::Type^ t)
    {
        if(t == Object::typeid)
        {
            return 99;
        }
        else
        {
            return (int)Type::GetTypeCode(t) - (int)TypeCode::Int32;
        }
    }

    ///<summary>
    ///�N���X�^���m�̋����𑪂�
    ///</summary>
    static int DistanceBetweenClass(System::Type^ fromType, System::Type^ toType) 
    {
        if(fromType == toType)
        {
            return 0;
        }

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

    //���̃��\�b�h��HasConversionFrom��p1��p2�Ƃ���true�ɂȂ邱�Ƃ��O��B
    static Nullable<int> CompareTo(ParameterWrapper^ p1, ParameterWrapper^ p2, ArgType^ argType)
    {
        System::Type^ t1 = p1->Type;
        System::Type^ t2 = p2->Type;
        if(t1 == t2) 
        {
            return 0;
        }

        switch(argType->attr)
        {
            //TODO GoshFixnum���󂯎��P�[�X���l����
        case OBJWRAP_INT:
            {
                int diff1 = DistanceBetweenInt32(t1);
                int diff2 = DistanceBetweenInt32(t2);
                if(diff1 == diff2)
                {
                    return Nullable<int>(); //nullptr
                }
                else if(diff1 < 0)
                {
                    if(diff2 < 0) 
                    {//�����Ƃ��������}�C�i�X�̏ꍇ(Int32���������͈͂̌^�̏ꍇ)
                        //��Βl�̏������ق����D��(���͈͂̍L���^)
                        return Math::Abs(diff1) < Math::Abs(diff2) ? 1 : -1;
                    }
                    else
                    {//diff1�̓}�C�i�X�ł�diff2��0�ȏ�̏ꍇ�Adiff2���D��
                        return -1;
                    }
                }
                else if(diff2 < 0)
                { //diff1��0�ȏ��diff2�̓}�C�i�X�̏ꍇ�Adiff1���D��
                    return 1;
                }
                else
                {//�����Ƃ��������v���X�̏ꍇ
                    //�������߂��ق����D��
                    return diff1 < diff2 ? 1 : -1;
                }
            }
        case OBJWRAP_STRING:
            {
                if(t1->IsAssignableFrom(String::typeid))
                {
                    if(t2->IsAssignableFrom(String::typeid))
                    {
                        //t1��t2�Ƃ���clr��String�I�u�W�F�N�g�̈����ɂȂ�
                        //�p�����[�^�̌^��String�N���X�Ƃ̋����𑪂��Ă��߂��ق���D��ɂ���
                        int diff1 = DistanceBetweenClass(argType->type, t1);
                        int diff2 = DistanceBetweenClass(argType->type, t2);
                        return diff1 == diff2 ? Nullable<int>() : //nullptr
                            diff1 < diff2 ? 1 : -1;
                    }
                    else
                    {
                        //t1��clr��String�I�u�W�F�N�g�At2��Gauche��String�I�u�W�F�N�g�ɂȂ�
                        //Gauche�I�u�W�F�N�g�̂ق����D��Ȃ̂�t2���D��
                        return -1;
                    }
                }
                else
                {
                    if(t2->IsAssignableFrom(String::typeid))
                    {
                        //t1��Gauche��String�I�u�W�F�N�g�At2��clr��String�I�u�W�F�N�g
                        //Gauche�I�u�W�F�N�g�̂ق����D��Ȃ̂�t1���D��
                        return 1;
                    }
                    else
                    {
                        //t1��t2�Ƃ���Gauche��String�I�u�W�F�N�g�ɂȂ�
                        //�p�����[�^�̌^��String�N���X�Ƃ̋����𑪂��Ă��߂��ق���D��ɂ���
                        int diff1 = DistanceBetweenClass(argType->type, t1);
                        int diff2 = DistanceBetweenClass(argType->type, t2);
                        return diff1 == diff2 ? Nullable<int>() : //nullptr
                            diff1 < diff2 ? 1 : -1;
                    }
                }
            }
        case OBJWRAP_PROC:
            {
                if(Delegate::typeid->IsAssignableFrom(t1))
                {
                    if(Delegate::typeid->IsAssignableFrom(t2))
                    {
                        //¼ûƂàfQ[g̏ꍇ͓¯¶£Ɣ»肷é
                        return Nullable<int>();
                    }
                    else
                    {
                        //t1ÍDelegateAt2ÍGauche̃IuWFNgȂ̂Åt2ªDæ
                        return -1;
                    }
                }
                else
                {
                    if(Delegate::typeid->IsAssignableFrom(t2))
                    {
                        //t1ÍGaucheIuWFNgAt2ªDelegateȂ̂Åt1ªDæ
                        return 1;
                    }
                    else
                    {
                        //t1Æt2ƂàÉGauche̃IuWFNg
                        //p[^̌^Ɗeø̌^Ƃ̋£ðªBĂæè߂¢ق¤ðDæɂ·é
                        int diff1 = DistanceBetweenClass(argType->type, t1);
                        int diff2 = DistanceBetweenClass(argType->type, t2);
                        return diff1 == diff2 ? Nullable<int>() : //nullptr
                            diff1 < diff2 ? 1 : -1;
                    }
                }
            }
        case OBJWRAP_CLROBJECT:
        default:
            {
                int diff1 = CompilerHelpers::DistanceBetweenType(argType->type, t1);
                int diff2 = CompilerHelpers::DistanceBetweenType(argType->type, t2);
                return diff1 == diff2 ? Nullable<int>() : //nullptr
                    diff1 < diff2 ? 1 : -1;
            }
        }
    }

public:
    initonly Type^ Type;
    initonly bool IsParamsArray;
    initonly String^ Name;
private:
    bool _prohibitNull;
};
