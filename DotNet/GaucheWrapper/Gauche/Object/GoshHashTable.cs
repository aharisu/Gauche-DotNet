/*
 * GoshKeyword.cs
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
using System.Text;
using GaucheDotNet.Native;
using System.Runtime.InteropServices;
using System.Collections;

namespace GaucheDotNet
{
    public class GoshHashEntry
    {
        public object Key { get; internal set; }
        public object Value { get; internal set; }

        public GoshHashEntry(object key, object value)
        {
            this.Key = key;
            this.Value = value;
        }
    }

    public class GoshHashTable : GoshObj, System.Collections.Generic.IEnumerable<GoshHashEntry>
    {
        protected readonly IntPtr _ptr;

        public GoshHashTable(HashType type)
            :this(type, 0)
        {
        }

        public GoshHashTable(HashType type, int initSize)
        {
            _ptr = GoshInvoke.Scm_MakeHashTableSimple(type, initSize);
        }

        public GoshHashTable(IntPtr ptr)
        {
            this._ptr = ptr;
        }

        public override IntPtr Ptr 
        {
            get { return _ptr; } 
        }

        /// <summary>
        /// System.Collections.Hashtableクラスのオブジェクトを取得します
        /// </summary>
        public override object Object
        {
            get { return ToHashtable(_ptr); }
        }

        /// <summary>
        /// 存在しないキーで取得した場合GoshUnboundが返ります
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[object key]
        {
            get
            {
                return HashTableRef(_ptr, key);
            }
            set
            {
                HashTableSet(_ptr, key, value);
            }
        }

        public HashType Type
        {
            get
            {
                unsafe
                {
                    return (HashType)((ScmHashTable*)_ptr)->type;
                }
            }
        }

        public GoshObj Keys
        {
            get
            {
                return new GoshRefObj(GoshInvoke.Scm_HashTableKeys(_ptr));
            }
        }

        public GoshObj Values
        {
            get
            {
                return new GoshRefObj(GoshInvoke.Scm_HashTableValues(_ptr));
            }
        }

        private static IntPtr GetKey(IntPtr ptr, object key)
        {
            unsafe
            {
                if (((ScmHashTable*)ptr)->type == (int)HashType.String)
                {
                    String strKey = key as String;
                    if (strKey != null)
                    {
                        return GoshInvoke.Scm_MakeString(strKey, -1, -1, StringFlags.Copying);
                    }
                }
            }

            return Cast.ToIntPtr(key);
        }

        public static object HashTableRef(IntPtr ptr, object key)
        {
            IntPtr ptrKey = GetKey(ptr, key);
            IntPtr val = GoshInvoke.Scm_HashTableRef(ptr, ptrKey, (IntPtr)GoshInvoke.SCM_UNBOUND);
            return Cast.ToObj(val);
        }

        public static void HashTableSet(IntPtr ptr, object key, object value)
        {
            IntPtr ptrKey = GetKey(ptr, key);
            IntPtr ptrValue = Cast.ToIntPtr(value);

            GoshInvoke.Scm_HashTableSet(ptr, ptrKey, ptrValue, DictSetFlags.None);
        }

        public static object HashTableDelete(IntPtr ptr, object key)
        {
            IntPtr ptrKey = GetKey(ptr, key);

            IntPtr ret = GoshInvoke.Scm_HashTableDelete(ptr, ptrKey);
            return Cast.ToObj(ret);
        }

        public static Hashtable ToHashtable(IntPtr ptr)
        {
            Hashtable hash = new Hashtable();

            using (GoshHashTableIter iter = new GoshHashTableIter(ptr, true))
            {
                while (iter.MoveNext())
                {
                    GoshHashEntry e = iter.Current;
                    hash[e.Key] = e.Value;
                }
            }

            return hash;
        }

        public static GoshHashTable FromDictionary(Hashtable dict, HashType type)
        {
            GoshHashTable table = new GoshHashTable(GoshInvoke.Scm_MakeHashTableSimple(type, dict.Keys.Count));

            foreach(DictionaryEntry pair in dict)
            {
                table[pair.Key] = pair.Value;
            }

            return table;
        }

        #region IEnumerable<GoshHashEntry> メンバー

        private sealed class GoshHashTableIter : System.Collections.Generic.IEnumerator<GoshHashEntry>
        {
            private readonly IntPtr _table;
            private readonly GCHandle _iterHandle;
            private readonly bool _isRecycle;
            private GoshHashEntry _cur = null;

            public GoshHashTableIter(IntPtr table, bool isRecycle)
            {
                this._table = table;
                _iterHandle = GCHandle.Alloc(new ScmHashIter(), GCHandleType.Pinned);
                _isRecycle = isRecycle;

                this.Reset();
            }


            #region IEnumerator<GoshHashEntry> メンバ

            public GoshHashEntry Current
            {
                get { return _cur; }
            }

            #endregion

            #region IDisposable メンバ

            public void Dispose()
            {
                _iterHandle.Free();
            }

            #endregion

            #region IEnumerator メンバ

            object System.Collections.IEnumerator.Current
            {
                get { return _cur; }
            }

            public bool MoveNext()
            {
                unsafe
                {
                    IntPtr ret = GoshInvoke.Scm_HashIterNext((IntPtr)_iterHandle.AddrOfPinnedObject());
                    if (ret == IntPtr.Zero)
                    {
                        _cur = null;
                        return false;
                    }

                    ScmDictEntry* entry = (ScmDictEntry*)ret;
                    object key = Cast.ToObj(entry->key);
                    if (((ScmHashTable*)_table)->type == (int)HashType.String)
                    { //文字列タイプのHashTableでKeyがGoshString型であれば、.NetのStringに変換
                        GoshString strKey = key as GoshString;
                        if (strKey != null)
                        {
                            key = strKey.Object;
                        }
                    }

                    if (_isRecycle)
                    {
                        _cur.Key = key;
                        _cur.Value = Cast.ToObj(entry->value);
                    }
                    else
                    {
                        _cur = new GoshHashEntry(key, Cast.ToObj(entry->value));
                    }
                    return true;
                }
            }

            public void Reset()
            {
                unsafe
                {
                    ScmHashCore* pCore = &((ScmHashTable*)_table)->core;

                    GoshInvoke.Scm_HashIterInit((IntPtr)_iterHandle.AddrOfPinnedObject(), (IntPtr)pCore);
                }

                //isRecycleがtrueのときは戻り値のオブジェクトを使いまわす
                if (_isRecycle)
                {
                    _cur = new GoshHashEntry(null, null);
                }
            }

            #endregion
        }

        public System.Collections.Generic.IEnumerator<GoshHashEntry> GetEnumerator()
        {
            return new GoshHashTableIter(_ptr, false);
        }

        #endregion

        #region IEnumerable メンバー

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
