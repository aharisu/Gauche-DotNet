/*
 * ClrMethod.h
 *
 * MIT License
 * Copyright 2013 aharisu
 * All rights reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 *
 * aharisu
 * foo.yobina@gmail.com
 */

#pragma once

#include "ClrMethodCallStruct.h"
#include "Microsoft.Scripting/ArgType.h"

ref class MethodCandidate;

using namespace System;
using namespace System::Text;
using namespace System::Reflection;
using namespace System::Runtime::InteropServices;

ref class ClrMethod sealed
{
private:
    ClrMethod(TypeSpec* methodSpec
        , void* obj, bool isStatic
        , MethodArg* args, int numArg)
        :_methodSpec(methodSpec)
        ,_obj(obj)
        ,_isStatic(isStatic)
        ,_args(args)
        ,_numArg(numArg)
    {}

    void* CallNew();
    void* CallMethod();

    bool CreateArgTypes(StringBuilder^ builder, array<ArgType>^% argTypes);
    MethodInfo^ MakeGenericMethod(MethodInfo^ mi, array<ArgType>^ argTypes);
    MethodBase^ CreateCandidate(MethodBase^ info, array<ArgType>^ argTypes);
    array<Object^>^ ConstractArguments(MethodCandidate^ callMethod);

public:
    static Type^ GetType(String^ name);

    static void* CallNew(TypeSpec* methodSpec, MethodArg* args, int numArg)
    {
        ClrMethod method(methodSpec, 0, true,  args, numArg);
        try
        {
            return method.CallNew();
        }
        catch (Exception^ e)
        {
            GaucheDotNet::Native::GoshInvoke::Scm_Raise(
                GaucheDotNet::Native::GoshInvoke::Scm_MakeClrError(
                    GaucheDotNet::Native::GoshInvoke::Scm_MakeString(
                        e->Message , -1, -1, GaucheDotNet::Gosh::StringFlags::Copying)
                    , GaucheDotNet::Native::GoshInvoke::Scm_MakeClrObject((IntPtr)GCHandle::Alloc(e))
                    ));
                        
            return 0;
        }
    }

    static void* CallMethod(TypeSpec* methodSpec, void* obj, bool isStatic, MethodArg* args, int numArg)
    {
        ClrMethod method(methodSpec, obj, isStatic,  args, numArg);
        try
        {
            return method.CallMethod();
        } 
        catch (Exception^ e)
        {
            GaucheDotNet::Native::GoshInvoke::Scm_Raise(
                GaucheDotNet::Native::GoshInvoke::Scm_MakeClrError(
                    GaucheDotNet::Native::GoshInvoke::Scm_MakeString(
                        e->Message , -1, -1, GaucheDotNet::Gosh::StringFlags::Copying)
                    , GaucheDotNet::Native::GoshInvoke::Scm_MakeClrObject((IntPtr)GCHandle::Alloc(e))
                    ));
                        
            return 0;
        }
    }

private:
    initonly TypeSpec* _methodSpec;
    initonly void* _obj;
    initonly bool _isStatic;
    MethodArg* _args;
    initonly int _numArg;
};

