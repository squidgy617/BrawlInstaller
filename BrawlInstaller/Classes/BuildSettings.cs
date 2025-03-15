using BrawlInstaller.Enums;
using BrawlLib.Wii.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Newtonsoft.Json;
using BrawlLib.Wii.Compression;
using BrawlLib.SSBB.Types;
using BrawlInstaller.StaticClasses;
using System.Runtime.CompilerServices;
using BrawlLib.Internal;
using System.Globalization;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlInstaller.Classes
{
    public class AppSettings
    {
        public string BuildPath { get; set; } = string.Empty;
        public string HDTextures { get; set; } = string.Empty;
        public bool ModifyHDTextures { get; set; } = false;
        [JsonIgnore] public string TempPath { get => Paths.TempPath; }
        public uint BackupCount { get; set; } = 10;

        public AppSettings Copy()
        {
            var copy = new AppSettings
            {
                BuildPath = BuildPath,
                HDTextures = HDTextures,
                ModifyHDTextures = ModifyHDTextures,
                BackupCount = BackupCount
            };
            return copy;
        }
    }

    public class BuildSettings
    {
        public List<CosmeticDefinition> CosmeticSettings { get; set; } = new List<CosmeticDefinition>();
        public SoundSettings SoundSettings { get; set; } = new SoundSettings();
        public MiscSettings MiscSettings { get; set; } = new MiscSettings();
        public FilePathSettings FilePathSettings { get; set; } = new FilePathSettings();

        public BuildSettings Copy()
        {
            var copy = JsonConvert.DeserializeObject<BuildSettings>(JsonConvert.SerializeObject(this));
            return copy;
        }
    }

    public class InstallLocation
    {
        public string FilePath { get; set; } = string.Empty;
        public string NodePath { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
    }

    public class CosmeticDefinition
    {
        public CosmeticType CosmeticType { get; set; }
        public string Style { get; set; }
        public InstallLocation InstallLocation { get; set; } = new InstallLocation();
        public string HDImageLocation { get; set; } = "";
        public bool CreateHDTextureFolder { get; set; } = false;
        public string ModelPath { get; set; } = null;
        public List<PatSettings> PatSettings { get; set; } = new List<PatSettings>();
        public string Prefix { get; set; } = "";
        public int Multiplier { get; set; } = 1;
        public int Offset { get; set; } = 0;
        public int SuffixDigits { get; set; } = 3;
        public int GroupMultiplier { get; set; } = 1;
        public string FilePrefix { get; set; } = string.Empty;
        public IdType IdType { get; set; } = IdType.Cosmetic;
        public ImageSize Size { get; set; } = new ImageSize(null, null);
        public WiiPixelFormat Format { get; set; } = WiiPixelFormat.CI8;
        public bool FirstOnly { get; set; } = false;
        public bool SeparateFiles { get; set; } = false;
        public CompressionType CompressionType { get; set; } = CompressionType.None;
        public ARCFileType FileType { get; set; } = ARCFileType.MiscData;
        public bool AlwaysInheritStyle { get; set; } = false;
        public bool Required { get; set; } = false;
        public bool Enabled { get; set; } = true;
        [JsonIgnore] public bool UseIndividualIds { get => CosmeticType == CosmeticType.FranchiseIcon || Selectable; }
        [JsonIgnore] public bool FighterCosmetic { get => CosmeticType != CosmeticType.FranchiseIcon && !StageCosmetic; }
        [JsonIgnore] public bool StageCosmetic { get => CosmeticType == CosmeticType.StagePreview || CosmeticType == CosmeticType.StageFranchiseIcon || CosmeticType == CosmeticType.StageIcon || CosmeticType == CosmeticType.StageName || CosmeticType == CosmeticType.StageGameLogo || CosmeticType == CosmeticType.StageAltName || CosmeticType == CosmeticType.StageRandomBanner || CosmeticType == CosmeticType.StageReplayBanner || CosmeticType == CosmeticType.StageStats; }
        public bool Selectable { get; set; }
        public bool AlwaysCreateArchive { get; set; } = true;

        public CosmeticDefinition Copy()
        {
            var copy = JsonConvert.DeserializeObject<CosmeticDefinition>(JsonConvert.SerializeObject(this));
            return copy;
        }
    }

    public class PatSettings
    {
        public string Path { get; set; } = string.Empty;
        public int FramesPerImage { get; set; } = 1;
        // TODO: Might be able to remove this? CSS icons are the only thing where this differs, and we might *want* those to get normalized
        // If we do not remove, need to update installation process as it does not respect this at all times
        public IdType? IdType { get; set; } = null;
        public int? Multiplier { get; set; } = null;
        public int? Offset { get; set; } = null;
        public bool AddTerminatorFrame { get; set; } = false;
        public bool NormalizeTextureIds { get; set; } = false;
    }

    public class SoundSettings
    {
        public SoundbankStyle SoundbankStyle { get; set; } = SoundbankStyle.InfoIndex;
        [JsonIgnore] public int SoundbankIncrement { get => SoundbankStyle == SoundbankStyle.InfoIndex ? 0 : 7; }
        [JsonIgnore] public string SoundbankFormat { get => SoundbankStyle == SoundbankStyle.InfoIndex ? "X3" : "D"; }
        [JsonIgnore] public NumberStyles SoundbankNumberStyle { get => SoundbankStyle == SoundbankStyle.InfoIndex ? NumberStyles.HexNumber : NumberStyles.Integer; }
    }

    public class MiscSettings
    {
        public bool InstallToSse { get; set; } = false;
        public bool InstallTrophies { get; set; } = false;
        public bool UpdateCreditsModule { get; set; } = true;
        public bool SubspaceEx { get; set; } = true;
        public bool InstallKirbyHats { get; set; } = false;
        public uint DefaultExConfigVersion { get; set; } = 1;
        public bool SyncTracklists { get; set; } = false;
        public bool UpdateCodeMenuNames { get; set; } = true;
    }

    public class FilePathSettings
    {
        [JsonProperty("FilePaths", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<FilePath> FilePaths { get; set; } = new List<FilePath>
        {
            new FilePath(FileType.FighterFiles, "pf\\fighter\\"),
            new FilePath(FileType.BrawlEx, "pf\\BrawlEx\\"),
            new FilePath(FileType.MasqueradePath, "pf\\info\\costumeslots\\"),
            new FilePath(FileType.Modules, "pf\\module\\"),
            new FilePath(FileType.StageSlots, "pf\\stage\\stageslot\\"),
            new FilePath(FileType.StageParamPath, "pf\\stage\\stageinfo\\"),
            new FilePath(FileType.StagePacPath, "pf\\stage\\melee\\"),
            new FilePath(FileType.TracklistPath, "pf\\sound\\tracklist\\"),
            new FilePath(FileType.NetplaylistPath, ""),
            new FilePath(FileType.ClassicIntroPath, "pf\\menu\\intro\\enter\\"),
            new FilePath(FileType.EndingPath, "pf\\menu\\intro\\ending\\"),
            new FilePath(FileType.MoviePath, "pf\\movie\\"),
            new FilePath(FileType.BrstmPath, "pf\\sound\\strm\\"),
            new FilePath(FileType.SoundbankPath, "pf\\sfx\\"),
            new FilePath(FileType.StageAltListPath, "pf\\stage\\stagelist\\"),
            new FilePath(FileType.CreditsModule, "pf\\module\\st_croll.rel"),
            new FilePath(FileType.SSEModule, "pf\\module\\sora_adv_stage.rel"),
            new FilePath(FileType.GctRealMateExe, "GCTRealMate.exe"),
            new FilePath(FileType.TargetBreakFolder, "pf\\stage\\stageslot\\tBreak\\"),
            new FilePath(FileType.CodeMenuConfig, "Code Menu Builder\\EX_Config.xml"),
            new FilePath(FileType.CodeMenuBuilder, "Code Menu Builder\\PowerPC Assembly Functions - Offline.exe"),
            new FilePath(FileType.CodeMenuSource, "Source\\CodeMenu\\CodeMenu.asm"),
            new FilePath(FileType.CodeMenuData, "pf\\menu3\\data.cmnu"),
            new FilePath(FileType.CodeMenuNetplayData, "pf\\menu3\\dnet.cmnu"),
            new FilePath(FileType.CodeMenuAddons, "Source\\CM_Addons\\"),
            new FilePath(FileType.TrophyNames, "pf\\toy\\fig\\ty_fig_name_list.msbin"),
            new FilePath(FileType.TrophyDescriptions, "pf\\toy\\fig\\ty_fig_ext_list.msbin"),
            new FilePath(FileType.TrophyBrresLocation, "pf\\toy\\fig\\"),
            new FilePath(FileType.FighterTrophyLocation, "Source\\ProjectM\\CloneEngine.asm"),
            new FilePath(FileType.SSETrophyModule, "pf\\module\\sora_adv_menu_game_over.rel"),
            new FilePath(FileType.CostumeSwapFile, "")
        };

        [JsonProperty("AsmPaths", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<AsmPath> AsmPaths { get; set; } = new List<AsmPath>()
        {
            new AsmPath(FileType.StageTablePath, "Source\\Project+\\StageTable.asm", "TABLE_STAGES:"),
            new AsmPath(FileType.CreditsThemeAsmFile, "Source\\Project+\\ResultsMusic.asm", "ClassicResultsTable:"),
            new AsmPath(FileType.EndingAsmFile, "Source\\ProjectM\\CloneEngine.asm", "ENDINGTABLE:"),
            new AsmPath(FileType.ThrowReleaseAsmFile, "Source\\ProjectM\\Modifier\\ThrowRelease.asm", "ThrowReleaseTable:"),
            new AsmPath(FileType.FighterSpecificAsmFile, "Source\\ProjectM\\CloneEngine.asm"),
            new AsmPath(FileType.LLoadAsmFile, "Source\\ProjectM\\CSS.asm", ".GOTO->Table_Skip"),
            new AsmPath(FileType.SlotExAsmFile, "Source\\P+Ex\\SlotEx.asm", "Table:")
        };

        [JsonProperty("FileNodePaths", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<FileNodePath> FileNodePaths { get; set; } = new List<FileNodePath>()
        {
            new FileNodePath(FileType.TrophyLocation, "pf\\system\\common3.pac", "Misc Data [0]/tyDataList"),
            new FileNodePath(FileType.TrophyGameIconsLocation, "pf\\menu\\collection\\Figure.brres", "AnmTexPat(NW4R)/MenFigureMark_TopN__0/MenFigureM06/Texture0")
        };

        [JsonIgnore] public string FighterFiles { get => GetFilePath(FileType.FighterFiles); }
        [JsonIgnore] public string BrawlEx { get => GetFilePath(FileType.BrawlEx); }
        [JsonIgnore] public string MasqueradePath { get => GetFilePath(FileType.MasqueradePath); }
        [JsonIgnore] public string Modules { get => GetFilePath(FileType.Modules); }
        [JsonIgnore] public string StageSlots { get => GetFilePath(FileType.StageSlots); }
        // This is the source of truth for where the "master" stage table is, there should only ever be one, it will be copied to all stage lists that have their own table
        // on save
        [JsonIgnore] public string StageTablePath { get => GetAsmPath(FileType.StageTablePath); }
        [JsonIgnore] public string StageTableLabel { get => GetLabel(FileType.StageTablePath); }

        [JsonProperty("StageListPaths", ObjectCreationHandling = ObjectCreationHandling.Replace)] 
        public List<FilePath> StageListPaths { get; set; } = new List<FilePath> 
        { 
            new FilePath(FileType.StageListFile, "Source\\Project+\\StageTable.asm")
        };

        [JsonProperty("RandomStageNamesLocations", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<InstallLocation> RandomStageNamesLocations { get; set; } = new List<InstallLocation>
        {
            new InstallLocation
            {
                FilePath = "pf\\menu2\\sc_selcharacter2.pac",
                NodePath = "MenuRule_en/Misc Data [1]",
                FileExtension = "pac"
            },
            new InstallLocation
            {
                FilePath = "pf\\menu2\\mu_menumain.pac",
                NodePath = "MenuRule_en/Misc Data [1]",
                FileExtension = "pac"
            }
        };
        [JsonIgnore] public string StageParamPath { get => GetFilePath(FileType.StageParamPath); }
        [JsonIgnore] public string StagePacPath { get => GetFilePath(FileType.StagePacPath); }
        [JsonIgnore] public string TracklistPath { get => GetFilePath(FileType.TracklistPath); }
        public string VictoryThemeTracklist { get; set; } = "Results";
        public string CreditsThemeTracklist { get; set; } = "Credits";
        [JsonIgnore] public string CreditsThemeAsmFile { get => GetAsmPath(FileType.CreditsThemeAsmFile); }
        [JsonIgnore] public string CreditsThemeTableLabel { get => GetLabel(FileType.CreditsThemeAsmFile); }
        [JsonIgnore] public string ClassicIntroPath { get => GetFilePath(FileType.ClassicIntroPath); }
        [JsonIgnore] public string EndingPath { get => GetFilePath(FileType.EndingPath); }
        [JsonIgnore] public string EndingAsmFile { get => GetAsmPath(FileType.EndingAsmFile); }
        [JsonIgnore] public string EndingTableLabel { get => GetLabel(FileType.EndingAsmFile); }
        [JsonIgnore] public string MoviePath { get => GetFilePath(FileType.MoviePath); }
        [JsonIgnore] public string BrstmPath { get => GetFilePath(FileType.BrstmPath); }
        [JsonIgnore] public string SoundbankPath { get => GetFilePath(FileType.SoundbankPath); }
        [JsonIgnore] public string StageAltListPath { get => GetFilePath(FileType.StageAltListPath); }
        [JsonIgnore] public string ThrowReleaseAsmFile { get => GetAsmPath(FileType.ThrowReleaseAsmFile); }
        [JsonIgnore] public string ThrowReleaseTableLabel { get => GetLabel(FileType.ThrowReleaseAsmFile); }
        [JsonIgnore] public string FighterSpecificAsmFile { get => GetAsmPath(FileType.FighterSpecificAsmFile); }
        [JsonIgnore] public string CreditsModule { get => GetFilePath(FileType.CreditsModule); }
        [JsonIgnore] public string SSEModule { get => GetFilePath(FileType.SSEModule); }
        [JsonIgnore] public string LLoadAsmFile { get => GetAsmPath(FileType.LLoadAsmFile); }
        [JsonIgnore] public string LLoadTableLabel { get => GetLabel(FileType.LLoadAsmFile); }
        [JsonIgnore] public string SlotExAsmFile { get => GetAsmPath(FileType.SlotExAsmFile); }
        [JsonIgnore] public string SlotExTableLabel { get => GetLabel(FileType.SlotExAsmFile); }
        [JsonIgnore] public string GctRealMateExe { get => GetFilePath(FileType.GctRealMateExe); }
        [JsonIgnore] public string TargetBreakFolder { get => GetFilePath(FileType.TargetBreakFolder); }
        [JsonIgnore] public string CodeMenuConfig { get => GetFilePath(FileType.CodeMenuConfig); }
        [JsonIgnore] public string CodeMenuBuilder { get =>  GetFilePath(FileType.CodeMenuBuilder); }
        [JsonIgnore] public string NetplaylistPath { get => GetFilePath(FileType.NetplaylistPath); }
        [JsonIgnore] public FileNodePath TrophyLocation { get => GetFileNodePath(FileType.TrophyLocation); }
        [JsonIgnore] public string TrophyNames { get => GetFilePath(FileType.TrophyNames); }
        [JsonIgnore] public string TrophyDescriptions { get => GetFilePath(FileType.TrophyDescriptions); }
        [JsonIgnore] public FileNodePath TrophyGameIconsLocation { get => GetFileNodePath(FileType.TrophyGameIconsLocation); }
        [JsonIgnore] public string TrophyBrresLocation { get => GetFilePath(FileType.TrophyBrresLocation); }
        [JsonIgnore] public string FighterTrophyLocation { get => GetFilePath(FileType.FighterTrophyLocation); }
        [JsonIgnore] public string SSETrophyModule { get => GetFilePath(FileType.SSETrophyModule); }
        [JsonIgnore] public string CodeMenuSource { get => GetFilePath(FileType.CodeMenuSource); }
        [JsonIgnore] public string CodeMenuData { get => GetFilePath(FileType.CodeMenuData); }
        [JsonIgnore] public string CodeMenuNetplayData { get => GetFilePath(FileType.CodeMenuNetplayData); }
        [JsonIgnore] public string CodeMenuAddons { get => GetFilePath(FileType.CodeMenuAddons); }
        [JsonIgnore] public string CostumeSwapFile { get => GetFilePath(FileType.CostumeSwapFile); }

        [JsonProperty("RosterFiles", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<RosterFile> RosterFiles { get; set; } = new List<RosterFile> 
        { 
            new RosterFile
            {
                FilePath = "pf\\BrawlEx\\CSSRoster.dat",
                AddNewCharacters = true
            }
        };

        [JsonProperty("CodeFilePaths", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<FilePath> CodeFilePaths { get; set; } = new List<FilePath>
        {
            new FilePath(FileType.GCTCodeFile, "RSBE01.txt"),
            new FilePath(FileType.GCTCodeFile, "BOOST.txt"),
            new FilePath(FileType.GCTCodeFile, "NETPLAY.txt"),
            new FilePath(FileType.GCTCodeFile, "NETBOOST.txt")
        };

        private string GetFilePath(FileType fileType)
        {
            var path = FilePaths.FirstOrDefault(x => x.FileType == fileType)?.Path;
            return path;
        }

        private string GetAsmPath(FileType fileType)
        {
            var path = AsmPaths.FirstOrDefault(x => x.FileType == fileType)?.Path;
            return path;
        }

        private string GetLabel(FileType fileType)
        {
            var label = AsmPaths.FirstOrDefault(x => x.FileType == fileType)?.Label;
            return label;
        }

        private FileNodePath GetFileNodePath(FileType fileType)
        {
            var path = FileNodePaths.FirstOrDefault(x => x.FileType == fileType);
            return path;
        }
    }

    public class RosterFile
    {
        public string FilePath { get; set; }
        public bool AddNewCharacters { get; set; } = true;
    }

    public class FilePath
    {
        public FilePath(FileType fileType, string path)
        {
            FileType = fileType;
            Path = path;
        }
        public FileType FileType { get; set; }
        [JsonIgnore] public string DisplayName { get => FileType.GetDescription(); }
        public string Path { get; set; }
        [JsonIgnore] public string Filter { get => DefaultSettings.GetFilePath(FileType)?.Filter ?? string.Empty; }
    }

    public class AsmPath : FilePath
    {
        public AsmPath(FileType fileType, string path, string label = "") : base(fileType, path)
        {
            FileType = fileType;
            Path = path;
            Label = label;
        }
        public string Label { get; set; }
    }

    public class FileNodePath : FilePath
    {
        public FileNodePath(FileType fileType, string path, string nodePath) : base(fileType, path)
        {
            FileType = fileType;
            Path = path;
            NodePath = nodePath;
        }
        public string NodePath { get; set; }
        [JsonIgnore] public List<Type> AllowedNodes { get => DefaultSettings.GetFilePath(FileType)?.AllowedNodes ?? new List<Type>(); }
    }
}
