using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Exceptions
{
    public class CompilerTimeoutException : Exception
    {
        public CompilerTimeoutException()
        {
        }

        public CompilerTimeoutException(string message)
            : base(message)
        {
        }

        public CompilerTimeoutException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
