﻿/*
 * Util.cs
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

namespace GaucheDotNet.Procedure
{
    static class Util
    {
        public static T DownCast<T>(object obj, bool callFromGauche)
        {
            try
            {
                Type a1Type = typeof(T);
                if (a1Type.IsValueType)
                {
                    return (T)Convert.ChangeType(obj, a1Type);
                }
                else
                {
                    return (T)obj;
                }
            }
            catch (InvalidCastException e)
            {
                if (callFromGauche)
                {
                    //TODO Gauche Error
                }

                throw e;
            }
        }

        public static object DownCast(Type castTo, object obj, bool callFromGauche)
        {
            try
            {
                if (castTo.IsValueType)
                {
                    return Convert.ChangeType(obj, castTo);
                }
                else if (castTo.IsAssignableFrom(obj.GetType()))
                {
                    return obj;
                }
                else
                {
                    throw new InvalidCastException("can not cast from "
                        + obj.ToString() + "<#" + obj.GetType().Name + "> to <#" +  castTo.Name + ">");
                }
            }
            catch (InvalidCastException e)
            {
                if (callFromGauche)
                {
                    //TODO Gauche Error
                }

                throw e;
            }
        }

    }
}

