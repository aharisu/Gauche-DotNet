/*
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
        public static IntPtr IntToScmFixnum(int num)
        {
            return (IntPtr)((num << 2) + 1); 
        }

        public static int ScmFixnumToInt(IntPtr ptr)
        {
#if X64
            return (int)(ptr.ToInt64() >> 2);
#else
            return ((int)(ptr.ToInt32() >> 2));
#endif
        }

        public static double ScmFlonumToDouble(IntPtr ptr)
        {
            unsafe
            {
#if X64
                return ((Native.ScmFlonum*)(ptr.ToInt64() & ~0x07))->val;
#else
                return ((Native.ScmFlonum*)(ptr.ToInt32() & ~0x07))->val;
#endif
            }
        }

        public static Int32 ScmCharToChar(IntPtr ptr)
        {
            return (Int32)((UInt32)ptr.ToInt32()) >> 8;
        }

        public static IntPtr CharToScmChar(Int32 ch)
        {
            return (IntPtr)((((UInt32)ch) << 8) + 3);
        }

        public static GoshObj Specify(IntPtr ptr)
        {
#if X64
            Int64 num = ptr.ToInt64();
#else
            Int32 num = ptr.ToInt32();
#endif
            if ((num & 0x03) == 1)
            {
                return new GoshFixnum(ScmFixnumToInt(ptr));
            }
            else if ((num & 0x03) == 2)
            {
                return new GoshFlonum(ptr);
            }
            else if ((num & 0xff) == 3)
            {
                return new GoshChar(ScmCharToChar(ptr));
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

                    case KnownClass.Keyword:
                        return new GoshKeyword(ptr);

                    case KnownClass.HashTable:
                        return new GoshHashTable(ptr);

                    case KnownClass.Closure:
                    case KnownClass.Method:
                    case KnownClass.Generic:
                    case KnownClass.NextMethod:
                        return new Procedure.GoshProcedure(ptr);

                    case KnownClass.Subr:
                        unsafe
                        {
                            ScmSubr* subr = (ScmSubr*)ptr;
                            GoshInvoke.ScmSubProc func = (GoshInvoke.ScmSubProc)Marshal.GetDelegateForFunctionPointer(subr->func, typeof(GoshInvoke.ScmSubProc));
                            if (func.Target == null)
                            {
                                return new Procedure.GoshProcedure(ptr);
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
                return Cast.ScmFixnumToInt(ptr);
            }
            else if ((num & 0x03) == 2)
            {
                return new GoshFlonum(ptr);
            }
            else if ((num & 0xff) == 3)
            {
                return Cast.ScmCharToChar(ptr);
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
                switch ((KnownClass)GoshInvoke.Scm_IsKnownType(ptr))
                {
                    case KnownClass.Pair:
                        return new GoshPair(ptr);

                    case KnownClass.String:
                        return GoshInvoke.Scm_GetStringConst(ptr);

                    case KnownClass.Symbol:
                        return new GoshSymbol(ptr);

                    case KnownClass.Keyword:
                        return new GoshKeyword(ptr);

                    case KnownClass.HashTable:
                        return GoshHashTable.ToHashtable(ptr);

                    case KnownClass.Closure:
                    case KnownClass.Method:
                    case KnownClass.Generic:
                    case KnownClass.NextMethod:
                        return (GoshFunc)new Procedure.GoshProcedure(ptr).Apply;

                    case KnownClass.Subr:
                        unsafe
                        {
                            ScmSubr* subr = (ScmSubr*)ptr;
                            GoshInvoke.ScmSubProc func = (GoshInvoke.ScmSubProc)Marshal.GetDelegateForFunctionPointer(subr->func, typeof(GoshInvoke.ScmSubProc));
                            if (func.Target == null)
                            {
                                return (GoshFunc)new Procedure.GoshProcedure(ptr).Apply;
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

        /// <summary>
        /// ptrの指すオブジェクトがGoshClrObjectであれば.Netのオブジェクトを取得します。
        /// それ以外のオブジェクトの場合はSpecifyしたGoshObjを返します
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static object ToClrObject(IntPtr ptr)
        {
            object obj =  Cast.Specify(ptr);
            GoshClrObject clrObj = obj as GoshClrObject;
            if (clrObj != null)
            {
                obj = clrObj.Object;
            }
            return obj;
        }

        /// <summary>
        /// GoshObjであればそのポインタ、.NetのオブジェクトならGoshClrObjectを作成してそのポインタを返す
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IntPtr ToGoshObjPtr(object obj)
        {
            IntPtr ret;
            GoshObj goshObj = obj as GoshObj;
            if (goshObj == null)
            {
                ret = GoshInvoke.Scm_MakeClrObject((IntPtr)GCHandle.Alloc(obj));
            }
            else
            {
                ret = goshObj.Ptr;
            }
            return ret;
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
                    return Cast.IntToScmFixnum(num);
                }
                else
                {
                    //TODO
                }
            }
            else if (obj is UInt32)
            {
                UInt32 num = (UInt32)obj;
                if (num <= SCM_SMALL_INT_MAX)
                {
                    return Cast.IntToScmFixnum((int)num);
                }
                else
                {
                    //TODO
                }
            }
            else if (obj is Int16)
            {
                return Cast.IntToScmFixnum((int)(Int16)obj);
            }
            else if (obj is UInt16)
            {
                return Cast.IntToScmFixnum((int)(UInt16)obj);
            }
            else if (obj is Int64)
            {
                Int64 num = (Int64)obj;
                if (num >= SCM_SMALL_INT_MIN && num <= SCM_SMALL_INT_MAX)
                {
                    return Cast.IntToScmFixnum((int)num);
                }
                else
                {
                    //TODO
                }
            }
            else if (obj is UInt64)
            {
                UInt64 num = (UInt64)obj;
                if (num <= (UInt64)SCM_SMALL_INT_MAX)
                {
                    return Cast.IntToScmFixnum((int)num);
                }
                else
                {
                    //TODO
                }
            }
            else if (obj is String)
            {
                return GoshInvoke.Scm_MakeString((String)obj, -1, -1, StringFlags.Copying);
            }

            GCHandle handle = GCHandle.Alloc(obj);
            return GoshInvoke.Scm_MakeClrObject((IntPtr)handle);
        }

    }
}

