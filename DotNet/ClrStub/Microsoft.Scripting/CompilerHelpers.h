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

using namespace System;
using namespace System::Reflection;
using namespace System::Collections::Generic;

namespace CompilerHelpers
{
    Type^ GetReturnType(MethodBase^ mi);
    bool IsParamsMethod(MethodBase^ method);
    bool IsParamsMethod(array<ParameterInfo^>^ pis);
    bool IsParamArray(ParameterInfo^ parameter);
    bool IsOutParameter(ParameterInfo^ pi);
    bool IsStatic(MethodBase^ mi);
    Type^ GetType(Object^ obj);
    array<Type^>^ GetTypes(array<Object^>^ args);
    bool CanImplicitConvertFrom(TypeCode fromTypeCode, TypeCode toTypeCode);
    bool CanConvertFrom(Type^ fromType, Type^ toType);
    int DistanceBetweenType(Type^ fromType, Type^ toType);
}

