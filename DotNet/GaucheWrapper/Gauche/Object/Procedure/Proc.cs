/*
 * Proc.cs
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
using System.Runtime.InteropServices;

namespace GaucheDotNet.Procedure
{
    public class GoshProcedure : GoshProc
    {
        private readonly static IIndexer<Type> _fixedIndexer = new FixedIndexer();

        public GoshProcedure(IntPtr ptr)
            : base(ptr)
        {
        }

        public override object Apply(params object[] args)
        {
            IntPtr pair = (IntPtr)GoshInvoke.SCM_NIL;
            for (int i = args.Length - 1; i >= 0; --i)
            {
                pair = GoshInvoke.Scm_Cons(Cast.ToIntPtr(args[i]), pair);
            }

            GoshEvalPacket packet = new GoshEvalPacket();
            if (GoshInvoke.Scm_Apply(_ptr, pair, packet.Ptr) < 0)
            {
                throw new GoshException(packet.Exception.ToString());
            }

            //TODO Values
            return packet[0].Object;
        }

        public override IIndexer<Type> ArgumentsType
        {
            get { return _fixedIndexer; }
        }

        public override IIndexer<Type> ReturnsType
        {
            get { return _fixedIndexer; }
        }

        private sealed class FixedIndexer : IIndexer<Type>
        {
            #region IIndexer<Type> メンバ

            public Type this[int index]
            {
                get { return typeof(Object); }
            }

            #endregion
        }

    }

    public class GoshTypedProcedure : GoshProc
    {
        private IIndexer<Type> _argTypeIndexer;
        private IIndexer<Type> _retTypeIndexer;

        public GoshTypedProcedure(IntPtr ptr)
            : base(ptr)
        {
            InitTypeAry();
        }

        public override object Apply(params object[] args)
        {
            IntPtr pair = (IntPtr)GoshInvoke.SCM_NIL;
            for (int i = args.Length - 1; i >= 0; --i)
            {
                Type argType = _argTypeIndexer[i];
                Object arg = args[i];
                if(argType != null && argType != typeof(Object))
                {
                    arg = Util.DownCast(argType, arg, false);
                }

                pair = GoshInvoke.Scm_Cons(Cast.ToIntPtr(arg), pair);
            }

            GoshEvalPacket packet = new GoshEvalPacket();
            if (GoshInvoke.Scm_Apply(_ptr, pair, packet.Ptr) < 0)
            {
                throw new GoshException(packet.Exception.ToString());
            }

            //TODO Values
            Type retType = _retTypeIndexer[0];
            if (retType != null && retType != typeof(Object))
            {
                return Util.DownCast(retType, packet[0].Object, false);
            }
            else
            {
                return packet[0].Object;
            }
        }

        public override IIndexer<Type> ArgumentsType
        {
            get { return _argTypeIndexer; }
        }

        public override IIndexer<Type> ReturnsType
        {
            get { return _retTypeIndexer; }
        }


        private unsafe void InitTypeAry()
        {
            ScmTypedClosure* c = (ScmTypedClosure*)_ptr;

            Type[] argTypeAry = new Type[c->numArgTypeSpec];
            for (int i = 0; i < c->numArgTypeSpec; ++i)
            {
                void* ptr = c->argTypeAry[i];
                argTypeAry[i] = (Type)((GCHandle)(IntPtr)ptr).Target;
            }
            _argTypeIndexer = new Indexer<Type>(argTypeAry);

            Type[] retTypeAry = new Type[c->numRetTypeSpec];
            for (int i = 0; i < c->numRetTypeSpec; ++i)
            {
                void* ptr = c->retTypeAry[i];
                retTypeAry[i] = (Type)((GCHandle)(IntPtr)ptr).Target;
            }
            _retTypeIndexer = new Indexer<Type>(retTypeAry);
        }

    }

}

