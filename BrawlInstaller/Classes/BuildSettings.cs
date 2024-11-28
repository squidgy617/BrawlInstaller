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
        public string DefaultKirbyHat { get; set; } = "0x21";
    }

    public class SoundSettings
    {
        public SoundbankStyle SoundbankStyle { get; set; } = SoundbankStyle.InfoIndex;
        public int SoundbankIncrement { get => SoundbankStyle == SoundbankStyle.InfoIndex ? 0 : 7; }
        public string SoundbankFormat { get => SoundbankStyle == SoundbankStyle.InfoIndex ? "X3" : "D"; }
    }

    public class MiscSettings
    {
        public bool InstallToSse { get; set; } = false;
        public SSEUnlockStage SSEUnlockStage { get; set; } = SSEUnlockStage.End;
        public bool InstallTrophies { get; set; } = false;
        public List<string> CustomStageLists { get; set; } = new List<string>();
        public bool UpdateCreditsModule { get; set; } = true;
        public bool SubspaceEx { get; set; } = true;
    }

    public class FilePathSettings
    {
        public string FighterFiles { get; set; } = "pf\\fighter";
        public string BrawlEx { get; set; } = "pf\\BrawlEx";
        public string MasqueradePath { get; set; } = "pf\\info\\costumeslots";
        public string Modules { get; set; } = "pf\\module";
        public string StageSlots { get; set; } = "pf\\stage\\stageslot";
        // This is the source of truth for where the "master" stage table is, there should only ever be one, it will be copied to all stage lists that have their own table
        // on save
        public string StageTablePath { get; set; } = "Source\\Project+\\StageTable.asm";
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
        public string StageParamPath { get; set; } = "pf\\stage\\stageinfo";
        public string StagePacPath { get; set; } = "pf\\stage\\melee";
        // TODO: Allow multiple for netplay?
        public string TracklistPath { get; set; } = "pf\\sound\\tracklist";
        public string VictoryThemeTracklist { get; set; } = "Results";
        public string CreditsThemeTracklist { get; set; } = "Credits";
        public string CreditsThemeAsmFile { get; set; } = "Source\\Project+\\ResultsMusic.asm";
        public string ClassicIntroPath { get; set; } = "pf\\menu\\intro\\enter";
        public string EndingPath { get; set; } = "pf\\menu\\intro\\ending";
        public string EndingAsmFile { get; set; } = "Source\\ProjectM\\CloneEngine.asm";
        public string MoviePath { get; set; } = "pf\\movie";
        public string BrstmPath { get; set; } = "pf\\sound\\strm";
        public string SoundbankPath { get; set; } = "pf\\sfx";
        public string StageAltListPath { get; set; } = "pf\\stage\\stagelist";
        public string ThrowReleaseAsmFile { get; set; } = "Source\\ProjectM\\Modifier\\ThrowRelease.asm";
        public string FighterSpecificAsmFile { get; set; } = "Source\\ProjectM\\CloneEngine.asm";
        public string CreditsModule { get; set; } = "pf\\module\\st_croll.rel";
        public string SSEModule { get; set; } = "pf\\module\\sora_adv_stage.rel";
        public string LLoadAsmFile { get; set; } = "Source\\ProjectM\\CSS.asm";
        public string SlotExAsmFile { get; set; } = "Source\\P+Ex\\SlotEx.asm";

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
    }

    public class RosterFile
    {
        public string FilePath { get; set; }
        public bool AddNewCharacters { get; set; } = true;
    }
}
