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
#include "CompilerHelpers.h"

using namespace System;
using namespace System::Reflection;

ref class SimpleArgBuilder : ArgBuilder
{
public:
	SimpleArgBuilder(int index, Type^ parameterType)
		:Index(index)
		,Type(parameterType)
		, IsParamsArray(false)
	{
	}

	SimpleArgBuilder(int index, Type^ parameterType, ParameterInfo^ paramInfo)
		:Index(index)
		,Type(parameterType)
		,IsParamsArray(CompilerHelpers::IsParamArray(paramInfo))
	{
	}

	property int Priority 
	{
		virtual int get() override {return 0;}
	}

public:
	initonly int Index;
	initonly Type^ Type;
	initonly bool IsParamsArray;
};