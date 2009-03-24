using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FOA
{
    public class DecoderException : ApplicationException
    {
        public DecoderException(string message) : base(message) { }
    }
}
