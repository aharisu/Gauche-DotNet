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

        #region gauche_dotnet {

        public static void Initialize()
        {
            GoshInvoke.GaucheDotNetInitialize();
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

