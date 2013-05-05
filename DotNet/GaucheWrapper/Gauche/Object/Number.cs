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

namespace GaucheDotNet
{
    #region GoshFixnum {
    public class GoshFixnum : GoshObj
    {
        private int _num;

		public int Num
		{
			get { return _num; }
			set { _num = value; }
		}

        public GoshFixnum(int num)
        {
            this._num = num;
        }

        public GoshFixnum(IntPtr ptr)
        {
#if X64
            Int64 num = ptr.ToInt64();
			this._num = (int)(num >> 2);
#else
            Int32 num = ptr.ToInt32();
			this._num = num >> 2;
#endif
        }

        public override IntPtr Ptr
        {
            get { return (IntPtr)((_num << 2) + 1); }
        }

        public override object Object
        {
            get
            {
                return _num;
            }
        }

        public override string ToString()
        {
            return _num.ToString();
        }
    }
    #endregion }
}
