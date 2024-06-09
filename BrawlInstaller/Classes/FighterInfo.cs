using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class FighterInfo
    {
        public string InternalName { get; set; }
        public string DisplayName { get; set; }
        public BrawlIds Ids { get; set; }
        public string FighterConfig { get; set; }
        public string CosmeticConfig { get; set; }
        public string CSSSlotConfig { get; set; }
        public string SlotConfig { get; set; }
        public uint VictoryThemeId { get; set; }
    }
}
