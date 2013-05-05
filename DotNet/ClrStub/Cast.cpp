using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

#include "Gosh.h"

namespace GaucheDotNet
{
    public ref class Cast abstract sealed
    {
        public static GoshObj Specify(IntPtr ptr)
        {
#if X64
            Int64 num = ptr.ToInt64();
#else
            Int32 num = ptr.ToInt32();
#endif
            if ((num & 0x03) == 1)
            {
                return new GoshFixnum(num >> 2);
            }
            else if (num == GoshInvoke::SCM_FALSE)
            {
                return Gosh::False;
            }
            else if (num == GoshInvoke::SCM_TRUE)
            {
                return Gosh::True;
            }
            else if (num == GoshInvoke::SCM_NIL)
            {
                return Gosh::NIL;
            }
            else if (num == GoshInvoke::SCM_EOF)
            {
                return Gosh::EOF;
            }
            else if (num == GoshInvoke::SCM_UNDEFINED)
            {
                return Gosh::Undefined;
            }
            else if (num == GoshInvoke::SCM_UNBOUND)
            {
                return Gosh::Unbound;
            }
            else
            {
                /*
                switch ((Gosh.KnownClass)GoshInvoke.Scm_IsKnownType(ptr))
                {
                    case Gosh.KnownClass.Pair:
                        return new GoshPair(ptr);

                    case Gosh.KnownClass.String:
                        return new GoshString(ptr);

                    case Gosh.KnownClass.Symbol:
                        return new GoshSymbol(ptr);

                    case Gosh.KnownClass.Closure:
                    case Gosh.KnownClass.Method:
                    case Gosh.KnownClass.Generic:
                    case Gosh.KnownClass.NextMethod:
                        return new Procedure.Procedure(ptr);

                    //TODO
                    case Gosh.KnownClass.Subr:
                        unsafe
                        {
                            ScmSubr* subr = (ScmSubr*)ptr;
                            GoshInvoke.ScmSubProc func = (GoshInvoke.ScmSubProc)Marshal.GetDelegateForFunctionPointer(subr->func, typeof(GoshInvoke.ScmSubProc));
                            if (func.Target == null)
                            {
                                return new Procedure.Procedure(ptr);
                            }
                            else
                            {
                                return (GoshProc)func.Target;
                            }
                        }

                    case Gosh.KnownClass.ClrObject:
                        return new GoshClrObject(ptr);

                    //TODO convert known class ...

                    default:
                        throw new InvalidCastException("unknown type");
                }
                */
                default:
                    throw new InvalidCastException("unknown type");
            }
        }

        /*
        public static object ToObj(IntPtr ptr)
        {
#if X64
            Int64 num = ptr.ToInt64();
#else
            Int32 num = ptr.ToInt32();
#endif
            if ((num & 0x03) == 1)
            {
                return num >> 2;
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
                return Gosh.NIL;
            }
            else if (num == GoshInvoke.SCM_EOF)
            {
                return Gosh.EOF;
            }
            else if (num == GoshInvoke.SCM_UNDEFINED)
            {
                return Gosh.Undefined;
            }
            else if (num == GoshInvoke.SCM_UNBOUND)
            {
                return Gosh.Unbound;
            }
            else
            {
                switch((Gosh.KnownClass)GoshInvoke.Scm_IsKnownType(ptr))
                {
                    case Gosh.KnownClass.Pair:
                        return new GoshPair(ptr);

                    case Gosh.KnownClass.String:
                        return GoshInvoke.Scm_GetStringConst(ptr);

                    case Gosh.KnownClass.Symbol:
                        return new GoshSymbol(ptr);

                    case Gosh.KnownClass.Closure:
                    case Gosh.KnownClass.Method:
                    case Gosh.KnownClass.Generic:
                    case Gosh.KnownClass.NextMethod:
                        return (GoshFunc)new Procedure.Procedure(ptr).Apply;

                        //TODO
                    case Gosh.KnownClass.Subr:
                        unsafe 
                        {
                            ScmSubr* subr = (ScmSubr*)ptr;
                            GoshInvoke.ScmSubProc func = (GoshInvoke.ScmSubProc)Marshal.GetDelegateForFunctionPointer(subr->func, typeof(GoshInvoke.ScmSubProc));
                            if (func.Target == null)
                            {
                                return (GoshFunc)new Procedure.Procedure(ptr).Apply;
                            }
                            else
                            {
                                return (GoshFunc)func.Target;
                            }
                        }

                    case Gosh.KnownClass.ClrObject:
                        return GCHandle.FromIntPtr(ptr).Target;

                    //TODO convert known class ...

                    default:
                        return new GoshRefObj(ptr);
                }
            }
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
                    return (IntPtr)((num << 2) + 1);
                }
            }
            else if (obj is UInt32)
            {
                UInt32 num = (UInt32)obj;
                if (num <= SCM_SMALL_INT_MAX)
                {
                    return (IntPtr)((num << 2) + 1);
                }
            }
            else if (obj is Int16)
            {
                return (IntPtr)(((Int16)obj << 2) + 1);
            }
            else if (obj is UInt16)
            {
                return (IntPtr)(((UInt16)obj << 2) + 1);
            }
            else if (obj is Int64)
            {
                Int64 num = (Int64)obj;
                if (num >= SCM_SMALL_INT_MIN && num <= SCM_SMALL_INT_MAX)
                {
                    return (IntPtr)((num << 2) + 1);
                }
            }
            else if (obj is UInt64)
            {
                UInt64 num = (UInt64)obj;
                if (num <= (UInt64)SCM_SMALL_INT_MAX)
                {
                    return (IntPtr)((num << 2) + 1);
                }
            }

            GCHandle handle = GCHandle.Alloc(obj);
            return GoshInvoke.Scm_MakeClrObject((IntPtr)handle);
        }
        */

    };
};
