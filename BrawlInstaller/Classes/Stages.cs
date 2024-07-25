using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class StageList
    {
        public List<StagePage> Pages = new List<StagePage>();
    }

    public class StagePage
    {
        public int PageNumber { get; set; }
        public List<StageSlot> StageSlots { get; set; } = new List<StageSlot>();
    }

    public class StageSlot
    {
        public BrawlIds StageIds { get; set; } = new BrawlIds();
        public List<StageEntry> StageEntries { get; set; } = new List<StageEntry>();

        public string Name { get => StageEntries.FirstOrDefault()?.Name; }
    }

    public class StageEntry
    {
        public string Name { get; set; } = "Unknown";
        public ushort ButtonFlags { get; set; } = 0x0000;
    }
}
