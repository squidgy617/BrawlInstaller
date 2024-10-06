using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class FighterInfo
    {
        public string EntryName { get; set; }
        public string InternalName { get; set; }
        public string DisplayName { get; set; }
        public BrawlIds Ids { get; set; }
        [JsonIgnore]
        public string FighterConfig { get; set; }
        [JsonIgnore]
        public string CosmeticConfig { get; set; }
        [JsonIgnore]
        public string CSSSlotConfig { get; set; }
        [JsonIgnore]
        public string SlotConfig { get; set; }
        // TODO: Should these even be in here? They are not used for costumes so aren't needed for P+ probably. Maybe should go in fighter package itself?
        public uint? VictoryThemeId { get; set; } = null;
        public uint? SoundbankId { get; set; } = null;
        [JsonIgnore] public int EndingId { get; set; } = -1;

        public FighterInfo Copy()
        {
            var newFighterInfo = new FighterInfo
            {
                EntryName = EntryName,
                InternalName = InternalName,
                DisplayName = DisplayName,
                Ids = new BrawlIds
                {
                    FighterConfigId = Ids.FighterConfigId,
                    CosmeticConfigId = Ids.CosmeticConfigId,
                    CSSSlotConfigId = Ids.CSSSlotConfigId,
                    SlotConfigId = Ids.SlotConfigId
                },
                VictoryThemeId = VictoryThemeId,
                SoundbankId = SoundbankId,
                EndingId = EndingId
            };
            return newFighterInfo;
        }
    }
}
