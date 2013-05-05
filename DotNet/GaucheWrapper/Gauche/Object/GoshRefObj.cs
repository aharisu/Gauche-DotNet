/*
 * GoshRefObj.cs
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

namespace GaucheDotNet
{
    public class GoshRefObj : GoshObj, IDisposable
    {
        protected readonly IntPtr _ptr;
        private GoshObj _specific;

        public bool IsDispose { get; protected set; }

        private bool _allocMemory = false;

        public GoshRefObj(IntPtr ptr)
        {
            this._ptr = ptr;
        }

        public GoshRefObj(int size)
        {
            this._ptr = AllocMemory(size);
        }

        ~GoshRefObj()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDispose)
            {
                if (disposing)
                {
                }

                if (_allocMemory)
                {
                    Marshal.FreeHGlobal(_ptr);

                    _allocMemory = false;
                }

                IsDispose = true;
            }
        }

        private IntPtr AllocMemory(int size)
        {
            _allocMemory = true;

            return Marshal.AllocHGlobal(size);
        }

        public override IntPtr Ptr
        {
            get { return _ptr; }
        }


        public override GoshObj Specify
        {
            get
            {
                if (_specific == null)
                {
                    _specific = Cast.Specify(this.Ptr);
                }

                return _specific;
            }
        }

        public override object Object
        {
            get
            {
                return Specify.Object;
            }
        }

    }
}
