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

#include "ArgBuilder.h"
#include "SimpleArgBuilder.h"
#include "ParamsArgBuilder.h"

#include "CompilerHelpers.h"

using namespace System::Reflection;
using namespace System::Collections::Generic;

ref class MethodBinder;

ref class MethodTarget
{
public:
	MethodTarget(MethodBinder^ binder, MethodBase^ method, int parameterCount, IList<ArgBuilder^>^ argBuilders, Type^ returnType)
		:Method(method)
		,ParameterCount(parameterCount)
		,ReturnType(returnType)
		,_binder(binder)
		,_argBuilders(argBuilders)
	{
	}

	int CompareEqualParameters(MethodTarget^ other)
	{
		if(this->Method->IsGenericMethod)
		{
			if(!other->Method->IsGenericMethod)
			{
				return -1;
			}
			else
			{
				return 0;
			}
		}
		else if(other->Method->IsGenericMethod)
		{
			return 1;
		}

		for(int i =Int32::MaxValue; i >= 0;)
		{
			int maxPriorityThis = FindMaxPriority(this->_argBuilders, i);
			int maxPriorityOther = FindMaxPriority(other->_argBuilders, i);

			if(maxPriorityThis < maxPriorityOther) return 1;
			if(maxPriorityOther < maxPriorityThis) return -1;

			i = maxPriorityThis - 1;
		}

		return 0;
	}

	MethodTarget^ MakeParamsExtended(int argCount)
	{
		List<ArgBuilder^>^ newArgBuilders = gcnew List<ArgBuilder^>(_argBuilders->Count);

		int curArg = CompilerHelpers::IsStatic(Method) ? 0 : 1;
		for each(ArgBuilder^ ab in _argBuilders)
		{
			SimpleArgBuilder^ sab = dynamic_cast<SimpleArgBuilder^>(ab);
			if(sab != nullptr)
			{
				if(sab->IsParamsArray)
				{
					int paramsUsed = argCount - GetConsumedArguments() - (CompilerHelpers::IsStatic(Method) ? 1 : 0);
					newArgBuilders->Add(gcnew ParamsArgBuilder(curArg, paramsUsed, sab->Type->GetElementType()));

					curArg += paramsUsed;
				}
				else
				{
					newArgBuilders->Add(gcnew SimpleArgBuilder(curArg++, sab->Type));
				}
			}
			else
			{
				newArgBuilders->Add(ab);
			}
		}

		return gcnew MethodTarget(_binder, Method, argCount, newArgBuilders, ReturnType);
	}

private:
	static int FindMaxPriority(IList<ArgBuilder^>^ abs, int ceiling)
	{
		int max = 0;
		for each(ArgBuilder^ ab in abs)
		{
			if(ab->Priority > ceiling) continue;

			max = System::Math::Max(max, ab->Priority);
		}
		return max;
	}

	int GetConsumedArguments()
	{
		int consuming = 0;
		for each(ArgBuilder^ argb in _argBuilders)
		{
			SimpleArgBuilder^ sab = dynamic_cast<SimpleArgBuilder^>(argb);
			if(sab != nullptr)
			{
				++consuming;
			}
		}
		return consuming;
	}

public:
	initonly MethodBase^ Method;
	initonly int ParameterCount;
	initonly Type^ ReturnType;
private:
	MethodBinder^ _binder;
	IList<ArgBuilder^>^ _argBuilders;
};