﻿/*
 * Cast.cs
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
    public static partial class Cast
    {
        public static GoshObj Specify(IntPtr ptr)
        {
#if X64
            Int64 num = ptr.ToInt64();
#else
            Int32 num = ptr.ToInt32();
#endif
            if ((num & 0x03) == 1)
            {
                return new GoshFixnum((int)(num >> 2));
            }
            else if (num == GoshInvoke.SCM_FALSE)
            {
                return GoshBool.False;
            }
            else if (num == GoshInvoke.SCM_TRUE)
            {
                return GoshBool.True;
            }
            else if (num == GoshInvoke.SCM_NIL)
            {
                return GoshNIL.NIL;
            }
            else if (num == GoshInvoke.SCM_EOF)
            {
                return GoshEOF.EOF;
            }
            else if (num == GoshInvoke.SCM_UNDEFINED)
            {
                return GoshUndefined.Undefined;
            }
            else if (num == GoshInvoke.SCM_UNBOUND)
            {
                return GoshUnbound.Unbound;
            }
            else
            {
                switch ((KnownClass)GoshInvoke.Scm_IsKnownType(ptr))
                {
                    case KnownClass.Pair:
                        return new GoshPair(ptr);

                    case KnownClass.String:
                        return new GoshString(ptr);

                    case KnownClass.Symbol:
                        return new GoshSymbol(ptr);

                    case KnownClass.Closure:
                    case KnownClass.Method:
                    case KnownClass.Generic:
                    case KnownClass.NextMethod:
                        return new Procedure.Procedure(ptr);

                    //TODO
                    case KnownClass.Subr:
                        unsafe
                        {
                            ScmSubr* subr = (ScmSubr*)ptr;
                            GoshInvoke.ScmSubProc func = (GoshInvoke.ScmSubProc)Marshal.GetDelegateForFunctionPointer(subr->func, typeof(GoshInvoke.ScmSubProc));
                            if (func.Target == null)
                            {
                                return new Procedure.Procedure(ptr);
                            }
                            else
                            {
                                return (GoshProc)func.Target;
                            }
                        }

                    case KnownClass.ClrObject:
                        return new GoshClrObject(ptr);

                    //TODO convert known class ...

                    default:
                        throw new InvalidCastException("unknown type");
                }
            }
        }

        public static object ToObj(IntPtr ptr)
        {
#if X64
            Int64 num = ptr.ToInt64();
#else
            Int32 num = ptr.ToInt32();
#endif
            if ((num & 0x03) == 1)
            {
                return num >> 2;
            }
            else if (num == GoshInvoke.SCM_FALSE)
            {
                return false;
            }
            else if (num == GoshInvoke.SCM_TRUE)
            {
                return true;
            }
            else if (num == GoshInvoke.SCM_NIL)
            {
                return GoshNIL.NIL;
            }
            else if (num == GoshInvoke.SCM_EOF)
            {
                return GoshEOF.EOF;
            }
            else if (num == GoshInvoke.SCM_UNDEFINED)
            {
                return GoshUndefined.Undefined;
            }
            else if (num == GoshInvoke.SCM_UNBOUND)
            {
                return GoshUnbound.Unbound;
            }
            else
            {
                switch((KnownClass)GoshInvoke.Scm_IsKnownType(ptr))
                {
                    case KnownClass.Pair:
                        return new GoshPair(ptr);

                    case KnownClass.String:
                        return GoshInvoke.Scm_GetStringConst(ptr);

                    case KnownClass.Symbol:
                        return new GoshSymbol(ptr);

                    case KnownClass.Closure:
                    case KnownClass.Method:
                    case KnownClass.Generic:
                    case KnownClass.NextMethod:
                        return (GoshFunc)new Procedure.Procedure(ptr).Apply;

                        //TODO
                    case KnownClass.Subr:
                        unsafe 
                        {
                            ScmSubr* subr = (ScmSubr*)ptr;
                            GoshInvoke.ScmSubProc func = (GoshInvoke.ScmSubProc)Marshal.GetDelegateForFunctionPointer(subr->func, typeof(GoshInvoke.ScmSubProc));
                            if (func.Target == null)
                            {
                                return (GoshFunc)new Procedure.Procedure(ptr).Apply;
                            }
                            else
                            {
                                return (GoshFunc)func.Target;
                            }
                        }

                    case KnownClass.ClrObject:
                        return GCHandle.FromIntPtr(ptr).Target;

                    //TODO convert known class ...

                    default:
                        return new GoshRefObj(ptr);
                }
            }
        }

#if X64
        private const int SCM_SMALL_INT_SIZE = 8* 8 - 3;
#else
        private const int SCM_SMALL_INT_SIZE = 4 * 8 - 3;
#endif
        private const int SCM_SMALL_INT_MAX = ((1 << SCM_SMALL_INT_SIZE) - 1);
        private const int SCM_SMALL_INT_MIN = (-SCM_SMALL_INT_MAX - 1);

        public static IntPtr ToIntPtr(object obj)
        {
            if (obj is GoshObj)
            {
                return ((GoshObj)obj).Ptr;
            }
            else if (obj is bool)
            {
                return (IntPtr)(((bool)obj) ? GoshInvoke.SCM_TRUE : GoshInvoke.SCM_FALSE);
            }
            else if (obj is Int32)
            {
                Int32 num = (Int32)obj;
                if (num >= SCM_SMALL_INT_MIN && num <= SCM_SMALL_INT_MAX)
                {
                    return (IntPtr)((num << 2) + 1);
                }
            }
            else if (obj is UInt32)
            {
                UInt32 num = (UInt32)obj;
                if (num <= SCM_SMALL_INT_MAX)
                {
                    return (IntPtr)((num << 2) + 1);
                }
            }
            else if (obj is Int16)
            {
                return (IntPtr)(((Int16)obj << 2) + 1);
            }
            else if (obj is UInt16)
            {
                return (IntPtr)(((UInt16)obj << 2) + 1);
            }
            else if (obj is Int64)
            {
                Int64 num = (Int64)obj;
                if (num >= SCM_SMALL_INT_MIN && num <= SCM_SMALL_INT_MAX)
                {
                    return (IntPtr)((num << 2) + 1);
                }
            }
            else if (obj is UInt64)
            {
                UInt64 num = (UInt64)obj;
                if (num <= (UInt64)SCM_SMALL_INT_MAX)
                {
                    return (IntPtr)((num << 2) + 1);
                }
            }

            GCHandle handle = GCHandle.Alloc(obj);
            return GoshInvoke.Scm_MakeClrObject((IntPtr)handle);
        }

    }
}

