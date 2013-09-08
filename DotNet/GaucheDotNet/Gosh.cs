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

        private static void TypeCheck(IntPtr ptr, TypeChecker checker, int argIndex)
        {
            if (!checker(ptr))
            {
                StackFrame caller = new StackFrame(1);
                System.Reflection.MethodBase callerMethod = caller.GetMethod();
                string callerName = callerMethod.Name;
                string argName = callerMethod.GetParameters()[argIndex].Name;

                string typename = checker.Method.Name.TrimEnd('P');
                string objectTypeName = Cast.Specify(ptr).Specify.GetType().Name.Replace("Gosh", "Scm_");

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
            TypeCheck(b.Ptr, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumCopy(b.Ptr));
        }

        public static GoshObj BignumToString(GoshObj b, int radix, bool useUpper)
        {
            TypeCheck(b.Ptr, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumToString(b.Ptr, radix, useUpper ? 1 : 0));
        }

        public static Int32 BignumToSI(GoshObj b, ClampMode clamp)
        {
            bool oor;
            return BignumToSI(b, clamp, out oor);
        }

        public static Int32 BignumToSI(GoshObj b, ClampMode clamp, out bool oor)
        {
            TypeCheck(b.Ptr, GoshInvoke.Scm_BignumP, 0);

            return GoshInvoke.Scm_BignumToSI(b.Ptr, clamp, out oor);
        }

        public static UInt32 BignumToUI(GoshObj b, ClampMode clamp)
        {
            bool oor;
            return BignumToUI(b, clamp, out oor);
        }

        public static UInt32 BignumToUI(GoshObj b, ClampMode clamp, out bool oor)
        {
            TypeCheck(b.Ptr, GoshInvoke.Scm_BignumP, 0);

            return GoshInvoke.Scm_BignumToUI(b.Ptr, clamp, out oor);
        }

        public static Int64 BignumToSI64(GoshObj b, ClampMode clamp)
        {
            bool oor;
            return BignumToSI64(b, clamp, out oor);
        }

        public static Int64 BignumToSI64(GoshObj b, ClampMode clamp, out bool oor)
        {
            TypeCheck(b.Ptr, GoshInvoke.Scm_BignumP, 0);

            return GoshInvoke.Scm_BignumToSI64(b.Ptr, clamp, out oor);
        }

        public static UInt64 BignumToUI64(GoshObj b, ClampMode clamp)
        {
            bool oor;
            return BignumToUI64(b, clamp, out oor);
        }

        public static UInt64 BignumToUI64(GoshObj b, ClampMode clamp, out bool oor)
        {
            TypeCheck(b.Ptr, GoshInvoke.Scm_BignumP, 0);

            return GoshInvoke.Scm_BignumToUI64(b.Ptr, clamp, out oor);
        }

        public static double BignumToDouble(GoshObj b)
        {
            TypeCheck(b.Ptr, GoshInvoke.Scm_BignumP, 0);

            return GoshInvoke.Scm_BignumToDouble(b.Ptr);
        }

        public static GoshObj NormalizeBignum(GoshObj b)
        {
            TypeCheck(b.Ptr, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_NormalizeBignum(b.Ptr));
        }

        public static GoshObj BignumNegete(GoshObj b)
        {
            TypeCheck(b.Ptr, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumNegate(b.Ptr));
        }

        public static bool BignumCmp(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by.Ptr, GoshInvoke.Scm_BignumP, 1);

            return GoshInvoke.Scm_BignumCmp(bx.Ptr, by.Ptr);
        }

        public static bool BignumAbsCmp(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by.Ptr, GoshInvoke.Scm_BignumP, 1);

            return GoshInvoke.Scm_BignumAbsCmp(bx.Ptr, by.Ptr);
        }

        public static bool BignumCmp3U(GoshObj bx, GoshObj off, GoshObj by)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(off.Ptr, GoshInvoke.Scm_BignumP, 1);
            TypeCheck(by.Ptr, GoshInvoke.Scm_BignumP, 2);

            return GoshInvoke.Scm_BignumCmp3U(bx.Ptr, off.Ptr, by.Ptr);
        }

        public static GoshObj BignumComplement(GoshObj bx)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumComplement(bx.Ptr));
        }

        public static GoshObj BignumAdd(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by.Ptr, GoshInvoke.Scm_BignumP, 1);

            return new GoshRefObj(GoshInvoke.Scm_BignumAdd(bx.Ptr, by.Ptr));
        }

        public static GoshObj BignumAddSI(GoshObj bx, Int32 y)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumAddSI(bx.Ptr, y));
        }

        public static GoshObj BignumSub(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by.Ptr, GoshInvoke.Scm_BignumP, 1);

            return new GoshRefObj(GoshInvoke.Scm_BignumSub(bx.Ptr, by.Ptr));
        }

        public static GoshObj BignumSubSI(GoshObj bx, Int32 y)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumSubSI(bx.Ptr, y));
        }

        public static GoshObj BignumMul(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by.Ptr, GoshInvoke.Scm_BignumP, 1);

            return new GoshRefObj(GoshInvoke.Scm_BignumMul(bx.Ptr, by.Ptr));
        }

        public static GoshObj BignumMulSI(GoshObj bx, Int32 y)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumMulSI(bx.Ptr, y));
        }

        public static GoshObj BignumDivSI(GoshObj bx, Int32 y)
        {
            int rem;
            return BignumDivSI(bx, y, out rem);
        }

        public static GoshObj BignumDivSI(GoshObj bx, Int32 y, out int remainder)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumDivSI(bx.Ptr, y, out remainder));
        }

        public static GoshObj BignumDivRem(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by.Ptr, GoshInvoke.Scm_BignumP, 1);

            return new GoshRefObj(GoshInvoke.Scm_BignumDivRem(bx.Ptr, by.Ptr));
        }

        public static GoshObj BignumRemSI(GoshObj bx, int y)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumRemSI(bx.Ptr, y));
        }

        public static GoshObj BignumLogAnd(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by.Ptr, GoshInvoke.Scm_BignumP, 1);

            return new GoshRefObj(GoshInvoke.Scm_BignumLogAnd(bx.Ptr, by.Ptr));
        }

        public static GoshObj BignumLogIor(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by.Ptr, GoshInvoke.Scm_BignumP, 1);

            return new GoshRefObj(GoshInvoke.Scm_BignumLogIor(bx.Ptr, by.Ptr));
        }

        public static GoshObj BignumLogXor(GoshObj bx, GoshObj by)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);
            TypeCheck(by.Ptr, GoshInvoke.Scm_BignumP, 1);

            return new GoshRefObj(GoshInvoke.Scm_BignumLogXor(bx.Ptr, by.Ptr));
        }

        public static GoshObj BignumLogNot(GoshObj bx)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumLogNot(bx.Ptr));
        }

        public static int BignumLogCount(GoshObj b)
        {
            TypeCheck(b.Ptr, GoshInvoke.Scm_BignumP, 0);

            return GoshInvoke.Scm_BignumLogCount(b.Ptr);
        }

        public static GoshObj BignumAsh(GoshObj bx, int cnt)
        {
            TypeCheck(bx.Ptr, GoshInvoke.Scm_BignumP, 0);

            return new GoshRefObj(GoshInvoke.Scm_BignumAsh(bx.Ptr, cnt));
        }

        public static GoshInteger MakeBignumWithSize(int size, UInt32 init)
        {
            return new GoshInteger(GoshInvoke.Scm_MakeBignumWithSize(size, init));
        }

        public static GoshInteger BignumAccMultAddUI(GoshObj acc, UInt32 coef, UInt32 c)
        {
            TypeCheck(acc.Ptr, GoshInvoke.Scm_BignumP, 0);

            return new GoshInteger(GoshInvoke.Scm_BignumAccMultAddUI(acc.Ptr, coef, c));
        }

        public static int DumpBignum(GoshObj b, GoshObj outPort)
        {
            TypeCheck(b.Ptr, GoshInvoke.Scm_BignumP, 0);
            //TODO outPort type check

            return GoshInvoke.Scm_DumpBignum(b.Ptr, outPort.Ptr);
        }

        #endregion }

        #region number.h {

        public static GoshInteger MakeInteger(Int32 i)
        {
            IntPtr ptr = GoshInvoke.Scm_MakeInteger(i);
            if (Cast.IsFixnum(ptr))
            {
                return new GoshFixnum(ptr);
            }
            else
            {
                return new GoshInteger(ptr);
            }
        }

        public static GoshInteger MakeIntegerU(UInt32 i)
        {
            IntPtr ptr = GoshInvoke.Scm_MakeIntegerU(i);
            if (Cast.IsFixnum(ptr))
            {
                return new GoshFixnum(ptr);
            }
            else
            {
                return new GoshInteger(ptr);
            }
        }

        public static Int32 GetIntegerClamp(GoshObj obj, ClampMode clamp)
        {
            return GoshInvoke.Scm_GetIntegerClamp(obj.Ptr, clamp, IntPtr.Zero);
        }

        public static UInt32 GetIntegerUClamp(GoshObj obj, ClampMode clamp)
        {
            return GoshInvoke.Scm_GetIntegerUClamp(obj.Ptr, clamp, IntPtr.Zero);
        }

        public static Int32 GetInteger8Clamp(GoshObj obj, ClampMode clamp)
        {
            return GoshInvoke.Scm_GetInteger8Clamp(obj.Ptr, clamp, IntPtr.Zero);
        }

        public static UInt32 GetIntegerU8Clamp(GoshObj obj, ClampMode clamp)
        {
            return GoshInvoke.Scm_GetIntegerU8Clamp(obj.Ptr, clamp, IntPtr.Zero);
        }

        public static Int32 GetInteger16Clamp(GoshObj obj, ClampMode clamp)
        {
            return GoshInvoke.Scm_GetInteger16Clamp(obj.Ptr, clamp, IntPtr.Zero);
        }

        public static UInt32 GetIntegerU16Clamp(GoshObj obj, ClampMode clamp)
        {
            return GoshInvoke.Scm_GetIntegerU16Clamp(obj.Ptr, clamp, IntPtr.Zero);
        }

        public static Int32 GetInteger32Clamp(GoshObj obj, ClampMode clamp)
        {
            return GoshInvoke.Scm_GetInteger32Clamp(obj.Ptr, clamp, IntPtr.Zero);
        }

        public static UInt32 GetIntegerU32Clamp(GoshObj obj, ClampMode clamp)
        {
            return GoshInvoke.Scm_GetIntegerU32Clamp(obj.Ptr, clamp, IntPtr.Zero);
        }

        public static GoshInteger MakeInteger(Int64 i)
        {
            return new GoshInteger(GoshInvoke.Scm_MakeInteger64(i));
        }

        public static GoshInteger MakeInteger(UInt64 i)
        {
            return new GoshInteger(GoshInvoke.Scm_MakeIntegerU64(i));
        }

        public static Int64 GetInteger64Clamp(GoshObj obj, ClampMode clamp)
        {
            return GoshInvoke.Scm_GetInteger64Clamp(obj.Ptr, clamp, IntPtr.Zero);
        }

        public static UInt64 GetIntegerU64Clamp(GoshObj obj, ClampMode clamp)
        {
            return GoshInvoke.Scm_GetIntegerU64Clamp(obj.Ptr, clamp, IntPtr.Zero);
        }

        //ScmRatnum(Rational)

        public static GoshRatnum MakeRational(GoshObj numer, GoshObj denom)
        {
            return new GoshRatnum(GoshInvoke.Scm_MakeRational(numer.Ptr, denom.Ptr));
        }

        public static GoshRatnum MakeRatnum(GoshObj numer, GoshObj denom)
        {
            return new GoshRatnum(GoshInvoke.Scm_MakeRatnum(numer.Ptr, denom.Ptr));
        }

        public static GoshRatnum ReduceRational(GoshObj rational)
        {
            return new GoshRatnum(GoshInvoke.Scm_ReduceRational(rational.Ptr));
        }

        //ScmFlonum

        public static GoshFlonum MakeFlonum(double d)
        {
            return new GoshFlonum(GoshInvoke.Scm_MakeFlonum(d));
        }

        public static double GetDouble(GoshObj obj)
        {
            return GoshInvoke.Scm_GetDouble(obj.Ptr);
        }

        public static double HalfToDouble(UInt16 v)
        {
            return GoshInvoke.Scm_HalfToDouble(v);
        }

        public static UInt16 DoubleToHalf(double v)
        {
            return GoshInvoke.Scm_DoubleToHalf(v);
        }

        //ScmCompnum

        public static GoshCompnum MakeCompnum(double real, double imag)
        {
            return new GoshCompnum(GoshInvoke.Scm_MakeCompnum(real, imag));
        }

        public static GoshCompnum MakeComplex(double real, double imag)
        {
            return new GoshCompnum(GoshInvoke.Scm_MakeComplex(real, imag));
        }

        public static GoshCompnum MakeComplexPolar(double real, double imag)
        {
            return new GoshCompnum(GoshInvoke.Scm_MakeComplexPolar(real, imag));
        }

        //Operation

        public static bool IntegerP(GoshObj obj)
        {
            return GoshInvoke.Scm_IntegerP(obj.Ptr);
        }

        public static bool OddP(GoshObj obj)
        {
            return GoshInvoke.Scm_OddP(obj.Ptr);
        }

        public static bool EvenP(GoshObj obj)
        {
            return !GoshInvoke.Scm_OddP(obj.Ptr);
        }

        public static bool FiniteP(GoshObj obj)
        {
            return GoshInvoke.Scm_FiniteP(obj.Ptr);
        }

        public static bool InfiniteP(GoshObj obj)
        {
            return GoshInvoke.Scm_InfiniteP(obj.Ptr);
        }

        public static bool NanP(GoshObj obj)
        {
            return GoshInvoke.Scm_NanP(obj.Ptr);
        }

        public static GoshNumber Abs(GoshObj obj)
        {
            return new GoshNumber(GoshInvoke.Scm_Abs(obj.Ptr));
        }

        public static int Sign(GoshObj obj)
        {
            return GoshInvoke.Scm_Sign(obj.Ptr);
        }

        public static GoshNumber Negate(GoshObj obj)
        {
            return new GoshNumber(GoshInvoke.Scm_Negate(obj.Ptr));
        }

        public static GoshNumber Reciprocal(GoshObj obj)
        {
            return new GoshNumber(GoshInvoke.Scm_Reciprocal(obj.Ptr));
        }

        public static GoshNumber ReciprocalInexact(GoshObj obj)
        {
            return new GoshNumber(GoshInvoke.Scm_ReciprocalInexact(obj.Ptr));
        }

        public static GoshNumber InexactToExact(GoshObj obj)
        {
            return new GoshNumber(GoshInvoke.Scm_InexactToExact(obj.Ptr));
        }

        public static GoshNumber ExactToInexact(GoshObj obj)
        {
            return new GoshNumber(GoshInvoke.Scm_ExactToInexact(obj.Ptr));
        }

        public static GoshNumber Add(GoshObj obj1, GoshObj obj2)
        {
            return new GoshNumber(GoshInvoke.Scm_Add(obj1.Ptr, obj2.Ptr));
        }

        public static GoshNumber Sub(GoshObj obj1, GoshObj obj2)
        {
            return new GoshNumber(GoshInvoke.Scm_Sub(obj1.Ptr, obj2.Ptr));
        }

        public static GoshNumber Mul(GoshObj obj1, GoshObj obj2)
        {
            return new GoshNumber(GoshInvoke.Scm_Mul(obj1.Ptr, obj2.Ptr));
        }

        public static GoshNumber Div(GoshObj obj1, GoshObj obj2)
        {
            return new GoshNumber(GoshInvoke.Scm_Div(obj1.Ptr, obj2.Ptr));
        }

        public static GoshNumber DivInexact(GoshObj obj1, GoshObj obj2)
        {
            return new GoshNumber(GoshInvoke.Scm_DivInexact(obj1.Ptr, obj2.Ptr));
        }

        public static GoshNumber DivCompat(GoshObj obj1, GoshObj obj2)
        {
            return new GoshNumber(GoshInvoke.Scm_DivCompat(obj1.Ptr, obj2.Ptr));
        }

        public static GoshNumber Quotient(GoshObj obj1, GoshObj obj2)
        {
            return new GoshNumber(GoshInvoke.Scm_Quotient(obj1.Ptr, obj2.Ptr, IntPtr.Zero));
        }

        public static GoshNumber Modulo(GoshObj obj1, GoshObj obj2)
        {
            return new GoshNumber(GoshInvoke.Scm_Modulo(obj1.Ptr, obj2.Ptr, false));
        }

        public static GoshNumber Remainder(GoshObj obj1, GoshObj obj2)
        {
            return new GoshNumber(GoshInvoke.Scm_Modulo(obj1.Ptr, obj2.Ptr, true));
        }

        public static GoshNumber Gcd(GoshObj obj1, GoshObj obj2)
        {
            return new GoshNumber(GoshInvoke.Scm_Gcd(obj1.Ptr, obj2.Ptr));
        }

        public static GoshNumber Expt(GoshObj obj1, GoshObj obj2)
        {
            return new GoshNumber(GoshInvoke.Scm_Expt(obj1.Ptr, obj2.Ptr));
        }

        public static int TwosPower(GoshObj obj)
        {
            return GoshInvoke.Scm_TwosPower(obj.Ptr);
        }

        public static bool NumEq(GoshObj obj1, GoshObj obj2)
        {
            return GoshInvoke.Scm_NumEq(obj1.Ptr, obj2.Ptr);
        }

        public static bool NumLT(GoshObj obj1, GoshObj obj2)
        {
            return GoshInvoke.Scm_NumLT(obj1.Ptr, obj2.Ptr);
        }

        public static bool NumLE(GoshObj obj1, GoshObj obj2)
        {
            return GoshInvoke.Scm_NumLE(obj1.Ptr, obj2.Ptr);
        }

        public static bool NumGT(GoshObj obj1, GoshObj obj2)
        {
            return GoshInvoke.Scm_NumGT(obj1.Ptr, obj2.Ptr);
        }

        public static bool NumGE(GoshObj obj1, GoshObj obj2)
        {
            return GoshInvoke.Scm_NumGE(obj1.Ptr, obj2.Ptr);
        }

        public static int NumCmp(GoshObj obj1, GoshObj obj2)
        {
            return GoshInvoke.Scm_NumCmp(obj1.Ptr, obj2.Ptr);
        }

        public static GoshNumber Min(GoshObj arg0, GoshObj args)
        {
            IntPtr min;
            GoshInvoke.Scm_MinMax(arg0.Ptr, args.Ptr, out min, IntPtr.Zero);
            return new GoshNumber(min);
        }

        public static GoshNumber Max(GoshObj arg0, GoshObj args)
        {
            IntPtr max;
            GoshInvoke.Scm_MinMax(arg0.Ptr, args.Ptr, IntPtr.Zero, out max);
            return new GoshNumber(max);
        }

        public static GoshNumber LogAnd(GoshObj x, GoshObj y)
        {
            return new GoshNumber(GoshInvoke.Scm_LogAnd(x.Ptr, y.Ptr));
        }

        public static GoshNumber LogIor(GoshObj x, GoshObj y)
        {
            return new GoshNumber(GoshInvoke.Scm_LogIor(x.Ptr, y.Ptr));
        }

        public static GoshNumber LogXor(GoshObj x, GoshObj y)
        {
            return new GoshNumber(GoshInvoke.Scm_LogXor(x.Ptr, y.Ptr));
        }

        public static GoshNumber LogNot(GoshObj x)
        {
            return new GoshNumber(GoshInvoke.Scm_LogNot(x.Ptr));
        }

        public static GoshNumber Ash(GoshObj x, int cnt)
        {
            return new GoshNumber(GoshInvoke.Scm_Ash(x.Ptr, cnt));
        }

        public static GoshNumber Round(GoshObj num, RoundMode mode)
        {
            return new GoshNumber(GoshInvoke.Scm_Round(num.Ptr, mode));
        }

        public static GoshNumber RoundToExact(GoshObj num, RoundMode mode)
        {
            return new GoshNumber(GoshInvoke.Scm_RoundToExact(num.Ptr, mode));
        }

        public static GoshNumber Numerator(GoshObj x)
        {
            return new GoshNumber(GoshInvoke.Scm_Numerator(x.Ptr));
        }

        public static GoshNumber Denominator(GoshObj x)
        {
            return new GoshNumber(GoshInvoke.Scm_Denominator(x.Ptr));
        }

        public static double Magnitude(GoshObj x)
        {
            return GoshInvoke.Scm_Magnitude(x.Ptr);
        }

        public static double Angle(GoshObj x)
        {
            return GoshInvoke.Scm_Angle(x.Ptr);
        }

        public static double RealPart(GoshObj x)
        {
            return GoshInvoke.Scm_RealPart(x.Ptr);
        }

        public static double ImagPart(GoshObj x)
        {
            return GoshInvoke.Scm_ImagPart(x.Ptr);
        }

        public static GoshString NumberToString(GoshObj num, int radix)
        {
            return NumberToString(num, radix, NumberFormat.None);
        }

        public static GoshString NumberToString(GoshObj num, int radix, NumberFormat flags)
        {
            return new GoshString(GoshInvoke.Scm_NumberToString(num.Ptr, radix , flags));
        }

        public static GoshSymbol NativeEndian()
        {
            return new GoshSymbol(GoshInvoke.Scm_NativeEndian());
        }

        public static GoshSymbol DefaultEndian()
        {
            return new GoshSymbol(GoshInvoke.Scm_DefaultEndian());
        }

        public static void SetDefaultEndian(GoshObj endian)
        {
            GoshInvoke.Scm_SetDefaultEndian(endian.Ptr);
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

        public static GoshObj ReverseX(object list)
        {
            return new GoshRefObj(GoshInvoke.Scm_ReverseX(Cast.ToIntPtr(list)));
        }

        public static GoshObj Reverse(object list)
        {
            return new GoshRefObj(GoshInvoke.Scm_Reverse(Cast.ToIntPtr(list)));
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
            return new GoshString(GoshInvoke.Scm_MakeString(str, StringFlags.Copying));
        }

        public static GoshString MakeFillString(int len, char fill)
        {
            return new GoshString(GoshInvoke.Scm_MakeFillString(len, fill));
        }

        public static string GetString(GoshString str)
        {
            UInt32 size, length, flags;
            IntPtr utf8buf = GoshInvoke.Scm_GetStringContent(str.Ptr, out size, out length, out flags);
            byte[] buf = new byte[size];
            Marshal.Copy(utf8buf, buf, 0, (int)size);

            return Encoding.UTF8.GetString(buf);
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
            TypeCheck(obj.Ptr, GoshInvoke.Scm_HashTableP, 0);
            return GoshHashTable.HashTableRef(obj.Ptr, key);
        }

        public static void HashTableSet(GoshObj obj, object key, object value)
        {
            TypeCheck(obj.Ptr, GoshInvoke.Scm_HashTableP, 0);
            GoshHashTable.HashTableSet(obj.Ptr, key, value);
        }

        public static object HashTableDelete(GoshObj obj, object key)
        {
            TypeCheck(obj.Ptr, GoshInvoke.Scm_HashTableP, 0);
            return GoshHashTable.HashTableDelete(obj.Ptr, key);
        }

        public static GoshObj HashTableKeys(GoshObj obj)
        {
            TypeCheck(obj.Ptr, GoshInvoke.Scm_HashTableP, 0);
            return new GoshRefObj(GoshInvoke.Scm_HashTableKeys(obj.Ptr));
        }

        public static GoshObj HashTableValues(GoshObj obj)
        {
            TypeCheck(obj.Ptr, GoshInvoke.Scm_HashTableP, 0);
            return new GoshRefObj(GoshInvoke.Scm_HashTableValues(obj.Ptr));
        }

        public static GoshObj HashTableStat(GoshObj obj)
        {
            TypeCheck(obj.Ptr, GoshInvoke.Scm_HashTableP, 0);
            return new GoshRefObj(GoshInvoke.Scm_HashTableStat(obj.Ptr));
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

        #region vector.h {

        public static GoshVector MakeVector(int size)
        {
            return new GoshVector(size);
        }

        public static GoshVector MakeVector(int size, GoshObj fill)
        {
            return new GoshVector(size, fill);
        }

        public static GoshObj VectorRef(GoshObj vec, int i, GoshObj fallback)
        {
            TypeCheck(vec.Ptr, GoshInvoke.Scm_VectorP, 0);
            return new GoshRefObj(GoshInvoke.Scm_VectorRef(vec.Ptr, i, fallback.Ptr));
        }

        public static GoshObj VectorSet(GoshObj vec, int i, GoshObj obj)
        {
            TypeCheck(vec.Ptr, GoshInvoke.Scm_VectorP, 0);
            return new GoshRefObj(GoshInvoke.Scm_VectorRef(vec.Ptr, i, obj.Ptr));
        }

        public static GoshObj VectorFill(GoshObj vec, GoshObj fill, int start, int end)
        {
            TypeCheck(vec.Ptr, GoshInvoke.Scm_VectorP, 0);
            return new GoshRefObj(GoshInvoke.Scm_VectorFill(vec.Ptr, fill.Ptr, start, end));
        }

        public static GoshVector ListToVector(GoshObj list)
        {
            return ListToVector(list, 0, -1);
        }
        public static GoshVector ListToVector(GoshObj list, int start)
        {
            return ListToVector(list, start, -1);
        }

        public static GoshVector ListToVector(GoshObj list, int start, int end)
        {
            return new GoshVector(GoshInvoke.Scm_ListToVector(list.Ptr, start, end));
        }

        public static GoshObj VectorToList(GoshObj vec)
        {
            return VectorToList(vec, 0);
        }
        public static GoshObj VectorToList(GoshObj vec, int start)
        {
            TypeCheck(vec.Ptr, GoshInvoke.Scm_VectorP, 0);

            int end;
            unsafe
            {
                end= (int)((GaucheDotNet.Native.ScmVector*)vec.Ptr)->size;
            }
            return new GoshRefObj(GoshInvoke.Scm_VectorToList(vec.Ptr, start, end));
        }

        public static GoshObj VectorToList(GoshObj vec, int start, int end)
        {
            TypeCheck(vec.Ptr, GoshInvoke.Scm_VectorP, 0);
            return new GoshRefObj(GoshInvoke.Scm_VectorToList(vec.Ptr, start, end));
        }

        public static GoshVector VectorCopy(GoshObj vec)
        {
            return VectorCopy(vec, 0, -1, Gosh.Undefined);
        }

        public static GoshVector VectorCopy(GoshObj vec, int start)
        {
            return VectorCopy(vec, start, -1, Gosh.Undefined);
        }

        public static GoshVector VectorCopy(GoshObj vec, int start, int end)
        {
            return VectorCopy(vec, start, end, Gosh.Undefined);
        }

        public static GoshVector VectorCopy(GoshObj vec, int start, int end, GoshObj fill)
        {
            TypeCheck(vec.Ptr, GoshInvoke.Scm_VectorP, 0);
            return new GoshVector(GoshInvoke.Scm_VectorCopy(vec.Ptr, start, end, fill.Ptr));
        }

        //
        // UVector
        //

        public static UVectorType UVectorType(GoshObj uvec)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_UVectorType(GoshInvoke.Scm_ClassOf(uvec.Ptr));
        }

        public static String UVectorTypeName(GoshObj uvec)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_UVectorTypeName(
                GoshInvoke.Scm_UVectorType(GoshInvoke.Scm_ClassOf(uvec.Ptr)));
        }

        public static int UVectorElementSize(GoshObj uvec)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_UVectorElementSize(GoshInvoke.Scm_ClassOf(uvec.Ptr));
        }

        public static int UVectorSizeInBytes(GoshObj uvec)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_UVectorSizeInBytes(uvec.Ptr);
        }

        public static GoshUVector UVectorSizeInBytes(UVectorType type, int size)
        {
            IntPtr klass =  GoshInvoke.Scm_GetUVectorClass(type);
            if (klass == IntPtr.Zero)
            {
                throw new ArgumentException();
            }

            return new GoshUVector(GoshInvoke.Scm_MakeUVector(klass, size, IntPtr.Zero));
        }

        public static GoshObj UVectorRef(GoshObj uvec, UVectorType type, int index, GoshObj fallback)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);

            return new GoshRefObj(GoshInvoke.Scm_VMUVectorRef(uvec.Ptr, type, index, fallback.Ptr));
        }

        public static int UVectorLength(GoshObj uvec)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_UVectorLength(uvec.Ptr);
        }

        public static GoshUVector MakeS8Vector(int size, sbyte fill)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeS8Vector(size, fill));
        }

        public static GoshUVector MakeS8Vector(sbyte[] array)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeS8VectorFromArray(array.Length, array));
        }

        public static sbyte S8VectorRef(GoshObj uvec, int index)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_S8VectorRef(uvec.Ptr, index);
        }

        public static void S8VectorSet(GoshObj uvec, int index, sbyte value)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            GoshInvoke.Scm_S8VectorSet(uvec.Ptr, index, value);
        }

        public static GoshUVector MakeU8Vector(int size, byte fill)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeU8Vector(size, fill));
        }

        public static GoshUVector MakeU8Vector(byte[] array)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeU8VectorFromArray(array.Length, array));
        }

        public static byte U8VectorRef(GoshObj uvec, int index)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_U8VectorRef(uvec.Ptr, index);
        }

        public static void U8VectorSet(GoshObj uvec, int index, byte value)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            GoshInvoke.Scm_U8VectorSet(uvec.Ptr, index, value);
        }

        public static GoshUVector MakeS16Vector(int size, Int16 fill)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeS16Vector(size, fill));
        }

        public static GoshUVector MakeS16Vector(Int16[] array)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeS16VectorFromArray(array.Length, array));
        }

        public static Int16 S16VectorRef(GoshObj uvec, int index)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_S16VectorRef(uvec.Ptr, index);
        }

        public static void S16VectorSet(GoshObj uvec, int index, Int16 value)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            GoshInvoke.Scm_S16VectorSet(uvec.Ptr, index, value);
        }

        public static GoshUVector MakeU16Vector(int size, UInt16 fill)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeU16Vector(size, fill));
        }

        public static GoshUVector MakeU16Vector(UInt16[] array)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeU16VectorFromArray(array.Length, array));
        }

        public static UInt16 U16VectorRef(GoshObj uvec, int index)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_U16VectorRef(uvec.Ptr, index);
        }

        public static void U16VectorSet(GoshObj uvec, int index, UInt16 value)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            GoshInvoke.Scm_U16VectorSet(uvec.Ptr, index, value);
        }

        public static GoshUVector MakeS32Vector(int size, Int32 fill)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeS32Vector(size, fill));
        }

        public static GoshUVector MakeS32Vector(Int32[] array)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeS32VectorFromArray(array.Length, array));
        }

        public static Int32 S32VectorRef(GoshObj uvec, int index)
        {
            TypeCheck(uvec.Ptr , GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_S32VectorRef(uvec.Ptr, index);
        }

        public static void S32VectorSet(GoshObj uvec, int index, Int32 value)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            GoshInvoke.Scm_S32VectorSet(uvec.Ptr, index, value);
        }

        public static GoshUVector MakeU32Vector(int size, UInt32 fill)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeU32Vector(size, fill));
        }

        public static GoshUVector MakeU32Vector(UInt32[] array)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeU32VectorFromArray(array.Length, array));
        }

        public static UInt32 U32VectorRef(GoshObj uvec, int index)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_U32VectorRef(uvec.Ptr, index);
        }

        public static void U32VectorSet(GoshObj uvec, int index, UInt32 value)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            GoshInvoke.Scm_U32VectorSet(uvec.Ptr, index, value);
        }

        public static GoshUVector MakeS64Vector(int size, Int64 fill)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeS64Vector(size, fill));
        }

        public static GoshUVector MakeS64Vector(Int64[] array)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeS64VectorFromArray(array.Length, array));
        }

        public static Int64 S64VectorRef(GoshObj uvec, int index)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_S64VectorRef(uvec.Ptr, index);
        }

        public static void S64VectorSet(GoshObj uvec, int index, Int64 value)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            GoshInvoke.Scm_S64VectorSet(uvec.Ptr, index, value);
        }

        public static GoshUVector MakeU64Vector(int size, UInt64 fill)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeU64Vector(size, fill));
        }

        public static GoshUVector MakeU64Vector(UInt64[] array)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeU64VectorFromArray(array.Length, array));
        }

        public static UInt64 U64VectorRef(GoshObj uvec, int index)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_U64VectorRef(uvec.Ptr, index);
        }

        public static void U64VectorSet(GoshObj uvec, int index, UInt64 value)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            GoshInvoke.Scm_U64VectorSet(uvec.Ptr, index, value);
        }

        public static GoshUVector MakeF16Vector(int size, double fill)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeF16Vector(size, GoshInvoke.Scm_DoubleToHalf(fill)));
        }

        public static double F16VectorRef(GoshObj uvec, int index)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_F16VectorRef(uvec.Ptr, index);
        }

        public static void F16VectorSet(GoshObj uvec, int index, double value)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            GoshInvoke.Scm_F16VectorSet(uvec.Ptr, index, value);
        }

        public static GoshUVector MakeF32Vector(int size, Single fill)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeF32Vector(size, fill));
        }

        public static GoshUVector MakeF32Vector(Single[] array)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeF32VectorFromArray(array.Length, array));
        }

        public static Single F32VectorRef(GoshObj uvec, int index)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_F32VectorRef(uvec.Ptr, index);
        }

        public static void F32VectorSet(GoshObj uvec, int index, Single value)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            GoshInvoke.Scm_F32VectorSet(uvec.Ptr, index, value);
        }

        public static GoshUVector MakeF64Vector(int size, double fill)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeF64Vector(size, fill));
        }

        public static GoshUVector MakeF64Vector(double[] array)
        {
            return new GoshUVector(GoshInvoke.Scm_MakeF64VectorFromArray(array.Length, array));
        }

        public static double F64VectorRef(GoshObj uvec, int index)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            return GoshInvoke.Scm_F64VectorRef(uvec.Ptr, index);
        }

        public static void F64VectorSet(GoshObj uvec, int index, double value)
        {
            TypeCheck(uvec.Ptr, GoshInvoke.Scm_UVectorP, 0);
            GoshInvoke.Scm_F64VectorSet(uvec.Ptr, index, value);
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

        #region gauche_dotnet {

        public static bool IsList(object obj)
        {
            GoshObj goshObj = obj as GoshObj;
            if (goshObj == null) return false;

            return GoshInvoke.Scm_ListP(goshObj.Ptr);
        }

        public static bool IsPair(object obj)
        {
            GoshObj goshObj = obj as GoshObj;
            if (goshObj == null) return false;

            return GoshInvoke.Scm_PairP(goshObj.Ptr);
        }

        public static bool IsNull(object obj)
        {
            GoshObj goshObj = obj as GoshObj;
            if (goshObj == null) return false;

            return GoshInvoke.Scm_NullP(goshObj.Ptr);
        }

        public static bool IsString(object obj)
        {
            GoshObj goshObj = obj as GoshObj;
            if (goshObj == null) return false;

            return GoshInvoke.Scm_StringP(goshObj.Ptr);
        }

        public static bool IsSymbol(object obj)
        {
            GoshObj goshObj = obj as GoshObj;
            if (goshObj == null) return false;

            return GoshInvoke.Scm_SymbolP(goshObj.Ptr);
        }

        public static bool IsKeyword(object obj)
        {
            GoshObj goshObj = obj as GoshObj;
            if (goshObj == null) return false;

            return GoshInvoke.Scm_KeywordP(goshObj.Ptr);
        }

        public static bool IsBignum(object obj)
        {
            GoshObj goshObj = obj as GoshObj;
            if (goshObj == null) return false;

            return GoshInvoke.Scm_BignumP(goshObj.Ptr);
        }

        public static bool IsVector(object obj)
        {
            GoshObj goshObj = obj as GoshObj;
            if (goshObj == null) return false;

            return GoshInvoke.Scm_VectorP(goshObj.Ptr);
        }

        public static bool IsUVector(object obj)
        {
            GoshObj goshObj = obj as GoshObj;
            if (goshObj == null) return false;

            return GoshInvoke.Scm_UVectorP(goshObj.Ptr);
        }

        public static bool IsExtendedPair(object obj)
        {
            GoshObj goshObj = obj as GoshObj;
            if (goshObj == null) return false;

            return GoshInvoke.Scm_ExtendedPairP(goshObj.Ptr);
        }

        public static bool IsHashTable(object obj)
        {
            GoshObj goshObj = obj as GoshObj;
            if (goshObj == null) return false;

            return GoshInvoke.Scm_HashTableP(goshObj.Ptr);
        }

        public static bool IsVM(object obj)
        {
            GoshObj goshObj = obj as GoshObj;
            if (goshObj == null) return false;

            return GoshInvoke.Scm_VMP(goshObj.Ptr);
        }

        public static bool IsPort(object obj)
        {
            GoshObj goshObj = obj as GoshObj;
            if (goshObj == null) return false;

            return GoshInvoke.Scm_PortP(goshObj.Ptr);
        }

        #endregion }

    }
}

