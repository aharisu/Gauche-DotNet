using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GaucheDotNet.Native
{
    public class Util
    {
        public static string IntPtrToUTF8String(IntPtr ptr)
        {
            int len = 0;
            unsafe
            {
                byte* b = (byte*)ptr;
                for (; b[len] != 0; ++len) ;
            }

            byte[] array = new byte[len];
            Marshal.Copy(ptr, array, 0, len);
            return Encoding.UTF8.GetString(array);
        }
    }
}
