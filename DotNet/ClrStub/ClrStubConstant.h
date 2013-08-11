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

        _memberKindType = IntPtr::Zero;
        _memberKindEvent = IntPtr::Zero;
        _memberKindField = IntPtr::Zero;
        _memberKindProperty = IntPtr::Zero;
        _memberKindMethod = IntPtr::Zero;
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

    static void RaiseClrError(Exception^ clrException)
    {
        GoshInvoke::Scm_Raise(
            GoshInvoke::Scm_MakeClrError(
                GoshInvoke::Scm_MakeString(clrException->Message, StringFlags::Copying)
                , GoshInvoke::Scm_MakeClrObject((IntPtr)GCHandle::Alloc(clrException))));
    }

    static void RaiseClrError(String^ msg, Exception^ clrException)
    {
        GoshInvoke::Scm_Raise(
            GoshInvoke::Scm_MakeClrError(
                GoshInvoke::Scm_MakeString(msg, StringFlags::Copying)
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

#pragma region MemberKind getter {

    static property IntPtr MemberKindType
    {
        IntPtr get()
        {
            if(_memberKindType == IntPtr::Zero)
            {
                _memberKindType = GoshInvoke::Scm_MakeKeyword(
                    GoshInvoke::Scm_MakeString("type", StringFlags::Copying));
            }

            return _memberKindType;
        }
    }

    static property IntPtr MemberKindEvent
    {
        IntPtr get()
        {
            if(_memberKindEvent == IntPtr::Zero)
            {
                _memberKindEvent = GoshInvoke::Scm_MakeKeyword(
                    GoshInvoke::Scm_MakeString("event", StringFlags::Copying));
            }

            return _memberKindEvent;
        }
    }

    static property IntPtr MemberKindField
    {
        IntPtr get()
        {
            if(_memberKindField == IntPtr::Zero)
            {
                _memberKindField = GoshInvoke::Scm_MakeKeyword(
                    GoshInvoke::Scm_MakeString("field", StringFlags::Copying));
            }

            return _memberKindField;
        }
    }

    static property IntPtr MemberKindProperty
    {
        IntPtr get()
        {
            if(_memberKindProperty == IntPtr::Zero)
            {
                _memberKindProperty = GoshInvoke::Scm_MakeKeyword(
                    GoshInvoke::Scm_MakeString("property", StringFlags::Copying));
            }

            return _memberKindProperty;
        }
    }

    static property IntPtr MemberKindMethod
    {
        IntPtr get()
        {
            if(_memberKindMethod == IntPtr::Zero)
            {
                _memberKindMethod = GoshInvoke::Scm_MakeKeyword(
                    GoshInvoke::Scm_MakeString("method", StringFlags::Copying));
            }

            return _memberKindMethod;
        }
    }

#pragma endregion }

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

    static IntPtr _memberKindType;
    static IntPtr _memberKindEvent;
    static IntPtr _memberKindField;
    static IntPtr _memberKindProperty;
    static IntPtr _memberKindMethod;
};