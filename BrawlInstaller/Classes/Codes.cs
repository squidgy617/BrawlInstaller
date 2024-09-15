﻿using System;
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
        public string HookLocation { get; set; }
        public bool IsHook { get; set; } = false;
        public List<string> Instructions { get; set; } = new List<string>();
    }
}
