/*
 * Gosh.cs
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
using System.Diagnostics;

namespace GaucheDotNet
{
    public static partial class Gosh
    {
        public static readonly GoshBool False = GoshBool.False;
        public static readonly GoshBool True = GoshBool.True;
        public static readonly GoshNIL NIL = GoshNIL.NIL;
        public static readonly GoshEOF EOF = GoshEOF.EOF;
        public static readonly GoshUndefined Undefined = GoshUndefined.Undefined;
        public static readonly GoshUnbound Unbound = GoshUnbound.Unbound;

        private delegate bool TypeChecker(IntPtr obj);

        private static void TypeCheck(GoshObj obj, TypeChecker checker, int argIndex)
        {
            if (!checker(obj.Ptr))
            {
                StackFrame caller = new StackFrame(1);
                System.Reflection.MethodBase callerMethod = caller.GetMethod();
                string callerName = callerMethod.Name;
                string argName = callerMethod.GetParameters()[argIndex].Name;

                string typename = checker.Method.Name.TrimEnd('P');
                string objectTypeName = obj.Specify.GetType().Name.Replace("Gosh", "Scm_");

                throw new GoshException(callerName + ": " + argName + " required " + typename + ", bug got " + objectTypeName + ".");
            }
        }

        #region gauche_dotnet {

        [DllImport("Kernel32.dll")]
        static extern bool SetEnvironmentVariable(string name, string val);

        /// <summary>
        /// GaucheのVMに設定されるデフォルトのエラーハンドラ
        /// </summary>
        /// <param name="args"></param>
        /// <param name="num"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static IntPtr ErrorHandler(IntPtr args, int num, IntPtr data)
        {
            Exception e = null;
            IntPtr obj = Marshal.ReadIntPtr(args);
            e = Cast.ToObj(obj) as Exception;
            if (e == null)
            {
                e = new GoshException(Cast.Specify(obj).ToString());
            }

            throw e;
        }
        private static IntPtr ErrorHandlerRec;

        public static void Initialize()
        {
            //Gaucheの拡張ライブラリのあるディレクトリのパスを取得
            string sitelibdir = Gosh.GetSiteArchitectureDirectory();
            //現在のプロセスのPATH環境変数に拡張ライブラリのディレクトリパスを追加
            String path = System.Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            if (path != "")
            {
                path += ";";
            }
            SetEnvironmentVariable("PATH", path + sitelibdir);

            //拡張ライブラリ内にあるGaucheの初期化関数を実行
            GoshInvoke.GaucheDotNetInitialize();

            //エラーハンドラの関数を作成
            ErrorHandlerRec = GoshInvoke.Scm_MakeSubr(ErrorHandler, IntPtr.Zero, 1, 0, Gosh.False.Ptr);
            //現在のVMにデフォルトエラーハンドラを設定する
            InstallErrorHandler();
        }

        /// <summary>
        /// 現在のVMにデフォルトエラーハンドラを設定する
        /// </summary>
        public static void InstallErrorHandler()
        {
            GoshInvoke.Scm_InstallErrorHandler(ErrorHandlerRec);
        }

        #endregion }

        #region vm.h {

        public static int Eval(object form, object env, GoshEvalPacket packet)
        {
            return GoshInvoke.Scm_Eval(Cast.ToIntPtr(form), Cast.ToIntPtr(env), packet.Ptr);
        }

        public static int EvalString(string form, object env, GoshEvalPacket packet)
        {
            return GoshInvoke.Scm_EvalCString(form, Cast.ToIntPtr(env), packet.Ptr);
        }

        public static int Apply(object proc, object args, GoshEvalPacket packet)
        {
            return GoshInvoke.Scm_Apply(Cast.ToIntPtr(proc), Cast.ToIntPtr(args), packet.Ptr);
        }

        #endregion }

        #region bignum.h {

        public static GoshObj MakeBignum(Int32 val)
        {
            return new GoshRefObj(GoshInvoke.Scm_MakeBignumFromSI(val));
        }

        public static GoshObj MakeBignum(UInt32 val)
        {
            return new GoshRefObj(GoshInvoke.Scm_MakeBignumFromUI(val));
        }

        public static GoshObj MakeBignum(int sign, UInt32[] values)
        {
            return new GoshRefObj(GoshInvoke.Scm_MakeBignumFromUIArray(sign, values, values.Length));
        }

        public static GoshObj MakeBignum(double val)
        {
            return new GoshRefObj(GoshInvoke.Scm_MakeBignumFromDouble(val));
        }

        public static GoshObj BignumCopy(GoshObj b)
        {
            TypeCheck(b, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumCopy(b.Ptr));
        }

        public static GoshObj BignumToString(GoshObj b, int radix, bool useUpper)
        {
            TypeCheck(b, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumToString(b.Ptr, radix, useUpper ? 1 : 0));
        }

        public static Int32 BignumToSI(GoshObj b, ClampMode clamp)
        {
            bool oor;
            return BignumToSI(b, clamp, out oor);
        }

        public static Int32 BignumToSI(GoshObj b, ClampMode clamp, out bool oor)
        {
            TypeCheck(b, GoshInvoke.Scm_BignumP, 0);

            return GoshInvoke.Scm_BignumToSI(b.Ptr, clamp, out oor);
        }

        public static UInt32 BignumToUI(GoshObj b, ClampMode clamp)
        {
            bool oor;
            return BignumToUI(b, clamp, out oor);
        }

        public static UInt32 BignumToUI(GoshObj b, ClampMode clamp, out bool oor)
        {
            TypeCheck(b, GoshInvoke.Scm_BignumP, 0);

            return GoshInvoke.Scm_BignumToUI(b.Ptr, clamp, out oor);
        }

        public static Int64 BignumToSI64(GoshObj b, ClampMode clamp)
        {
            bool oor;
            return BignumToSI64(b, clamp, out oor);
        }

        public static Int64 BignumToSI64(GoshObj b, ClampMode clamp, out bool oor)
        {
            TypeCheck(b, GoshInvoke.Scm_BignumP, 0);

            return GoshInvoke.Scm_BignumToSI64(b.Ptr, clamp, out oor);
        }

        public static UInt64 BignumToUI64(GoshObj b, ClampMode clamp)
        {
            bool oor;
            return BignumToUI64(b, clamp, out oor);
        }

        public static UInt64 BignumToUI64(GoshObj b, ClampMode clamp, out bool oor)
        {
            TypeCheck(b, GoshInvoke.Scm_BignumP, 0);

            return GoshInvoke.Scm_BignumToUI64(b.Ptr, clamp, out oor);
        }

        public static double BignumToDouble(GoshObj b)
        {
            TypeCheck(b, GoshInvoke.Scm_BignumP, 0);

            return GoshInvoke.Scm_BignumToDouble(b.Ptr);
        }

        public static GoshObj NormalizeBignum(GoshObj b)
        {
            TypeCheck(b, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_NormalizeBignum(b.Ptr));
        }

        public static GoshObj BignumNegete(GoshObj b)
        {
            TypeCheck(b, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumNegate(b.Ptr));
        }

        public static bool BignumCmp(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by, GoshInvoke.Scm_BignumP, 1);

            return GoshInvoke.Scm_BignumCmp(bx.Ptr, by.Ptr);
        }

        public static bool BignumAbsCmp(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by, GoshInvoke.Scm_BignumP, 1);

            return GoshInvoke.Scm_BignumAbsCmp(bx.Ptr, by.Ptr);
        }

        public static bool BignumCmp3U(GoshObj bx, GoshObj off, GoshObj by)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(off, GoshInvoke.Scm_BignumP, 1);
            TypeCheck(by, GoshInvoke.Scm_BignumP, 2);

            return GoshInvoke.Scm_BignumCmp3U(bx.Ptr, off.Ptr, by.Ptr);
        }

        public static GoshObj BignumComplement(GoshObj bx)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumComplement(bx.Ptr));
        }

        public static GoshObj BignumAdd(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by, GoshInvoke.Scm_BignumP, 1);

            return new GoshRefObj(GoshInvoke.Scm_BignumAdd(bx.Ptr, by.Ptr));
        }

        public static GoshObj BignumAddSI(GoshObj bx, Int32 y)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumAddSI(bx.Ptr, y));
        }

        public static GoshObj BignumSub(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by, GoshInvoke.Scm_BignumP, 1);

            return new GoshRefObj(GoshInvoke.Scm_BignumSub(bx.Ptr, by.Ptr));
        }

        public static GoshObj BignumSubSI(GoshObj bx, Int32 y)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumSubSI(bx.Ptr, y));
        }

        public static GoshObj BignumMul(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by, GoshInvoke.Scm_BignumP, 1);

            return new GoshRefObj(GoshInvoke.Scm_BignumMul(bx.Ptr, by.Ptr));
        }

        public static GoshObj BignumMulSI(GoshObj bx, Int32 y)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumMulSI(bx.Ptr, y));
        }

        public static GoshObj BignumDivSI(GoshObj bx, Int32 y)
        {
            int rem;
            return BignumDivSI(bx, y, out rem);
        }

        public static GoshObj BignumDivSI(GoshObj bx, Int32 y, out int remainder)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumDivSI(bx.Ptr, y, out remainder));
        }

        public static GoshObj BignumDivRem(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by, GoshInvoke.Scm_BignumP, 1);

            return new GoshRefObj(GoshInvoke.Scm_BignumDivRem(bx.Ptr, by.Ptr));
        }

        public static GoshObj BignumRemSI(GoshObj bx, int y)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumRemSI(bx.Ptr, y));
        }

        public static GoshObj BignumLogAnd(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by, GoshInvoke.Scm_BignumP, 1);

            return new GoshRefObj(GoshInvoke.Scm_BignumLogAnd(bx.Ptr, by.Ptr));
        }

        public static GoshObj BignumLogIor(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by, GoshInvoke.Scm_BignumP, 1);

            return new GoshRefObj(GoshInvoke.Scm_BignumLogIor(bx.Ptr, by.Ptr));
        }

        public static GoshObj BignumLogXor(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by, GoshInvoke.Scm_BignumP, 1);

            return new GoshRefObj(GoshInvoke.Scm_BignumLogXor(bx.Ptr, by.Ptr));
        }

        public static GoshObj BignumLogNot(GoshObj bx)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumLogNot(bx.Ptr));
        }

        public static int BignumLogCount(GoshObj b)
        {
            TypeCheck(b, GoshInvoke.Scm_BignumP, 0);

            return GoshInvoke.Scm_BignumLogCount(b.Ptr);
        }

        public static GoshObj BignumAsh(GoshObj bx, int cnt)
        {
            TypeCheck(bx, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumAsh(bx.Ptr, cnt));
        }

        public static GoshBignum MakeBignumWithSize(int size, UInt32 init)
        {
            return new GoshBignum(GoshInvoke.Scm_MakeBignumWithSize(size, init));
        }

        public static GoshBignum BignumAccMultAddUI(GoshObj acc, UInt32 coef, UInt32 c)
        {
            TypeCheck(acc, GoshInvoke.Scm_BignumP, 0);

            return new GoshBignum(GoshInvoke.Scm_BignumAccMultAddUI(acc.Ptr, coef, c));
        }

        public static int DumpBignum(GoshObj b, GoshObj outPort)
        {
            TypeCheck(b, GoshInvoke.Scm_BignumP, 0);
            //TODO outPort type check

            return GoshInvoke.Scm_DumpBignum(b.Ptr, outPort.Ptr);
        }

        #endregion }

        #region pair / list {

        public static GoshPair Cons(object car, object cdr)
        {
            return new GoshPair(GoshInvoke.Scm_Cons(Cast.ToIntPtr(car), Cast.ToIntPtr(cdr)));
        }

        public static GoshPair Acons(object caar, object cdar, object cdr)
        {
            return new GoshPair(GoshInvoke.Scm_Acons(Cast.ToIntPtr(caar), Cast.ToIntPtr(cdar), Cast.ToIntPtr(cdr)));
        }

        public static GoshObj List(params object[] elt)
        {
            GoshObj pair = Gosh.NIL;
            int length = elt.Length;
            for (int i = length - 1; i >= 0; --i)
            {
                pair = Cons(elt[i], pair);
            }

            return pair;
        }

        public static GoshObj Car(object obj)
        {
            return new GoshRefObj(GoshInvoke.Scm_Car(Cast.ToIntPtr(obj)));
        }

        public static GoshObj Cdr(object obj)
        {
            return new GoshRefObj(GoshInvoke.Scm_Cdr(Cast.ToIntPtr(obj)));
        }

        public static GoshObj Caar(object obj)
        {
            return new GoshRefObj(GoshInvoke.Scm_Caar(Cast.ToIntPtr(obj)));
        }

        public static GoshObj Cadr(object obj)
        {
            return new GoshRefObj(GoshInvoke.Scm_Cadr(Cast.ToIntPtr(obj)));
        }

        public static GoshObj Cdar(object obj)
        {
            return new GoshRefObj(GoshInvoke.Scm_Cdar(Cast.ToIntPtr(obj)));
        }

        public static GoshObj Cddr(object obj)
        {
            return new GoshRefObj(GoshInvoke.Scm_Cddr(Cast.ToIntPtr(obj)));
        }

        public static int Length(object obj)
        {
            return GoshInvoke.Scm_Length(Cast.ToIntPtr(obj));
        }

        public static GoshObj CopyList(object obj)
        {
            return new GoshRefObj(GoshInvoke.Scm_CopyList(Cast.ToIntPtr(obj)));
        }

        public static GoshObj MakeList(int len, object fill)
        {
            return new GoshRefObj(GoshInvoke.Scm_MakeList(len, Cast.ToIntPtr(fill)));
        }

        public static GoshObj Append2X(object list, object obj)
        {
            return new GoshRefObj(GoshInvoke.Scm_Append2X(Cast.ToIntPtr(list), Cast.ToIntPtr(obj)));
        }

        public static GoshObj Append2(object list, object obj)
        {
            return new GoshRefObj(GoshInvoke.Scm_Append2(Cast.ToIntPtr(list), Cast.ToIntPtr(obj)));
        }

        public static GoshObj Append2(object args)
        {
            return new GoshRefObj(GoshInvoke.Scm_Append(Cast.ToIntPtr(args)));
        }

        public static GoshObj ListTail(object list, int i, object fallback)
        {
            IntPtr ptrFallback;
            if (fallback == Gosh.Unbound)
            {
                ptrFallback = Gosh.Unbound.Ptr;
            }
            else
            {
                ptrFallback = IntPtr.Zero;
            }

            IntPtr ret = GoshInvoke.Scm_ListTail(Cast.ToIntPtr(list), i, ptrFallback);
            if (ret == IntPtr.Zero)
            {
                if(fallback is GoshObj)
                {
                    return (GoshObj)fallback;
                }
                else
                {
                    return new GoshClrObject(fallback);
                }
            }
            else
            {
                return new GoshRefObj(ret);
            }
        }

        public static GoshObj ListRef(object list, int i, object fallback)
        {
            IntPtr ptrFallback;
            if (fallback == Gosh.Unbound)
            {
                ptrFallback = Gosh.Unbound.Ptr;
            }
            else
            {
                ptrFallback = IntPtr.Zero;
            }

            IntPtr ret = GoshInvoke.Scm_ListRef(Cast.ToIntPtr(list), i, ptrFallback);
            if (ret == IntPtr.Zero)
            {
                if(fallback is GoshObj)
                {
                    return (GoshObj)fallback;
                }
                else
                {
                    return new GoshClrObject(fallback);
                }
            }
            else
            {
                return new GoshRefObj(ret);
            }
        }

        public static GoshObj LastPair(object list)
        {
            return new GoshRefObj(GoshInvoke.Scm_LastPair(Cast.ToIntPtr(list)));
        }

        public static GoshObj Memq(object obj, object list)
        {
            return new GoshRefObj(GoshInvoke.Scm_Memq(Cast.ToIntPtr(obj), Cast.ToIntPtr(list)));
        }

        public static GoshObj Memv(object obj, object list)
        {
            return new GoshRefObj(GoshInvoke.Scm_Memv(Cast.ToIntPtr(obj), Cast.ToIntPtr(list)));
        }

        public static GoshObj Member(object obj, object list, CmpMode cmpmode)
        {
            return new GoshRefObj(GoshInvoke.Scm_Member(Cast.ToIntPtr(obj), Cast.ToIntPtr(list), (int)cmpmode));
        }

        public static GoshObj Assq(object obj, object alist)
        {
            return new GoshRefObj(GoshInvoke.Scm_Assq(Cast.ToIntPtr(obj), Cast.ToIntPtr(alist)));
        }

        public static GoshObj Assv(object obj, object alist)
        {
            return new GoshRefObj(GoshInvoke.Scm_Assv(Cast.ToIntPtr(obj), Cast.ToIntPtr(alist)));
        }

        public static GoshObj Assoc(object obj, object alist, CmpMode cmpmode)
        {
            return new GoshRefObj(GoshInvoke.Scm_Assoc(Cast.ToIntPtr(obj), Cast.ToIntPtr(alist), (int)cmpmode));
        }

        public static GoshObj Delete(object obj, object list, CmpMode cmpmode)
        {
            return new GoshRefObj(GoshInvoke.Scm_Delete(Cast.ToIntPtr(obj), Cast.ToIntPtr(list), (int)cmpmode));
        }

        public static GoshObj DeleteX(object obj, object list, CmpMode cmpmode)
        {
            return new GoshRefObj(GoshInvoke.Scm_DeleteX(Cast.ToIntPtr(obj), Cast.ToIntPtr(list), (int)cmpmode));
        }

        public static GoshObj AssocDelete(object obj, object alist, CmpMode cmpmode)
        {
            return new GoshRefObj(GoshInvoke.Scm_AssocDelete(Cast.ToIntPtr(obj), Cast.ToIntPtr(alist), (int)cmpmode));
        }

        public static GoshObj AssocDeleteX(object obj, object alist, CmpMode cmpmode)
        {
            return new GoshRefObj(GoshInvoke.Scm_AssocDeleteX(Cast.ToIntPtr(obj), Cast.ToIntPtr(alist), (int)cmpmode));
        }

        public static GoshObj DeleteDuplicates(object alist, CmpMode cmpmode)
        {
            return new GoshRefObj(GoshInvoke.Scm_DeleteDuplicates(Cast.ToIntPtr(alist), (int)cmpmode));
        }

        public static GoshObj DeleteDuplicatesX(object alist, CmpMode cmpmode)
        {
            return new GoshRefObj(GoshInvoke.Scm_DeleteDuplicatesX(Cast.ToIntPtr(alist), (int)cmpmode));
        }

        public static GoshObj Union(object list1, object list2)
        {
            return new GoshRefObj(GoshInvoke.Scm_Union(Cast.ToIntPtr(list1), Cast.ToIntPtr(list2)));
        }

        public static GoshObj Intersection(object list1, object list2)
        {
            return new GoshRefObj(GoshInvoke.Scm_Intersection(Cast.ToIntPtr(list1), Cast.ToIntPtr(list2)));
        }

        //
        // Extended Pair

        public static GoshPair ExtendedCons(object car, object cdr)
        {
            return new GoshPair(GoshInvoke.Scm_ExtendedCons(Cast.ToIntPtr(car), Cast.ToIntPtr(cdr)));
        }

        public static GoshObj PairAttr(object pair)
        {
            return new GoshRefObj(GoshInvoke.Scm_PairAttr(Cast.ToIntPtr(pair)));
        }

        public static GoshObj PairAttrGet(object pair, object key, object fallback)
        {
            IntPtr ptrFallback;
            if (fallback == Gosh.Unbound)
            {
                ptrFallback = Gosh.Unbound.Ptr;
            }
            else
            {
                ptrFallback = IntPtr.Zero;
            }

            IntPtr ret = GoshInvoke.Scm_PairAttrGet(Cast.ToIntPtr(pair), Cast.ToIntPtr(key), ptrFallback);
            if (ret == IntPtr.Zero)
            {
                if(fallback is GoshObj)
                {
                    return (GoshObj)fallback;
                }
                else
                {
                    return new GoshClrObject(fallback);
                }
            }
            else
            {
                return new GoshRefObj(ret);
            }
        }

        public static GoshObj PairAttrSet(object pair, object key, object value)
        {
            return new GoshRefObj(GoshInvoke.Scm_PairAttrSet(Cast.ToIntPtr(pair), Cast.ToIntPtr(key), Cast.ToIntPtr(value)));
        }

        public static bool ExtendedPairP(object obj)
        {
            return GoshInvoke.Scm_ExtendedPairP(Cast.ToIntPtr(obj));
        }

        #endregion

        #region string.h {

        public static GoshString MakeString(string str)
        {
            return new GoshString(GoshInvoke.Scm_MakeString(str, -1, -1, StringFlags.Copying));
        }

        public static GoshString MakeFillString(int len, char fill)
        {
            return new GoshString(GoshInvoke.Scm_MakeFillString(len, fill));
        }

        public static string GetString(GoshString str)
        {
            return GoshInvoke.Scm_GetString(str.Ptr);
        }

        public static string GetStringConst(GoshString str)
        {
            return GoshInvoke.Scm_GetStringConst(str.Ptr);
        }

        #endregion

        #region hash.h {

        public static GoshHashTable MakeHashTable(HashType type, int initSize)
        {
            return new GoshHashTable(type, initSize);
        }

        public static GoshObj HashTableCopy(GoshObj obj)
        {
            return new GoshRefObj(GoshInvoke.Scm_HashTableCopy(obj.Ptr));
        }

        public static object HashTableRef(GoshObj obj, object key)
        {
            if (GoshInvoke.Scm_HashTableP(obj.Ptr))
            {
                return GoshHashTable.HashTableRef(obj.Ptr, key);
            }
            else
            {
                throw new GoshException("Scm_HashTable required");
            }
        }

        public static void HashTableSet(GoshObj obj, object key, object value)
        {
            if (GoshInvoke.Scm_HashTableP(obj.Ptr))
            {
                GoshHashTable.HashTableSet(obj.Ptr, key, value);
            }
            else
            {
                throw new GoshException("Scm_HashTable required");
            }
        }

        public static object HashTableDelete(GoshObj obj, object key)
        {
            if (GoshInvoke.Scm_HashTableP(obj.Ptr))
            {
                return GoshHashTable.HashTableDelete(obj.Ptr, key);
            }
            else
            {
                throw new GoshException("Scm_HashTable required");
            }
        }

        public static GoshObj HashTableKeys(GoshObj obj)
        {
            if (GoshInvoke.Scm_HashTableP(obj.Ptr))
            {
                return new GoshRefObj(GoshInvoke.Scm_HashTableKeys(obj.Ptr));
            }
            else
            {
                throw new GoshException("Scm_HashTable required");
            }
        }

        public static GoshObj HashTableValues(GoshObj obj)
        {
            if (GoshInvoke.Scm_HashTableP(obj.Ptr))
            {
                return new GoshRefObj(GoshInvoke.Scm_HashTableValues(obj.Ptr));
            }
            else
            {
                throw new GoshException("Scm_HashTable required");
            }
        }

        public static GoshObj HashTableStat(GoshObj obj)
        {
            if (GoshInvoke.Scm_HashTableP(obj.Ptr))
            {
                return new GoshRefObj(GoshInvoke.Scm_HashTableStat(obj.Ptr));
            }
            else
            {
                throw new GoshException("Scm_HashTable required");
            }
        }

        public static bool IsHashTable(GoshObj obj)
        {
            return GoshInvoke.Scm_HashTableP(obj.Ptr);
        }

        #endregion }

        #region module.h {

        public static GoshGloc FindBinding(GoshModule module, GoshSymbol symbol, BindingFlag flags)
        {
            return new GoshGloc(GoshInvoke.Scm_FindBinding(module.Ptr, symbol.Ptr, flags));
        }

        public static GoshGloc MakeBinding(GoshModule module, GoshSymbol symbol, object value, BindingFlag flags)
        {
            return new GoshGloc(GoshInvoke.Scm_MakeBinding(module.Ptr, symbol.Ptr, Cast.ToIntPtr(value), flags));
        }

        public static GoshGloc Define(GoshModule module, GoshSymbol symbol, object value)
        {
            return MakeBinding(module, symbol, value, BindingFlag.None);
        }

        public static GoshGloc DefineConst(GoshModule module, GoshSymbol symbol, object value, BindingFlag flags)
        {
            return MakeBinding(module, symbol, value, BindingFlag.Const);
        }

#if HAS_DELETE_BINDING
        public static void DeleteBinding(GoshModule module, GoshSymbol symbol, BindingFlag flags)
        {
            GoshInvoke.Scm_DeleteBinding(module.Ptr, symbol.Ptr, flags);
        }
#endif

        public static GoshObj GlobalVariableRef(GoshModule module, GoshSymbol symbol, BindingFlag flags)
        {
            return new GoshRefObj(GoshInvoke.Scm_GlobalVariableRef(module.Ptr, symbol.Ptr, flags));
        }

        public static void HideBinding(GoshModule module, GoshSymbol symbol)
        {
            GoshInvoke.Scm_HideBinding(module.Ptr, symbol.Ptr);
        }

        public static int AliasBindings(GoshModule target, GoshSymbol targetName,
            GoshModule origin, GoshSymbol originName)
        {
            return GoshInvoke.Scm_AliasBinding(target.Ptr, targetName.Ptr, origin.Ptr, originName.Ptr);
        }

        public static GoshObj ExtendModule(GoshModule module, object supers)
        {
            return new GoshRefObj(GoshInvoke.Scm_ExtendModule(module.Ptr, Cast.ToIntPtr(supers)));
        }

        public static GoshObj ImportModule(GoshModule module, object imported, object prefix, UInt32 flags)
        {
            return new GoshRefObj(GoshInvoke.Scm_ImportModule(module.Ptr, Cast.ToIntPtr(imported), Cast.ToIntPtr(prefix), flags));
        }

        public static GoshObj ExportSymbols(GoshModule module, object list)
        {
            return new GoshRefObj(GoshInvoke.Scm_ExportSymbols(module.Ptr, Cast.ToIntPtr(list)));
        }

        public static GoshObj ExportAll(GoshModule module)
        {
            return new GoshRefObj(GoshInvoke.Scm_ExportAll(module.Ptr));
        }

        public static GoshModule FindModule(GoshSymbol name, FindModuleFlag flags)
        {
            return new GoshModule(GoshInvoke.Scm_FindModule(name.Ptr, flags));
        }

        public static GoshObj AllModules()
        {
            return new GoshRefObj(GoshInvoke.Scm_AllModules());
        }

        public static void SelectModule(GoshModule module)
        {
            GoshInvoke.Scm_SelectModule(module.Ptr);
        }

#if !GAUCHE_9_3_3

        public static GoshObj ModuleExports(GoshModule module)
        {
            return new GoshRefObj(GoshInvoke.Scm_ModuleExports(module.Ptr));
        }

#endif

        public static GoshObj ModuleNameToPath(GoshSymbol name)
        {
            return new GoshRefObj(GoshInvoke.Scm_ModuleNameToPath(name.Ptr));
        }

        public static GoshObj PathToModuleName(GoshString path)
        {
            return new GoshRefObj(GoshInvoke.Scm_PathToModuleName(path.Ptr));
        }

        public static GoshModule NullModule()
        {
            return new GoshModule(GoshInvoke.Scm_NullModule());
        }

        public static GoshModule SchemeModule()
        {
            return new GoshModule(GoshInvoke.Scm_SchemelModule());
        }

        public static GoshModule GaucheModule()
        {
            return new GoshModule(GoshInvoke.Scm_GaucheModule());
        }

        public static GoshModule UserModule()
        {
            return new GoshModule(GoshInvoke.Scm_UserModule());
        }

        public static GoshModule CurrentModule()
        {
            return new GoshModule(GoshInvoke.Scm_CurrentModule());
        }

        #endregion }

        #region symbol.h {

        public static GoshSymbol MakeSymbol(GoshString name, bool interned)
        {
            return new GoshSymbol(GoshInvoke.Scm_MakeSymbol(name.Ptr, interned));
        }

        public static GoshSymbol MakeSymbol(string name, bool interned)
        {
            return MakeSymbol(MakeString(name), interned);
        }

        public static GoshSymbol Gensym(GoshString name)
        {
            return new GoshSymbol(GoshInvoke.Scm_Gensym(name.Ptr));
        }

        public static GoshSymbol Gensym(string name)
        {
            return Gensym(MakeString(name));
        }

        public static GoshSymbol Intern(GoshString name)
        {
            return MakeSymbol(name, true);
        }

        public static GoshSymbol Intern(string name)
        {
            return Intern(MakeString(name));
        }

        #endregion }

        #region keyword.h {

        public static GoshKeyword MakeKeyword(GoshString name)
        {
            return new GoshKeyword(GoshInvoke.Scm_MakeKeyword(name.Ptr));
        }

        public static GoshObj GetKeyword(GoshObj key, GoshObj list, GoshObj fallback)
        {
            return new GoshRefObj(GoshInvoke.Scm_GetKeyword( key.Ptr, list.Ptr, fallback.Ptr));
        }

        public static GoshObj DeleteKeyword(GoshObj key, GoshObj list)
        {
            return new GoshRefObj(GoshInvoke.Scm_DeleteKeyword(key.Ptr, list.Ptr));
        }

        public static GoshObj DeleteKeywordX(GoshObj key, GoshObj list)
        {
            return new GoshRefObj(GoshInvoke.Scm_DeleteKeywordX(key.Ptr, list.Ptr));
        }

        #endregion }

        #region proc.h {

        #endregion }

        #region exception {

        public static GoshObj ConditionMessage(object condition)
        {
            return new GoshRefObj(GoshInvoke.Scm_ConditionMessage(Cast.ToIntPtr(condition)));
        }

        public static GoshObj ConditionTypeName(object condition)
        {
            return new GoshRefObj(GoshInvoke.Scm_ConditionTypeName(Cast.ToIntPtr(condition)));
        }

        public static Exception ClrConditionInnerException(object condition)
        {
            IntPtr obj = GoshInvoke.Scm_ClrConditionInnerException(Cast.ToIntPtr(condition));
            if(obj == IntPtr.Zero)
            {
                return null;
            }
            else
            {
                return GCHandle.FromIntPtr(obj).Target as Exception;
            }
        }

        #endregion }

        #region paths.h {

        public static String GetLibraryDirectory()
        {
            return GoshInvoke.Scm_GetLibraryDirectory();
        }

        public static String GetArchitectureDirectory()
        {
            return GoshInvoke.Scm_GetArchitectureDirectory();
        }

        public static String GetSiteLibraryDirectory()
        {
            return GoshInvoke.Scm_GetSiteLibraryDirectory();
        }

        public static String GetSiteArchitectureDirectory()
        {
            return GoshInvoke.Scm_GetSiteArchitectureDirectory();
        }

        public static String GetRuntimeDirectory()
        {
            return GoshInvoke.Scm_GetRuntimeDirectory();
        }

        #endregion }

        #region gc {

        public static void GC()
        {
            GoshInvoke.Scm_GC();
        }

        public static void PrintStaticRoots()
        {
            GoshInvoke.Scm_PrintStaticRoots();
        }

        public static void GCSentinel(GoshObj obj, string name)
        {
            GoshInvoke.Scm_GCSentinel(obj.Ptr, name);
        }

        #endregion }

    }
}

