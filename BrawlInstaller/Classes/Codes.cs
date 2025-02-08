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
        public List<Instruction> Instructions { get; set; } = new List<Instruction>();
        public string Comment { get; set; } = string.Empty;
    }

    public class Instruction
    {
        public string Text { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class AsmMacro
    {
        public string MacroName { get; set; }
        public List<string> Parameters { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class Alias
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int Index { get; set; }
    }
}
