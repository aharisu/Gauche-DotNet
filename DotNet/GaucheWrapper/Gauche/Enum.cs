/*
 * Enum.cs
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
    public static partial class Gosh
    {

        #region {
        public enum CmpMode : int
        {
            Eq = 0,
            Eqv = 1,
            Equal = 2,
        }
        #endregion }

        #region string.h {

        [Flags]
        public enum StringFlags : int
        {
            Immutable = 1 << 0,
            Incomplete = 1 << 1,
            Terminated = 1 << 2,
            Copying = 1 << 16,
        }

        #endregion }

        #region module.h {

        public enum CompMode : int
        {
            Eq = 0,
            Eqv = 1,
            Equal = 2,
        }

        [Flags]
        public enum BindingFlag : int
        {
            None = 0,
            StayInModule = 1 << 0,
            Const = 1 << 1,
            Inlinable = 1 << 2,
        }

        public enum FindModuleFlag : int
        {
            /// <summary>
            /// Create if there's no named module
            /// </summary>
            Create = 1,
            /// <summary>
            /// Do not signal an error if there's no named module, but return NULL instead.
            /// </summary>
            Quiet = 2,
        }

        #endregion }
    }
}

