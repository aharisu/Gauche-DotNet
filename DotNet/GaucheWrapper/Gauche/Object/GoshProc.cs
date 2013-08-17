/*
 * GoshProc.cs
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
using GaucheDotNet.Native;

namespace GaucheDotNet
{
    public delegate object GoshFunc(params object[] args);

    public interface IIndexer<T>
    {
        T this[int index] { get; }
    }

    public class Indexer<T> : IIndexer<T>
    {
        private readonly IList<T> _list;

        public Indexer(IList<T> list)
        {
            this._list = list;
        }

        public T this[int index]
        {
            get
            {
                if (index < _list.Count)
                {
                    return _list[index];
                }
                else
                {
                    return default(T);
                }
            }
        }
    }

    public abstract class GoshProc : GoshObj
    {
        protected IntPtr _ptr;

        public GoshProc(IntPtr ptr)
        {
            _ptr = ptr;
        }

        public override IntPtr Ptr
        {
            get { return _ptr; }
        }

        public override object Object
        {
            get { return (GoshFunc)Apply; }
        }

        public virtual int Required
        {
            get
            {
                unsafe
                {
                    return (int)((ScmProcedure*)_ptr)->required;
                }
            }
        }

        public abstract object Apply(params object[] args);

        public abstract IIndexer<Type> ArgumentsType { get; }
        public abstract IIndexer<Type> ReturnsType { get; }
    }
}

