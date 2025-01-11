﻿using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.StaticClasses;
using BrawlLib.Internal;
using BrawlLib.SSBB.ResourceNodes;
using lKHM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrawlLib.BrawlManagerLib.TextureContainer;

namespace BrawlInstaller.Services
{
    public interface IPackageService
    {
        /// <inheritdoc cref="PackageService.ExtractFighter(FighterInfo)"/>
        FighterPackage ExtractFighter(FighterInfo fighterInfo);

        /// <inheritdoc cref="PackageService.SaveFighter(FighterPackage, FighterPackage)"/>
        void SaveFighter(FighterPackage fighterPackage, FighterPackage oldFighter);

        /// <inheritdoc cref="PackageService.ExportFighter(FighterPackage, string)"/>
        void ExportFighter(FighterPackage fighterPackage, string outFile);

        /// <inheritdoc cref="PackageService.LoadFighterPackage(string)"/>
        FighterPackage LoadFighterPackage(string inFile);
    }
    [Export(typeof(IPackageService))]
    internal class PackageService : IPackageService
    {
        // Services
        IFileService _fileService { get; }
        ISettingsService _settingsService { get; }
        ICosmeticService _cosmeticService { get; }
        IFighterService _fighterService { get; }
        ICodeService _codeService { get; }

        [ImportingConstructor]
        public PackageService(IFileService fileService, ISettingsService settingsService, ICosmeticService cosmeticService, IFighterService fighterService, ICodeService codeService) 
        {
            _fileService = fileService;
            _settingsService = settingsService;
            _cosmeticService = cosmeticService;
            _fighterService = fighterService;
            _codeService = codeService;
        }

        // Methods

        /// <summary>
        /// Generate fighter package from build
        /// </summary>
        /// <param name="fighterIds">IDs of fighter</param>
        /// <returns>Fighter package</returns>
        public FighterPackage ExtractFighter(FighterInfo fighterInfo)
        {
            var fighterPackage = new FighterPackage();
            // Get fighter info
            fighterInfo = _fighterService.GetFighterInfo(fighterInfo);
            fighterPackage.FighterInfo = fighterInfo;

            // Get fighter files
            fighterPackage = _fighterService.GetFighterFiles(fighterPackage);

            // Get Effect.pac IDs
            fighterPackage.FighterInfo.EffectPacId = _fighterService.GetFighterEffectPacId(fighterPackage.PacFiles, fighterInfo.PacFileName);
            fighterPackage.FighterInfo.OriginalEffectPacId = fighterPackage.FighterInfo.EffectPacId;
            fighterPackage.FighterInfo.KirbyEffectPacId = _fighterService.GetFighterEffectPacId(fighterPackage.PacFiles, fighterInfo.KirbyPacFileName);
            fighterPackage.FighterInfo.OriginalKirbyEffectPacId = fighterPackage.FighterInfo.KirbyEffectPacId;

            // Get fighter settings
            fighterPackage.FighterSettings = _fighterService.GetFighterSettings(fighterPackage);

            // Get cosmetics
            var cosmetics = _cosmeticService.GetFighterCosmetics(fighterInfo.Ids);

            // Get costumes
            fighterPackage.Costumes = _fighterService.GetCostumeCosmetics(fighterPackage.Costumes, cosmetics);

            // Set up inheritance for styles
            var inheritedStyles = new Dictionary<(CosmeticType, string), string>();
            foreach(var cosmeticType in cosmetics.GroupBy(x => x.CosmeticType).Select(x => x.Key))
            {
                var typedCosmetics = cosmetics.Where(x => x.CosmeticType == cosmeticType);
                var groups = typedCosmetics.GroupBy(x => x.Style);
                foreach (var group in groups)
                {
                    // All MD5 hashes must be contained in both groups and count must be the same for it to be a match
                    var match = groups.Where(x => x.Key != group.Key)
                        .FirstOrDefault(x => group.Select(y => y.Texture?.MD5Str()).All(x.Select(y => y.Texture?.MD5Str()).Contains)
                        && x.Select(y => y.Texture?.MD5Str()).All(group.Select(y => y.Texture?.MD5Str()).Contains)
                        && group.Count() == x.Count());
                    if (match != null && !inheritedStyles.Any(x => (x.Key == (cosmeticType, group.Key)) || x.Value == group.Key))
                    {
                        inheritedStyles.Add((cosmeticType, group.Key), match.Key);
                    }
                }
            }

            fighterPackage.Cosmetics.Items = cosmetics;
            fighterPackage.Cosmetics.InheritedStyles = inheritedStyles;
            return fighterPackage;
        }

        /// <summary>
        /// Save fighter to build
        /// </summary>
        /// <param name="fighterPackage">Fighter package to save</param>
        public void SaveFighter(FighterPackage fighterPackage, FighterPackage oldFighter)
        {
            // Get old fighter if none exists
            if (oldFighter == null)
            {
                oldFighter = new FighterPackage();
                oldFighter.FighterInfo = fighterPackage.FighterInfo.Copy();
                oldFighter.FighterInfo = _fighterService.GetFighterInfo(oldFighter.FighterInfo);
            }
            var buildPath = _settingsService.AppSettings.BuildPath;
            // Get unused IDs for null IDs
            fighterPackage.FighterInfo = _fighterService.UpdateNullIdsToFirstUnused(fighterPackage.FighterInfo);
            // Only update cosmetics that have changed
            var changedDefinitions = _settingsService.BuildSettings.CosmeticSettings.Where(x => fighterPackage.Cosmetics.ChangedItems
            .Any(y => y.CosmeticType == x.CosmeticType && y.Style == x.Style)).ToList();

            var inheritedDefinitions = new List<CosmeticDefinition>();

            // Handle inherited styles
            foreach (var definition in changedDefinitions)
            {
                // If a key is found matching the definition, and it has a value different from the key style, it should be replaced
                if (fighterPackage.Cosmetics.InheritedStyles.Any(x => x.Key.Item1 == definition.CosmeticType && x.Value == definition.Style))
                {
                    var inheritedStyle = fighterPackage.Cosmetics.InheritedStyles.FirstOrDefault(x => x.Key.Item1 == definition.CosmeticType && x.Value == definition.Style).Key.Item2;
                    if (inheritedStyle != definition.Style)
                    {
                        var inheritedDefinition = _settingsService.BuildSettings.CosmeticSettings.FirstOrDefault(x => x.Style == inheritedStyle);
                        if (inheritedDefinition != null)
                        {
                            // Copy the definition
                            var newDefinition = inheritedDefinition.Copy();
                            // Change it's style to match the style of the original definition
                            newDefinition.Style = definition.Style;
                            // Add it to the change list so it will be detected
                            inheritedDefinitions.Add(newDefinition);
                        }
                    }
                }
            }

            changedDefinitions.AddRange(inheritedDefinitions);

            // Import cosmetics
            _cosmeticService.ImportCosmetics(changedDefinitions, fighterPackage.Cosmetics, fighterPackage.FighterInfo.Ids, fighterPackage.FighterInfo.DisplayName);
            // Import fighter files
            _fighterService.ImportFighterFiles(fighterPackage, oldFighter);
            // Set original Effect.pac and soundbank ID to current
            fighterPackage.FighterInfo.OriginalEffectPacId = fighterPackage.FighterInfo.EffectPacId;
            fighterPackage.FighterInfo.OriginalKirbyEffectPacId = fighterPackage.FighterInfo.KirbyEffectPacId;
            fighterPackage.FighterInfo.OriginalSoundbankId = fighterPackage.FighterInfo.SoundbankId;
            fighterPackage.FighterInfo.OriginalKirbySoundbankId = fighterPackage.FighterInfo.KirbySoundbankId;
            // Update fighter settings if they have changed or if fighter name has changed
            var updateSettings = fighterPackage.PackageType == PackageType.New || !fighterPackage.FighterSettings.Compare(oldFighter.FighterSettings) || fighterPackage.FighterInfo.DisplayName != oldFighter.FighterInfo.DisplayName;
            _fighterService.UpdateFighterSettings(fighterPackage, updateSettings);
            // Update credits module
            if (changedDefinitions.Any(x => x.CosmeticType == CosmeticType.CreditsIcon))
            {
                _fighterService.UpdateCreditsModule(fighterPackage);
            }
            // Compile GCT
            _codeService.CompileCodes();
            // Set package type to update, in case it was a new package
            fighterPackage.PackageType = PackageType.Update;
        }

        /// <summary>
        /// Load fighter package from folder
        /// </summary>
        /// <returns>Fighter package</returns>
        public FighterPackage LoadFighterPackage(string inFile)
        {
            var path = _settingsService.AppSettings.TempPath + "\\FighterPackageImport";
            var output = _fileService.ExtractZipFile(inFile, path);
            if (!string.IsNullOrEmpty(output))
            {
                if (_fileService.DirectoryExists(path))
                {
                    path = Path.GetFullPath(path);
                }
                else
                {
                    return null;
                }
                var fighterPackage = new FighterPackage();
                // Get fighter info
                var fighterInfoPath = _fileService.GetFiles(path, "FighterInfo.json").FirstOrDefault();
                if (!string.IsNullOrEmpty(fighterInfoPath))
                {
                    var fighterInfoJson = _fileService.ReadTextFile(fighterInfoPath);
                    fighterPackage.FighterInfo = JsonConvert.DeserializeObject<FighterInfo>(fighterInfoJson);
                    fighterPackage.FighterInfo.OriginalSoundbankId = fighterPackage.FighterInfo.SoundbankId;
                    fighterPackage.FighterInfo.OriginalKirbySoundbankId = fighterPackage.FighterInfo.KirbySoundbankId;
                    fighterPackage.FighterInfo.OriginalEffectPacId = fighterPackage.FighterInfo.EffectPacId;
                    fighterPackage.FighterInfo.OriginalKirbyEffectPacId = fighterPackage.FighterInfo.KirbyEffectPacId;
                    // Set IDs to first available
                    fighterPackage.FighterInfo = _fighterService.UpdateIdsToFirstUnused(fighterPackage.FighterInfo);
                }
                // Get fighter settings
                var fighterSettingsPath = _fileService.GetFiles(path, "FighterSettings.json").FirstOrDefault();
                if (!string.IsNullOrEmpty(fighterSettingsPath))
                {
                    var fighterSettingsJson = _fileService.ReadTextFile(fighterSettingsPath);
                    fighterPackage.FighterSettings = JsonConvert.DeserializeObject<FighterSettings>(fighterSettingsJson);
                }
                // Get Kirby hat data
                fighterPackage.FighterSettings.KirbyHatData = _fighterService.ConvertXMLToKirbyHatData(Path.Combine(path, "KirbyHat.xml"));
                // Get cosmetics
                fighterPackage.Cosmetics = _cosmeticService.LoadCosmetics($"{path}\\Cosmetics");
                // Get costumes
                var costumeJson = _fileService.ReadTextFile($"{path}\\Costumes\\Costumes.json");
                if (!string.IsNullOrEmpty(costumeJson))
                {
                    var costumes = JsonConvert.DeserializeObject<List<Costume>>(costumeJson);
                    costumes = _fighterService.GetCostumeCosmetics(costumes, fighterPackage.Cosmetics.Items);
                    foreach (var costume in costumes)
                    {
                        var dirs = new List<string>();
                        var costumePacPath = $"{path}\\Costumes\\PacFiles\\{costume.CostumeId:D4}";
                        dirs.Add(costumePacPath);
                        dirs.AddRange(_fileService.GetDirectories(costumePacPath, "*", SearchOption.TopDirectoryOnly));
                        foreach (var dir in dirs)
                        {
                            var pacFiles = _fileService.GetFiles(dir, "*.pac");
                            foreach (var pacFile in pacFiles)
                            {
                                var newPacFile = new FighterPacFile { FilePath = pacFile };
                                newPacFile = _fighterService.GetFighterPacName(newPacFile, fighterPackage.FighterInfo, true);
                                costume.PacFiles.Add(newPacFile);
                            }
                        }
                    }
                    fighterPackage.Costumes = costumes;
                }
                // Get pac files
                var pacDirs = new List<string>();
                var pacPath = $"{path}\\PacFiles";
                pacDirs.Add(pacPath);
                pacDirs.AddRange(_fileService.GetDirectories(pacPath, "*", SearchOption.TopDirectoryOnly));
                foreach (var dir in pacDirs)
                {
                    var pacFiles = _fileService.GetFiles(dir, "*.pac");
                    foreach (var pacFile in pacFiles)
                    {
                        var newPacFile = new FighterPacFile { FilePath = pacFile };
                        newPacFile = _fighterService.GetFighterPacName(newPacFile, fighterPackage.FighterInfo, false);
                        fighterPackage.PacFiles.Add(newPacFile);
                    }
                }
                // Get module
                fighterPackage.Module = _fileService.GetFiles($"{path}\\Module", "*.rel").FirstOrDefault();
                // Get ending pac files
                fighterPackage.EndingPacFiles = _fileService.GetFiles($"{path}\\EndingPacFiles", "*.pac");
                // Get other files
                fighterPackage.Soundbank = _fileService.GetFiles($"{path}\\Soundbank", "*.sawnd").FirstOrDefault();
                fighterPackage.KirbySoundbank = _fileService.GetFiles($"{path}\\KirbySoundbank", "*.sawnd").FirstOrDefault();
                fighterPackage.ClassicIntro = _fileService.GetFiles($"{path}\\ClassicIntro", "*.brres").FirstOrDefault();
                fighterPackage.EndingMovie = _fileService.GetFiles($"{path}\\EndingMovie", "*.thp").FirstOrDefault();
                // Get victory and credits themes
                var victoryThemeJson = _fileService.GetFiles(path, "VictoryTheme.json").FirstOrDefault();
                var victoryTheme = _fileService.GetFiles($"{path}\\VictoryTheme", "*.brstm").FirstOrDefault();
                if (!string.IsNullOrEmpty(victoryThemeJson))
                {
                    fighterPackage.VictoryTheme = JsonConvert.DeserializeObject<TracklistSong>(_fileService.ReadTextFile(victoryThemeJson));
                    fighterPackage.VictoryTheme.SongFile = victoryTheme;
                }
                else
                {
                    fighterPackage.VictoryTheme = new TracklistSong
                    {
                        Name = !string.IsNullOrEmpty(victoryTheme) ? fighterPackage.FighterInfo.DisplayName : string.Empty,
                        SongFile = victoryTheme,
                        SongPath = !string.IsNullOrEmpty(victoryTheme) ? $"Victory!/{Path.GetFileNameWithoutExtension(victoryTheme)}" : string.Empty,
                        SongId = fighterPackage.FighterInfo.VictoryThemeId
                    };
                }
                var creditsThemeJson = _fileService.GetFiles(path, "CreditsTheme.json").FirstOrDefault();
                var creditsTheme = _fileService.GetFiles($"{path}\\CreditsTheme", "*.brstm").FirstOrDefault();
                if (!string.IsNullOrEmpty(creditsThemeJson))
                {
                    fighterPackage.CreditsTheme = JsonConvert.DeserializeObject<TracklistSong>(_fileService.ReadTextFile(creditsThemeJson));
                    fighterPackage.CreditsTheme.SongFile = creditsTheme;
                }
                else
                {
                    fighterPackage.CreditsTheme = new TracklistSong
                    {
                        Name = !string.IsNullOrEmpty(creditsTheme) ? (!string.IsNullOrEmpty(creditsTheme) ? Path.GetFileNameWithoutExtension(creditsTheme) : fighterPackage.FighterInfo.DisplayName) : string.Empty,
                        SongFile = creditsTheme,
                        SongPath = !string.IsNullOrEmpty(creditsTheme) ? $"Credits/{Path.GetFileNameWithoutExtension(creditsTheme)}" : string.Empty,
                        SongId = fighterPackage.FighterInfo.CreditsThemeId
                    };
                }
                // Get Effect.pac IDs
                if (fighterPackage.FighterInfo.EffectPacId == null)
                {
                    fighterPackage.FighterInfo.EffectPacId = _fighterService.GetFighterEffectPacId(fighterPackage.PacFiles, fighterPackage.FighterInfo.PacFileName);
                    fighterPackage.FighterInfo.OriginalEffectPacId = fighterPackage.FighterInfo.EffectPacId;
                }
                if (fighterPackage.FighterInfo.KirbyEffectPacId == null)
                {
                    fighterPackage.FighterInfo.KirbyEffectPacId = _fighterService.GetFighterEffectPacId(fighterPackage.PacFiles, fighterPackage.FighterInfo.KirbyPacFileName);
                    fighterPackage.FighterInfo.OriginalKirbyEffectPacId = fighterPackage.FighterInfo.KirbyEffectPacId;
                }
                fighterPackage.PackageType = PackageType.New;
                return fighterPackage;
            }
            return null;
        }

        /// <summary>
        /// Export fighter package to filesystem
        /// </summary>
        /// <param name="fighterPackage">Fighter package to export</param>
        public void ExportFighter(FighterPackage fighterPackage, string outFile)
        {
            var path = _settingsService.AppSettings.TempPath + "\\FighterPackageExport";
            _cosmeticService.ExportCosmetics($"{path}\\Cosmetics", fighterPackage.Cosmetics);
            var fighterSettings = JsonConvert.SerializeObject(fighterPackage.FighterSettings, Formatting.Indented);
            // Set pac files for export
            foreach(var pacFile in fighterPackage.PacFiles)
            {
                pacFile.SavePath = $"{path}\\PacFiles\\{pacFile.GetPrefix(fighterPackage.FighterInfo)}{pacFile.Suffix}{fighterPackage.FighterInfo.PacExtension}";
            }
            // Set costumes for export
            var costumeJson = JsonConvert.SerializeObject(fighterPackage.Costumes, Formatting.Indented);
            _fileService.SaveTextFile($"{path}\\Costumes\\Costumes.json", costumeJson);
            foreach (var costume in fighterPackage.Costumes)
            {
                var costumePath = $"{path}\\Costumes\\PacFiles\\{costume.CostumeId:D4}";
                foreach (var pacFile in costume.PacFiles)
                {
                    pacFile.SavePath = $"{costumePath}\\{pacFile.GetPrefix(fighterPackage.FighterInfo)}{pacFile.Suffix}{costume.CostumeId:D2}{fighterPackage.FighterInfo.PacExtension}";
                }
            }
            // Update and export pac files
            var pacFileNodes = _fighterService.UpdatePacFiles(fighterPackage, false);
            foreach(var file in pacFileNodes)
            {
                _fileService.SaveFile(file);
                _fileService.CloseFile(file);
            }
            // Export other files
            var files = new List<(string Folder, string File)>
            {
                ("Module", fighterPackage.Module),
                ("Soundbank", fighterPackage.Soundbank),
                ("KirbySoundbank", fighterPackage.KirbySoundbank),
                ("ClassicIntro", fighterPackage.ClassicIntro),
                ("EndingMovie", fighterPackage.EndingMovie),
                ("CreditsTheme", fighterPackage.CreditsTheme.SongFile),
                ("VictoryTheme", fighterPackage.VictoryTheme.SongFile)
            };
            foreach(var endingPacFile in fighterPackage.EndingPacFiles)
            {
                files.Add(("EndingPacFiles",  endingPacFile));
            }
            foreach(var file in files)
            {
                _fileService.CopyFile(file.File, $"{path}\\{file.Folder}\\{Path.GetFileName(file.File)}");
            }
            // Export Kirby hat data
            _fighterService.ExportKirbyHatDataToXml($"{path}\\KirbyHat.xml", fighterPackage.FighterSettings.KirbyHatData);
            // Set theme IDs
            fighterPackage.FighterInfo.VictoryThemeId = fighterPackage.VictoryTheme.SongId;
            fighterPackage.FighterInfo.CreditsThemeId = fighterPackage.CreditsTheme.SongId;
            var fighterInfo = JsonConvert.SerializeObject(fighterPackage.FighterInfo, Formatting.Indented);
            // Export tracklist info
            if (fighterPackage.VictoryTheme != null)
            {
                var victoryThemeJson = JsonConvert.SerializeObject(fighterPackage.VictoryTheme.CopyNoFile(), Formatting.Indented);
                _fileService.SaveTextFile($"{path}\\VictoryTheme.json", victoryThemeJson);
            }
            if (fighterPackage.CreditsTheme != null)
            {
                var creditsThemeJson = JsonConvert.SerializeObject(fighterPackage.CreditsTheme.CopyNoFile(), Formatting.Indented);
                _fileService.SaveTextFile($"{path}\\CreditsTheme.json", creditsThemeJson);
            }
            // Export info and settings
            _fileService.SaveTextFile($"{path}\\FighterInfo.json", fighterInfo);
            _fileService.SaveTextFile($"{path}\\FighterSettings.json", fighterSettings);
            // Update info
            fighterPackage.FighterInfo.OriginalSoundbankId = fighterPackage.FighterInfo.SoundbankId;
            fighterPackage.FighterInfo.OriginalKirbySoundbankId = fighterPackage.FighterInfo.KirbySoundbankId;
            fighterPackage.FighterInfo.OriginalEffectPacId = fighterPackage.FighterInfo.EffectPacId;
            fighterPackage.FighterInfo.OriginalKirbyEffectPacId = fighterPackage.FighterInfo.OriginalKirbyEffectPacId;
            // Delete fighter package file if it exists
            _fileService.DeleteFile(outFile);
            // Generate fighter package file
            _fileService.GenerateZipFileFromDirectory(path, outFile);
            _fileService.DeleteDirectory(path);
        }
    }
}
