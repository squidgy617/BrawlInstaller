using BrawlInstaller.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.StaticClasses
{
    public static class DefaultCosmetics
    {
        public static List<(CosmeticType CosmeticType, string Style)> DefaultStageCosmetics = new List<(CosmeticType CosmeticType, string Style)>
        {
            (CosmeticType.StagePreview, "Preview"),
            (CosmeticType.StageIcon, "Icon"),
            (CosmeticType.StageFranchiseIcon, "Icon")
        };
    }
}
