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
using Newtonsoft.Json;
using System.IO;
using lKHM;
using static BrawlLib.SSBB.ResourceNodes.FCFGNode;

namespace BrawlInstaller.Classes
{
    public class FighterPackage
    {
        public FighterInfo FighterInfo { get; set; }
        public List<Costume> Costumes { get; set; }
        public CosmeticList Cosmetics { get; set; } = new CosmeticList();
        public List<FighterPacFile> PacFiles { get; set; }
        public List<string> ExConfigs { get; set; }
        public string Module { get; set; }
        public string Soundbank { get; set; }
        public string ClassicIntro { get; set; }
        public List<string> EndingPacFiles { get; set; }
        public string EndingMovie { get; set; }
        public TracklistSong CreditsTheme { get; set; }
        public TracklistSong VictoryTheme { get; set; } = new TracklistSong();
        public FighterSettings FighterSettings { get; set; } = new FighterSettings();
        public FighterDeleteOptions FighterDeleteOptions { get; set; } = new FighterDeleteOptions();
    }

    public class FighterPacFile
    {
        public string FilePath { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
    }

    public class Costume
    {
        public List<Cosmetic> Cosmetics { get; set; }
        public List<FighterPacFile> PacFiles { get; set; }
        public byte Color { get; set; }
        public int CostumeId { get; set; }
    }

    public class CosmeticList : TrackedList<Cosmetic>
    {
        public Dictionary<(CosmeticType, string), string> InheritedStyles { get; set; } = new Dictionary<(CosmeticType, string), string>();
    }

    public class Cosmetic
    {
        [JsonIgnore] public string Name { get => Texture != null ? Texture.Name : !string.IsNullOrEmpty(ImagePath) ? Path.GetFileNameWithoutExtension(ImagePath) : 
                TextureId != null ? $"Texture {TextureId}" : string.Empty; }
        public CosmeticType CosmeticType { get; set; }
        public string Style { get; set; }
        [JsonIgnore] public BitmapImage Image { get; set; } = null;
        [JsonIgnore] public BitmapImage HDImage { get; set; } = null;
        public string ImagePath { get; set; } = string.Empty;
        public string HDImagePath { get; set; } = string.Empty;
        [JsonIgnore] public TEX0Node Texture { get; set; } = null;
        [JsonIgnore] public PLT0Node Palette { get; set; } = null;
        [JsonIgnore] public MDL0Node Model { get; set; } = null;
        public string ModelPath { get; set; } = string.Empty;
        [JsonIgnore] public CLR0Node ColorSequence { get; set; } = null;
        public bool? SharesData { get; set; }
        public int? InternalIndex { get; set; }
        public int? CostumeIndex { get; set; }
        public int? Id { get; set; }
        public int? TextureId { get; set; }
        public bool ColorSmashChanged { get; set; } = false;
        public bool SelectionOption { get; set; } = false;
    }

    public class FranchiseCosmetic : Cosmetic
    {
        public BitmapImage TransparentImage { get; set; } = null;
        public TEX0Node TransparentTexture { get; set; } = null;
    }

    public class FighterSettings
    {
        public HatInfoPack KirbyHatData { get; set; } = null;
        public LucarioSettings LucarioSettings { get; set; } = null;
        public BowserSettings BowserSettings { get; set; } = null;
        public JigglypuffSettings JigglypuffSettings { get; set; } = null;
        public Position ThrowReleasePoint { get; set; } = new Position(0.0, 0.0);
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

    public class FighterDeleteOptions
    {
        public bool DeleteVictoryTheme { get; set; } = true;
        public bool DeleteCreditsTheme { get; set; } = true;
    }
}
