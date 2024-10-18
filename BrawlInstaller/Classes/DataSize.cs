using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class DataSize
    {
        private DataSize(string value) { Value = value; }

        public string Value { get; private set; }

        public static DataSize Byte { get { return new DataSize("byte"); } }
        public static DataSize Halfword { get { return new DataSize("half"); } }
        public static DataSize Word { get { return new DataSize("word"); } }
        public static DataSize Float { get { return new DataSize("float"); } }

        public override string ToString()
        {
            return Value;
        }
    }
}
