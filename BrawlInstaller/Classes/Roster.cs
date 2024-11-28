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
    }

    public class RosterEntry
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Unknown";
        public bool InRandom { get; set; } = false;
        public bool InCss { get; set; } = false;
    }
}
