#pragma once

using namespace System;

Delegate^ GetWrappedDelegate(Type^ type, GaucheDotNet::GoshProc^ proc, IntPtr delegateTableKey);