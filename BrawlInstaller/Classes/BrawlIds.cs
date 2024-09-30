﻿using BrawlInstaller.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class BrawlIds
    {
        [JsonIgnore]
        public List<BrawlId> Ids { get; set; } = new List<BrawlId>();
        public int FighterConfigId { get => GetId(IdType.FighterConfig); set => SetId(IdType.FighterConfig, value); }
        public int CosmeticConfigId { get => GetId(IdType.CosmeticConfig); set => SetId(IdType.CosmeticConfig, value); }
        public int SlotConfigId { get => GetId(IdType.SlotConfig); set => SetId(IdType.SlotConfig, value); }
        public int CSSSlotConfigId { get => GetId(IdType.CSSSlotConfig); set => SetId(IdType.CSSSlotConfig, value); }
        public int CosmeticId { get => GetId(IdType.Cosmetic); set => SetId(IdType.Cosmetic, value); }
        public int FranchiseId { get => GetId(IdType.Franchise); set => SetId(IdType.Franchise, value); }
        public int TrophyThumbnailId { get => GetId(IdType.Thumbnail); set => SetId(IdType.Thumbnail, value); }
        public int RecordsIconId { get => GetId(IdType.RecordsIcon); set => SetId(IdType.RecordsIcon, value); }
        public int StageId { get => GetId(IdType.Stage); set => SetId(IdType.Stage, value); }
        public int StageCosmeticId { get => GetId(IdType.StageCosmetic); set => SetId(IdType.StageCosmetic, value); }
        public int SoundbankId { get => GetId(IdType.Soundbank); set => SetId(IdType.Soundbank, value); }

        private int GetId(IdType type)
        {
            return Ids.Any(x => x.Type == type) ? Ids.First(x => x.Type == type).Id : -1;
        }

        private void SetId(IdType type, int newId)
        {
            var match = Ids.Any(x => x.Type == type) ? Ids.FirstOrDefault(x => x.Type == type) : null;
            if (match != null)
                match.Id = newId;
            else
                Ids.Add(new BrawlId { Id = newId, Type = type });
        }

        public int GetIdOfType(IdType type)
        {
            return Ids.FirstOrDefault(x => x.Type == type)?.Id ?? 0;
        }

        public BrawlIds Copy()
        {
            var copy = new BrawlIds();
            foreach(var id in Ids)
            {
                copy.Ids.Add(new BrawlId { Id = id.Id, Type = id.Type });
            }
            return copy;
        }
    }

    public class BrawlId
    {
        public IdType Type { get; set; }
        public int Id { get; set; }
    }
}
