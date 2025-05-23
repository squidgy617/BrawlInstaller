﻿using BrawlLib.SSBB.ResourceNodes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BrawlInstaller.Classes
{
    public class Trophy
    {
        public string Name { get; set; } = "New_Trophy";
        public string Brres { get; set; } = "New_Trophy";
        [JsonIgnore] public string BrresFile { get; set; }
        public BrawlIds Ids { get; set; } = new BrawlIds();
        [JsonIgnore] public CosmeticList Thumbnails { get; set; } = new CosmeticList();
        public int GameIcon1 { get; set; } = 0;
        public int GameIcon2 { get; set; } = 0;
        public string DisplayName { get; set; } = "New Trophy";
        [JsonIgnore] public int? NameIndex { get; set; }
        public string GameName1 { get; set; } = string.Empty;
        public string GameName2 { get; set; } = string.Empty;
        [JsonIgnore] public int? GameIndex { get; set; }
        public string Description { get; set; } = "<color=E6E6E6FF>A new trophy.</end>";
        [JsonIgnore] public int? DescriptionIndex { get; set; }
        public int SeriesIndex { get; set; } = 0;
        public int CategoryIndex { get; set; } = 23;

        // Unknowns
        public float Unknown0x34 { get; set; } = 1;
        public float Unknown0x38 { get; set; } = 1;
        public int Unknown0x40 { get; set; } = 1;
        public int Unknown0x44 { get; set; } = 1;
        public float Unknown0x50 { get; set; } = 0;
        public float Unknown0x54 { get; set; } = 0;
        public float Unknown0x58 { get; set; } = 0;
        public float Unknown0x5C { get; set; } = 0;

        // Padding
        public int Pad0x28 { get; set; } = 0;
        public int Pad0x2C { get; set; } = 0;
        public int Pad0x30 { get; set; } = 0;
        public int Pad0x3C { get; set; } = 0;
        public int Pad0x48 { get; set; } = 0;

        public TyDataListEntryNode ToNode()
        {
            var node = new TyDataListEntryNode
            {
                Name = Name,
                BRRES = Brres,
                Id = (int)Ids.TrophyId,
                ThumbnailIndex = (int)Ids.TrophyThumbnailId,
                GameIcon1 = GameIcon1,
                GameIcon2 = GameIcon2,
                NameIndex = (int)NameIndex,
                GameIndex = (int)GameIndex,
                DescriptionIndex = (int)DescriptionIndex,
                SeriesIndex = SeriesIndex,
                CategoryIndex = CategoryIndex,
                // Unknowns
                Unknown0x34 = Unknown0x34,
                Unknown0x38 = Unknown0x38,
                Unknown0x40 = Unknown0x40,
                Unknown0x44 = Unknown0x44,
                Unknown0x50 = Unknown0x50,
                Unknown0x54 = Unknown0x54,
                Unknown0x58 = Unknown0x58,
                Unknown0x5C = Unknown0x5C,
                // Padding
                Pad0x28 = Pad0x28,
                Pad0x2C = Pad0x2C,
                Pad0x30 = Pad0x30,
                Pad0x3C = Pad0x3C,
                Pad0x48 = Pad0x48
            };
            return node;
        }

        public Trophy Copy()
        {
            if (this == null)
            {
                return null;
            }
            var copy = JsonConvert.DeserializeObject<Trophy>(JsonConvert.SerializeObject(this));
            copy.BrresFile = BrresFile;
            copy.Thumbnails = Thumbnails.Copy();
            copy.NameIndex = NameIndex;
            copy.GameIndex = GameIndex;
            copy.DescriptionIndex = DescriptionIndex;
            return copy;
        }
    }

    public class TrophyGameIcon
    {
        public int Id { get; set; }
        public BitmapImage Image { get; set; }
    }
}
