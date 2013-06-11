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
#include "ClrStubConstant.h"
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
        , ObjWrapper* args, int numArg)
        :_methodSpec(methodSpec)
        ,_obj(obj)
        ,_isStatic(isStatic)
        ,_args(args)
        ,_numArg(numArg)
    {}

    void* CallNew();
    void* CallMethod(void* module);

    bool CreateArgTypes(StringBuilder^ builder, array<ArgType>^% argTypes);
    array<Object^>^ ConstractArguments(MethodCandidate^ callMethod, bool callStatic);

    static ClrMethod()
    {
        _extensionMethods = gcnew Dictionary<String^, Dictionary<String^, List<MethodInfo^>^>^>();
        _eachModuleExtensionMethods = gcnew Dictionary<IntPtr, Dictionary<String^, List<MethodInfo^>^>^>();
        _namespaceToModuleDict = gcnew Dictionary<String^, List<IntPtr>^>();
        _extensionsAttributeType = nullptr;

        //今後アセンブリロード後に実行されるメソッドを登録する
        AppDomain::CurrentDomain->AssemblyLoad += gcnew AssemblyLoadEventHandler(AssemblyLoadEvent);

        //現時点で読み込まれているアセンブリについて
        for each(Assembly^ a in AppDomain::CurrentDomain->GetAssemblies())
        {
            LoadExtensionMethod(a);
        }
    }

    //アセンブリロード後に実行されるメソッド
    static void AssemblyLoadEvent(Object^ sender, AssemblyLoadEventArgs^ args)
    {
        LoadExtensionMethod(args->LoadedAssembly);
    }

    static void LoadExtensionMethod(Assembly^ assembly)
    {
        String^ name = assembly->FullName->Split(',')[0];
        //確実に拡張メソッドが存在しないアセンブリはこの後の処理を全てスキップ
        if(name == "mscorlib" || name == "System" 
            || name == "System.Windows.Forms" || name == "System.Drawing"
            || name == "GaucheDotNet" || name == "ClrStub" || name == "GaucheWrapper"
            )
        {
            return;
        }

        //拡張メソッドマーク用属性クラスを取得する
        if(_extensionsAttributeType == nullptr)
        {
            _extensionsAttributeType = assembly->GetType("System.Runtime.CompilerServices.ExtensionAttribute");
        }
        //拡張メソッドマーク属性をまだ持っていない場合は処理を終了
        if(_extensionsAttributeType == nullptr) return;

        String^ previousNamespace = nullptr;
        List<IntPtr>^ moduleList = nullptr;

        for each(Type^ t in assembly->GetExportedTypes())
        {
            //対象クラスがC#レベルでstaticクラスとして宣言されていれば
            if(t->IsAbstract && t->IsSealed)
            {
                Dictionary<String^, List<MethodInfo^>^>^ nameToMethodDict = nullptr;
                //拡張メソッドの可能性があるpublicでstaticなメソッドを全てについて
                for each(MethodInfo^ m in t->GetMethods(BindingFlags::Public | BindingFlags::Static))
                {
                    //拡張メソッドマーク属性がついていれば
                    if(m->IsDefined(_extensionsAttributeType, false))
                    {
                        //すでに対象名前空間をusing済みのGaucheモジュールに対して、参照可能メソッドを追加する
                        if(previousNamespace != t->Namespace)
                        {
                            previousNamespace = t->Namespace;
                            moduleList = nullptr;
                            _namespaceToModuleDict->TryGetValue(previousNamespace, moduleList);
                        }
                        if(moduleList != nullptr)
                        {
                            for each(IntPtr module in moduleList)
                            {
                                Dictionary<String^, List<MethodInfo^>^>^ eachModuleMethods = _eachModuleExtensionMethods[module];

                                List<MethodInfo^>^ methods;
                                if(!eachModuleMethods->TryGetValue(m->Name, methods))
                                {
                                    methods = gcnew List<MethodInfo^>();
                                    eachModuleMethods[m->Name] = methods;
                                }
                                methods->Add(m);
                            }
                        }

                        //メソッド名からメソッドを参照するための辞書を作成して、拡張メソッドを追加する
                        if(nameToMethodDict == nullptr &&
                            !_extensionMethods->TryGetValue(t->Namespace, nameToMethodDict))
                        {
                            nameToMethodDict = gcnew Dictionary<String^, List<MethodInfo^>^>();
                            _extensionMethods[t->Namespace] = nameToMethodDict;
                        }

                        List<MethodInfo^>^ methods;
                        if(!nameToMethodDict->TryGetValue(m->Name, methods))
                        {
                            methods = gcnew List<MethodInfo^>();
                            nameToMethodDict[m->Name] = methods;
                        }

                        methods->Add(m);
                    }
                }
            }
        }
    }

public:
    static Object^ ToObject(ObjWrapper* obj);
    static Type^ GetType(String^ name, bool valid);
    static Type^ TypeSpecToType(TypeSpec* spec);

    static void* CallNew(TypeSpec* methodSpec, ObjWrapper* args, int numArg)
    {
        ClrMethod method(methodSpec, 0, true,  args, numArg);
        try
        {
            return method.CallNew();
        }
        catch (Exception^ e)
        {
            ClrStubConstant::RaiseClrError(e);
            //does not reach
            return 0;
        }
    }

    static void* CallMethod(void* module, TypeSpec* methodSpec, void* obj, bool isStatic, ObjWrapper* args, int numArg)
    {
        ClrMethod method(methodSpec, obj, isStatic,  args, numArg);
        try
        {
            return method.CallMethod(module);
        } 
        catch (Exception^ e)
        {
            ClrStubConstant::RaiseClrError(e);
            //does not reach
            return 0;
        }
    }

    static void ModuleUsingNamespace(IntPtr module, String^ ns)
    {
        //モジュールが参照可能な拡張メソッドの辞書オブジェクトを取得
        Dictionary<String^, List<MethodInfo^>^>^ eachModuleMethods;
        if(!_eachModuleExtensionMethods->TryGetValue(module, eachModuleMethods))
        {
            eachModuleMethods = gcnew Dictionary<String^, List<MethodInfo^>^>();
            _eachModuleExtensionMethods[module] = eachModuleMethods;
        }

        //すでに読み込んでいるアセンブリ内に存在する、今回追加した名前空間に属する拡張メソッドを、
        //モジュール別参照可能メソッドリストに追加する
        Dictionary<String^, List<MethodInfo^>^>^ nameToMethodDict;
        if(_extensionMethods->TryGetValue(ns, nameToMethodDict))
        {
            for each(KeyValuePair<String^, List<MethodInfo^>^>^ kv in nameToMethodDict)
            {
                List<MethodInfo^>^ methods;
                if(!eachModuleMethods->TryGetValue(kv->Key, methods))
                {
                    methods = gcnew List<MethodInfo^>();
                    eachModuleMethods[kv->Key] = methods;
                }

                methods->AddRange(kv->Value);
            }
        }

        //後で追加されたアセンブリ内にある拡張メソッドもモジュール別参照可能メソッドリストに追加できるように
        //モジュールがusingしている名前空間とキーとなるモジュールのポインタを保持しておく
        List<IntPtr>^ moduleList;
        if(!_namespaceToModuleDict->TryGetValue(ns, moduleList))
        {
            moduleList = gcnew List<IntPtr>();
            _namespaceToModuleDict[ns] = moduleList;
        }
        moduleList->Add(module);
    }

private:
    initonly TypeSpec* _methodSpec;
    initonly void* _obj;
    initonly bool _isStatic;
    ObjWrapper* _args;
    initonly int _numArg;

    //namespace -> (MethodName -> MethodList)
    static initonly Dictionary<String^, Dictionary<String^, List<MethodInfo^>^>^>^ _extensionMethods;
    //GaucheModulePointer -> (MethodName -> MethodList)
    static initonly Dictionary<IntPtr, Dictionary<String^, List<MethodInfo^>^>^>^ _eachModuleExtensionMethods;
    //namespace -> GaucheModulePointer List
    static initonly Dictionary<String^, List<IntPtr>^>^ _namespaceToModuleDict;

    static Type^ _extensionsAttributeType;
};

