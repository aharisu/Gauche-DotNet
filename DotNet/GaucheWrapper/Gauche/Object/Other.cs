/*
 * Other.cs
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
using GaucheDotNet.Native;

namespace GaucheDotNet
{

    public class GoshGloc : GoshObj
    {
        protected readonly IntPtr _ptr;

        public GoshGloc(IntPtr ptr)
        {
            this._ptr = ptr;
        }

        public override IntPtr Ptr { get { return _ptr; } }
    }

    public class GoshEvalPacket : GoshRefObj
    {
        public static readonly int SizeOf = Marshal.SizeOf(typeof(ScmEvalPacket));

        public GoshEvalPacket(IntPtr ptr)
            : base(ptr)
        {
        }

        public GoshEvalPacket()
            : base(SizeOf)
        {
        }

        public int NumResults
        {
            get
            {
                unsafe
                {
                    return ((ScmEvalPacket*)_ptr)->numResults;
                }
            }
        }

        public GoshModule Module
        {
            get
            {
                unsafe
                {
                    return new GoshModule((IntPtr)((ScmEvalPacket*)_ptr)->module);
                }
            }
        }

        public GoshObj this[int index]
        {
            get
            {
                if (index < 0 || GoshInvoke.SCM_VM_MAX_VALUES <= index)
                {
                    throw new ArgumentOutOfRangeException("0 <= index < " + GoshInvoke.SCM_VM_MAX_VALUES);
                }

                unsafe
                {
                    ScmEvalPacket* packet = (ScmEvalPacket*)_ptr;

                    return new GoshRefObj((IntPtr)(((ScmEvalPacket*)_ptr)->results[index]));
                }
            }
        }

        public bool HasException
        {
            get
            {
                unsafe
                {
                    return (IntPtr) ((ScmEvalPacket*)_ptr)->exception != (IntPtr)GoshInvoke.SCM_FALSE;
                }
            }
        }

        public GoshCondition Exception
        {
            get
            {
                unsafe
                {
                    return new GoshCondition(((ScmEvalPacket*)_ptr)->exception);
                }
            }
        }

    }
}
