/*
 * GoshObj.cs
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
    /// <summary>
    /// Gaucheで定義された構造体をラップするクラス群のトップレベルクラス
    /// </summary>
    public abstract class GoshObj
    {
        public abstract IntPtr Ptr { get; }

        /// <summary>
        /// GoshObj型のオブジェクトのダウンキャストを行い、特定化したGoshObjのサブクラスに変換します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual GoshObj Specify { get { return this; } }

        public virtual object Object { get { return this; } }

        public T To<T>()
        {
            if (typeof(T) == this.GetType())
            {
                return (T)(object)this;
            }
            else if (typeof(T).IsSubclassOf(typeof(GoshObj)))
            {
                return (T)(object)Specify;
            }
            else
            {
                return (T)Object;
            }
        }

    }

}

