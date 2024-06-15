using BrawlInstaller.Classes;
using BrawlLib.SSBB.ResourceNodes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using BrawlLib.Wii.Textures;
using BrawlInstaller.Enums;

namespace BrawlInstaller.Services
{
    public interface ISettingsService
    {
        // Properties
        // Should be accessible from any ViewModel implementing the service
        BuildSettings BuildSettings { get; set; }
        string BuildPath { get; set; }

        // Methods
        void SaveSettings(BuildSettings buildSettings, string path);
        BuildSettings LoadSettings(string path);
        BuildSettings GetDefaultSettings();
        void SaveFighterInfoSettings(List<FighterInfo> fighterInfoList);
        List<FighterInfo> LoadFighterInfoSettings();
    }
    [Export(typeof(ISettingsService))]
    internal class SettingsService : ISettingsService
    {
        [ImportingConstructor]
        public SettingsService()
        {

        }

        // Properties
        public BuildSettings BuildSettings { get; set; } = null;
        public string BuildPath { get; set; } = "";

        // Methods

        /// <summary>
        /// Save build settings
        /// </summary>
        /// <param name="buildSettings">Build settings</param>
        /// <param name="path">Path to save to</param>
        public void SaveSettings(BuildSettings buildSettings, string path)
        {
            var jsonString = JsonConvert.SerializeObject(buildSettings);
            File.WriteAllText(path, jsonString);
        }

        /// <summary>
        /// Load build settings
        /// </summary>
        /// <param name="path">Path to load from</param>
        /// <returns>Build settings</returns>
        public BuildSettings LoadSettings(string path)
        {
            var text = File.ReadAllText(path);
            var buildSettings = JsonConvert.DeserializeObject<BuildSettings>(text);
            return buildSettings;
        }

        /// <summary>
        /// Save list of fighters to build
        /// </summary>
        /// <param name="fighterInfoList">List of fighters to save</param>
        public void SaveFighterInfoSettings(List<FighterInfo> fighterInfoList)
        {
            var fighterInfoSettings = JsonConvert.SerializeObject(fighterInfoList, Formatting.Indented);
            File.WriteAllText($"{BuildPath}\\FighterList.json", fighterInfoSettings);
        }

        /// <summary>
        /// Load list of fighters in build
        /// </summary>
        /// <returns>Information for all fighters in build</returns>
        public List<FighterInfo> LoadFighterInfoSettings()
        {
            var fighterList = new List<FighterInfo>();
            var path = $"{BuildPath}\\FighterList.json";
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                fighterList = JsonConvert.DeserializeObject<List<FighterInfo>>(text);
            }
            return fighterList;
        }

        /// <summary>
        /// Get default build settings
        /// </summary>
        /// <returns>Default build settings</returns>
        public BuildSettings GetDefaultSettings()
        {
            var buildSettings = new BuildSettings
            {
                CosmeticSettings = new List<CosmeticDefinition>
                {
                    new CosmeticDefinition
                    {
                        CosmeticType = CosmeticType.CSP,
                        Style = "vBrawl",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\menu\\common\\char_bust_tex\\",
                            NodePath = "",
                            FileExtension = "brres"
                        },
                        Prefix = "MenSelchrFaceB",
                        Multiplier = 10,
                        IdType = IdType.Cosmetic,
                        Size = new Size(128, 160),
                        FirstOnly = false
                    },
                    new CosmeticDefinition
                    {
                        CosmeticType = CosmeticType.PortraitName,
                        Style = "PM",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\menu2\\sc_selcharacter.pac",
                            NodePath = "Misc Data [30]",
                            FileExtension = "pac"
                        },
                        PatSettings = new PatSettings
                        {
                            Paths = new List<string>
                            {
                                "Misc Data [30]/AnmTexPat(NW4R)/MenSelchrCname4_TopN__0/Card010/Texture0",
                                "Misc Data [30]/AnmTexPat(NW4R)/MenSelchrCname4_TopN__0/Card011/Texture0"
                            }
                        },
                        Prefix = "MenSelchrChrNm",
                        Multiplier = 10,
                        IdType = IdType.Cosmetic,
                        Size = new Size(144, 32),
                        FirstOnly = false
                    },
                    new CosmeticDefinition
                    {
                        CosmeticType = CosmeticType.BP,
                        Style = "vBrawl",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\info\\portrite\\",
                            NodePath = "",
                            FileExtension = "brres"
                        },
                        Prefix = "InfFace",
                        Multiplier = 50,
                        SuffixDigits = 4,
                        IdType = IdType.Cosmetic,
                        Size = new Size(48, 56),
                        FirstOnly = false,
                        SeparateFiles = true
                    },
                    new CosmeticDefinition
                    {
                        CosmeticType = CosmeticType.StockIcon,
                        Style = "P+",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\menu\\common\\StockFaceTex.brres",
                            NodePath = "",
                            FileExtension = "brres"
                        },
                        Prefix = "InfStc",
                        Multiplier = 50,
                        SuffixDigits = 4,
                        IdType = IdType.Cosmetic,
                        Size = new Size(32, 32),
                        FirstOnly = false
                    },
                    new CosmeticDefinition
                    {
                        CosmeticType = CosmeticType.CSSIcon,
                        Style = "P+",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\menu2\\sc_selcharacter.pac",
                            NodePath = "Misc Data [70]",
                            FileExtension = "pac"
                        },
                        PatSettings = new PatSettings
                        {
                            Paths = new List<string> { "Misc Data [30]/AnmTexPat(NW4R)/MenSelchrFaceI_TopN__0/Face02/Texture0" },
                            Multiplier = 10,
                            IdType = IdType.Cosmetic
                        },
                        Prefix = "MenSelchrChrFace",
                        Multiplier = 1,
                        IdType = IdType.Cosmetic,
                        Size = new Size(64, 64)
                    },
                    new CosmeticDefinition
                    {
                        CosmeticType = CosmeticType.CSSIcon,
                        Style = "vBrawl",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\menu\\adventure\\selchrcd_common.brres",
                            NodePath = "",
                            FileExtension = "brres"
                        },
                        PatSettings = new PatSettings
                        {
                            Paths = new List<string> { "AnmTexPat(NW4R)/MenAdvChrCd0001_TopN__0/Face02/Texture0" },
                            Multiplier = 10,
                            IdType = IdType.Cosmetic
                        },
                        Prefix = "MenSelchrChrFace",
                        Multiplier = 1,
                        IdType = IdType.Cosmetic,
                        Size = new Size(80, 56)
                    },
                    new CosmeticDefinition
                    {
                        CosmeticType = CosmeticType.ReplayIcon,
                        Style = "P+",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\menu\\collection\\Replay.brres",
                            NodePath = "",
                            FileExtension = "brres"
                        },
                        PatSettings = new PatSettings
                        {
                            Paths = new List<string> { "AnmTexPat(NW4R)/MenReplayPreview2_TopN__0/lambert78/Texture1" }
                        },
                        Prefix = "MenReplayChr",
                        Multiplier = 10,
                        IdType = IdType.Cosmetic,
                        Size = new Size(64, 24)
                    },
                    new CosmeticDefinition
                    {
                        CosmeticType = CosmeticType.CreditsIcon,
                        Style = "vBrawl",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\stage\\melee\\STGCHARAROLL.PAC",
                            NodePath = "2/Texture Data [0]",
                            FileExtension = "pac"
                        },
                        Prefix = "ChrRollFighter",
                        IdType = IdType.CosmeticConfig,
                        Offset = 1,
                        Size = new Size(128, 160)
                    },
                    new CosmeticDefinition
                    {
                        CosmeticType = CosmeticType.FranchiseIcon,
                        Style = "Icon",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\menu2\\sc_selcharacter.pac",
                            NodePath = "Misc Data [30]",
                            FileExtension = "pac"
                        },
                        PatSettings = new PatSettings
                        {
                            Paths = new List<string> { "Misc Data [30]/AnmTexPat(NW4R)/MenSelchrCmark4_TopN__0/Card04/Texture0" }
                        },
                        Prefix = "MenSelchrMark",
                        IdType = IdType.Franchise,
                        Format = WiiPixelFormat.I4,
                        SuffixDigits = 2,
                        Size = new Size(128, 128)
                    },
                    new CosmeticDefinition
                    {
                        CosmeticType = CosmeticType.FranchiseIcon,
                        Style = "Model",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\stage\\melee\\STGRESULT.pac",
                            NodePath = "2/Misc Data [110]",
                            FileExtension = "pac"
                        },
                        ModelPath = "2/Misc Data [110]",
                        Prefix = "InfResultMark",
                        IdType = IdType.Franchise,
                        Size = new Size(80, 80),
                        SuffixDigits = 2
                    },
                    new CosmeticDefinition
                    {
                        CosmeticType = CosmeticType.TrophyThumbnail,
                        Style = "vBrawl",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\menu\\collection\\Figure.brres",
                            NodePath = "",
                            FileExtension = "brres"
                        },
                        Prefix = "MenCollDisply01",
                        IdType = IdType.Thumbnail,
                        Size = new Size(56, 48)
                    }
                },
                ToolPathSettings = new ToolPathSettings
                {
                    AssemblyFunctionsExe = "",
                    GfxChangeExe = "",
                    KirbyHatExe = "",
                    SawndReplaceExe = "",
                    SfxChangeExe = ""
                },
                KirbyHatSettings = new KirbyHatSettings
                {
                    DefaultKirbyHat = "0x21",
                    InstallKirbyHats = false
                },
                SoundSettings = new SoundSettings
                {
                    SoundbankStyle = "hex",
                    IncrementSoundbankIds = true,
                    IncrementSoundbankNames = false
                },
                MiscSettings = new MiscSettings
                {
                    InstallToSse = false,
                    SSEUnlockStage = SSEUnlockStage.End,
                    InstallTrophies = false,
                    CustomStageLists = null
                },
                FilePathSettings = new FilePathSettings
                {
                    FighterFiles = "pf\\fighter",
                    BrawlEx = "pf\\BrawlEx",
                    Modules = "pf\\module",
                    HDTextures = "F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild Launcher - For Netplay\\User\\Load\\Textures\\RSBE01"
                }
            };
            return buildSettings;
        }
    }
}
