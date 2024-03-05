using BrawlInstaller.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class FighterPackage
    {
        public List<MultiCosmeticGroup> CSPs { get; set; }
        public List<CosmeticGroup> BPs { get; set; }
        public List<CosmeticGroup> CSSIcons { get; set; }
        public List<CosmeticGroup> StockIcons { get; set; }
        public List<CosmeticGroup> ReplayIcons { get; set; }
        public List<FranchiseIcon> FranchiseIcons { get; set; }
        public List<FighterPath> FighterFolders { get; set; }
        public List<FighterPath> ExConfigFolders { get; set; }
        public List<FighterPath> Modules { get; set; }
        public List<FighterPath> Soundbanks { get; set; }
        public List<FighterPath> CreditsThemes { get; set; }
        public List<FighterPath> VictoryThemes { get; set; }
        public FighterSettings FighterSettings { get; set; }
    }

    public class FighterOption
    {
        public string Name { get; set; } = "Default";
        public string Description { get; set; } = "The default option";
    }

    // If there are multiple cosmetic groups with the same Style, user will be allowed to choose between the two
    public class CosmeticGroup : FighterOption
    {
        public CosmeticType CosmeticType { get; set; }
        public string Style { get; set; }
        public List<string> Images { get; set; }
        public List<string> HDImages { get; set; }
        public List<string> NameImages { get; set; }
        public List<string> HDNameImages { get; set; }
    }

    public class MultiCosmeticGroup : FighterOption
    {
        public CosmeticType CosmeticType { get; set; }
        public string Style { get; set; }
        public List<ColorSmashGroup> ColorSmashGroups { get; set; }
        public List<string> NameImages { get; set; }
        public List<string> HDNameImages { get; set; }
    }

    public class ColorSmashGroup
    {
        public List<string> Images { get; set; }
        public List<string> HDImages { get; set; }
    }

    public class FranchiseIcon : FighterOption
    {
        public string Image { get; set; }
        public string TransparentImage { get; set; }
        public string Model { get; set; }
    }

    public class FighterPath : FighterOption
    {
        public string Path { get; set; }
    }

    public class FighterSettings
    {
        public LucarioSettings LucarioSettings { get; set; } = null;
        public BowserSettings BowserSettings { get; set; } = null;
        public JigglypuffSettings JigglypuffSettings { get; set; } = null;
        public Tuple<float, float> ThrowReleasePoint { get; set; } = null;
        public int? CreditsThemeId { get; set; }
        public int? TrophyId { get; set; }
        public int? DoorId { get; set; }
    }

    public class LucarioSettings
    {
        public int? BoneId { get; set; }
        public int? KirbyEffectId { get; set; }
    }

    public class JigglypuffSettings
    {
        public int? BoneId { get; set; }
        public int? EFLSId { get; set; }
        public List<int> SfxIds { get; set; }
    }

    public class BowserSettings
    {
        public int BoneId { get; set; }
    }
}
