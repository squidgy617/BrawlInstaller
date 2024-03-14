﻿using BrawlInstaller.Classes;
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
using System.Windows;

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
        public void SaveSettings(BuildSettings buildSettings, string path)
        {
            var jsonString = JsonConvert.SerializeObject(buildSettings);
            File.WriteAllText(path, jsonString);
        }

        public BuildSettings LoadSettings(string path)
        {
            var text = File.ReadAllText(path);
            var buildSettings = JsonConvert.DeserializeObject<BuildSettings>(text);
            return buildSettings;
        }

        public BuildSettings GetDefaultSettings()
        {
            var buildSettings = new BuildSettings
            {
                CosmeticSettings = new List<CosmeticDefinition>
                {
                    new CosmeticDefinition
                    {
                        CosmeticType = Enums.CosmeticType.CSP,
                        Style = "vBrawl",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\menu\\common\\char_bust_tex\\",
                            NodePath = "",
                            FileExtension = "brres"
                        },
                        Prefix = "MenSelchrFaceB",
                        Multiplier = 10,
                        IdType = Enums.IdType.Cosmetic,
                        Size = new Size(48, 56),
                        FirstOnly = false
                    },
                    new CosmeticDefinition
                    {
                        CosmeticType = Enums.CosmeticType.BP,
                        Style = "vBrawl",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\info\\portrite\\",
                            NodePath = "",
                            FileExtension = "brres"
                        },
                        Prefix = "InfFace",
                        Multiplier = 50,
                        IdType = Enums.IdType.Cosmetic,
                        Size = new Size(48, 56),
                        FirstOnly = false,
                        SeparateFiles = true
                    },
                    new CosmeticDefinition
                    {
                        CosmeticType = Enums.CosmeticType.StockIcon,
                        Style = "P+",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\menu\\common\\StockFaceTex.brres",
                            NodePath = "",
                            FileExtension = "brres"
                        },
                        Prefix = "InfStc",
                        Multiplier = 50,
                        IdType = Enums.IdType.Cosmetic,
                        Size = new Size(32,32),
                        FirstOnly = false
                    },
                    new CosmeticDefinition
                    {
                        CosmeticType = Enums.CosmeticType.CSSIcon,
                        Style = "P+",
                        InstallLocation = new InstallLocation
                        {
                            FilePath = "pf\\menu2\\sc_selcharacter.pac",
                            NodePath = "Misc Data [70]",
                            FileExtension = "pac"
                        },
                        PatSettings = new PatSettings
                        {
                            Path = "Misc Data [30]/AnmTexPat(NW4R)/MenSelchrFaceI_TopN__0/Face02/Texture0"
                        },
                        Prefix = "MenSelchrChrFace",
                        Multiplier = 10,
                        IdType = Enums.IdType.Cosmetic,
                        Size = new Size(64,64)
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
                    SSEUnlockStage = Enums.SSEUnlockStage.End,
                    InstallTrophies = false,
                    CustomStageLists = null
                }
            };
            return buildSettings;
        }
    }
}
