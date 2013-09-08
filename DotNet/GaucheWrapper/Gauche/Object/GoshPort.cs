using System;
using System.Collections.Generic;
using System.Text;
using GaucheDotNet.Native;
using System.Runtime.InteropServices;

namespace GaucheDotNet
{
    public class GoshLoadPacket : GoshRefObj
    {
        public static readonly int SizeOf = Marshal.SizeOf(typeof(ScmLoadPacket));

        public GoshLoadPacket()
            : base(SizeOf)
        {
        }

        public bool HasException
        {
            get
            {
                unsafe
                {
                    return (IntPtr) ((ScmEvalPacket*)_ptr)->exception != (IntPtr)GoshInvoke.SCM_FALSE;
                }
            }
        }

        public GoshCondition Exception
        {
            get
            {
                unsafe
                {
                    return new GoshCondition(((ScmEvalPacket*)_ptr)->exception);
                }
            }
        }


    }

}
