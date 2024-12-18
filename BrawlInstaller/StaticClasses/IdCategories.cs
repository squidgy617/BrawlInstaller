using BrawlInstaller.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.StaticClasses
{
    public static class IdCategories
    {
        public static List<IdType> FighterConfigTypes = new List<IdType> { IdType.FighterConfig, IdType.CosmeticConfig, IdType.CSSSlotConfig, IdType.SlotConfig };
        public static List<IdType> FighterIdTypes = new List<IdType> { IdType.FighterConfig, IdType.CosmeticConfig, IdType.CSSSlotConfig, IdType.SlotConfig, IdType.Cosmetic };
    }
}
