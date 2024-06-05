using BrawlInstaller.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrawlLib.Internal;
using BrawlLib.SSBB.ResourceNodes;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace BrawlInstaller.Classes
{
    // TODO: This class does NOT contain options. We will use a different class to store selected options when installing a character package, and their choices
    //will result in an instance of this class being created. This class is used both when extracting and installing characters.
    public class FighterPackage
    {
        public FighterInfo FighterInfo { get; set; }
        public List<Costume> Costumes { get; set; }
        public TrackedList<Cosmetic> Cosmetics { get; set; } = new TrackedList<Cosmetic>();
        public List<FighterFiles> FighterFiles { get; set; }
        public string Soundbank { get; set; }
        public string CreditsTheme { get; set; }
        public string VictoryTheme { get; set; }
        public FighterSettings FighterSettings { get; set; } = null;
    }

    public class InstallOption
    {
        public string Name { get; set; } = "Default";
        public string Description { get; set; } = "Default option";
    }

    public class FighterFiles : InstallOption
    {
        public List<string> PacFiles { get; set; }
        public List<string> KirbyPacFiles { get; set; }
        public List<string> ItemFiles { get; set; }
        public List<string> ExConfigs { get; set; }
        public string Module { get; set; }
    }

    // TODO: Order of costumes dictates order on CSS/in Ex Config. Internal order dictates order in BRRES. ColorSmashGroup dictates what cosmetics are color smashed.
    public class Costume
    {
        public List<Cosmetic> Cosmetics { get; set; }
        public List<string> PacFiles { get; set; }
        public byte Color { get; set; }
        public int CostumeId { get; set; }
    }

    public class Cosmetic
    {
        public CosmeticType CosmeticType { get; set; }
        public string Style { get; set; }
        public BitmapImage Image { get; set; } = null;
        public BitmapImage HDImage { get; set; } = null;
        public string ImagePath { get; set; } = string.Empty;
        public string HDImagePath { get; set; } = string.Empty;
        public TEX0Node Texture { get; set; } = null;
        public PLT0Node Palette { get; set; } = null;
        public MDL0Node Model { get; set; } = null;
        public string ModelPath { get; set; } = string.Empty;
        public CLR0Node ColorSequence { get; set; } = null;
        public bool? SharesData { get; set; }
        public int? InternalIndex { get; set; }
        public int? CostumeIndex { get; set; }
        public int? Id { get; set; }
        public bool ColorSmashChanged { get; set; } = false;
    }

    public class FranchiseCosmetic : Cosmetic
    {
        public BitmapImage TransparentImage { get; set; } = null;
        public TEX0Node TransparentTexture { get; set; } = null;
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
