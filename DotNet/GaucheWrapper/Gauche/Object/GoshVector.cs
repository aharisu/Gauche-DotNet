using System;
using System.Text;
using GaucheDotNet;
using GaucheDotNet.Native;
using System.Runtime.InteropServices;

namespace GaucheDotNet
{
    public class GoshVector : GoshObj, System.Collections.ICollection
    {
        protected readonly IntPtr _ptr;

        public GoshVector(int size)
            :this(size, GoshUndefined.Undefined)
        {
        }

        public GoshVector(int size, GoshObj fill)
        {
            _ptr = GoshInvoke.Scm_MakeVector(size, fill.Ptr);
        }

        public GoshVector(IntPtr ptr)
        {
            this._ptr = ptr;
        }

        public override IntPtr Ptr 
        {
            get { return _ptr; } 
        }

        /// <summary>
        /// object[]型のオブジェクトを取得します
        /// </summary>
        public override object Object
        {
            get { return ToArray(_ptr); }
        }

        public object this[int index]
        {
            get
            {
                return Cast.ToClrObject(GoshInvoke.Scm_VectorRef(_ptr, index, GoshUnbound.Unbound.Ptr));
            }
            set
            {
                GoshInvoke.Scm_VectorSet(_ptr, index, Cast.ToGoshObjPtr(value));
            }
        }

        public static object[] ToArray(IntPtr vec)
        {
            unsafe
            {
                int size = (int)((ScmVector*)vec)->size;
                object[] ret = new object[size];

                for (int i = 0; i < size; ++i)
                {
                    ret[i] = Cast.ToClrObject(GoshInvoke.Scm_VectorRef(vec, i, GoshUnbound.Unbound.Ptr));
                }

                return ret;
            }
        }

        #region IEnumerable メンバ

        public System.Collections.IEnumerator GetEnumerator()
        {
            int size = this.Count;
            for (int i = 0; i < size; ++i)
            {
                yield return this[i];
            }
            yield break;
        }

        #endregion

        #region ICollection メンバ

        public void CopyTo(Array array, int index)
        {
            int count = Count;
            for (int i = index; i < count; ++i)
            {
                array.SetValue(this[i], i);
            }
        }

        public int Count
        {
            get 
            {
                unsafe
                {
                    return (int)((ScmVector*)_ptr)->size;
                }
            }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return _ptr; }
        }

        #endregion
    }
}
