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
using BrawlInstaller.Services;
using BrawlInstaller.Common;
using BrawlInstaller.StaticClasses;

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
        public FighterInfo FighterInfo { get; set; } = new FighterInfo();
        public List<Costume> Costumes { get; set; } = new List<Costume>();
        public CosmeticList Cosmetics { get; set; } = new CosmeticList();
        public List<FighterPacFile> PacFiles { get; set; } = new List<FighterPacFile>();
        public string MasqueradeFile { get; set; } = string.Empty;
        public string Module { get; set; }
        public string Soundbank { get; set; }
        public string KirbySoundbank { get; set; }
        public string ClassicIntro { get; set; }
        public List<string> EndingPacFiles { get; set; } = new List<string>();
        public string EndingMovie { get; set; }
        public TracklistSong CreditsTheme { get; set; } = new TracklistSong();
        public TracklistSong VictoryTheme { get; set; } = new TracklistSong();
        public FighterSettings FighterSettings { get; set; } = new FighterSettings();
        public FighterDeleteOptions FighterDeleteOptions { get; set; } = new FighterDeleteOptions();
        public PackageType PackageType { get; set; } = PackageType.Update;
        public List<FighterTrophy> Trophies { get; set; } = new List<FighterTrophy>();
        public List<FighterInstallOption> InstallOptions { get; set; } = new List<FighterInstallOption> 
        { 
            new FighterInstallOption(InstallOptionType.MovesetFile),
            new FighterInstallOption(InstallOptionType.Module),
            new FighterInstallOption(InstallOptionType.Sounbank),
            new FighterInstallOption(InstallOptionType.KirbySoundbank)
        };

        // TODO: Alternate files
        // It will be a list of alternate files you can supply. Each file has a type associated with it that tells you what type of file it is. This list, along with the associate files,
        // is saved in its own folder in the fighter package. In the UI, there's a simple button somewhere that lets you see the whole list, adding or removing the files and their types.
        // When you load a package, the list is loaded too. When you go to import a package, if the list has items in it, you will be prompted to select options from the list.
        // On import, the options the user selected automatically replace the actual files for those types.

        public FighterPackage Copy()
        {
            var costumes = Costumes.Copy();
            var fighterPackage = new FighterPackage
            {
                FighterInfo = FighterInfo.Copy(),
                Costumes = costumes,
                Cosmetics = Cosmetics.Copy(costumes),
                PacFiles = PacFiles.Copy(),
                MasqueradeFile = MasqueradeFile,
                Module = Module,
                Soundbank = Soundbank,
                KirbySoundbank = KirbySoundbank,
                ClassicIntro = ClassicIntro,
                EndingPacFiles = EndingPacFiles.ToList(),
                EndingMovie = EndingMovie,
                CreditsTheme = CreditsTheme.Copy(),
                VictoryTheme = VictoryTheme.Copy(),
                FighterSettings = FighterSettings.Copy(),
                FighterDeleteOptions = FighterDeleteOptions.Copy(),
                PackageType = PackageType,
                Trophies = Trophies.Copy(),
                InstallOptions = InstallOptions.Copy()
            };
            return fighterPackage;
        }
    }

    public class FighterPacFile
    {
        public string FilePath { get; set; }
        public FighterFileType FileType { get; set; }
        public string Suffix { get; set; }
        [JsonIgnore] public string SavePath { get; set; } = string.Empty; // Path to save the file, if one is specified

        public FighterPacFile Copy()
        {
            var copy = JsonConvert.DeserializeObject<FighterPacFile>(JsonConvert.SerializeObject(this));
            copy.SavePath = SavePath;
            return copy;
        }

        public string GetPrefix(FighterInfo fighterInfo)
        {
            switch (FileType)
            {
                case FighterFileType.FighterPacFile:
                    return fighterInfo.PacFileName;
                case FighterFileType.KirbyPacFile:
                    return fighterInfo.KirbyPacFileName;
                case FighterFileType.ItemPacFile:
                    return $"Itm{fighterInfo.PartialPacName}";
                default:
                    return fighterInfo.PacFileName;
            }
        }

        public static string GetPrefix(FighterFileType fileType, FighterInfo fighterInfo)
        {
            switch (fileType)
            {
                case FighterFileType.FighterPacFile:
                    return fighterInfo.PacFileName;
                case FighterFileType.KirbyPacFile:
                    return fighterInfo.KirbyPacFileName;
                case FighterFileType.ItemPacFile:
                    return $"Itm{fighterInfo.PartialPacName}";
                default:
                    return fighterInfo.PacFileName;
            }
        }

        public FighterFileType GetFileType(string pacPrefix, FighterInfo fighterInfo)
        {
                if (pacPrefix.ToLower() == fighterInfo.PacFileName.ToLower())
                    return FighterFileType.FighterPacFile;
                else if (pacPrefix.ToLower() == fighterInfo.KirbyPacFileName.ToLower())
                    return FighterFileType.KirbyPacFile;
                else if (pacPrefix.ToLower() == $"Itm{fighterInfo.PartialPacName}".ToLower())
                    return FighterFileType.ItemPacFile;
                else
                    return FighterFileType.FighterPacFile;
        }
    }

    public class Costume
    {
        [JsonIgnore] public List<Cosmetic> Cosmetics { get; set; } = new List<Cosmetic>();
        [JsonIgnore] public List<FighterPacFile> PacFiles { get; set; } = new List<FighterPacFile>();
        public byte Color { get; set; }
        public int CostumeId { get; set; }

        public Costume Copy()
        {
            return new Costume
            {
                Cosmetics = Cosmetics.ToList(),
                PacFiles = PacFiles.Copy(),
                Color = Color,
                CostumeId = CostumeId
            };
        }
    }

    public class CosmeticList : TrackedList<Cosmetic>
    {
        public Dictionary<(CosmeticType BaseType, string BaseStyle), string> InheritedStyles { get; set; } = new Dictionary<(CosmeticType, string), string>();

        public CosmeticList Copy(List<Costume> costumes = null)
        {
            var copy = new CosmeticList();
            copy.InheritedStyles = InheritedStyles.ToDictionary(entry => entry.Key, entry => entry.Value);
            // Change any items that are in changed item list but AREN'T in regular items - done this way to preserve references
            foreach(var cosmetic in ChangedItems.Where(x => !Items.Contains(x)))
            {
                var cosmeticCopy = cosmetic.Copy();
                copy.ItemChanged(cosmeticCopy);
            }
            // Copy items that are in regular item list
            foreach (var cosmetic in Items)
            {
                var cosmeticCopy = cosmetic.Copy();
                copy.Items.Add(cosmeticCopy);
                // Copy cosmetics to costumes as needed to preserve references
                if (costumes != null)
                {
                    foreach (var costume in costumes)
                    {
                        var index = costume.Cosmetics.IndexOf(cosmetic);
                        if (index > -1)
                        {
                            costume.Cosmetics.RemoveAt(index);
                            costume.Cosmetics.Insert(index, cosmeticCopy);
                        }
                    }
                }
                // Change those that are in changed items
                if (ChangedItems.Contains(cosmetic))
                {
                    copy.ItemChanged(cosmeticCopy);
                }
            }
            return copy;
        }
    }

    public class Cosmetic
    {
        [JsonIgnore] public string Name { get => Texture != null ? Texture.Name : !string.IsNullOrEmpty(ImagePath) ? Path.GetFileNameWithoutExtension(ImagePath) : 
                TextureId != null ? $"Texture {TextureId}" : string.Empty; }
        public CosmeticType CosmeticType { get; set; }
        public string Style { get; set; }
        [JsonIgnore] public BitmapImage Image { get; set; } = null;
        [JsonIgnore] public BitmapImage HDImage { get; set; } = null;
        [JsonIgnore] public string ImagePath { get; set; } = string.Empty;
        [JsonIgnore] public string HDImagePath { get; set; } = string.Empty;
        [JsonIgnore] public TEX0Node Texture { get; set; } = null;
        [JsonIgnore] public PLT0Node Palette { get; set; } = null;
        [JsonIgnore] public MDL0Node Model { get; set; } = null;
        [JsonIgnore] public string ModelPath { get; set; } = string.Empty;
        [JsonIgnore] public CLR0Node ColorSequence { get; set; } = null;
        public bool? SharesData { get; set; }
        public int? InternalIndex { get; set; }
        public int? CostumeIndex { get; set; }
        public int? Id { get; set; }
        public int? TextureId { get; set; }
        public bool ColorSmashChanged { get; set; } = false;
        public bool SelectionOption { get; set; } = false;

        public Cosmetic Copy()
        {
            var copy = JsonConvert.DeserializeObject<Cosmetic>(JsonConvert.SerializeObject(this));
            copy.Image = Image;
            copy.HDImage = HDImage;
            copy.ImagePath = ImagePath;
            copy.HDImagePath = HDImagePath;
            copy.Texture = Texture;
            copy.Palette = Palette;
            copy.Model = Model;
            copy.ModelPath = ModelPath;
            copy.ColorSequence = ColorSequence;
            return copy;
        }
    }

    public class FranchiseCosmetic : Cosmetic
    {
        public BitmapImage TransparentImage { get; set; } = null;
        public TEX0Node TransparentTexture { get; set; } = null;
    }

    public class FighterSettings
    {
        [JsonIgnore] public HatInfoPack KirbyHatData { get; set; } = null;
        public LucarioSettings LucarioSettings { get; set; } = new LucarioSettings();
        public SamusSettings SamusSettings { get; set; } = new SamusSettings();
        public BowserSettings BowserSettings { get; set; } = new BowserSettings();
        public JigglypuffSettings JigglypuffSettings { get; set; } = new JigglypuffSettings();
        public DededeSettings DededeSettings { get; set; } = new DededeSettings();
        public Position ThrowReleasePoint { get; set; } = new Position(0.0, 0.0);
        public int? TrophyId { get; set; }
        public uint DoorId { get; set; } = 0;
        public int? SSESubCharacterId { get; set; } = 0;
        public int? LLoadCharacterId { get; set; } = 0;

        [JsonProperty("ExSlotIds", ObjectCreationHandling = ObjectCreationHandling.Replace)] 
        public List<uint> ExSlotIds { get; set; } = new List<uint> { 0xFF, 0xFF, 0xFF, 0xFF };

        public FighterSettings Copy()
        {
            var copy = JsonConvert.DeserializeObject<FighterSettings>(JsonConvert.SerializeObject(this));
            copy.KirbyHatData = KirbyHatData;
            return copy;
        }
    }

    public class LucarioSettings
    {
        [JsonProperty("BoneIds", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<int?> BoneIds { get; set; } = new List<int?> { null, null, null, null };
        public bool UseGfxFix { get; set; } = false;
        public bool UseKirbyGfxFix { get; set; } = false;

        public LucarioSettings Copy()
        {
            return JsonConvert.DeserializeObject<LucarioSettings>(JsonConvert.SerializeObject(this));
        }
    }

    public class SamusSettings
    {
        public bool UseGfxFix { get; set; } = false;
        public bool UseKirbyGfxFix { get; set; } = false;

        public SamusSettings Copy()
        {
            return JsonConvert.DeserializeObject<SamusSettings>(JsonConvert.SerializeObject(this));
        }
    }

    public class JigglypuffSettings
    {
        [JsonProperty("BoneIds", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<int?> BoneIds { get; set; } = new List<int?> { null, null, null, null };
        public int? EFLSId { get; set; } = null;

        // TODO: Somehow update these when soundbank ID is changed, probably at UI level
        [JsonProperty("SfxIds", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<int?> SfxIds { get; set; } = new List<int?> { null, null, null, null };

        public JigglypuffSettings Copy()
        {
            return JsonConvert.DeserializeObject<JigglypuffSettings>(JsonConvert.SerializeObject(this));
        }
    }

    public class DededeSettings
    {
        public bool UseFix { get; set; } = false;

        public DededeSettings Copy()
        {
            return JsonConvert.DeserializeObject<DededeSettings>(JsonConvert.SerializeObject(this));
        }
    }

    public class BowserSettings
    {
        public int? BoneId { get; set; } = null;

        public BowserSettings Copy()
        {
            return JsonConvert.DeserializeObject<BowserSettings>(JsonConvert.SerializeObject(this));
        }
    }

    public class FighterDeleteOptions
    {
        public bool DeleteVictoryTheme { get; set; } = true;
        public bool DeleteCreditsTheme { get; set; } = true;
        public bool DeleteVictoryEntry { get; set; } = true;
        public bool DeleteCreditsEntry { get; set; } = true;

        public FighterDeleteOptions Copy()
        {
            return JsonConvert.DeserializeObject<FighterDeleteOptions>(JsonConvert.SerializeObject(this));
        }
    }

    public class FighterInstallOption
    {
        public InstallOptionType Type { get; set; } = InstallOptionType.MovesetFile;
        [JsonIgnore] public string File { get; set; } = string.Empty;
        public string Name { get; set; } = "New Option";
        public string Description { get; set; } = string.Empty;
        [JsonIgnore] public string Filter { get => InstallOptions.InstallOptionFilters.TryGetValue(Type, out var filter) ? filter : string.Empty; }
        [JsonIgnore] public string Extension { get => InstallOptions.InstallOptionExtensions.TryGetValue(Type, out var extension) ? extension : string.Empty; }

        public FighterInstallOption()
        {

        }

        public FighterInstallOption(InstallOptionType type, string name = "Default", string description = "The default option for this package.", string file = "")
        {
            Type = type;
            Name = name;
            Description = description;
            File = file;
        }

        public FighterInstallOption Copy()
        {
            var copy = new FighterInstallOption
            {
                Type = Type,
                Name = Name,
                Description = Description,
                File = File
            };
            return copy;
        }
    }

    public enum TrophyType
    {
        [Description("Classic")]
        Fighter,
        [Description("All-Star")]
        AllStar
    }

    public class FighterTrophy
    {
        public TrophyType Type { get; set; }
        public Trophy Trophy { get; set; }
        [JsonIgnore] public Trophy OldTrophy { get; set; }

        public FighterTrophy Copy()
        {
            var copy = new FighterTrophy
            {
                Type = Type,
                Trophy = Trophy?.Copy(),
                OldTrophy = OldTrophy?.Copy()
            };
            return copy;
        }
    }
}
