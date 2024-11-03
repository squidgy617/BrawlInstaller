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
using System.ComponentModel;

namespace BrawlInstaller.Classes
{
    public enum PackageType
    {
        Update,
        New,
        Delete
    }

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
        public int? EffectPacId { get; set; } = null;
        public int? OriginalEffectPacId { get; set; } = null;
        public int? KirbyEffectPacId { get; set; } = null;
        public int? OriginalKirbyEffectPacId { get; set; } = null;
        public TracklistSong CreditsTheme { get; set; }
        public TracklistSong VictoryTheme { get; set; } = new TracklistSong();
        public FighterSettings FighterSettings { get; set; } = new FighterSettings();
        public FighterDeleteOptions FighterDeleteOptions { get; set; } = new FighterDeleteOptions();
        public PackageType PackageType { get; set; } = PackageType.Update;
    }

    public class FighterPacFile
    {
        public string FilePath { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string Subdirectory { get; set; } = string.Empty;
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
        public LucarioSettings LucarioSettings { get; set; } = new LucarioSettings();
        public SamusSettings SamusSettings { get; set; } = new SamusSettings();
        public BowserSettings BowserSettings { get; set; } = new BowserSettings();
        public JigglypuffSettings JigglypuffSettings { get; set; } = new JigglypuffSettings();
        public DededeSettings DededeSettings { get; set; } = new DededeSettings();
        public Position ThrowReleasePoint { get; set; } = new Position(0.0, 0.0);
        public int? CreditsThemeId { get; set; }
        public int? TrophyId { get; set; }
        public uint DoorId { get; set; } = 0;
        public int? SSESubCharacterId { get; set; } = 0;
        public int? LLoadCharacterId { get; set; } = 0;
        public List<uint> ExSlotIds { get; set; } = new List<uint> { 0xFF, 0xFF, 0xFF, 0xFF };
    }

    public class LucarioSettings
    {
        public List<int?> BoneIds { get; set; } = new List<int?> { null, null, null, null };
        public bool UseGfxFix { get; set; } = false;
        public bool UseKirbyGfxFix { get; set; } = false;
    }

    public class SamusSettings
    {
        public bool UseGfxFix { get; set; } = false;
        public bool UseKirbyGfxFix { get; set; } = false;
    }

    public class JigglypuffSettings
    {
        public List<int?> BoneIds { get; set; } = new List<int?> { null, null, null, null };
        public int? EFLSId { get; set; } = null;
        // TODO: Somehow update these when soundbank ID is changed
        public List<int?> SfxIds { get; set; } = new List<int?> { null, null, null, null };
    }

    public class DededeSettings
    {
        public bool UseFix { get; set; } = false;
    }

    public class BowserSettings
    {
        public int? BoneId { get; set; } = null;
    }

    public class FighterDeleteOptions
    {
        public bool DeleteVictoryTheme { get; set; } = true;
        public bool DeleteCreditsTheme { get; set; } = true;
    }
}
