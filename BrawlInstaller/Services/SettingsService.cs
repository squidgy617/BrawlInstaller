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
                CosmeticSettings = new CosmeticSettings
                {
                    CSPs = new List<CosmeticDefinition> 
                    { 
                        new CosmeticDefinition
                        {
                            CosmeticType = Enums.CosmeticType.CSP,
                            Style = "vBrawl",
                            InstallLocation = new InstallLocation
                            {
                                FilePath = "pf\\menu\\common\\char_bust_tex\\",
                                NodePath = "",
                                FilExtension = "brres"
                            },
                            Prefix = "MenSelchrFaceB",
                            FiftyCC = false,
                            Size = new Size(48, 56),
                            FirstOnly = false
                        }
                    },
                    BPs = new List<CosmeticDefinition> 
                    {
                        new CosmeticDefinition
                        {
                            CosmeticType = Enums.CosmeticType.BP,
                            Style = "vBrawl",
                            InstallLocation = new InstallLocation
                            {
                                FilePath = "pf\\info\\portrite\\",
                                NodePath = "",
                                FilExtension = "brres"
                            }
                        }   
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
