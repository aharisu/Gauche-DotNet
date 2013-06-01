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

    #region {
    public enum CmpMode : int
    {
        Eq = 0,
        Eqv = 1,
        Equal = 2,
    }
    #endregion }

    #region number.h {

    public enum ClampMode : int
    {
        /// <summary>
        /// throws an error when out-of-range
        /// </summary>
        Error = 0,
        Hi = 1,
        Lo = 2,
        Both = 3,
        /// <summary>
        /// do not convert when out-of-range
        /// </summary>
        None = 4,
    }

    public enum RoundMode : int
    {
        Floor = 0,
        Ceil,
        Trunc,
        Round,
    }

    [Flags]
    public enum NumberFormat : int
    {
        None = 0,
        /// <summary>
        /// use ABCDEF.. for base > 10
        /// </summary>
        UseUpper = 1,
        /// <summary>
        /// show '+' in positive number
        /// </summary>
        ShowPlus = 1 << 1,
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

    [Flags]
    public enum BindingFlag : int
    {
        None = 0,
        StayInModule = 1 << 0,
        Const = 1 << 1,
        Inlinable = 1 << 2,
#if !GAUCHE_9_3_3
        SCM_BINDING_EXTERNAL = (1 << 3)
#endif
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

    #region vector.h {

    public enum UVectorType : int
    {
        S8 = 0,
        U8,
        S16,
        U16,
        S32,
        U32,
        S64,
        U64,
        F16,
        F32,
        F64,
    }

    #endregion }

    #region hash.h {

    public enum HashType : int
    {
        Eq = 0,
        Eqv,
        Equal,
        String,
        General,
    }

    #endregion }

    #region collection.h {

    /// <summary>
    /// Common operation argument for *Search functions
    /// </summary>
    public enum DictOp : int
    {
        /// <summary>
        /// returns ScmDictEntry* if found, NULL otherwise.
        /// </summary>
        Get = 0,
        /// <summary>
        /// if not found, create a new entry.always return ScmDictEntry*.
        /// </summary>
        Create,
        /// <summary>
        /// deletes found entry
        /// </summary>
        Delete
    }

    /// <summary>
    /// Common flags for *Set functions
    /// </summary>
    [Flags]
    public enum DictSetFlags : int
    {
        None = 0,

        /// <summary>
        /// do not overwrite the existing entry
        /// </summary>
        NoOverwrite = (1<< 0),

        /// <summary>
        /// do not create new one if no match
        /// </summary>
        NoCreate = (1 << 1),
    }

    #endregion }

}
