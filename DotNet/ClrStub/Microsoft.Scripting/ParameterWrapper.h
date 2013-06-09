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
            case OBJWRAP_BOOL:
                if(Type->IsAssignableFrom(Boolean::typeid))
                {
                    return true;
                }
                return Type->IsAssignableFrom(GaucheDotNet::GoshBool::typeid);
            case OBJWRAP_INT:
                if(Type->IsAssignableFrom(GoshFixnum::typeid))
                {
                    return true;
                }
                else
                {
                    //Gauche上のfixnumで引数が指定されている場合は、
                    //メソッドのパラメータが数値であれば何でもマッチするように判定させる
                    return CompilerHelpers::CanConvertFrom(Byte::typeid, Type);
                }
            case OBJWRAP_FLONUM:
                if(Type->IsAssignableFrom(GoshFixnum::typeid))
                {
                    return true;
                }
                else
                {
                    //Gauche上のflonumで引数が指定されている場合は、
                    //メソッドのパラメータが浮動小数点数値であれば何でもマッチするように判定させる
                    return CompilerHelpers::CanConvertFrom(Single::typeid, Type);
                }
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
    ///型と引数のoriginCodeとの距離を測る。
    ///ByteやInt16など本来は暗黙的なキャスト不可な型との距離もはかり、
    ///それらの場合距離はマイナスになる。
    ///</summary>
    static int DistanceBetweenTypeCode(System::Type^ t, TypeCode originCode)
    {
        if(t == Object::typeid)
        {
            return 99;
        }
        else
        {
            return (int)Type::GetTypeCode(t) - (int)originCode;
        }
    }

    ///<summary>
    ///クラス型同士の距離を測る
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
                return Int32::MaxValue;
            }
            ++diff;
        }
    }

    static Nullable<int> TypeComparePrimitive(System::Type^ t1, System::Type^ t2, TypeCode originCode)
    {
        int diff1 = DistanceBetweenTypeCode(t1, originCode);
        int diff2 = DistanceBetweenTypeCode(t2, originCode);
        if(diff1 == diff2)
        {
            return Nullable<int>(); //nullptr
        }
        else if(diff1 < 0)
        {
            if(diff2 < 0) 
            {//両方とも距離がマイナスの場合(Int32よりも狭い範囲の型の場合)
                //絶対値の小さいほうが優先(より範囲の広い型)
                return Math::Abs(diff1) < Math::Abs(diff2) ? 1 : -1;
            }
            else
            {//diff1はマイナスででdiff2は0以上の場合、diff2が優先
                return -1;
            }
        }
        else if(diff2 < 0)
        { //diff1は0以上でdiff2はマイナスの場合、diff1が優先
            return 1;
        }
        else
        {//両方とも距離がプラスの場合
            //距離が近いほうが優先
            return diff1 < diff2 ? 1 : -1;
        }
    }

    static Nullable<int> TypeCompareClrOrGosh(Type^ argType, Type^ t1, Type^ t2, TypeCode targetTypeCode)
    {
        if(GoshObj::typeid->IsAssignableFrom(t1))
        {
            if(GoshObj::typeid->IsAssignableFrom(t2))
            {
                //t1とt2ともにGaucheのオブジェクトになる
                //パラメータの型とクラスとの距離を測ってより近いほうを優先にする
                int diff1 = DistanceBetweenClass(argType, t1);
                int diff2 = DistanceBetweenClass(argType, t2);
                return diff1 == diff2 ? Nullable<int>() : //nullptr
                    diff1 < diff2 ? 1 : -1;
            }
            else
            {
                //t1はGaucheのオブジェクト、t2はclrのオブジェクト
                //Gaucheオブジェクトのほうが優先なのでt1が優先
                return 1;
            }
        }
        else
        {
            if(GoshObj::typeid->IsAssignableFrom(t2))
            {
                //t1はclrのオブジェクト、t2はGaucheのオブジェクトになる
                //Gaucheオブジェクトのほうが優先なのでt2が優先
                return -1;
            }
            else
            {
                //t1とt2ともにclrのオブジェクトの引数になる
                //パラメータの型とクラスとの距離を測ってより近いほうを優先にする
                return TypeComparePrimitive(t1, t2, targetTypeCode);
            }
        }
    }

    //このメソッドはHasConversionFromでp1とp2ともにtrueになることが前提。
    static Nullable<int> CompareTo(ParameterWrapper^ p1, ParameterWrapper^ p2, ArgType^ argType)
    {
        System::Type^ t1 = p1->Type;
        System::Type^ t2 = p2->Type;
        if(t1 == t2) 
        {
            return 0;
        }

        switch(argType->kind)
        {
        case OBJWRAP_BOOL:
            return TypeCompareClrOrGosh(argType->type, t1, t2, TypeCode::Boolean);
        case OBJWRAP_INT:
            return TypeCompareClrOrGosh(argType->type, t1, t2, TypeCode::Int32);
        case OBJWRAP_FLONUM:
            return TypeCompareClrOrGosh(argType->type, t1, t2, TypeCode::Single);
        case OBJWRAP_STRING:
            return TypeCompareClrOrGosh(argType->type, t1, t2, TypeCode::String);
        case OBJWRAP_PROC:
            {
                if(Delegate::typeid->IsAssignableFrom(t1))
                {
                    if(Delegate::typeid->IsAssignableFrom(t2))
                    {
                        //両方ともデリゲートの場合は同じ距離と判定する
                        return Nullable<int>();
                    }
                    else
                    {
                        //t1はDelegate、t2はGaucheのオブジェクトなのでt2が優先
                        return -1;
                    }
                }
                else
                {
                    if(Delegate::typeid->IsAssignableFrom(t2))
                    {
                        //t1はGaucheオブジェクト、t2がDelegateなのでt1が優先
                        return 1;
                    }
                    else
                    {
                        //t1とt2ともにGaucheのオブジェクト
                        //パラメータの型と各引数の型との距離を測ってより近いほうを優先にする
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
