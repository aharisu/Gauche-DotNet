/*
 * GoshInvoke.cs
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

namespace GaucheDotNet.Native
{
    public static partial class GoshInvoke
    {
        public const string GaucheLib = "libgauche-0.9.dll";
        public const string GaucheDotNetLib = "gauche_dotnet.dll";


#if X64
        internal const Int64 SCM_FALSE = ((0) << 8) + 0x0b;
        internal const Int64 SCM_TRUE = ((1) << 8) + 0x0b;
        internal const Int64 SCM_NIL = ((2) << 8) + 0x0b;
        internal const Int64 SCM_EOF = ((3) << 8) + 0x0b;
        internal const Int64 SCM_UNDEFINED = ((4) << 8) + 0x0b;
        internal const Int64 SCM_UNBOUND = ((5) << 8) + 0x0b;
#else
        internal const Int32 SCM_FALSE = ((0) << 8) + 0x0b;
        internal const Int32 SCM_TRUE = ((1) << 8) + 0x0b;
        internal const Int32 SCM_NIL = ((2) << 8) + 0x0b;
        internal const Int32 SCM_EOF = ((3) << 8) + 0x0b;
        internal const Int32 SCM_UNDEFINED = ((4) << 8) + 0x0b;
        internal const Int32 SCM_UNBOUND = ((5) << 8) + 0x0b;

#endif


        //TODO declare type

        //TODO declare const object (e.g. SCM_FALSE SCM_TRUE SCM_NIL)

        //
        // BOOLEAN
        //
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Scm_EqP(IntPtr x, IntPtr y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Scm_EqvP(IntPtr x, IntPtr y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Scm_EqualP(IntPtr x, IntPtr y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Scm_EqualM(IntPtr x, IntPtr y, CmpMode mode);

        #region bignum.h {

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeBignumFromSI(Int32 val);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeBignumFromUI(UInt32 val);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeBignumFromUIArray(Int32 sign,
            [MarshalAs(UnmanagedType.LPArray)] UInt32[] values,
            Int32 size);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeBignumFromDouble(Double val);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumCopy(IntPtr b);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumToString(IntPtr b, Int32 radix, Int32 use_upper);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Scm_BignumToSI(IntPtr b,
            [MarshalAs(UnmanagedType.I4)] ClampMode clamp,
            [MarshalAs(UnmanagedType.Bool)] out bool oor);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 Scm_BignumToUI(IntPtr b,
            [MarshalAs(UnmanagedType.I4)] ClampMode clamp,
            [MarshalAs(UnmanagedType.Bool)] out bool oor);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int64 Scm_BignumToSI64(IntPtr b,
            [MarshalAs(UnmanagedType.I4)] ClampMode clamp,
            [MarshalAs(UnmanagedType.Bool)] out bool oor);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 Scm_BignumToUI64(IntPtr b,
            [MarshalAs(UnmanagedType.I4)] ClampMode clamp,
            [MarshalAs(UnmanagedType.Bool)] out bool oor);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Double Scm_BignumToDouble(IntPtr b);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_NormalizeBignum(IntPtr b);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumNegate(IntPtr b);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_BignumCmp(IntPtr bx, IntPtr by);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_BignumAbsCmp(IntPtr bx, IntPtr by);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_BignumCmp3U(IntPtr bx, IntPtr off, IntPtr by);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumComplement(IntPtr bx);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumAdd(IntPtr bx, IntPtr by);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumAddSI(IntPtr bx, Int32 y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumSub(IntPtr bx, IntPtr by);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumSubSI(IntPtr bx, Int32 y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumMul(IntPtr bx, IntPtr by);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumMulSI(IntPtr bx, Int32 y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumDivSI(IntPtr bx, Int32 y, out Int32 remainder);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumDivRem(IntPtr bx, IntPtr by);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Scm_BignumRemSI(IntPtr bx, Int32 y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumLogAnd(IntPtr bx, IntPtr by);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumLogIor(IntPtr bx, IntPtr by);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumLogXor(IntPtr bx, IntPtr by);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumLogNot(IntPtr bx);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Scm_BignumLogCount(IntPtr b);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumAsh(IntPtr bx, Int32 cnt);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeBignumWithSize(Int32 size, UInt32 init);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_BignumAccMultAddUI(IntPtr acc, UInt32 coef, UInt32 c);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Scm_DumpBignum(IntPtr b, IntPtr outPort);

        /// <param name="obj">ScmObj</param>
        /// <returns>objがScm_Bignumのオブジェクトならtrue</returns>
        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_BignumP(IntPtr obj);

        #endregion }

        #region number.h {

        //TODO SIZEOF_LONG >= 8

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeInteger(Int32 i);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeIntegerU(UInt32 i);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Scm_GetIntegerClamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, out int oor);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Scm_GetIntegerClamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, IntPtr oor);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 Scm_GetIntegerUClamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, out int oor);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 Scm_GetIntegerUClamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, IntPtr oor);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Scm_GetInteger8Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, out int oor);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Scm_GetInteger8Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, IntPtr oor);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 Scm_GetIntegerU8Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, out int oor);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 Scm_GetIntegerU8Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, IntPtr oor);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Scm_GetInteger16Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, out int oor);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Scm_GetInteger16Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, IntPtr oor);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 Scm_GetIntegerU16Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, out int oor);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 Scm_GetIntegerU16Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, IntPtr oor);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Scm_GetInteger32Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, out int oor);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Scm_GetInteger32Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, IntPtr oor);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 Scm_GetIntegerU32Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, out int oor);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 Scm_GetIntegerU32Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, IntPtr oor);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeInteger64(Int64 i);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeIntegerU64(UInt64 i);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int64 Scm_GetInteger64Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, out int oor);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int64 Scm_GetInteger64Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, IntPtr oor);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 Scm_GetIntegerU64Clamp(IntPtr obj
            , [MarshalAs(UnmanagedType.I4)]ClampMode clamp, IntPtr oor);

        //ScmRatnum(Rational)

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeRatnum(IntPtr numer, IntPtr denom);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeRational(IntPtr numer, IntPtr denom);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ReduceRational(IntPtr rational);

        //ScmFlonum

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeFlonum(double d);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern double Scm_GetDouble(IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern double Scm_HalfToDouble(UInt16 v);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt16 Scm_DoubleToHalf(double v);

        //ScmCompnum

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeCompnum(double real, double imag);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeComplex(double real, double imag);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeComplexPolar(double real, double imag);

        //Operation

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_IntegerP(IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_OddP(IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_FiniteP(IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_InfiniteP(IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_NanP(IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Abs(IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_Sign(IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Negate(IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Reciprocal(IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ReciprocalInexact(IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_InexactToExact(IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ExactToInexact(IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Add(IntPtr obj1, IntPtr obj2);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Sub(IntPtr obj1, IntPtr obj2);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Mul(IntPtr obj1, IntPtr obj2);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Div(IntPtr obj1, IntPtr obj2);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_DivInexact(IntPtr obj1, IntPtr obj2);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_DivCompat(IntPtr obj1, IntPtr obj2);

        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <param name="rem">allow NULL</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Quotient(IntPtr obj1, IntPtr obj2, IntPtr rem);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Modulo(IntPtr obj1, IntPtr obj2
            , [MarshalAs(UnmanagedType.I4)] bool remainder);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Gcd(IntPtr x, IntPtr y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Expt(IntPtr x, IntPtr y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Scm_TwosPower(IntPtr n);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_NumEq(IntPtr x, IntPtr y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_NumLT(IntPtr x, IntPtr y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_NumLE(IntPtr x, IntPtr y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_NumGT(IntPtr x, IntPtr y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_NumGE(IntPtr x, IntPtr y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_NumCmp(IntPtr x, IntPtr y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_MinMax(IntPtr arg0, IntPtr args, out IntPtr min, IntPtr max);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_MinMax(IntPtr arg0, IntPtr args, IntPtr min, out IntPtr max);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_MinMax(IntPtr arg0, IntPtr args, out IntPtr min, out IntPtr max);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_LogAnd(IntPtr x, IntPtr y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_LogIor(IntPtr x, IntPtr y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_LogXor(IntPtr x, IntPtr y);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_LogNot(IntPtr x);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Ash(IntPtr x, Int32 cnt);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Round(IntPtr num,
            [MarshalAs(UnmanagedType.I4)] RoundMode mode);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_RoundToExact(IntPtr num,
            [MarshalAs(UnmanagedType.I4)] RoundMode mode);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Numerator(IntPtr num);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Denominator(IntPtr num);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern double Scm_Magnitude(IntPtr z);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern double Scm_Angle(IntPtr z);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern double Scm_RealPart(IntPtr z);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern double Scm_ImagPart(IntPtr z);

        //TODO
        //ScmNumberFormatInit
        //Scm_PrintNumber
        //Scm_PrintDouble

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_NumberToString(IntPtr num, int radix,
            [MarshalAs(UnmanagedType.U4)]NumberFormat flags);

        /// <param name="str">ScmString*</param>
        /// <param name="radix"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_StringToNumber(IntPtr str, int radix,
            [MarshalAs(UnmanagedType.U4)]NumberFormat flags);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_NativeEndian();

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_DefaultEndian();

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_SetDefaultEndian(IntPtr endian);

        #endregion }

        //
        // CHARACTERS
        //

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_DigitToInt(IntPtr ch, int radix);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_IntToDigit(int n, int radix);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_CharToUcs(IntPtr ch);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_UcsToChar(int ucs);
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_CharEncodingName();
        //TODO
        //[DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        //private static extern char** Scm_SupportedCharacterEncodings();
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_SupportedCharacterEncodingP(string encoding);

        #region vm.h

        public const int SCM_VM_MAX_VALUES = 20;

        /// <param name="form">ScmObj</param>
        /// <param name="env">ScmObj</param>
        /// <param name="packet">ScmEvalPacket*</param>
        /// <returns>int</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_Eval(IntPtr form, IntPtr env, IntPtr packet);

        /// <param name="form">const char*</param>
        /// <param name="env">ScmObj</param>
        /// <param name="packet">ScmEvalPacket*</param>
        /// <returns>int</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_EvalCString(string form, IntPtr env, IntPtr packet);

        /// <param name="proc">ScmObj</param>
        /// <param name="args">ScmObj</param>
        /// <param name="packet">ScmEvalPacket*</param>
        /// <returns>int</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_Apply(IntPtr proc, IntPtr args, IntPtr packet);

        /// <param name="form">ScmObj</param>
        /// <param name="env">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_EvalRec(IntPtr form, IntPtr env);

        /// <param name="proc">ScmObj</param>
        /// <param name="args">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ApplyRec(IntPtr proc, IntPtr args);

        /// <param name="proc">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ApplyRec0(IntPtr proc);

        /// <param name="proc">ScmObj</param>
        /// <param name="arg0">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ApplyRec1(IntPtr proc, IntPtr arg0);

        /// <param name="proc">ScmObj</param>
        /// <param name="arg0">ScmObj</param>
        /// <param name="arg1">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ApplyRec2(IntPtr proc, IntPtr arg0, IntPtr arg1);

        /// <param name="proc">ScmObj</param>
        /// <param name="arg0">ScmObj</param>
        /// <param name="arg1">ScmObj</param>
        /// <param name="arg2">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ApplyRec3(IntPtr proc, IntPtr arg0, IntPtr arg1, 
            IntPtr arg2);

        /// <param name="proc">ScmObj</param>
        /// <param name="arg0">ScmObj</param>
        /// <param name="arg1">ScmObj</param>
        /// <param name="arg2">ScmObj</param>
        /// <param name="arg3">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ApplyRec4(IntPtr proc, IntPtr arg0, IntPtr arg1, 
            IntPtr arg2, IntPtr arg3);

        /// <param name="proc">ScmObj</param>
        /// <param name="arg0">ScmObj</param>
        /// <param name="arg1">ScmObj</param>
        /// <param name="arg2">ScmObj</param>
        /// <param name="arg3">ScmObj</param>
        /// <param name="arg4">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib)]
        public static extern IntPtr Scm_ApplyRec5(IntPtr proc, IntPtr arg0, IntPtr arg1, 
            IntPtr arg2, IntPtr arg3, IntPtr arg4);

        /// <param name="args">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Values(IntPtr args);

        /// <param name="val0">ScmObj</param>
        /// <param name="val1">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Values2(IntPtr val0, IntPtr val1);

        /// <param name="val0">ScmObj</param>
        /// <param name="val1">ScmObj</param>
        /// <param name="val2">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Values3(IntPtr val0, IntPtr val1, IntPtr val2);

        /// <param name="val0">ScmObj</param>
        /// <param name="val1">ScmObj</param>
        /// <param name="val2">ScmObj</param>
        /// <param name="val3">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Values4(IntPtr val0, IntPtr val1, IntPtr val2, IntPtr val3);

        /// <param name="val0">ScmObj</param>
        /// <param name="val1">ScmObj</param>
        /// <param name="val2">ScmObj</param>
        /// <param name="val3">ScmObj</param>
        /// <param name="val4">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Values5(IntPtr val0, IntPtr val1, IntPtr val2, IntPtr val3, IntPtr val4);

        //TODO Scm_VMApply ... VMEval ... VM ...

        #endregion


        #region pair / list {

        /// <param name="car">ScmObj</param>
        /// <param name="cdr">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Cons(IntPtr car, IntPtr cdr);

        /// <param name="caar">ScmObj</param>
        /// <param name="cdar">ScmObj</param>
        /// <param name="cdr">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Acons(IntPtr caar, IntPtr cdar, IntPtr cdr);

        //TODO
        //[DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr Scm_List(IntPtr elt, ...);

        //TODO
        //[DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr Scm_Conses(IntPtr elt, ...);

        /// <param name="obj">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Car(IntPtr obj);

        /// <param name="obj">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Cdr(IntPtr obj);

        /// <param name="obj">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Caar(IntPtr obj);

        /// <param name="obj">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Cadr(IntPtr obj);

        /// <param name="obj">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Cdar(IntPtr obj);

        /// <param name="obj">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Cddr(IntPtr obj);

        /// <param name="obj">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_Length(IntPtr obj);

        /// <param name="list">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_CopyList(IntPtr list);

        /// <param name="fill">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeList(int len, IntPtr fill);

        /// <param name="list">ScmObj</param>
        /// <param name="obj">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Append2X(IntPtr list, IntPtr obj);

        /// <param name="list">ScmObj</param>
        /// <param name="obj">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Append2(IntPtr list, IntPtr obj);

        /// <param name="args">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Append(IntPtr args);

        /// <param name="list">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ListTail(IntPtr list, int i, IntPtr fallback);

        /// <param name="list">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ListRef(IntPtr list, int i, IntPtr fallback);

        /// <param name="list">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_LastPair(IntPtr list);

        /// <param name="obj">ScmObj</param>
        /// <param name="list">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Memq(IntPtr obj, IntPtr list);

        /// <param name="obj">ScmObj</param>
        /// <param name="list">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Memv(IntPtr obj, IntPtr list);

        //TODO cmpmode
        /// <param name="obj">ScmObj</param>
        /// <param name="list">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Member(IntPtr obj, IntPtr list, int cmpmode);

        /// <param name="obj">ScmObj</param>
        /// <param name="alist">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Assq(IntPtr obj, IntPtr alist);

        /// <param name="obj">ScmObj</param>
        /// <param name="alist">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Assv(IntPtr obj, IntPtr alist);

        /// <param name="obj">ScmObj</param>
        /// <param name="alist">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Assoc(IntPtr obj, IntPtr alist, int cmpmode);

        /// <param name="obj">ScmObj</param>
        /// <param name="list">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Delete(IntPtr obj, IntPtr list, int cmpmode);

        /// <param name="obj">ScmObj</param>
        /// <param name="list">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_DeleteX(IntPtr obj, IntPtr list, int cmpmode);

        /// <param name="obj">ScmObj</param>
        /// <param name="alist">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_AssocDelete(IntPtr obj, IntPtr alist, int cmpmode);

        /// <param name="obj">ScmObj</param>
        /// <param name="alist">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_AssocDeleteX(IntPtr obj, IntPtr alist, int cmpmode);

        /// <param name="list">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_DeleteDuplicates(IntPtr list, int cmpmode);

        /// <param name="list">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_DeleteDuplicatesX(IntPtr list, int cmpmode);

        /// <param name="start">ScmObj</param>
        /// <param name="sequences">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MonotonicMerge(IntPtr start, IntPtr sequences);

        /// <param name="sequences">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MonotonicMerge1(IntPtr sequences);

        /// <param name="list1">ScmObj</param>
        /// <param name="list2">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Union(IntPtr list1, IntPtr list2);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Intersection(IntPtr list1, IntPtr list2);

        //
        // Extended Cons

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ExtendedCons(IntPtr car, IntPtr cdr);

        /// <param name="pair">ScmPair*</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_PairAttr(IntPtr pair);

        /// <param name="pair">ScmPair*</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_PairAttrGet(IntPtr pair, IntPtr key, IntPtr fallback);

        /// <param name="pair">ScmPair*</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_PairAttrSet(IntPtr pair, IntPtr key, IntPtr value);

        /// <param name="obj">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Scm_ExtendedPairP(IntPtr obj);

        #endregion }


        #region string.h {

        /// <param name="str">const char*</param>
        /// <param name="size"></param>
        /// <param name="len"></param>
        /// <param name="flags"></param>
        /// <returns>ScmObj(ScmString*)</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeString(
            [MarshalAs(UnmanagedType.LPStr)][In] string str 
            , int size, int len
            , [MarshalAs(UnmanagedType.I4)][In] StringFlags flags);

        /// <param name="len"></param>
        /// <param name="fill">ScmChar</param>
        /// <returns>ScmObj(ScmString*)</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeFillString(int len, [MarshalAs(UnmanagedType.I4)][In] Int32 fill);

        /// <param name="str">ScmString*</param>
        /// <returns>char*</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return : MarshalAs(UnmanagedType.LPStr)]
        public static extern String Scm_GetString(IntPtr str);

        /// <param name="str">ScmString*</param>
        /// <returns>char*</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return : MarshalAs(UnmanagedType.LPStr)]
        public static extern String Scm_GetStringConst(IntPtr str);

        #endregion }

        #region hash.h {

        /// <param name="iter">ScmHashIter*</param>
        /// <param name="core">ScmHashCore*</param>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_HashIterInit(IntPtr iter, IntPtr core);

        /// <param name="iter">ScmHashIter*</param>
        /// <returns>ScmDictEntry*</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_HashIterNext(IntPtr iter);

        /// <param name="type"></param>
        /// <param name="initSize"></param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeHashTableSimple(
            [MarshalAs(UnmanagedType.I4)] HashType type
            , int initSize);

        /// <param name="tab">ScmHashTable*</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_HashTableCopy(IntPtr tab);

        /// <param name="ht">ScmHashTable*</param>
        /// <param name="key">ScmObj</param>
        /// <param name="fallback">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_HashTableRef(IntPtr ht, IntPtr key, IntPtr fallback);

        /// <param name="ht">ScmHashTable*</param>
        /// <param name="key">ScmObj</param>
        /// <param name="value">ScmObj</param>
        /// <param name="flags"></param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_HashTableSet(IntPtr ht, IntPtr key, IntPtr value,
            [MarshalAs(UnmanagedType.I4)]DictSetFlags flags);

        /// <param name="ht">ScmHashTable*</param>
        /// <param name="key">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_HashTableDelete(IntPtr ht, IntPtr key);

        /// <param name="table">ScmHashTable*</param>
        /// <returns>ScmObj (List)</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_HashTableKeys(IntPtr table);

        /// <param name="table">ScmHashTable*</param>
        /// <returns>ScmObj (List)</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_HashTableValues(IntPtr table);

        /// <param name="table">ScmHashTable*</param>
        /// <returns>ScmObj (List)</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_HashTableStat(IntPtr table);

        /// <param name="obj">ScmObj</param>
        /// <returns>objがScm_HashTableのオブジェクトならtrue</returns>
        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_HashTableP(IntPtr obj);

        #endregion }

        #region module.h {

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeModule(IntPtr name, [MarshalAs(UnmanagedType.I4)] bool error_if_exists);

        /// <param name="module">ScmModule*</param>
        /// <param name="symbol">ScmSymbol*</param>
        /// <param name="flags"></param>
        /// <returns>ScmGloc*</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_FindBinding(IntPtr module, IntPtr symbol, [MarshalAs(UnmanagedType.I4)] BindingFlag flags);

        /// <param name="module">ScmModule*</param>
        /// <param name="symbol">ScmSymbol*</param>
        /// <param name="value">ScmObj</param>
        /// <param name="flags"></param>
        /// <returns>ScmGloc*</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeBinding(IntPtr module, IntPtr symbol, IntPtr value, [MarshalAs(UnmanagedType.I4)] BindingFlag flags);

#if HAS_DELETE_BINDING
        /// <param name="module">ScmModule*</param>
        /// <param name="symbol">ScmSymbol*</param>
        /// <param name="flags"></param>
        /// <returns>ScmGloc*</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_DeleteBinding(IntPtr module, IntPtr symbol, [MarshalAs(UnmanagedType.I4)] BindingFlag flags);
#endif

        /// <param name="module">ScmModule*</param>
        /// <param name="symbol">ScmSymbol*</param>
        /// <param name="flags"></param>
        /// <returns>ScmGloc*</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_GlobalVariableRef(IntPtr module, IntPtr symbol, [MarshalAs(UnmanagedType.I4)] BindingFlag flags);

        /// <param name="module">ScmModule*</param>
        /// <param name="symbol">ScmSymbol*</param>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_HideBinding(IntPtr module, IntPtr symbol);

        /// <param name="target">ScmModule*</param>
        /// <param name="targetName">ScmSymbol*</param>
        /// <param name="origin">ScmModule*</param>
        /// <param name="originName">ScmSymbol*</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_AliasBinding(IntPtr target, IntPtr targetName, IntPtr origin, IntPtr originName);

        /// <param name="module">ScmModule*</param>
        /// <param name="supers">ScmObject</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ExtendModule(IntPtr module, IntPtr supers);

        /// <param name="module">ScmModule*</param>
        /// <param name="imported">ScmObject</param>
        /// <param name="prefix">ScmObject</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ImportModule(IntPtr module, IntPtr imported, IntPtr prefix, uint flags);

        /// <param name="module">ScmModule*</param>
        /// <param name="list">ScmObject</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ExportSymbols(IntPtr module, IntPtr list);

        /// <param name="module">ScmModule*</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ExportAll(IntPtr module);

        /// <param name="name">ScmSymbol*</param>
        /// <param name="flags"></param>
        /// <returns>ScmModule*</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_FindModule(IntPtr name, [MarshalAs(UnmanagedType.I4)] FindModuleFlag flags);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_AllModules();

        /// <param name="module">ScmModule*</param>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_SelectModule(IntPtr module);

#if !GAUCHE_9_3_3

        /// <param name="module">ScmModule*</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ModuleExports(IntPtr module);

#endif

        /// <param name="name">ScmSymbol*</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ModuleNameToPath(IntPtr name);

        /// <param name="path">ScmSymbol*</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_PathToModuleName(IntPtr path);

        /// <returns>ScmModule*</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_NullModule();

        /// <returns>ScmModule*</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_SchemelModule();

        /// <returns>ScmModule*</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_GaucheModule();

        /// <returns>ScmModule*</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_UserModule();

        /// <returns>ScmModule*</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_CurrentModule();

        #endregion }

        #region vector.h {

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeVector(Int32 size, IntPtr fill);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_VectorRef(IntPtr vec, Int32 i, IntPtr fallback);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_VectorSet(IntPtr vec, Int32 i, IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_VectorFill(IntPtr vec, IntPtr fill, Int32 start, Int32 end);

        /// <param name="l">ScmObj(List)</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ListToVector(IntPtr l, Int32 start, Int32 end);

        /// <param name="v">ScmVector*</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_VectorToList(IntPtr v, Int32 start, Int32 end);

        /// <param name="v">ScmVector*</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_VectorCopy(IntPtr v, Int32 start, Int32 end, IntPtr fill);

        /// <param name="obj">ScmObj</param>
        /// <returns>objがScm_Vectorのオブジェクトならtrue</returns>
        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_VectorP(IntPtr obj);

        /// <param name="klass">ScmClass*</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern UVectorType Scm_UVectorType(IntPtr klass);

        /// <param name="klass">ScmClass*</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern String Scm_UVectorTypeName([MarshalAs(UnmanagedType.I4)]UVectorType type);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_UVectorElementSize(IntPtr klass);

        /// <param name="v">ScmUVector*</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_UVectorSizeInBytes(IntPtr v);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeUVector(IntPtr klass, int size, IntPtr init);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeUVectorFill(IntPtr klass, int size, IntPtr init,
            [MarshalAs(UnmanagedType.Bool)] bool immutablep, IntPtr owner);

        /// <param name="v">ScmUVector*</param>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <param name="fallback">ScmObj</param>
        /// <returns></returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_VMUVectorRef(IntPtr v, 
            [MarshalAs(UnmanagedType.I4)] UVectorType type,
            Int32 index, IntPtr fallback);

        /// <param name="type"></param>
        /// <returns>ScmClass*</returns>
        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_GetUVectorClass([MarshalAs(UnmanagedType.I4)]UVectorType type);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_UVectorLength(IntPtr obj);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_UVectorCopy(IntPtr srcVec, IntPtr dest, int sizeInBytes);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_UVectorCopyF16(IntPtr srcVec, IntPtr dest, int length);

        /// <param name="obj">ScmObj</param>
        /// <returns>objがScm_UVectorのオブジェクトならtrue</returns>
        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Scm_UVectorP(IntPtr obj);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeS8Vector(int size, sbyte fill);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeS8VectorFromArray(int size,
            [MarshalAs(UnmanagedType.LPArray)] sbyte[] array);

        //not support Scm_MakeS8VectorFromArrayShared

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern sbyte Scm_S8VectorRef(IntPtr vec, int index);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_S8VectorSet(IntPtr vec, int index, sbyte value);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeU8Vector(int size, byte fill);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeU8VectorFromArray(int size,
            [MarshalAs(UnmanagedType.LPArray)] byte[] array);

        //not support Scm_MakeU8VectorFromArrayShared

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte Scm_U8VectorRef(IntPtr vec, int index);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_U8VectorSet(IntPtr vec, int index, byte value);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeS16Vector(int size, Int16 fill);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeS16VectorFromArray(int size,
            [MarshalAs(UnmanagedType.LPArray)] Int16[] array);

        //not support Scm_MakeS16VectorFromArrayShared

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int16 Scm_S16VectorRef(IntPtr vec, int index);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_S16VectorSet(IntPtr vec, int index, Int16 value);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeU16Vector(int size, UInt16 fill);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeU16VectorFromArray(int size,
            [MarshalAs(UnmanagedType.LPArray)] UInt16[] array);

        //not support Scm_MakeU16VectorFromArrayShared

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt16 Scm_U16VectorRef(IntPtr vec, int index);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_U16VectorSet(IntPtr vec, int index, UInt16 value);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeS32Vector(int size, Int32 fill);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeS32VectorFromArray(int size,
            [MarshalAs(UnmanagedType.LPArray)] Int32[] array);

        //not support Scm_MakeS32VectorFromArrayShared

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 Scm_S32VectorRef(IntPtr vec, int index);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_S32VectorSet(IntPtr vec, int index, Int32 value);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeU32Vector(int size, UInt32 fill);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeU32VectorFromArray(int size,
            [MarshalAs(UnmanagedType.LPArray)] UInt32[] array);

        //not support Scm_MakeU32VectorFromArrayShared

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 Scm_U32VectorRef(IntPtr vec, int index);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_U32VectorSet(IntPtr vec, int index, UInt32 value);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeS64Vector(int size, Int64 fill);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeS64VectorFromArray(int size,
            [MarshalAs(UnmanagedType.LPArray)] Int64[] array);

        //not support Scm_MakeS64VectorFromArrayShared

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int64 Scm_S64VectorRef(IntPtr vec, int index);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_S64VectorSet(IntPtr vec, int index, Int64 value);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeU64Vector(int size, UInt64 fill);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeU64VectorFromArray(int size,
            [MarshalAs(UnmanagedType.LPArray)] UInt64[] array);

        //not support Scm_MakeU64VectorFromArrayShared

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 Scm_U64VectorRef(IntPtr vec, int index);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_U64VectorSet(IntPtr vec, int index, UInt64 value);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeF16Vector(int size, UInt16 fill);

        // not support
        //[DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr Scm_MakeF16VectorFromArray(int size,
        //    [MarshalAs(UnmanagedType.LPArray)] float[] array);

        //not support Scm_MakeF16VectorFromArrayShared

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern double Scm_F16VectorRef(IntPtr vec, int index);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_F16VectorSet(IntPtr vec, int index, double value);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeF32Vector(int size, float fill);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeF32VectorFromArray(int size,
            [MarshalAs(UnmanagedType.LPArray)] float[] array);

        //not support Scm_MakeF32VectorFromArrayShared

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern float Scm_F32VectorRef(IntPtr vec, int index);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_F32VectorSet(IntPtr vec, int index, float value);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeF64Vector(int size, double fill);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeF64VectorFromArray(int size,
            [MarshalAs(UnmanagedType.LPArray)] double[] array);

        //not support Scm_MakeF64VectorFromArrayShared

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern double Scm_F64VectorRef(IntPtr vec, int index);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_F64VectorSet(IntPtr vec, int index, double value);

        #endregion }

        #region symbol.h {

        /// <param name="name">ScmString*</param>
        /// <param name="interned">int</param>
        /// <returns>ScmObject</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeSymbol(IntPtr name, bool interned);

        /// <param name="name">ScmString*</param>
        /// <returns>ScmObject</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Gensym(IntPtr prefix);

        #endregion }

        #region keyword.h {

        /// <param name="name">ScmString*</param>
        /// <returns>ScmObject</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeKeyword(IntPtr name);

        /// <param name="key">ScmObj</param>
        /// <param name="list">ScmObj</param>
        /// <param name="fallback">ScmObj</param>
        /// <returns>ScmObject</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_GetKeyword(IntPtr key, IntPtr list, IntPtr fallback);

        /// <param name="key">ScmObj</param>
        /// <param name="list">ScmObj</param>
        /// <returns>ScmObject</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_DeleteKeyword(IntPtr key, IntPtr list);

        /// <param name="key">ScmObj</param>
        /// <param name="list">ScmObj</param>
        /// <returns>ScmObject</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_DeleteKeywordX(IntPtr key, IntPtr list);

        #endregion }

        #region proc.h {

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ScmSubProc(IntPtr args, int argc, IntPtr data);

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeSubr(
            [MarshalAs(UnmanagedType.FunctionPtr)][In] ScmSubProc func
            , IntPtr data, int required, int optional, IntPtr info);

        #endregion }


        #region exception {

        /// <param name="c">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ConditionMessage(IntPtr c);

        /// <param name="c">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ConditionTypeName(IntPtr c);

        /// <param name="message">ScmString*</param>
        /// <param name="clrException">ScmClrObject*</param>
        /// <returns>ScmCondition*</returns>
        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeClrError(IntPtr message, IntPtr clrException);

        /// <param name="condition">ScmObj</param>
        /// <returns>ScmObj</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_Raise(IntPtr condition);

        #endregion }


        #region gc {

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_GC();

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_PrintStaticRoots();

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_GCSentinel(IntPtr obj,
            [MarshalAs(UnmanagedType.LPStr)][In] string name);

        #endregion

        #region paths.h {

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void Scm_GetPathErrorFn(IntPtr errs);
        private const int GETDIRECTORY_BUFFER_LENGTH = 512;

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Scm_GetLibraryDirectory(StringBuilder buf, int buflen,
            [MarshalAs(UnmanagedType.FunctionPtr)][In] Scm_GetPathErrorFn errfn);

        public static String Scm_GetLibraryDirectory()
        {
            StringBuilder buf = new StringBuilder(GETDIRECTORY_BUFFER_LENGTH);
            GoshInvoke.Scm_GetLibraryDirectory(buf, buf.Capacity, 
                (errs) =>
                    {
                        throw new Exception("Scm_GetLibraryDirectory Error.");
                    });
            return buf.ToString();
        }

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Scm_GetArchitectureDirectory(StringBuilder buf, int buflen,
            [MarshalAs(UnmanagedType.FunctionPtr)][In] Scm_GetPathErrorFn errfn);

        public static String Scm_GetArchitectureDirectory()
        {
            StringBuilder buf = new StringBuilder(GETDIRECTORY_BUFFER_LENGTH);
            GoshInvoke.Scm_GetArchitectureDirectory(buf, buf.Capacity, 
                (errs) =>
                    {
                        throw new Exception("Scm_GetArchitectureDirectory Error.");
                    });
            return buf.ToString();
        }

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Scm_GetSiteLibraryDirectory(StringBuilder buf, int buflen,
            [MarshalAs(UnmanagedType.FunctionPtr)][In] Scm_GetPathErrorFn errfn);

        public static String Scm_GetSiteLibraryDirectory()
        {
            StringBuilder buf = new StringBuilder(GETDIRECTORY_BUFFER_LENGTH);
            GoshInvoke.Scm_GetSiteLibraryDirectory(buf, buf.Capacity, 
                (errs) =>
                    {
                        throw new Exception("Scm_GetSiteLibraryDirectory Error.");
                    });
            return buf.ToString();
        }

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Scm_GetSiteArchitectureDirectory(StringBuilder buf, int buflen,
            [MarshalAs(UnmanagedType.FunctionPtr)][In] Scm_GetPathErrorFn errfn);

        public static String Scm_GetSiteArchitectureDirectory()
        {
            StringBuilder buf = new StringBuilder(GETDIRECTORY_BUFFER_LENGTH);
            GoshInvoke.Scm_GetSiteArchitectureDirectory(buf, buf.Capacity, 
                (errs) =>
                    {
                        throw new Exception("Scm_GetSiteArchitectureDirectory Error.");
                    });
            return buf.ToString();
        }

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Scm_GetRuntimeDirectory(StringBuilder buf, int buflen,
            [MarshalAs(UnmanagedType.FunctionPtr)][In] Scm_GetPathErrorFn errfn);

        public static String Scm_GetRuntimeDirectory()
        {
            StringBuilder buf = new StringBuilder(GETDIRECTORY_BUFFER_LENGTH);
            GoshInvoke.Scm_GetRuntimeDirectory(buf, buf.Capacity, 
                (errs) =>
                    {
                        throw new Exception("Scm_GetRuntimeDirectory Error.");
                    });
            return buf.ToString();
        }

        #endregion }

        #region class.h {

        /// <param name="obj">ScmObj</param>
        /// <returns>ScmClass*</returns>
        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ClassOf(IntPtr obj);

        #endregion }

        #region gauche_dotnet {

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GaucheDotNetInitialize();

        /// <param name="data">IntPtr(GCHandle)</param>
        /// <returns>ScmObj(GdnObject*)</returns>
        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeClrObject(IntPtr data);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_IsKnownType(IntPtr ptr);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Scm_InstallErrorHandler(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr UnwindProtectThunk();

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_WithUnwindProtect(
            [MarshalAs(UnmanagedType.FunctionPtr)][In] UnwindProtectThunk thunk);



        /// <param name="obj">ScmObj</param>
        /// <returns>GCHandle ptr</returns>
        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_ClrConditionInnerException(IntPtr obj);

        #endregion }

        #region gc.h {

        [DllImport(GaucheLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GC_init();

        #endregion }

    }
}

