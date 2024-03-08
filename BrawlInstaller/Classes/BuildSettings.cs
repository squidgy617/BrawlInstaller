﻿using BrawlInstaller.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BrawlInstaller.Classes
{
    public class BuildSettings
    {
        public CosmeticSettings CosmeticSettings { get; set; }
        public ToolPathSettings ToolPathSettings { get; set; }
        public KirbyHatSettings KirbyHatSettings { get; set; }
        public SoundSettings SoundSettings { get; set; }
        public MiscSettings MiscSettings { get; set; }
    }

    public class InstallLocation
    {
        public string FilePath { get; set; }
        public string NodePath { get; set; } = string.Empty;
        public string FilExtension { get; set; } = string.Empty;
    }

    public class CosmeticDefinition
    {
        public CosmeticType CosmeticType { get; set; }
        public string Style { get; set; }
        public InstallLocation InstallLocation { get; set; }
        public PatSettings PatSettings { get; set; } = null;
        public string Prefix { get; set; }
        public bool FiftyCC { get; set; }
        public Size? Size { get; set; } = null;
        public bool FirstOnly { get; set; } = false;
    }

    public class CosmeticSettings
    {
        public List<CosmeticDefinition> CSPs { get; set; }
        public List<CosmeticDefinition> CSSIcons { get; set; }
        public List<CosmeticDefinition> BPs { get; set; }
        public List<CosmeticDefinition> PortraitNames { get; set; }
        public List<CosmeticDefinition> BPNames { get; set; }
        public List<CosmeticDefinition> CSSIconNames { get; set; }
        public List<CosmeticDefinition> ReplayIcons { get; set; }
        public List<CosmeticDefinition> FranchiseIcons { get; set; }
        public List<CosmeticDefinition> StockIcons { get; set; }
    }

    public class PatSettings
    {
        public string Path { get; set; }
        public int FrameCountOffset { get; set; } = 0;
        public int FrameCountOverride { get; set; } = 0;
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
}
