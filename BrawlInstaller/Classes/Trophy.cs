using BrawlLib.SSBB.ResourceNodes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class Trophy
    {
        public string Name { get; set; } = "New_Trophy";
        public string Brres { get; set; } = "New_Trophy";
        public string BrresFile { get; set; }
        public BrawlIds Ids { get; set; }
        [JsonIgnore] public Cosmetic Thumbnail { get; set; }
        public int GameIcon1 { get; set; } = 0;
        public int GameIcon2 { get; set; } = 0;
        public string DisplayName { get; set; } = "New Trophy";
        [JsonIgnore] public int? NameIndex { get; set; }
        public List<string> GameNames { get; set; } = new List<string>();
        [JsonIgnore] public int? GameIndex { get; set; }
        public string Description { get; set; } = "A new trophy.";
        [JsonIgnore] public int? DescriptionIndex { get; set; }
        public int SeriesIndex { get; set; }
        public int CategoryIndex { get; set; }

        // Unknowns
        public float Unknown0x34 { get; set; } = 1;
        public float Unknown0x38 { get; set; } = 1;
        public int Unknown0x40 { get; set; } = 1;
        public int Unknown0x44 { get; set; } = 1;
        public float Unknown0x50 { get; set; } = 0;
        public float Unknown0x54 { get; set; } = 0;
        public float Unknown0x58 { get; set; } = 0;
        public float Unknown0x5C { get; set; } = 0;

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
                Pad0x28 = 0,
                Pad0x2C = 0,
                Pad0x30 = 0,
                Pad0x3C = 0,
                Pad0x48 = 0
            };
            return node;
        }

        public Trophy Copy()
        {
            var copy = JsonConvert.DeserializeObject<Trophy>(JsonConvert.SerializeObject(this));
            copy.Thumbnail = Thumbnail.Copy();
            copy.NameIndex = NameIndex;
            copy.GameIndex = GameIndex;
            copy.DescriptionIndex = DescriptionIndex;
            return copy;
        }
    }
}
