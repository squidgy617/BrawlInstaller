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
            (CosmeticType.StageFranchiseIcon, "Icon"),
            (CosmeticType.StageName, "Name"),
            (CosmeticType.StageGameLogo, "Game Logo"),
            (CosmeticType.StageAltName, "Alt Name"),
            (CosmeticType.StageRandomBanner, "Random Banner"),
            (CosmeticType.StageReplayBanner, "Replay Banner"),
            (CosmeticType.StageStats, "Stats")
        };

        public static List<(CosmeticType CosmeticType, string Style)> DefaultCostumeCosmetics = new List<(CosmeticType CosmeticType, string Style)>
        {
            (CosmeticType.CSP, "Result"),
            (CosmeticType.CSP, "CSS"),
            (CosmeticType.PortraitName, "vBrawl"),
            (CosmeticType.PortraitName, "PM"),
            (CosmeticType.PortraitName, "REMIX"),
            (CosmeticType.BP, "vBrawl"),
            (CosmeticType.BP, "REMIX"),
            (CosmeticType.BPName, "vBrawl"),
            (CosmeticType.StockIcon, "P+"),
            (CosmeticType.CSSIcon, "vBrawl"),
            (CosmeticType.CSSIcon, "P+"),
            (CosmeticType.CSSIcon, "PM"),
            (CosmeticType.CSSIcon, "REMIX"),
            (CosmeticType.CSSIconName, "vBrawl"),
            (CosmeticType.ReplayIcon, "vBrawl"),
            (CosmeticType.ReplayIcon, "P+")
        };

        public static List<(CosmeticType CosmeticType, string Style)> DefaultFighterCosmetics = new List<(CosmeticType CosmeticType, string Style)>
        {
            (CosmeticType.RecordsIcon, "vBrawl"),
            (CosmeticType.CreditsIcon, "vBrawl")
        };
    }
}
