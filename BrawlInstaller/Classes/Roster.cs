using BrawlInstaller.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class Roster
    {
        public string Name { get; set; } = "Roster";
        public List<RosterEntry> Entries { get; set; } = new List<RosterEntry>();
        public string FilePath { get; set; } = string.Empty;
        public bool AddNewCharacters { get; set; } = true;
        public RosterType RosterType { get; set; } = RosterType.CSS;
        public IdType IdType { get => RosterType == RosterType.CodeMenu ? IdType.SlotConfig : IdType.CSSSlotConfig; }

        public List<AsmTableEntry> ConvertToAsmTable()
        {
            var newAsmTable = new List<AsmTableEntry>();
            foreach (var entry in Entries)
            {
                var newAsmEntry = new AsmTableEntry
                {
                    Item = $"0x{entry.Id:X2}",
                    Comment = entry.Name
                };
                newAsmTable.Add(newAsmEntry);
            }
            return newAsmTable;
        }
    }

    public class RosterEntry
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Unknown";
        public bool InRandom { get; set; } = false;
        public bool InCss { get; set; } = false;
    }

    public enum RosterType
    {
        CSS,
        CodeMenu,
        SSE,
        Bonus
    }
}
