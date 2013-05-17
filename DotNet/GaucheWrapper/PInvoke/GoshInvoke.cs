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

        //
        // FIXNUM
        //

        //
        // FLONUM
        //

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


        #region gauche_dotnet {

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GaucheDotNetInitialize();

        /// <param name="data">IntPtr(GCHandle)</param>
        /// <returns>ScmObj(GdnObject*)</returns>
        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Scm_MakeClrObject(IntPtr data);

        [DllImport(GaucheDotNetLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Scm_IsKnownType(IntPtr ptr);

        #endregion }

    }
}

