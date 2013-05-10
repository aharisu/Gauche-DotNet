#pragma once

#include "Lock.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Reflection::Emit;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;
using namespace GaucheDotNet;
using namespace GaucheDotNet::Native;


public delegate Delegate^ DelegateCreator(GaucheDotNet::GoshProc^ proc, IntPtr key);

public ref class ClrStubConstant abstract sealed
{
public:
    static ClrStubConstant()
    {
        DelegateConstructorArgs = gcnew array<Type^>(2);
        DelegateConstructorArgs[0] = Object::typeid;
        DelegateConstructorArgs[1] = IntPtr::typeid;

        _asmBuilder = nullptr;
        _modBuilder = nullptr;

        GoshProcMethodInfo = (GaucheDotNet::GoshProc::typeid)->GetMethod("Apply"
            , BindingFlags::Public | BindingFlags::Instance
            );

        ObjectFinalizeMethodInfo = (Object::typeid)->GetMethod("Finalize"
            , BindingFlags::NonPublic | BindingFlags::Instance
            );

        UnregisterDelegateMethodInfo = (ClrStubConstant::typeid)->GetMethod("UnregisterDelegate"
            , BindingFlags::Public | BindingFlags::Static
            );
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

    static void RegisterDelegate(IntPtr key, Delegate^ d)
    {
        _delegateTable.Add(key, GCHandle::Alloc(d, GCHandleType::Weak));
    }

    static Delegate^ UnregisterDelegate(IntPtr key)
    {
        GCHandle ret;
        if(_delegateTable.TryGetValue(key, ret ))
        {
            _delegateTable.Remove(key);
            return (Delegate^)ret.Target;
        }
        else
        {
            return nullptr;
        }
    }

public: //static field
    static initonly array<Type^>^ DelegateConstructorArgs;

    static initonly MethodInfo^ ObjectFinalizeMethodInfo;
    static initonly MethodInfo^ GoshProcMethodInfo;
    static initonly MethodInfo^ UnregisterDelegateMethodInfo;

private:
    static AssemblyBuilder^ _asmBuilder;
    static ModuleBuilder^ _modBuilder;

    static Dictionary<Type^, DelegateCreator^> _typeToEventHandlerMap;
    static Dictionary<IntPtr, GCHandle> _delegateTable;
};