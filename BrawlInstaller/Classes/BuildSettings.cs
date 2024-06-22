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
        public string FilePath { get; set; }
        public string NodePath { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
    }

    public class CosmeticDefinition
    {
        public CosmeticType CosmeticType { get; set; }
        public string Style { get; set; }
        public InstallLocation InstallLocation { get; set; }
        public string HDImageLocation { get; set; } = "";
        public string ModelPath { get; set; } = null;
        public PatSettings PatSettings { get; set; } = null;
        public string Prefix { get; set; }
        public int Multiplier { get; set; } = 1;
        public int Offset { get; set; } = 0;
        public int SuffixDigits { get; set; } = 3; // TODO: instead of displaying this on the interface, automatically fill this in based on IdType and Multiplier? Most cosmetics use 2, any that use cosmetic ID use 3 (10CC) or 4 (50CC)
        public IdType IdType { get; set; }
        public Size? Size { get; set; } = null;
        public WiiPixelFormat Format { get; set; } = WiiPixelFormat.CI8;
        public bool FirstOnly { get; set; } = false;
        public bool SeparateFiles { get; set; } = false;

        public CosmeticDefinition Copy()
        {
            var copy = JsonConvert.DeserializeObject<CosmeticDefinition>(JsonConvert.SerializeObject(this));
            return copy;
        }
    }

    public class PatSettings
    {
        public List<string> Paths { get; set; }
        public int FramesPerImage { get; set; } = 1;
        public IdType? IdType { get; set; } = null;
        public int? Multiplier { get; set; } = null;
        public int? Offset { get; set; } = null;
    }

    public class ToolPathSettings
    {
        public string KirbyHatExe { get; set; }
        public string AssemblyFunctionsExe { get; set; }
        public string SawndReplaceExe { get; set; }
        public string SfxChangeExe { get; set; }
        public string GfxChangeExe { get; set; }
    }

    public class KirbyHatSettings
    {
        public bool InstallKirbyHats { get; set; }
        public string DefaultKirbyHat { get; set; }
    }

    public class SoundSettings
    {
        public string SoundbankStyle { get; set; }
        public bool IncrementSoundbankIds { get; set; }
        public bool IncrementSoundbankNames { get; set; }
    }

    public class MiscSettings
    {
        public bool InstallToSse { get; set; }
        public SSEUnlockStage SSEUnlockStage { get; set; }
        public bool InstallTrophies { get; set; }
        public List<string> CustomStageLists { get; set; }
    }

    public class FilePathSettings
    {
        public string FighterFiles { get; set; }
        public string BrawlEx { get; set; }
        public string Modules { get; set; }
        public string HDTextures { get; set; }
    }
}
