#pragma once

#include "Lock.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Reflection::Emit;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;
using namespace GaucheDotNet;
using namespace GaucheDotNet::Native;


delegate Delegate^ DelegateCreator(GaucheDotNet::GoshProc^ proc);

ref class ClrStubConstant abstract sealed
{
public:
    static ClrStubConstant()
    {
        DelegateConstructorArgs = gcnew array<Type^>(2);
        DelegateConstructorArgs[0] = Object::typeid;
        DelegateConstructorArgs[1] = IntPtr::typeid;

        _asmBuilder = nullptr;
        _modBuilder = nullptr;

        _goshProcMethodInfo = nullptr;
    }

    static ModuleBuilder^ GetModuleBuilder()
    {
        if(_asmBuilder == nullptr)
        {
            AssemblyName asmName;
            asmName.Name = "ClrStubAutoGen";
            AppDomain^ domain = System::Threading::Thread::GetDomain();
            _asmBuilder = domain->DefineDynamicAssembly(%asmName, AssemblyBuilderAccess::Run);

            _modBuilder = _asmBuilder->DefineDynamicModule(asmName.Name, false);
        }

        return _modBuilder;
    }

    static MethodInfo^ GetGoshProcApply()
    {
        if(_goshProcMethodInfo == nullptr)
        {
            _goshProcMethodInfo = (GaucheDotNet::GoshProc::typeid)->GetMethod("Apply"
                , BindingFlags::Public | BindingFlags::Instance
                );
        }

        return _goshProcMethodInfo;
    }

    static void AddDelegateCreator(Type^ type, DelegateCreator^ creator)
    {
        //Lock statement
        Lock lock(%_typeToEventHandlerMap);

        _typeToEventHandlerMap[type] = creator;
    }

    static DelegateCreator^ GetDelegateCreator(Type^ type)
    {
        //Lock statement
        Lock lock(%_typeToEventHandlerMap);

        DelegateCreator^ creator = nullptr;
        _typeToEventHandlerMap.TryGetValue(type, creator);
        return creator;
    }

    static void RaiseClrError(String^ msg, Exception^ clrException)
    {
        GoshInvoke::Scm_Raise(
            GoshInvoke::Scm_MakeClrError(
                GoshInvoke::Scm_MakeString(msg, -1, -1, StringFlags::Copying)
                , GoshInvoke::Scm_MakeClrObject((IntPtr)GCHandle::Alloc(clrException))));
    }

    static initonly array<Type^>^ DelegateConstructorArgs;
private:
    static AssemblyBuilder^ _asmBuilder;
    static ModuleBuilder^ _modBuilder;
    static MethodInfo^ _goshProcMethodInfo;
    static Dictionary<Type^, DelegateCreator^> _typeToEventHandlerMap;
};