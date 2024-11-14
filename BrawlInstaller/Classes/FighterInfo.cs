using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrawlLib.SSBB.ResourceNodes.FCFGNode;

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
        // TODO: Should credits theme ID go in here too?
        public uint? VictoryThemeId { get; set; } = null;
        public uint? SoundbankId { get; set; } = null;
        [JsonIgnore] public  uint? OriginalSoundbankId { get; set; } = null;
        public uint? KirbySoundbankId { get; set; } = null;
        [JsonIgnore] public uint? OriginalKirbySoundbankId { get; set; } = null;
        [JsonIgnore] public int EndingId { get; set; } = -1;
        public KirbyLoadFlags KirbyLoadType { get; set; } = KirbyLoadFlags.None;
        public int? EffectPacId { get; set; } = null;
        [JsonIgnore] public int? OriginalEffectPacId { get; set; } = null;
        public int? KirbyEffectPacId { get; set; } = null;
        [JsonIgnore] public int? OriginalKirbyEffectPacId { get; set; } = null;

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
                    SlotConfigId = Ids.SlotConfigId,
                    CosmeticId = Ids.CosmeticId,
                    RecordsIconId = Ids.RecordsIconId,
                    FranchiseId = Ids.FranchiseId
                },
                VictoryThemeId = VictoryThemeId,
                SoundbankId = SoundbankId,
                OriginalSoundbankId = OriginalSoundbankId,
                KirbySoundbankId = KirbySoundbankId,
                OriginalKirbySoundbankId= OriginalKirbySoundbankId,
                EndingId = EndingId,
                KirbyLoadType = KirbyLoadType,
                EffectPacId = EffectPacId,
                OriginalEffectPacId = OriginalEffectPacId,
                KirbyEffectPacId = KirbyEffectPacId,
                OriginalKirbyEffectPacId = OriginalKirbyEffectPacId
            };
            return newFighterInfo;
        }
    }
}
