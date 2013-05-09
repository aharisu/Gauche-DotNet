#pragma once


using namespace System;
using namespace System::Threading;

ref class Lock
{
public:
    Lock(Object^ obj)
        :_obj(obj)
    {
        Monitor::Enter(_obj);
    }

    ~Lock()
    {
        Monitor::Exit(_obj);
    }

private:
    Object^ _obj;
};