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
        public List<CosmeticDefinition> CosmeticSettings { get; set; }
        public ToolPathSettings ToolPathSettings { get; set; }
        public KirbyHatSettings KirbyHatSettings { get; set; }
        public SoundSettings SoundSettings { get; set; }
        public MiscSettings MiscSettings { get; set; }
        public FilePathSettings FilePathSettings { get; set; }
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
        public ImageSize Size { get; set; } = new ImageSize(64, 64);
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

    public class ToolPathSettings
    {
        public string KirbyHatExe { get; set; } = "";
        public string AssemblyFunctionsExe { get; set; } = "";
        public string SawndReplaceExe { get; set; } = "";
        public string SfxChangeExe { get; set; } = "";
        public string GfxChangeExe { get; set; } = "";
        public string GctRealMateExe { get; set; } = "GCTRealMate.exe";
    }

    public class KirbyHatSettings
    {
        public bool InstallKirbyHats { get; set; } = false;
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
    }

    public class FilePathSettings
    {
        [JsonProperty("FilePaths", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<FilePath> FilePaths { get; set; } = new List<FilePath>
        {
            new FilePath(nameof(FighterFiles), "Fighter Files", "pf\\fighter"),
            new FilePath(nameof(BrawlEx), "BrawlEx Folder", "pf\\BrawlEx"),
            new FilePath(nameof(MasqueradePath), "Masquerade Folder", "pf\\info\\costumeslots"),
            new FilePath(nameof(Modules), "Module Folder", "pf\\module"),
            new FilePath(nameof(StageSlots), "Stage Slot Folder", "pf\\stage\\stageslot"),
            new FilePath(nameof(StageParamPath), "Stage Param Folder", "pf\\stage\\stageinfo"),
            new FilePath(nameof(StagePacPath), "Stage PAC Path", "pf\\stage\\melee"),
            new FilePath(nameof(TracklistPath), "Tracklist Path", "pf\\sound\\tracklist"),
            new FilePath(nameof(ClassicIntroPath), "Classic Intro Path", "pf\\menu\\intro\\enter"),
            new FilePath(nameof(EndingPath), "Ending File Path", "pf\\menu\\intro\\ending"),
            new FilePath(nameof(MoviePath), "Movie Path", "pf\\movie"),
            new FilePath(nameof(BrstmPath), "BRSTM Path", "pf\\sound\\strm"),
            new FilePath(nameof(SoundbankPath), "Soundbank Path", "pf\\sfx"),
            new FilePath(nameof(StageAltListPath), "Stage Alt List Path", "pf\\stage\\stagelist"),
            new FilePath(nameof(CreditsModule), "Credits Module", "pf\\module\\st_croll.rel", "REL file (.rel)|*.rel"),
            new FilePath(nameof(SSEModule), "SSE Module", "pf\\module\\sora_adv_stage.rel", "REL file (.rel)|*.rel"),
            new AsmPath(nameof(StageTablePath), "Stage Table", "Source\\Project+\\StageTable.asm", "TABLE_STAGES:"),
            new AsmPath(nameof(CreditsThemeAsmFile), "Credits Theme Table", "Source\\Project+\\ResultsMusic.asm", "ClassicResultsTable:"),
            new AsmPath(nameof(EndingAsmFile), "Ending Table", "Source\\ProjectM\\CloneEngine.asm", "ENDINGTABLE:"),
            new AsmPath(nameof(ThrowReleaseAsmFile), "Throw Release Table", "Source\\ProjectM\\Modifier\\ThrowRelease.asm", "ThrowReleaseTable:"),
            new AsmPath(nameof(FighterSpecificAsmFile), "Fighter-Specific Codes", "Source\\ProjectM\\CloneEngine.asm"),
            new AsmPath(nameof(LLoadAsmFile), "L-Load Table", "Source\\ProjectM\\CSS.asm", ".GOTO->Table_Skip"),
            new AsmPath(nameof(SlotExAsmFile), "SlotEx Table", "Source\\P+Ex\\SlotEx.asm", "Table:")
        };
        [JsonIgnore] public string FighterFiles { get => GetFilePath(nameof(FighterFiles)); }
        [JsonIgnore] public string BrawlEx { get => GetFilePath(nameof(BrawlEx)); }
        [JsonIgnore] public string MasqueradePath { get => GetFilePath(nameof(MasqueradePath)); }
        [JsonIgnore] public string Modules { get => GetFilePath(nameof(Modules)); }
        [JsonIgnore] public string StageSlots { get => GetFilePath(nameof(StageSlots)); }
        // This is the source of truth for where the "master" stage table is, there should only ever be one, it will be copied to all stage lists that have their own table
        // on save
        [JsonIgnore] public string StageTablePath { get => GetFilePath(nameof(StageTablePath)); }
        [JsonProperty("StageListPaths", ObjectCreationHandling = ObjectCreationHandling.Replace)] public List<string> StageListPaths { get; set; } = new List<string> { "Source\\Project+\\StageTable.asm" };

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
        [JsonIgnore] public string StageParamPath { get => GetFilePath(nameof(StageParamPath)); }
        [JsonIgnore] public string StagePacPath { get => GetFilePath(nameof(StagePacPath)); }
        // TODO: Allow multiple for netplay?
        [JsonIgnore] public string TracklistPath { get => GetFilePath(nameof(TracklistPath)); }
        public string VictoryThemeTracklist { get; set; } = "Results";
        public string CreditsThemeTracklist { get; set; } = "Credits";
        [JsonIgnore] public string CreditsThemeAsmFile { get => GetFilePath(nameof(CreditsThemeAsmFile)); }
        [JsonIgnore] public string ClassicIntroPath { get => GetFilePath(nameof(ClassicIntroPath)); }
        [JsonIgnore] public string EndingPath { get => GetFilePath(nameof(EndingPath)); }
        [JsonIgnore] public string EndingAsmFile { get => GetFilePath(nameof(EndingAsmFile)); }
        [JsonIgnore] public string MoviePath { get => GetFilePath(nameof(MoviePath)); }
        [JsonIgnore] public string BrstmPath { get => GetFilePath(nameof(BrstmPath)); }
        [JsonIgnore] public string SoundbankPath { get => GetFilePath(nameof(SoundbankPath)); }
        [JsonIgnore] public string StageAltListPath { get => GetFilePath(nameof(StageAltListPath)); }
        [JsonIgnore] public string ThrowReleaseAsmFile { get => GetFilePath(nameof(ThrowReleaseAsmFile)); }
        [JsonIgnore] public string FighterSpecificAsmFile { get => GetFilePath(nameof(FighterSpecificAsmFile)); }
        [JsonIgnore] public string CreditsModule { get => GetFilePath(nameof(CreditsModule)); }
        [JsonIgnore] public string SSEModule { get => GetFilePath(nameof(SSEModule)); }
        [JsonIgnore] public string LLoadAsmFile { get => GetFilePath(nameof(LLoadAsmFile)); }
        [JsonIgnore] public string SlotExAsmFile { get => GetFilePath(nameof(SlotExAsmFile)); }

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
        public List<string> CodeFilePaths { get; set; } = new List<string>
        {
            "RSBE01.txt",
            "BOOST.txt"
            // TODO: Re-add these when done testing
            //"NETPLAY.txt",
            //"NETBOOST.txt"
        };

        private string GetFilePath(string key)
        {
            var path = FilePaths.FirstOrDefault(x => x.Key == key)?.Path;
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
        public FilePath(string key, string displayName, string path, string filter = "")
        {
            Key = key;
            DisplayName = displayName;
            Path = path;
            Filter = filter;
        }
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string Path { get; set; }
        public string Filter { get; set; }
    }

    public class AsmPath : FilePath
    {
        public AsmPath(string key, string displayName, string path, string label = "", string filter = "ASM file (.asm)|*.asm") : base(key, displayName, path, filter)
        {
            Key = key;
            DisplayName = displayName;
            Path = path;
            Filter = filter;
            Label = label;
        }
        public string Label { get; set; }
    }
}
