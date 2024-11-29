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
        AppSettings AppSettings { get; set; }
        List<FighterInfo> FighterInfoList { get; set; }

        // Methods

        /// <inheritdoc cref="SettingsService.SaveSettings(BuildSettings, string)"/>
        void SaveSettings(BuildSettings buildSettings, string path);

        /// <inheritdoc cref="SettingsService.LoadSettings(string)"/>
        BuildSettings LoadSettings(string path);

        /// <inheritdoc cref="SettingsService.SaveFighterInfoSettings(List{FighterInfo})"/>
        void SaveFighterInfoSettings(List<FighterInfo> fighterInfoList);

        /// <inheritdoc cref="SettingsService.LoadFighterInfoSettings()"/>
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
        public AppSettings AppSettings { get; set; } = new AppSettings();
        public List<FighterInfo> FighterInfoList { get; set; } = new List<FighterInfo>();

        // Methods

        // TODO: Should path just be hardcoded into these? Rather than passing it, since it will be same every time?
        /// <summary>
        /// Save build settings
        /// </summary>
        /// <param name="buildSettings">Build settings</param>
        /// <param name="path">Path to save to</param>
        public void SaveSettings(BuildSettings buildSettings, string path)
        {
            var jsonString = JsonConvert.SerializeObject(buildSettings, Formatting.Indented);
            File.WriteAllText(path, jsonString);
            BuildSettings = buildSettings;
        }

        /// <summary>
        /// Load build settings
        /// </summary>
        /// <param name="path">Path to load from</param>
        /// <returns>Build settings</returns>
        public BuildSettings LoadSettings(string path)
        {
            var buildSettings = new BuildSettings();
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                buildSettings = JsonConvert.DeserializeObject<BuildSettings>(text);
            }
            return buildSettings;
        }

        /// <summary>
        /// Save list of fighters to build
        /// </summary>
        /// <param name="fighterInfoList">List of fighters to save</param>
        public void SaveFighterInfoSettings(List<FighterInfo> fighterInfoList)
        {
            var fighterInfoSettings = JsonConvert.SerializeObject(fighterInfoList, Formatting.Indented);
            File.WriteAllText($"{AppSettings.BuildPath}\\FighterList.json", fighterInfoSettings);
            FighterInfoList = fighterInfoList;
        }

        /// <summary>
        /// Load list of fighters in build
        /// </summary>
        /// <returns>Information for all fighters in build</returns>
        public List<FighterInfo> LoadFighterInfoSettings()
        {
            var fighterList = new List<FighterInfo>();
            var path = $"{AppSettings.BuildPath}\\FighterList.json";
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                fighterList = JsonConvert.DeserializeObject<List<FighterInfo>>(text);
            }
            return fighterList;
        }
    }
}
