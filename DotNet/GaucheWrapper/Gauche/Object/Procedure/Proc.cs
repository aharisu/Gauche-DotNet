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

namespace GaucheDotNet.Procedure
{
    public class GoshProcedure : GoshProc
    {
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
                //TODO
                //throw packet.Exception;
                throw new Exception(packet.Exception.ConditionTypeName + ":" + packet.Exception.Message);
            }

            //TODO Values
            return packet[0].Object;
        }

    }

}

