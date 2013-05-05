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

using namespace System;

ref class ParamsArgBuilder : ArgBuilder
{
public:
	ParamsArgBuilder(int start, int count, Type^ elementType)
		:_start(start)
		, _count(count)
		, _elementType(elementType)
	{
	}

	property int Priority 
	{
		virtual int get() override {return 4;}
	}

private:
	int _start;
	int _count;
	Type^ _elementType;
};