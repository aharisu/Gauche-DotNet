/*
 * WrapNativeStruct.cs
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GaucheDotNet.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmFlonum
    {
        public Double val;
    }


    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmInstance
    {
        public IntPtr tag;
        public IntPtr slots;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmObj
    {
        public IntPtr tag;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmPair
    {
        public IntPtr car;
        public IntPtr cdr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmExtendedPair
    {
        public IntPtr car;
        public IntPtr cdr;
        public IntPtr attributes;
    }


    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmEvalPacket
    {
#if X64
        public fixed UInt64 results[GoshInvoke.SCM_VM_MAX_VALUES];
#else
        public fixed UInt32 results[GoshInvoke.SCM_VM_MAX_VALUES];
#endif
        public Int32 numResults;
        public IntPtr exception;
        public ScmModule* module;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmStringBody
    {
        public UInt32 flags;
        public UInt32 length;
        public UInt32 size;
        public byte*  start;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmString
    {
        public IntPtr tag;
        public ScmStringBody* body;
        public ScmStringBody initialBody;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmSymbol
    {
        public IntPtr tag;
        public ScmString* name;
        public int flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ScmModule
    {
        public IntPtr tag;
        public IntPtr name;
        public IntPtr imported;
#if GAUCHE_9_3_3
        public IntPtr exported;
#endif
        public Int32 exportAll;
        public IntPtr parents;
        public IntPtr mpl;
        public IntPtr depended;
#if GAUCHE_9_3_3
        public IntPtr table; 
#else
        public IntPtr internalTable;
        public IntPtr externalTable;
#endif
        public IntPtr origin;
        public IntPtr prefix;
    }

    /// <param name="gloc">ScmGloc*</param>
    /// <returns>ScmObj</returns>
    public delegate IntPtr GlocGetter(IntPtr gloc);

    /// <param name="gloc">ScmGloc*</param>
    /// <param name="value">ScmObj</param>
    /// <returns>ScmObj</returns>
    public delegate IntPtr GlocSetter(IntPtr gloc, IntPtr value);

    //TODO 最新のGaucheでは構造が変わっている
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmGloc
    {
        public IntPtr tag;
        public IntPtr name;
        public ScmModule* module;
        public IntPtr value;
#if GAUCHE_9_3_3
        public byte exported;
#endif
        public byte hidden;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public GlocGetter getter;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public GlocSetter setter;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmGdnObject
    {
        public IntPtr tag;
        /// <summary>
        /// GCHandle pointer
        /// </summary>
        public IntPtr data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmProcedure
    {
        public ScmInstance hdr;
        public UInt16 required;
        public byte optional;
        /// <summary>
        /// type 3bit,
        /// locked 1bit,
        /// constant 1bit,
        /// reserved 1bit
        /// </summary>
        public byte flag;
        public IntPtr info;
        public IntPtr setter;
        public IntPtr inliner;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmClosure
    {
        public ScmProcedure common;
        /// <summary>
        /// ScmObj
        /// </summary>
        public IntPtr code;
        /// <summary>
        /// ScmEnvFrame*
        /// </summary>
        public IntPtr env;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmSubr
    {
        public ScmProcedure common;
        public int flags;
        public IntPtr func;
        public IntPtr data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ScmClrObject
    {
        public IntPtr tag;
        public IntPtr handle;
    }


}

