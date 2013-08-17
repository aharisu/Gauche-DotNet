/*
 * PrimitiveOpCall.h
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
#include "Microsoft.Scripting/CompilerHelpers.h"

using namespace System;
using namespace System::Reflection;

static Object^ PrimitiveTypeImplicitConversion(Object^ obj, Type^ toType)
{
    TypeCode objTypeCode = Type::GetTypeCode(obj->GetType());
    TypeCode toTypeCode = Type::GetTypeCode(toType);

    if(objTypeCode == toTypeCode)
    {
        return obj;
    }
    else if(CompilerHelpers::CanImplicitConvertFrom(objTypeCode, toTypeCode))
    {
        return Convert::ChangeType(obj, toTypeCode);
    }
    else
    {
        return nullptr;
    }
}

static void PreprocessPrimitiveOp(array<ArgType>^ typeSpec, Object^% instance, Object^% secondArg, bool allowUnaryOp, bool allowBinaryOp)
{
    if(typeSpec != nullptr && 
        typeSpec->Length != ((secondArg == nullptr) ? 2 : 3))
    {
        //メソッドの型指定子と引数の数が合っていません
        throw gcnew GoshException("not match type specifier and number of arguments");
    }

    if(secondArg == nullptr)
    {
        if(!allowUnaryOp)
        {
            //単項演算は定義されていません
            throw gcnew GoshException("Unary operation is not defined");
        }

        if(typeSpec != nullptr)
        {
            instance = PrimitiveTypeImplicitConversion(instance, typeSpec[1].typeInfo.type);
            if(instance == nullptr)
            {
                //型指定子と実際のオブジェクトの型が異なります。
                throw gcnew GoshException("not match type specifier and type of instance object");
            }
        }
    }
    else
    {
        if(!allowBinaryOp)
        {
            //2項演算は定義されていません
            throw gcnew GoshException("Binary operation is not defined");
        }

        if(typeSpec != nullptr)
        {
            instance = PrimitiveTypeImplicitConversion(instance, typeSpec[1].typeInfo.type);
            if(instance == nullptr)
            {
                //型指定子と実際のオブジェクトの型が異なります。
                throw gcnew GoshException("not match type specifier and type of instance object");
            }
            secondArg = PrimitiveTypeImplicitConversion(secondArg, typeSpec[2].typeInfo.type);
            if(secondArg == nullptr)
            {
                //型指定子と実際のオブジェクトの型が異なります。
                throw gcnew GoshException("not match type specifier and type of argument object");
            }
        }
    }
}
#include "ImplicitConvertionOp.h"

static Object^ PrimitiveAdd(array<ArgType>^ typeSpec, Object^ instance, Object^ secondArg)
{
    PreprocessPrimitiveOp(typeSpec, instance, secondArg, true, true);

    if(secondArg == nullptr)
    {
        switch(Type::GetTypeCode(instance->GetType()))
        {
        case TypeCode::Byte:
        case TypeCode::SByte:
        case TypeCode::Int16:
        case TypeCode::UInt16:
        case TypeCode::Int32:
        case TypeCode::UInt32:
        case TypeCode::Int64:
        case TypeCode::UInt64:
        case TypeCode::Single:
        case TypeCode::Double:
        case TypeCode::Decimal:
            return instance;
        }
    }
    else
    {
        Object^ ret = ImplicitConversionAdd(instance, secondArg);
        if(ret != nullptr)
        {
            return ret;
        }
    }
    //無効な演算子
    throw gcnew GoshException("invalid operation");
}

static Object^ PrimitiveSub(array<ArgType>^ typeSpec, Object^ instance, Object^ secondArg)
{
    PreprocessPrimitiveOp(typeSpec, instance, secondArg, true, true);

    if(secondArg == nullptr)
    {
        switch(Type::GetTypeCode(instance->GetType()))
        {
        case TypeCode::Byte:
            return -(Byte)instance;
        case TypeCode::SByte:
            return -(SByte)instance;
        case TypeCode::Int16:
            return -(Int16)instance;
        case TypeCode::UInt16:
            return -(UInt16)instance;
        case TypeCode::Int32:
            return -(Int32)instance;
        case TypeCode::Int64:
            return -(Int64)instance;
        case TypeCode::Single:
            return -(Single)instance;
        case TypeCode::Double:
            return -(Double)instance;
        case TypeCode::Decimal:
            return -(Decimal)instance;
        }
    }
    else
    {
        Object^ ret = ImplicitConversionSub(instance, secondArg);
        if(ret != nullptr)
        {
            return ret;
        }
    }

    //無効な演算子
    throw gcnew GoshException("invalid operation");
}

static Object^ PrimitiveNot(array<ArgType>^ typeSpec, Object^ instance, Object^ secondArg)
{
    PreprocessPrimitiveOp(typeSpec, instance, secondArg, true, false);

    if(instance->GetType() == Boolean::typeid)
    {
        return !(Boolean)instance;
    }

    //無効な演算子
    throw gcnew GoshException("invalid operation");
}

static Object^ PrimitiveOnesComplement(array<ArgType>^ typeSpec, Object^ instance, Object^ secondArg)
{
    PreprocessPrimitiveOp(typeSpec, instance, secondArg, true, false);

    switch(Type::GetTypeCode(instance->GetType()))
    {
    case TypeCode::Byte:
        return ~(Byte)instance;
    case TypeCode::SByte:
        return ~(SByte)instance;
    case TypeCode::Int16:
        return ~(Int16)instance;
    case TypeCode::UInt16:
        return ~(UInt16)instance;
    case TypeCode::Int32:
        return ~(Int32)instance;
    case TypeCode::Int64:
        return ~(Int64)instance;
    }

    //無効な演算子
    throw gcnew GoshException("invalid operation");
}


static Object^ PrimitiveLogAnd(array<ArgType>^ typeSpec, Object^ instance, Object^ secondArg)
{
    PreprocessPrimitiveOp(typeSpec, instance, secondArg, false, true);

    if(instance->GetType() == Boolean::typeid && secondArg->GetType() == Boolean::typeid)
    {
        return (Boolean)instance && (Boolean)secondArg;
    }
    else
    {
        Object^ ret = ImplicitConversionLogAnd(instance, secondArg);
        if(ret != nullptr)
        {
            return ret;
        }
    }

    //無効な演算子
    throw gcnew GoshException("invalid operation");
}

static Object^ PrimitiveLogOr(array<ArgType>^ typeSpec, Object^ instance, Object^ secondArg)
{
    PreprocessPrimitiveOp(typeSpec, instance, secondArg, false, true);

    if(instance->GetType() == Boolean::typeid && secondArg->GetType() == Boolean::typeid)
    {
        return (Boolean)instance || (Boolean)secondArg;
    }
    else
    {
        Object^ ret = ImplicitConversionLogOr(instance, secondArg);
        if(ret != nullptr)
        {
            return ret;
        }
    }

    //無効な演算子
    throw gcnew GoshException("invalid operation");
}

