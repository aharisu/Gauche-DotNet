/*
 * ClrMethod.cpp
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


#include "ClrMethod.h"

#include "Microsoft.Scripting/MethodBinder.h"
#include "PrimitiveOpCall.h"

using namespace System;
using namespace System::Text;
using namespace System::Reflection;
using namespace System::Runtime::InteropServices;
using namespace GaucheDotNet;
using namespace GaucheDotNet::Native;

Type^ ClrMethod::GetType(String^ name)
{
	for each(Assembly^ a in AppDomain::CurrentDomain->GetAssemblies())
	{
		Type^t = a->GetType(name);
		if(t != nullptr)
		{
			return t;
		}
	}
	return nullptr;
}

//TODO delegate type
static void TypeSpecToString(TypeSpec* spec, StringBuilder^ builder)
{
	//builder->Append(gcnew String(spec->name));
	builder->Append(Marshal::PtrToStringAnsi(IntPtr(const_cast<char*>(spec->name))));
}

static array<Type^>^ ConvertToTypeArray(TypeSpec* typeSpec, int numTypeSpec)
{
	StringBuilder builder; 
	array<Type^>^ typeAry = gcnew array<Type^>(numTypeSpec);
	for(int i = 0;i < numTypeSpec;++i)
	{
		TypeSpecToString(&(typeSpec[i]), %builder);
		typeAry[i] = ClrMethod::GetType(builder.ToString());
		builder.Length = 0;
	}

	return typeAry;
}


static Type^ GetHigherLevel(Type^ t1, Type^ t2)
{
    if (t1 == t2)
    {
        return t1;
    }
    else if (t1->IsPrimitive)
    {
        if (t2->IsPrimitive)
        {
			TypeCode t1TypeCode = Type::GetTypeCode(t1);
			TypeCode t2TypeCode = Type::GetTypeCode(t2);
            if (t1TypeCode == t2TypeCode)
            {
                return t1;
            }
			if (CompilerHelpers::CanImplicitConvertFrom(t1TypeCode, t2TypeCode))
            {
                return t2;
            }
			else if (CompilerHelpers::CanImplicitConvertFrom(t2TypeCode, t1TypeCode))
            {
                return t1;
            }
            else
            {
                return nullptr;
            }
        }
		else if (t2 == Object::typeid)
        {
            return t2;
        }
        else
        {
            return nullptr;
        }
    }
    else if (t2->IsPrimitive)
    {
		if (t1 == Object::typeid)
        {
            return t1;
        }
        else
        {
            return nullptr;
        }
    }
    else
    {
        if (t1->IsSubclassOf(t2))
        {
            return t2;
        }
        else if (t2->IsSubclassOf(t1))
        {
            return t1;
        }
        else
        {
            return nullptr;
        }
    }
}

MethodInfo^ ClrMethod::MakeGenericMethod(MethodInfo^ mi, array<ArgType>^ argTypes)
{
    array<Type^>^ genericArgs = mi->GetGenericArguments();
	if (_methodSpec->numGenericSpec >= 0)
    {
        //ジェネリック型の指定がある場合は指定された型でメソッドを作成する

        if (genericArgs->Length != _methodSpec->numGenericSpec)
        {
            return nullptr;
        }

		return mi->MakeGenericMethod(ConvertToTypeArray(_methodSpec->genericSpec, _methodSpec->numGenericSpec));
    }
    else
    {
        //ジェネリック型指定がない場合は実際の引数から型をジェネリック型を判別する
        array<ParameterInfo^>^ piAry = mi->GetParameters();

        array<Type^>^ paramTypes = nullptr;
		if (_methodSpec->numParamSpec >= 0)
        {
			if (piAry->Length != _methodSpec->numParamSpec)
            {
                return nullptr;
            }
			paramTypes = ConvertToTypeArray(_methodSpec->paramSpec, _methodSpec->numParamSpec);
        }
        else
        {
			int startIndex = (_isStatic ? 0 : 1);
			paramTypes = gcnew array<Type^>(argTypes->Length - startIndex);
			for(int i = startIndex;i < argTypes->Length;++i)
			{
				switch(argTypes[i].kind)
				{
				case METHOD_ARG_CLROBJECT:
					paramTypes[i - startIndex] = argTypes[i].type;
					break;
				case METHOD_ARG_INT:
					paramTypes[i - startIndex] = GoshFixnum::typeid;
					break;
				case METHOD_ARG_STRING:
					paramTypes[i - startIndex] = GoshString::typeid;
					break;
				//TODO more primitive type
				}
			}
        }

        array<Type^, 2>^ genericTypeCandidate = gcnew array<Type^, 2>(genericArgs->Length, piAry->Length);
        int index = 0;
        for each (ParameterInfo^ pi in piAry)
        {
            Type^ paramType = pi->ParameterType;
            if (paramType->IsGenericParameter)
            {
                for (int i = 0; i < genericArgs->Length; ++i)
                {
                    if (genericArgs[i] == paramType)
                    {
                        genericTypeCandidate[i, index] = paramTypes[index];
                        break;
                    }
                }
            }
            ++index;
        }

        array<Type^>^ genericType = gcnew array<Type^>(genericArgs->Length);
        for (int y = 0; y < piAry->Length; ++y)
        {
            Type^ candidate = nullptr;
            for (int x = 0; x < piAry->Length; ++x)
            {
                Type^ t = genericTypeCandidate[y, x];
                if (t != nullptr)
                {
                    if (candidate == nullptr)
                    {
                        candidate = t;
                    }
                    else
                    {
                        candidate = GetHigherLevel(candidate, t);
                        if (candidate == nullptr)
                        {
                            //throw new InvalidOperationException("ジェネリック型を持つ引数の整合性が取れていません。");
                            return nullptr;
                        }
                    }
                }
            }

            if (candidate == nullptr)
            {
                //throw new InvalidOperationException("ジェネリック型を特定できませんでした。");
                return nullptr;
            }
            genericType[y] = candidate;
        }

		for(int i = 0;i < genericType->Length;++i)
		{
			if(genericType[i] == nullptr)
			{
	            //throw new InvalidOperationException("ジェネリック型を特定できませんでした。");
	            return nullptr;
			}
		}

        return mi->MakeGenericMethod(genericType);
    }
}

MethodBase^ ClrMethod::CreateCandidate(MethodBase^ info, array<ArgType>^ argTypes)
{
	bool hasParams = false;
	array<ParameterInfo^>^ paramInfoAry = info->GetParameters();
	if(paramInfoAry->Length != 0)
	{
		hasParams = CompilerHelpers::IsParamArray(paramInfoAry[paramInfoAry->Length - 1]);
	}

	if(hasParams)
	{
		if(_numArg < paramInfoAry->Length - 1) return nullptr;
	}
	else
	{
		if(_numArg != paramInfoAry->Length) return nullptr;
	}

	if(info->ContainsGenericParameters)
	{
		return MakeGenericMethod((MethodInfo^)info, argTypes);
	}
	else
	{
		return info;
	}
}

static Object^ ToArgumentObject(Type^ type, MethodArg* arg)
{
	switch(arg->kind)
	{
	case METHOD_ARG_INT:
		if(type->IsAssignableFrom(GoshFixnum::typeid))
		{
			return gcnew GoshFixnum((int)arg->value);
		}
		else
		{
			return Convert::ChangeType((int)arg->value, type);
		}
	case METHOD_ARG_STRING:
		if(type->IsAssignableFrom(GoshString::typeid))
		{
			return gcnew GoshString((IntPtr)arg->ptr);
		}
		else
		{
			return Marshal::PtrToStringAnsi(IntPtr(arg->value));
		}
	case METHOD_ARG_CLROBJECT:
	default:
		return GCHandle::FromIntPtr(IntPtr(arg->ptr)).Target;
	}
}

static Object^ ToArgumentObject(MethodArg* arg)
{
	switch(arg->kind)
	{
	case METHOD_ARG_INT:
		return (int)arg->value;
	case METHOD_ARG_STRING:
		return Marshal::PtrToStringAnsi(IntPtr(arg->value));
	case METHOD_ARG_CLROBJECT:
	default:
		return GCHandle::FromIntPtr(IntPtr(arg->ptr)).Target;
	}
}

array<Object^>^ ClrMethod::ConstractArguments(MethodCandidate^ callMethod)
{
	int paramCount = callMethod->Parameters->Count;
	int paramStartIndex = _isStatic ? 0 : 1;
	if(callMethod->ParamsArgumentIndex != -1)
	{
		int aryCount = (paramCount - callMethod->ParamsArgumentIndex);
		int actualParamCount = paramCount - aryCount;

		array<Object^>^ actualArgs = gcnew array<Object^>(actualParamCount);
		int index = paramStartIndex;
		for(;index < callMethod->ParamsArgumentIndex; ++index)
		{
			actualArgs[index - paramStartIndex] = 
				ToArgumentObject(callMethod->Parameters[index]->Type, &_args[index - paramStartIndex]);
		}

		Array^ paramsAry = Array::CreateInstance(callMethod->ParamsElementType, aryCount);
		actualArgs[index - paramStartIndex] = paramsAry;
		for(int i = 0;index < paramCount; ++index, ++i)
		{
			paramsAry->SetValue(
				ToArgumentObject(callMethod->Parameters[index]->Type, &_args[index - paramStartIndex])
				, i);
		}

		return actualArgs;
	}
	else
	{
		array<Object^>^ arguments = gcnew array<Object^>(_numArg);
		for(int i = 0;i < _numArg;++i)
		{
			arguments[i] = ToArgumentObject(callMethod->Parameters[i + paramStartIndex]->Type, &_args[i]);
		}

		return arguments;
	}
}

bool ClrMethod::CreateArgTypes(StringBuilder^ builder, array<ArgType>^% argTypes)
{
	int startIndex = _isStatic ? 0 : 1;
	if(_methodSpec->numParamSpec >= 0)
	{
		argTypes = gcnew array<ArgType>(_methodSpec->numParamSpec + startIndex);
		for(int i = 0;i < _methodSpec->numParamSpec;++i)
		{
			TypeSpecToString(&(_methodSpec->paramSpec[i]), builder);
			argTypes[i + startIndex].type = ClrMethod::GetType(builder->ToString());
			argTypes[i + startIndex].kind = METHOD_ARG_CLROBJECT;
			argTypes[i + startIndex].attr = _methodSpec->paramSpec[i].attr;
			builder->Length = 0;
		}
		return true;
	}
	else
	{
		argTypes = gcnew array<ArgType>(_numArg + startIndex);
		for(int i = 0;i < _numArg;++i)
		{
			switch(_args[i].kind)
			{
			case METHOD_ARG_CLROBJECT:
				argTypes[i + startIndex].type = GCHandle::FromIntPtr(IntPtr(_args[i].ptr)).Target->GetType();
				argTypes[i + startIndex].kind = METHOD_ARG_CLROBJECT;
				argTypes[i + startIndex].attr = TYPESPEC_ATTR_NORMAL;
				break;
			case METHOD_ARG_INT:
				argTypes[i + startIndex].type = Int32::typeid;
				argTypes[i + startIndex].kind = METHOD_ARG_INT;
				argTypes[i + startIndex].attr = TYPESPEC_ATTR_NORMAL;
				break;
			case METHOD_ARG_STRING:
				argTypes[i + startIndex].type = String::typeid;
				argTypes[i + startIndex].kind = METHOD_ARG_STRING;
				argTypes[i + startIndex].attr = TYPESPEC_ATTR_NORMAL;
				break;
			}
		}
		return false;
	}
}

void* ClrMethod::CallNew()
{
	StringBuilder builder;
	TypeSpecToString((TypeSpec*)_methodSpec, %builder);
	Type^ targetType = ClrMethod::GetType(builder.ToString());
	if(targetType == nullptr)
	{
		throw gcnew ArgumentException("unknown type");
	}
	builder.Length = 0;

	//メソッド名と一緒にパラメータ型指定がある場合はtrue
	array<ArgType>^ argTypes;
	bool isSpecifyParamType = CreateArgTypes(%builder, argTypes);

	List<MethodBase^> candidates;
	for each(MethodBase^ info in targetType->GetConstructors(BindingFlags::Public | BindingFlags::Instance))
	{
		bool hasParams = false;
		array<ParameterInfo^>^ paramInfoAry = info->GetParameters();
		if(paramInfoAry->Length != 0)
		{
			hasParams = CompilerHelpers::IsParamArray(paramInfoAry[paramInfoAry->Length - 1]);
		}

		if(hasParams)
		{
			if(_numArg < paramInfoAry->Length - 1) continue;
		}
		else
		{
			if(_numArg != paramInfoAry->Length) continue;
		}
		candidates.Add(info);
	}

	if(candidates.Count == 0)
	{
		throw gcnew ArgumentException("Callable constructor can not be found");
	}

	MethodBinder mb(".ctor", %candidates, BinderType::Constractor);
	MethodCandidate^ mc = mb.MakeBindingTarget(CallType::None, _numArg, argTypes, isSpecifyParamType);
	if(mc == nullptr)
	{
		throw gcnew ArgumentException("Callable constructor can not be found");
	}

	array<Object^>^ arguments = ConstractArguments(mc);
	Object^ result = ((ConstructorInfo^)mc->Target->Method)->Invoke(arguments);
	return (void*)(IntPtr)GCHandle::Alloc(result);
}

void* ClrMethod::CallMethod()
{
	StringBuilder builder;
	Type^ targetType;
	Object^ instance;

	//実行メソッド検索に使用するパラメータの型配列
	array<ArgType>^ argTypes = nullptr;
	//メソッド名と一緒にパラメータ型指定がある場合はtrue
	bool isSpecifyParamType = _methodSpec->numParamSpec >= 0;
	//明示的なパラメータ型指定がある場合だけ、
	//プリミティブ型同士の演算で使用するためこのタイミングで作成する。
	if(isSpecifyParamType)
	{
		CreateArgTypes(%builder, argTypes);
	}

	bool isOperator = false;
	String^ method = gcnew String(_methodSpec->name);
	if(_isStatic)
	{
		TypeSpec* spec = (TypeSpec*)_obj;
		TypeSpecToString(spec, %builder);
		targetType = ClrMethod::GetType(builder.ToString());
		if(targetType == nullptr)
		{
			throw gcnew ArgumentException("unknown type");
		}

		builder.Length = 0;
	}
	else
	{
		GCHandle gchObj = GCHandle::FromIntPtr(IntPtr(_obj));
		instance = gchObj.Target;
		targetType = instance->GetType();

#pragma region プリミティブ型同士の演算子を実行する {
		if(_numArg < 2 && method->Length <= 2)
		{
			Object^ secondArg = (_numArg == 1) ?  ToArgumentObject(&_args[0]) : nullptr;

			Object^ result = nullptr;
			if(method == "+")
			{
				if(targetType->IsPrimitive
					&& (_numArg == 0 || secondArg->GetType()->IsPrimitive))
				{
					result = PrimitiveAdd(argTypes, instance, secondArg);
				}
				else if(targetType == String::typeid && _numArg == 1)
				{
					result = String::Concat(instance, secondArg);
				}
				else
				{
					method = (_numArg == 0) ? "op_UnaryPlus" : "op_Addition";
					isOperator = true;
				}
			}
			else if(method == "-")
			{
				if(targetType->IsPrimitive
					&& (_numArg == 0 || secondArg->GetType()->IsPrimitive))
				{
					result = PrimitiveSub(argTypes, instance, secondArg);
				}
				else
				{
					method = (_numArg == 0) ? "op_UnaryNegation" : "op_Subtraction";
					isOperator = true;
				}
			}
			else if(method == "*")
			{
				method = "op_Multiply";
				isOperator = true;
			}
			else if(method == "/")
			{
                method = "op_Division";
                isOperator = true;
			}
			else if(method == "%")
			{
                method = "op_Modulus";
                isOperator = true;
			}
			else if(method == "++")
			{
                method = "op_Increment";
                isOperator = true;
			}
			else if(method == "--")
			{
                method = "op_Decrement";
                isOperator = true;
			}
			else if(method == "&")
			{
                method = "op_BitwiseAnd";
                isOperator = true;
			}
			else if(method == "|")
			{
                method = "op_BitwiseOr";
                isOperator = true;
			}
			else if(method == "^")
			{
                method = "op_ExclusiveOr";
                isOperator = true;
			}
			else if(method == "<<")
			{
                method = "op_LeftShift";
                isOperator = true;
			}
			else if(method == ">>")
			{
                method = "op_RightShift";
                isOperator = true;
			}
			else if(method == "~")
			{
                method = "op_OnesComplement";
                isOperator = true;
			}
			else if(method == "!")
			{
				if(targetType->IsPrimitive)
				{
					result = PrimitiveNot(argTypes, instance, secondArg);
				}
				else
				{
	                method = "op_LogicalNot";
	                isOperator = true;
				}
			}
			else if(method == "==")
			{
                method = "op_Equality";
                isOperator = true;
			}
			else if(method == "!=")
			{
                method = "op_Inequality";
                isOperator = true;
			}
			else if(method == ">")
			{
                method = "op_GreaterThan";
                isOperator = true;
			}
			else if(method == "<")
			{
                method = "op_LessThan";
                isOperator = true;
			}
			else if(method == ">=")
			{
                method = "op_GreaterThanOrEqual";
                isOperator = true;
			}
			else if(method == "<=")
			{
                method = "op_LessThanOrEqual";
                isOperator = true;
			}

			if(result != nullptr)
			{
				return (void*)(IntPtr)GCHandle::Alloc(result);
			}
		}
#pragma endregion }
	}

	//明示的なパラメータ型指定がない場合はこのタイミングで
	//メソッド検索用引数型配列を作成する
	if(!isSpecifyParamType)
	{
		CreateArgTypes(%builder, argTypes);
	}
	if(!_isStatic)
	{
		argTypes[0].kind = METHOD_ARG_CLROBJECT;
		argTypes[0].type = targetType;
		argTypes[0].attr = TYPESPEC_ATTR_NORMAL;
	}

	//実行メソッドの候補リストを作成する
	List<MethodBase^> candidates;
	for each(MethodBase^ info in targetType->GetMember(method, MemberTypes::Method 
		, BindingFlags::Public | ((_isStatic | isOperator) ? BindingFlags::Static : BindingFlags::Instance)
		))
	{
		MethodBase^ candidate = CreateCandidate(info, argTypes);
		if(candidate != nullptr)
		{
			candidates.Add(candidate);
		}
	}

	if(candidates.Count == 0)
	{
		throw gcnew ArgumentException("Applicable method can not be found");
	}

	//パラメータ型配列から適用可能なメソッドを取得する
	MethodBinder mb(method, %candidates 
		, isOperator ? BinderType::BinaryOperator : BinderType::Normal);
	MethodCandidate^ mc = mb.MakeBindingTarget(
		_isStatic ? CallType::None : CallType::ImplicitInstance
		, (_isStatic ? 0 : 1) + _numArg
		, argTypes
		, isSpecifyParamType
		);
	if(mc == nullptr)
	{
		throw gcnew ArgumentException("Applicable method can not be found");
	}

	//メソッド実行
	array<Object^>^ arguments = ConstractArguments(mc);
	Object^ result = mc->Target->Method->Invoke(_isStatic ? nullptr : instance, arguments);
	//TODO 戻り値がvoidのメソッドと本当にnullが返ってきたときの場合わけ
	if(result == nullptr)
	{
		return 0;
	}
	else
	{
		return (void*)(IntPtr)GCHandle::Alloc(result);
	}
}