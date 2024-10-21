using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class AsmTableEntry
    {
        public string Item { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class AsmHook
    {
        public string Address { get; set; }
        public bool IsHook { get; set; } = false;
        public List<string> Instructions { get; set; } = new List<string>();
        public string Comment { get; set; } = string.Empty;
    }

    public class AsmMacro
    {
        public string MacroName { get; set; }
        public List<string> Parameters { get; set; }
    }
}
