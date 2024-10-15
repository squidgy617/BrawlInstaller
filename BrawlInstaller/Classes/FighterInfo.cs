﻿using Newtonsoft.Json;
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
        [JsonIgnore] public int EndingId { get; set; } = -1;
        [JsonIgnore] public KirbyLoadFlags KirbyLoadType { get; set; } = KirbyLoadFlags.None;

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
                EndingId = EndingId,
                KirbyLoadType = KirbyLoadType
            };
            return newFighterInfo;
        }
    }
}
