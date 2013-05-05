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
#include "MethodCandidate.h"
#include "CallType.h"
#include "ReferenceArgBuilder.h"
#include "SimpleArgBuilder.h"
#include "ArgType.h"

using namespace System;
using namespace System::Collections::Generic;

enum class BinderType {
	Normal,
	BinaryOperator,
	ComparsionOperator,
	Constractor
};

ref class TargetSet
{
public:
	TargetSet(MethodBinder^ binder, int count)
		:Count(count)
	{
	}

	MethodCandidate^ MakeBindingTarget(CallType callType, array<ArgType>^ types, bool isSpecifyParamType)
	{
		List<MethodCandidate^> applicableTargets;
		for each(MethodCandidate^ target in Targets)
		{
			if(target->IsApplicable(types, isSpecifyParamType))
			{
				applicableTargets.Add(target);
			}
		}

		if(applicableTargets.Count == 1)
		{
			return applicableTargets[0];
		}
		else if(applicableTargets.Count > 1)
		{
			//Find best match method
			for each(MethodCandidate^ candidate in applicableTargets)
			{
				if(IsBest(candidate, %applicableTargets, callType, types)) return candidate;
			}

			return nullptr;
		}
		else
		{
			return nullptr;
		}
	}

private:
	static bool IsBest(MethodCandidate^ candidate, List<MethodCandidate^>^ applicableTargets
		, CallType callType,  array<ArgType>^ types)
	{
		for each(MethodCandidate^ target in applicableTargets)
		{
			if(candidate == target) continue;
			if(candidate->CompareTo(target, callType, types) != 1) return false;
		}

		return true;
	}

public:
	initonly int Count;
	List<MethodCandidate^> Targets;
};

ref class MethodBinder
{
public:
	MethodBinder(String^ name, IList<MethodBase^>^ methods, BinderType binderType)
		:Name(name)
		,_binderType(binderType)
		,_paramsCandidates(nullptr)
	{
		for each(MethodBase^ method in methods)
		{
			if(IsUnsupported(method)) continue;

			AddBasicMethodTargets(method);
		}

		if(_paramsCandidates != nullptr)
		{
			for each(MethodCandidate^ maker in _paramsCandidates)
			{
				for each(int count in _targetSets.Keys)
				{
					MethodCandidate^ target = maker->MakeParamsExtended(count);
					if(target != nullptr)
					{
						AddTarget(target);
					}
				}
			}
		}
	}

	MethodCandidate^ MakeBindingTarget(CallType callType
		, int numArgs, array<ArgType>^ types, bool isSpecifyParamType)
	{
		TargetSet^ ts = nullptr;

		if(!_targetSets.TryGetValue(numArgs, ts))
		{
			ts = BuildTargetSet(numArgs);
		}

		if(ts != nullptr)
		{
			return ts->MakeBindingTarget(callType, types, isSpecifyParamType);
		}

		return nullptr;
	}

private:
	static bool IsUnsupported(MethodBase^ method)
	{
		return (int)(method->CallingConvention & CallingConventions::VarArgs) != 0
			|| method->ContainsGenericParameters;
	}

	void AddTarget(MethodCandidate^ target)
	{
		int count = target->Target->ParameterCount;
		TargetSet^ set;
		if(!_targetSets.TryGetValue(count, set))
		{
			set = gcnew TargetSet(this, count);
			_targetSets[count] = set;
		}

		set->Targets.Add(target);
	}

	void AddBasicMethodTargets(MethodBase^ method)
	{
		List<ParameterWrapper^>^ parameters = gcnew List<ParameterWrapper^>();
		int argIndex = 0;
		if(!CompilerHelpers::IsStatic(method))
		{
			parameters->Add(gcnew ParameterWrapper(method->DeclaringType, true));
		}

		array<ParameterInfo^>^ methodParams = method->GetParameters();
		List<ArgBuilder^>^ argBuilders = gcnew List<ArgBuilder^>(methodParams->Length);

		for each(ParameterInfo^ pi in methodParams)
		{
			int newIndex = argIndex++;

			ArgBuilder^ ab;
			if(pi->ParameterType->IsByRef || CompilerHelpers::IsOutParameter(pi))
			{
				ParameterWrapper^ param = gcnew ParameterWrapper(pi->ParameterType, true, pi->Name);
				parameters->Add(param);
				ab = gcnew ReferenceArgBuilder(newIndex, param->Type);
			}
			else
			{
				ParameterWrapper^ param = gcnew ParameterWrapper(pi);
				parameters->Add(param);
				ab = gcnew SimpleArgBuilder(newIndex, param->Type, pi);
			}
			argBuilders->Add(ab);
		}

		MethodCandidate^ target = gcnew MethodCandidate(
			gcnew MethodTarget(this, method, parameters->Count, argBuilders, CompilerHelpers::GetReturnType(method))
			, parameters
			, -1, nullptr);
		AddTarget(target);

		if(CompilerHelpers::IsParamsMethod(method))
		{
			if(_paramsCandidates == nullptr)
			{
				_paramsCandidates = gcnew List<MethodCandidate^>();
			}
			_paramsCandidates->Add(target);
		}
	}

	TargetSet^ BuildTargetSet(int count)
	{
		TargetSet^ ts = nullptr;
		if(_paramsCandidates != nullptr)
		{
			for each(MethodCandidate^ maker in _paramsCandidates)
			{
				MethodCandidate^ target = maker->MakeParamsExtended(count);
				if(target != nullptr)
				{
					if(ts == nullptr)
					{
						ts = gcnew TargetSet(this, count);
					}
					ts->Targets.Add(target);
				}
			}
		}

		return ts;
	}

public:
	initonly String^ Name;
private:
	BinderType _binderType;
	Dictionary<int, TargetSet^> _targetSets;
	List<MethodCandidate^>^ _paramsCandidates;
};