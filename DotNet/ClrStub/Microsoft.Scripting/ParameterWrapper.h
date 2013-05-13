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
            //TODO GoshFixnumを受け取るケースを考える
            case OBJWRAP_INT:
                //Gauche上のfixnumで引数が指定されている場合は、
                //メソッドのパラメータが数値であれば何でもマッチするように判定させる
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
    ///型とInt32との距離を測る。
    ///ByteやInt16など本来は暗黙的なキャスト不可な型との距離もはかり、
    ///それらの場合距離はマイナスになる。
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
                return -1;
            }
            ++diff;
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

        switch(argType->attr)
        {
            //TODO GoshFixnumを受け取るケースを考える
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
        case OBJWRAP_STRING:
            {
                if(t1->IsAssignableFrom(String::typeid))
                {
                    if(t2->IsAssignableFrom(String::typeid))
                    {
                        //t1とt2ともにclrのStringオブジェクトの引数になる
                        //パラメータの型とStringクラスとの距離を測ってより近いほうを優先にする
                        int diff1 = DistanceBetweenClass(argType->type, t1);
                        int diff2 = DistanceBetweenClass(argType->type, t2);
                        return diff1 == diff2 ? Nullable<int>() : //nullptr
                            diff1 < diff2 ? 1 : -1;
                    }
                    else
                    {
                        //t1はclrのStringオブジェクト、t2はGaucheのStringオブジェクトになる
                        //Gaucheオブジェクトのほうが優先なのでt2が優先
                        return -1;
                    }
                }
                else
                {
                    if(t2->IsAssignableFrom(String::typeid))
                    {
                        //t1はGaucheのStringオブジェクト、t2はclrのStringオブジェクト
                        //Gaucheオブジェクトのほうが優先なのでt1が優先
                        return 1;
                    }
                    else
                    {
                        //t1とt2ともにGaucheのStringオブジェクトになる
                        //パラメータの型とStringクラスとの距離を測ってより近いほうを優先にする
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
                        //ﾂ猟ｼﾂ陛ｻﾂとづﾂデﾂδ環ゲﾂーﾂトﾂの場合ﾂは督ｯﾂつｶﾂ仰猟猟｣ﾂと板ｻﾂ定すﾂづｩ
                        return Nullable<int>();
                    }
                    else
                    {
                        //t1ﾂづ好elegateﾂ、t2ﾂづ宏aucheﾂのオﾂブﾂジﾂェﾂクﾂトﾂなのづ�t2ﾂつｪﾂ優ﾂ静ｦ
                        return -1;
                    }
                }
                else
                {
                    if(Delegate::typeid->IsAssignableFrom(t2))
                    {
                        //t1ﾂづ宏aucheﾂオﾂブﾂジﾂェﾂクﾂトﾂ、t2ﾂつｪDelegateﾂなのづ�t1ﾂつｪﾂ優ﾂ静ｦ
                        return 1;
                    }
                    else
                    {
                        //t1ﾂづ�t2ﾂとづﾂづ烏aucheﾂのオﾂブﾂジﾂェﾂクﾂト
                        //ﾂパﾂδ可δ�ﾂーﾂタﾂの型ﾂと各ﾂ暗ｸﾂ青板の型ﾂとの仰猟猟｣ﾂづｰﾂ堕ｪﾂ�Bﾄづｦﾂづｨﾂ近つ｢ﾂほつ､ﾂづｰﾂ優ﾂ静ｦﾂにつｷﾂづｩ
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
