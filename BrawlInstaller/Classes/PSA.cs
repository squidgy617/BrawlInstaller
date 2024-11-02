using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class OpCode
    {
        public OpCode(byte[] bytes, int offset = 0)
        {
            Bytes = bytes;
            Offset = offset;
        }

        public byte[] Bytes { get; set; }

        public int Offset = 0;
    }
}
