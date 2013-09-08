using System;
using System.Collections.Generic;
using System.Text;
using GaucheDotNet.Native;
using System.Runtime.InteropServices;

namespace GaucheDotNet
{
    public class GoshUVector : GoshObj, System.Collections.ICollection
    {
        protected readonly IntPtr _ptr;
        private readonly UVectorType _type;

        public GoshUVector(UVectorType type, int size)
            : this(type, size, 0)
        {
        }

        public GoshUVector(UVectorType type, int size, object value)
        {
            _type = type;
            switch (type)
            {
                case UVectorType.S8: _ptr = GoshInvoke.Scm_MakeS8Vector(size, (SByte)Convert.ChangeType(value, TypeCode.SByte)); break;
                case UVectorType.U8: _ptr = GoshInvoke.Scm_MakeU8Vector(size, (Byte)Convert.ChangeType(value, TypeCode.Byte)); break;
                case UVectorType.S16: _ptr = GoshInvoke.Scm_MakeS16Vector(size, (Int16)Convert.ChangeType(value, TypeCode.Int16)); break;
                case UVectorType.U16: _ptr = GoshInvoke.Scm_MakeU16Vector(size, (UInt16)Convert.ChangeType(value, TypeCode.UInt16)); break;
                case UVectorType.S32: _ptr = GoshInvoke.Scm_MakeS32Vector(size, (Int32)Convert.ChangeType(value, TypeCode.Int32)); break;
                case UVectorType.U32: _ptr = GoshInvoke.Scm_MakeU32Vector(size, (UInt32)Convert.ChangeType(value, TypeCode.UInt32)); break;
                case UVectorType.S64: _ptr = GoshInvoke.Scm_MakeS64Vector(size, (Int64)Convert.ChangeType(value, TypeCode.Int64)); break;
                case UVectorType.U64: _ptr = GoshInvoke.Scm_MakeU64Vector(size, (UInt64)Convert.ChangeType(value, TypeCode.UInt64)); break;
                case UVectorType.F16: _ptr = GoshInvoke.Scm_MakeF16Vector(size, GoshInvoke.Scm_DoubleToHalf((Double)Convert.ChangeType(value, TypeCode.Double))); break;
                case UVectorType.F32: _ptr = GoshInvoke.Scm_MakeF32Vector(size, (Single)Convert.ChangeType(value, TypeCode.Single)); break;
                case UVectorType.F64: _ptr = GoshInvoke.Scm_MakeF64Vector(size, (Double)Convert.ChangeType(value, TypeCode.Double)); break;
                default: throw new ArgumentException();
            }
        }

        public GoshUVector(IntPtr ptr)
        {
            this._ptr = ptr;
            this._type = GoshInvoke.Scm_UVectorType(GoshInvoke.Scm_ClassOf(_ptr));
        }

        public override IntPtr Ptr 
        {
            get { return _ptr; } 
        }

        public UVectorType Type
        {
            get { return _type; }
        }

        public object this[int index]
        {
            get
            {
                switch (_type)
                {
                    case UVectorType.S8: return GoshInvoke.Scm_S8VectorRef(_ptr, index);
                    case UVectorType.U8: return GoshInvoke.Scm_U8VectorRef(_ptr, index);
                    case UVectorType.S16: return GoshInvoke.Scm_S16VectorRef(_ptr, index);
                    case UVectorType.U16: return GoshInvoke.Scm_U16VectorRef(_ptr, index);
                    case UVectorType.S32: return GoshInvoke.Scm_S32VectorRef(_ptr, index);
                    case UVectorType.U32: return GoshInvoke.Scm_U32VectorRef(_ptr, index);
                    case UVectorType.S64: return GoshInvoke.Scm_S64VectorRef(_ptr, index);
                    case UVectorType.U64: return GoshInvoke.Scm_U64VectorRef(_ptr, index);
                    case UVectorType.F16: return GoshInvoke.Scm_F16VectorRef(_ptr, index);
                    case UVectorType.F32: return GoshInvoke.Scm_F32VectorRef(_ptr, index);
                    case UVectorType.F64: return GoshInvoke.Scm_F64VectorRef(_ptr, index);
                    default: throw new InvalidOperationException();
                }
            }
            set
            {
                switch (_type)
                {
                    case UVectorType.S8: GoshInvoke.Scm_S8VectorSet(_ptr, index, (SByte)Convert.ChangeType(value, TypeCode.SByte)); break;
                    case UVectorType.U8: GoshInvoke.Scm_U8VectorSet(_ptr, index, (Byte)Convert.ChangeType(value, TypeCode.Byte)); break;
                    case UVectorType.S16: GoshInvoke.Scm_S16VectorSet(_ptr, index, (Int16)Convert.ChangeType(value, TypeCode.Int16)); break;
                    case UVectorType.U16: GoshInvoke.Scm_U16VectorSet(_ptr, index, (UInt16)Convert.ChangeType(value, TypeCode.UInt16)); break;
                    case UVectorType.S32: GoshInvoke.Scm_S32VectorSet(_ptr, index, (Int32)Convert.ChangeType(value, TypeCode.Int32)); break;
                    case UVectorType.U32: GoshInvoke.Scm_U32VectorSet(_ptr, index, (UInt32)Convert.ChangeType(value, TypeCode.UInt32)); break;
                    case UVectorType.S64: GoshInvoke.Scm_S64VectorSet(_ptr, index, (Int64)Convert.ChangeType(value, TypeCode.Int64)); break;
                    case UVectorType.U64: GoshInvoke.Scm_U64VectorSet(_ptr, index, (UInt64)Convert.ChangeType(value, TypeCode.UInt64)); break;
                    case UVectorType.F16: GoshInvoke.Scm_F16VectorSet(_ptr, index, (double)Convert.ChangeType(value, TypeCode.Double)); break;
                    case UVectorType.F32: GoshInvoke.Scm_F32VectorSet(_ptr, index, (float)Convert.ChangeType(value, TypeCode.Single)); break;
                    case UVectorType.F64: GoshInvoke.Scm_F64VectorSet(_ptr, index, (double)Convert.ChangeType(value, TypeCode.Double)); break;
                    default: throw new InvalidOperationException();
                }
            }
        }

        public override object Object
        {
            get { return ToArray(_ptr); }
        }

        public static Array ToArray(IntPtr ptr)
        {
            Array ary;
            int size = GoshInvoke.Scm_UVectorLength(ptr);
            int sizeInBytes;
            UVectorType type = GoshInvoke.Scm_UVectorType(GoshInvoke.Scm_ClassOf(ptr));

            switch (type)
            {
                case UVectorType.S8: 
                    ary = new SByte[size];
                    sizeInBytes = size;
                    break;
                case UVectorType.U8:
                    ary = new Byte[size];
                    sizeInBytes = size;
                    break;
                case UVectorType.S16:
                    ary = new Int16[size];
                    sizeInBytes = size * 2;
                    break;
                case UVectorType.U16:
                    ary = new UInt16[size];
                    sizeInBytes = size * 2;
                    break;
                case UVectorType.S32:
                    ary = new Int32[size];
                    sizeInBytes = size * 4;
                    break;
                case UVectorType.U32:
                    ary = new UInt32[size];
                    sizeInBytes = size * 4;
                    break;
                case UVectorType.S64:
                    ary = new Int64[size];
                    sizeInBytes = size * 8;
                    break;
                case UVectorType.U64:
                    ary = new UInt64[size];
                    sizeInBytes = size * 8;
                    break;
                case UVectorType.F16:
                    ary = new Double[size];
                    sizeInBytes = size * 2;
                    break;
                case UVectorType.F32:
                    ary = new Single[size];
                    sizeInBytes = size * 4;
                    break;
                case UVectorType.F64:
                    ary = new Double[size];
                    sizeInBytes = size * 8;
                    break;
                default: throw new InvalidOperationException();
            }

            if (type == UVectorType.F16)
            {
                GCHandle h = GCHandle.Alloc(ary, GCHandleType.Pinned);
                GoshInvoke.Scm_UVectorCopyF16(ptr, h.AddrOfPinnedObject(), size);
                h.Free();
            }
            else
            {
                GCHandle h = GCHandle.Alloc(ary, GCHandleType.Pinned);
                GoshInvoke.Scm_UVectorCopy(ptr, h.AddrOfPinnedObject(), sizeInBytes);
                h.Free();
            }

            return ary;
        }

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
            get { return GoshInvoke.Scm_UVectorLength(_ptr); }
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
    }
}
