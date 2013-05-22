using System;
using System.Collections.Generic;
using System.Text;

namespace GaucheDotNet
{
    public class GoshException : Exception
    {
        public GoshException()
        {
        }

        public GoshException(string message)
            : base(message)
        {
        }

    }
}
