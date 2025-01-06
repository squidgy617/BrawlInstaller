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
using BrawlInstaller.StaticClasses;

namespace BrawlInstaller.Services
{
    public interface ISettingsService
    {
        // Properties
        // Should be accessible from any ViewModel implementing the service
        BuildSettings BuildSettings { get; set; }
        AppSettings AppSettings { get; set; }
        List<FighterInfo> FighterInfoList { get; set; }
        string BuildSettingsPath { get; }
        string FighterListPath { get; }

        // Methods

        /// <inheritdoc cref="SettingsService.SaveSettings(BuildSettings, string)"/>
        void SaveSettings(BuildSettings buildSettings, string path);

        /// <inheritdoc cref="SettingsService.SaveAppSettings(AppSettings)"/>
        void SaveAppSettings(AppSettings appSettings);

        /// <inheritdoc cref="SettingsService.LoadSettings(string)"/>
        BuildSettings LoadSettings(string path);

        /// <inheritdoc cref="SettingsService.LoadAppSettings()"/>
        AppSettings LoadAppSettings();

        /// <inheritdoc cref="SettingsService.SaveFighterInfoSettings(List{FighterInfo})"/>
        void SaveFighterInfoSettings(List<FighterInfo> fighterInfoList);

        /// <inheritdoc cref="SettingsService.LoadFighterInfoSettings()"/>
        List<FighterInfo> LoadFighterInfoSettings();

        /// <inheritdoc cref="SettingsService.GetBuildFilePath(string)"/>
        string GetBuildFilePath(string path);

        /// <inheritdoc cref="SettingsService.GetAllPaths()"/>
        List<string> GetAllPaths();
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
        public string BuildSettingsFolder { get => "BrawlInstaller"; }
        public string BuildSettingsPath { get => Path.Combine(AppSettings?.BuildPath, BuildSettingsFolder, "BuildSettings.json"); }
        public string FighterListPath { get => Path.Combine(AppSettings?.BuildPath, BuildSettingsFolder, "FighterList.json"); }

        // Methods

        // TODO: Should path just be hardcoded into these? Rather than passing it, since it will be same every time?
        /// <summary>
        /// Save build settings
        /// </summary>
        /// <param name="buildSettings">Build settings</param>
        /// <param name="path">Path to save to</param>
        public void SaveSettings(BuildSettings buildSettings, string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            var jsonString = JsonConvert.SerializeObject(buildSettings, Formatting.Indented);
            File.WriteAllText(path, jsonString);
            BuildSettings = buildSettings;
        }

        /// <summary>
        /// Save app settings
        /// </summary>
        /// <param name="appSettings">App settings to save</param>
        public void SaveAppSettings(AppSettings appSettings)
        {
            var jsonString = JsonConvert.SerializeObject(appSettings, Formatting.Indented);
            File.WriteAllText(Paths.AppSettingsPath, jsonString);
            AppSettings = appSettings;
        }

        /// <summary>
        /// Load build settings
        /// </summary>
        /// <param name="path">Path to load from</param>
        /// <returns>Build settings</returns>
        public BuildSettings LoadSettings(string path)
        {
            var buildSettings = new BuildSettings();
            var defaultSettings = new BuildSettings();
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                buildSettings = JsonConvert.DeserializeObject<BuildSettings>(text);
            }
            // If any paths are missing, add them
            var missingPaths = defaultSettings.FilePathSettings.FilePaths.Where(x => !buildSettings.FilePathSettings.FilePaths.Select(y => y.FileType).Contains(x.FileType));
            foreach(var missingPath in missingPaths)
            {
                buildSettings.FilePathSettings.FilePaths.Add(missingPath);
            }
            var missingAsms = defaultSettings.FilePathSettings.AsmPaths.Where(x => !buildSettings.FilePathSettings.AsmPaths.Select(y => y.FileType).Contains(x.FileType));
            foreach (var missingAsm in missingAsms)
            {
                buildSettings.FilePathSettings.AsmPaths.Add(missingAsm);
            }
            return buildSettings;
        }

        /// <summary>
        /// Load app settings
        /// </summary>
        /// <returns>App settings</returns>
        public AppSettings LoadAppSettings()
        {
            var appSettings = new AppSettings();
            if (File.Exists(Paths.AppSettingsPath))
            {
                var text = File.ReadAllText(Paths.AppSettingsPath);
                appSettings = JsonConvert.DeserializeObject<AppSettings>(text);
            }
            AppSettings = appSettings;
            return appSettings;
        }

        /// <summary>
        /// Save list of fighters to build
        /// </summary>
        /// <param name="fighterInfoList">List of fighters to save</param>
        public void SaveFighterInfoSettings(List<FighterInfo> fighterInfoList)
        {
            var fighterInfoSettings = JsonConvert.SerializeObject(fighterInfoList, Formatting.Indented);
            File.WriteAllText(FighterListPath, fighterInfoSettings);
            FighterInfoList = fighterInfoList;
        }

        /// <summary>
        /// Load list of fighters in build
        /// </summary>
        /// <returns>Information for all fighters in build</returns>
        public List<FighterInfo> LoadFighterInfoSettings()
        {
            var fighterList = new List<FighterInfo>();
            var path = FighterListPath;
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                fighterList = JsonConvert.DeserializeObject<List<FighterInfo>>(text);
            }
            return fighterList;
        }

        /// <summary>
        /// Get the build path of a file from a partial path
        /// </summary>
        /// <param name="path">Partial path of file</param>
        /// <returns>Full build path of file</returns>
        public string GetBuildFilePath(string path)
        {
            return Path.Combine(AppSettings.BuildPath, path);
        }

        /// <summary>
        /// Get all paths in settings
        /// </summary>
        /// <returns>List of all paths</returns>
        public List<string> GetAllPaths()
        {
            var paths = BuildSettings.FilePathSettings.FilePaths.Select(x => x.Path);
            paths.Concat(BuildSettings.FilePathSettings.CodeFilePaths.Select(x => x.Path));
            paths.Concat(BuildSettings.CosmeticSettings.Select(x => x.InstallLocation.FilePath));
            paths.Concat(BuildSettings.FilePathSettings.StageListPaths.Select(x => x.Path));
            paths.Concat(BuildSettings.FilePathSettings.CodeFilePaths.Select(x => x.Path));
            paths.Concat(BuildSettings.FilePathSettings.RosterFiles.Select(x => x.FilePath));
            paths.Concat(BuildSettings.FilePathSettings.RandomStageNamesLocations.Select(x => x.FilePath));
            return paths.ToList();
        }
    }
}
