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

namespace BrawlInstaller.Classes
{
    public class AppSettings
    {
        public string BuildPath { get; set; } = string.Empty;
        public string HDTextures { get; set; } = string.Empty;
        [JsonIgnore] public string TempPath { get => Paths.TempPath; }
    }

    public class BuildSettings
    {
        public List<CosmeticDefinition> CosmeticSettings { get; set; } = new List<CosmeticDefinition>();
        public SoundSettings SoundSettings { get; set; } = new SoundSettings();
        public MiscSettings MiscSettings { get; set; } = new MiscSettings();
        public FilePathSettings FilePathSettings { get; set; } = new FilePathSettings();
        // TODO: Either add this to build settings, or make it an app setting right next to the HD texture path box
        public bool HDTextures { get; set; } = true;

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
        public int SuffixDigits { get; set; } = 3; // TODO: instead of displaying this on the interface, automatically fill this in based on IdType and Multiplier? Most cosmetics use 2, any that use cosmetic ID use 3 (10CC) or 4 (50CC)
        public IdType IdType { get; set; } = IdType.Cosmetic;
        public ImageSize Size { get; set; } = new ImageSize(null, null);
        public WiiPixelFormat Format { get; set; } = WiiPixelFormat.CI8;
        public bool FirstOnly { get; set; } = false;
        public bool SeparateFiles { get; set; } = false;
        public CompressionType CompressionType { get; set; } = CompressionType.None;
        public ARCFileType FileType { get; set; } = ARCFileType.MiscData;
        public bool AlwaysInheritStyle { get; set; } = false;
        [JsonIgnore] public bool UseIndividualIds { get => CosmeticType == CosmeticType.FranchiseIcon || Selectable; }
        [JsonIgnore] public bool FighterCosmetic { get => CosmeticType != CosmeticType.FranchiseIcon && !StageCosmetic; }
        [JsonIgnore] public bool StageCosmetic { get => CosmeticType == CosmeticType.StagePreview || CosmeticType == CosmeticType.StageFranchiseIcon || CosmeticType == CosmeticType.StageIcon || CosmeticType == CosmeticType.StageName || CosmeticType == CosmeticType.StageGameLogo || CosmeticType == CosmeticType.StageAltName; }
        [JsonIgnore] public bool Selectable { get => CosmeticType == CosmeticType.StageFranchiseIcon || CosmeticType == CosmeticType.StageGameLogo || CosmeticType == CosmeticType.StageAltName; }

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
    }

    public class SoundSettings
    {
        public SoundbankStyle SoundbankStyle { get; set; } = SoundbankStyle.InfoIndex;
        [JsonIgnore] public int SoundbankIncrement { get => SoundbankStyle == SoundbankStyle.InfoIndex ? 0 : 7; }
        [JsonIgnore] public string SoundbankFormat { get => SoundbankStyle == SoundbankStyle.InfoIndex ? "X3" : "D"; }
    }

    public class MiscSettings
    {
        public bool InstallToSse { get; set; } = false;
        public bool InstallTrophies { get; set; } = false;
        public bool UpdateCreditsModule { get; set; } = true;
        public bool SubspaceEx { get; set; } = true;
        public bool InstallKirbyHats { get; set; } = false;
        public uint DefaultExConfigVersion { get; set; } = 1;
    }

    public class FilePathSettings
    {
        [JsonProperty("FilePaths", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<FilePath> FilePaths { get; set; } = new List<FilePath>
        {
            new FilePath(FileType.FighterFiles, "pf\\fighter"),
            new FilePath(FileType.BrawlEx, "pf\\BrawlEx"),
            new FilePath(FileType.MasqueradePath, "pf\\info\\costumeslots"),
            new FilePath(FileType.Modules, "pf\\module"),
            new FilePath(FileType.StageSlots, "pf\\stage\\stageslot"),
            new FilePath(FileType.StageParamPath, "pf\\stage\\stageinfo"),
            new FilePath(FileType.StagePacPath, "pf\\stage\\melee"),
            new FilePath(FileType.TracklistPath, "pf\\sound\\tracklist"),
            new FilePath(FileType.ClassicIntroPath, "pf\\menu\\intro\\enter"),
            new FilePath(FileType.EndingPath, "pf\\menu\\intro\\ending"),
            new FilePath(FileType.MoviePath, "pf\\movie"),
            new FilePath(FileType.BrstmPath, "pf\\sound\\strm"),
            new FilePath(FileType.SoundbankPath, "pf\\sfx"),
            new FilePath(FileType.StageAltListPath, "pf\\stage\\stagelist"),
            new FilePath(FileType.CreditsModule, "pf\\module\\st_croll.rel", "REL file (.rel)|*.rel"),
            new FilePath(FileType.SSEModule, "pf\\module\\sora_adv_stage.rel", "REL file (.rel)|*.rel"),
            new FilePath(FileType.GctRealMateExe, "GCTRealMate.exe", "EXE file (.exe)|*.exe")
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

        [JsonIgnore] public string FighterFiles { get => GetFilePath(FileType.FighterFiles); }
        [JsonIgnore] public string BrawlEx { get => GetFilePath(FileType.BrawlEx); }
        [JsonIgnore] public string MasqueradePath { get => GetFilePath(FileType.MasqueradePath); }
        [JsonIgnore] public string Modules { get => GetFilePath(FileType.Modules); }
        [JsonIgnore] public string StageSlots { get => GetFilePath(FileType.StageSlots); }
        // This is the source of truth for where the "master" stage table is, there should only ever be one, it will be copied to all stage lists that have their own table
        // on save
        [JsonIgnore] public string StageTablePath { get => GetAsmPath(FileType.StageTablePath); }

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
        // TODO: Allow multiple for netplay?
        [JsonIgnore] public string TracklistPath { get => GetFilePath(FileType.TracklistPath); }
        public string VictoryThemeTracklist { get; set; } = "Results";
        public string CreditsThemeTracklist { get; set; } = "Credits";
        [JsonIgnore] public string CreditsThemeAsmFile { get => GetAsmPath(FileType.CreditsThemeAsmFile); }
        [JsonIgnore] public string ClassicIntroPath { get => GetFilePath(FileType.ClassicIntroPath); }
        [JsonIgnore] public string EndingPath { get => GetFilePath(FileType.EndingPath); }
        [JsonIgnore] public string EndingAsmFile { get => GetAsmPath(FileType.EndingAsmFile); }
        [JsonIgnore] public string MoviePath { get => GetFilePath(FileType.MoviePath); }
        [JsonIgnore] public string BrstmPath { get => GetFilePath(FileType.BrstmPath); }
        [JsonIgnore] public string SoundbankPath { get => GetFilePath(FileType.SoundbankPath); }
        [JsonIgnore] public string StageAltListPath { get => GetFilePath(FileType.StageAltListPath); }
        [JsonIgnore] public string ThrowReleaseAsmFile { get => GetAsmPath(FileType.ThrowReleaseAsmFile); }
        [JsonIgnore] public string FighterSpecificAsmFile { get => GetAsmPath(FileType.FighterSpecificAsmFile); }
        [JsonIgnore] public string CreditsModule { get => GetFilePath(FileType.CreditsModule); }
        [JsonIgnore] public string SSEModule { get => GetFilePath(FileType.SSEModule); }
        [JsonIgnore] public string LLoadAsmFile { get => GetAsmPath(FileType.LLoadAsmFile); }
        [JsonIgnore] public string SlotExAsmFile { get => GetAsmPath(FileType.SlotExAsmFile); }
        [JsonIgnore] public string GctRealMateExe { get => GetFilePath(FileType.GctRealMateExe); }

        // TODO: Should SSE roster be handled like all of these?
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
    }

    public class RosterFile
    {
        public string FilePath { get; set; }
        public bool AddNewCharacters { get; set; } = true;
    }

    public class FilePath
    {
        public FilePath(FileType fileType, string path, string filter = "")
        {
            FileType = fileType;
            DisplayName = fileType.GetDescription();
            Path = path;
            Filter = filter;
        }
        public FileType FileType { get; set; }
        public string DisplayName { get; set; }
        public string Path { get; set; }
        public string Filter { get; set; }
    }

    public class AsmPath : FilePath
    {
        public AsmPath(FileType fileType, string path, string label = "", string filter = "ASM file (.asm)|*.asm") : base(fileType, path, filter)
        {
            FileType = fileType;
            DisplayName = fileType.GetDescription();
            Path = path;
            Filter = filter;
            Label = label;
        }
        public string Label { get; set; }
    }
}
