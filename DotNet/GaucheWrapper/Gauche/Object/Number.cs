/*
 * Number.cs
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

    public class GoshNumber : GoshObj
    {
        private GoshObj _specific = null;
        protected readonly IntPtr _ptr;

        public virtual double Double
        {
            get { return GoshInvoke.Scm_GetDouble(_ptr);}
        }

        public virtual Int64 Int64
        {
            get
            {
                return GoshInvoke.Scm_GetInteger64Clamp(_ptr, ClampMode.None, IntPtr.Zero);
            }
        }

        public virtual int Int
        {
            get 
            { 
                return GoshInvoke.Scm_GetInteger32Clamp(_ptr, ClampMode.None, IntPtr.Zero);
            }
        }

        public GoshNumber(IntPtr ptr)
        {
            this._ptr = ptr;
        }

        public override IntPtr Ptr
        {
            get { return _ptr; }
        }

        public override object Object
        {
            get { return Double; }
        }

        public override GoshObj Specify
        {
            get 
            {
                if (_specific == null)
                {
                    _specific = Cast.Specify(_ptr); 
                }

                return _specific;
            }
        }

        public override string  ToString()
        {
            return GoshInvoke.Scm_GetString(
                GoshInvoke.Scm_NumberToString(_ptr, 10, NumberFormat.None));
        }
    }

    public class GoshCompnum : GoshNumber
    {
        public GoshCompnum(IntPtr ptr)
            : base(ptr)
        {
        }
    }

    public class GoshFlonum : GoshCompnum
    {
        public override double Double
        {
            get { return Cast.ScmFlonumToDouble(_ptr); }
        }

        public GoshFlonum(IntPtr ptr)
            :base(ptr)
        {
        }

        public GoshFlonum(double val)
            : base(GoshInvoke.Scm_MakeFlonum(val))
        {
        }
    }

    public class GoshRatnum : GoshFlonum
    {
        public override double Double
        {
            get { return GoshInvoke.Scm_GetDouble(_ptr);}
        }

        public GoshRatnum(IntPtr ptr)
            :base(ptr)
        {
        }
    }

    public class GoshInteger : GoshRatnum
    {
        public override double Double
        {
            get { return GoshInvoke.Scm_BignumToDouble(_ptr); }
        }

        public override Int64 Int64
        {
            get
            {
                bool oor;
                return GoshInvoke.Scm_BignumToSI64(_ptr, ClampMode.None, out oor);
            }
        }

        public override int Int
        {
            get 
            { 
                bool oor;
                return GoshInvoke.Scm_BignumToSI(_ptr, ClampMode.None, out oor);
            }
        }

        public GoshInteger(IntPtr ptr)
            :base(ptr)
        {
        }

        public override object Object
        {
            get { return Int64; }
        }

        public override string ToString()
        {
            return GoshInvoke.Scm_GetString(
                GoshInvoke.Scm_BignumToString(_ptr, 10, 1));
        }
    }

    public class GoshFixnum : GoshInteger
    {
        public override double Double
        {
            get { return Int; }
        }

        public override long Int64
        {
            get { return Int; }
        }

        public override int Int
        {
            get { return Cast.ScmFixnumToInt(_ptr); }
        }

        public GoshFixnum(int num)
            :base(Cast.IntToScmFixnum(num))
        {
        }

        public GoshFixnum(IntPtr ptr)
            :base(ptr)
        {
        }

        public override object Object
        {
            get { return this.Int; }
        }

        public override string ToString()
        {
            return Cast.ScmFixnumToInt(_ptr).ToString();
        }
    }

}

