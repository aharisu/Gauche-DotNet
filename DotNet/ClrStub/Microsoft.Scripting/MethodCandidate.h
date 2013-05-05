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

#include "MethodTarget.h"
#include "ParameterWrapper.h"
#include "CallType.h"
#include "CompilerHelpers.h"
#include "ArgType.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Collections::Generic;

ref class MethodCandidate
{
public:
    MethodCandidate(MethodTarget^ target, List<ParameterWrapper^>^ parameters
        , int paramsArgIndex, Type^ paramsElementType)
        :Target(target)
        , Parameters(parameters)
        , ParamsArgumentIndex(paramsArgIndex)
        , ParamsElementType(paramsElementType)
    {
        Parameters->TrimExcess();
    }

    bool IsApplicable(array<ArgType>^ types, bool isSpecifyParamType)
    {
        if(isSpecifyParamType)
        {
            if(ParamsArgumentIndex == -1)
            {
                if(types->Length != Parameters->Count)
                {
                    return false;
                }
            }
            else
            {
                //可変長引数のメソッドの場合は
                if(ParamsArgumentIndex != types->Length - 1)
                {
                    return false;
                }
                //パラメータ型指定の属性にもparamsがあるか？
                if(types[ParamsArgumentIndex].attr != TYPESPEC_ATTR_PARAMS)
                {
                    return false;
                }
            }
        }

        for(int i = 0;i < types->Length;++i)
        {
            //TODO CHECK ATTR

            if(!Parameters[i]->HasConversionFrom(%types[i]))
            {
                return false;
            }
        }

        return true;
    }

    int CompareTo(MethodCandidate^ other, CallType callType, array<ArgType>^ types)
    {
        Nullable<int> cmpParams = ParameterWrapper::CompareParameters(this->Parameters, other->Parameters, types);
        if(cmpParams.HasValue && (cmpParams.Value == 1 || cmpParams.Value == -1)) return cmpParams.Value;

        int ret = Target->CompareEqualParameters(other->Target);
        if(ret != 0) return ret;

        bool isStaticThis = CompilerHelpers::IsStatic(this->Target->Method);
        bool isStaticOther = CompilerHelpers::IsStatic(other->Target->Method);
        if(isStaticThis && !isStaticOther)
        {
            return callType == CallType::ImplicitInstance ? -1 : 1;
        }
        else if(!isStaticThis && isStaticOther)
        {
            return callType == CallType::ImplicitInstance ? 1 : -1;
        }

        return 0;
    }

    MethodCandidate^ MakeParamsExtended(int count)
    {
        List<ParameterWrapper^>^ newParameters = gcnew List<ParameterWrapper^>(count);
        Type^ elementType = nullptr;
        int index = -1;

        for(int i = 0;i < Parameters->Count;++i)
        {
            ParameterWrapper^ pw = Parameters[i];

            if(Parameters[i]->IsParamsArray)
            {
                elementType = pw->Type->GetElementType();
                index = i;
            }
            else
            {
                newParameters->Add(pw);
            }
        }

        if(index != -1)
        {
            while(newParameters->Count < count)
            {
                ParameterWrapper^ params = gcnew ParameterWrapper(elementType);
                newParameters->Insert(System::Math::Min(index, newParameters->Count), params);
            }
        }

        if(count != newParameters->Count) return nullptr;

        return gcnew MethodCandidate(Target->MakeParamsExtended(count), newParameters
            , index, elementType);
    }

public:
    initonly MethodTarget^ Target;
    initonly List<ParameterWrapper^>^ Parameters;
    initonly int ParamsArgumentIndex;
    initonly Type^ ParamsElementType;
};
