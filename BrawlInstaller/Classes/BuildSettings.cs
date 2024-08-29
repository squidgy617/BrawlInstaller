using BrawlInstaller.Enums;
using BrawlLib.Wii.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Newtonsoft.Json;

namespace BrawlInstaller.Classes
{
    public class AppSettings
    {
        public string BuildPath { get; set; } = string.Empty;
        [JsonIgnore] public string TempPath { get; set; } = "temp";
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
        [JsonIgnore] public bool UseIndividualIds { get => CosmeticType == CosmeticType.FranchiseIcon || Selectable; }
        [JsonIgnore] public bool FighterCosmetic { get => CosmeticType != CosmeticType.FranchiseIcon && !StageCosmetic; }
        [JsonIgnore] public bool StageCosmetic { get => CosmeticType == CosmeticType.StagePreview || CosmeticType == CosmeticType.StageFranchiseIcon || CosmeticType == CosmeticType.StageIcon; }
        [JsonIgnore] public bool Selectable { get => CosmeticType == CosmeticType.StageFranchiseIcon; }

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

        // TODO: Add this to UI, or make it automatic based on type? This is for stats images
        public bool AddTerminatorFrame { get; set; } = false;
    }

    public class ToolPathSettings
    {
        public string KirbyHatExe { get; set; } = "";
        public string AssemblyFunctionsExe { get; set; } = "";
        public string SawndReplaceExe { get; set; } = "";
        public string SfxChangeExe { get; set; } = "";
        public string GfxChangeExe { get; set; } = "";
    }

    public class KirbyHatSettings
    {
        public bool InstallKirbyHats { get; set; } = false;
        public string DefaultKirbyHat { get; set; } = "0x21";
    }

    public class SoundSettings
    {
        public string SoundbankStyle { get; set; } = "hex";
        public bool IncrementSoundbankIds { get; set; } = true;
        public bool IncrementSoundbankNames { get; set; } = false;
    }

    public class MiscSettings
    {
        public bool InstallToSse { get; set; } = false;
        public SSEUnlockStage SSEUnlockStage { get; set; } = SSEUnlockStage.End;
        public bool InstallTrophies { get; set; } = false;
        public List<string> CustomStageLists { get; set; } = new List<string>();
    }

    public class FilePathSettings
    {
        public string FighterFiles { get; set; } = "pf\\fighter";
        public string BrawlEx { get; set; } = "pf\\BrawlEx";
        public string Modules { get; set; } = "pf\\module";
        public string StageSlots { get; set; } = "pf\\stage\\stageslot";

        // TODO: change default on this and make it save to a separate config file
        public string HDTextures { get; set; } = "F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild Launcher - For Netplay\\User\\Load\\Textures\\RSBE01";
        public string StageTablePath { get; set; } = "Source\\Project+\\StageTable.asm";
        [JsonProperty("StageListPaths", ObjectCreationHandling = ObjectCreationHandling.Replace)] public List<string> StageListPaths { get; set; } = new List<string> { "Source\\Project+\\StageTable.asm" };
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
    }
}
