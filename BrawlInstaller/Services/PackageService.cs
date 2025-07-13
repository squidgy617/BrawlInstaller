using BrawlInstaller.Classes;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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

        /// <inheritdoc cref="PackageService.LoadLegacyPackage(string)"/>
        FighterPackage LoadLegacyPackage(string inFile);
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
        ITrophyService _trophyService { get; }

        [ImportingConstructor]
        public PackageService(IFileService fileService, ISettingsService settingsService, ICosmeticService cosmeticService, IFighterService fighterService, ICodeService codeService, ITrophyService trophyService) 
        {
            _fileService = fileService;
            _settingsService = settingsService;
            _cosmeticService = cosmeticService;
            _fighterService = fighterService;
            _codeService = codeService;
            _trophyService = trophyService;
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

            // Get trophies
            fighterPackage.Trophies = _fighterService.GetFighterTrophies(fighterInfo.Ids.SlotConfigId);

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
            foreach (var definition in changedDefinitions.ToList())
            {
                // If a key is found matching the definition, and it has a value different from the key style, it should be replaced
                if (fighterPackage.Cosmetics.InheritedStyles.Any(x => x.Key.Item1 == definition.CosmeticType && x.Value == definition.Style))
                {
                    foreach(var inheritedDefinitionPair in fighterPackage.Cosmetics.InheritedStyles.Where(x => x.Key.Item1 == definition.CosmeticType && x.Value == definition.Style))
                    {
                        var inheritedStyle = inheritedDefinitionPair.Key.Item2;
                        if (inheritedStyle != definition.Style)
                        {
                            var inheritedDefinition = _settingsService.BuildSettings.CosmeticSettings.FirstOrDefault(x => x.CosmeticType == definition.CosmeticType && x.Style == inheritedStyle);
                            if (inheritedDefinition != null)
                            {
                                // Copy the definition
                                var newDefinition = inheritedDefinition.Copy();
                                // Change it's style to match the style of the original definition
                                newDefinition.Style = definition.Style;
                                // Add it to the change list so it will be detected
                                inheritedDefinitions.Add(newDefinition);
                                // Remove old definition
                                changedDefinitions.Remove(inheritedDefinition);
                            }
                        }
                    }
                }
            }

            changedDefinitions.AddRange(inheritedDefinitions);

            // Update victory and credits theme IDs as needed
            if (_settingsService.BuildSettings.MiscSettings.VictoryThemesUseFighterIds && fighterPackage.VictoryTheme != null)
            {
                var fighterId = fighterPackage.FighterInfo.Ids.FighterConfigId;
                if (fighterId != null)
                {
                    fighterPackage.VictoryTheme.SongId = (uint)(0xFF00 + fighterId.Value);
                }
            }
            if (_settingsService.BuildSettings.MiscSettings.CreditsThemesUseFighterIds && fighterPackage.CreditsTheme != null)
            {
                var fighterId = fighterPackage.FighterInfo.Ids.FighterConfigId;
                if (fighterId != null)
                {
                    fighterPackage.CreditsTheme.SongId = (uint)(0xFF00 + fighterId.Value);
                }
            }

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
            var updateSettings = fighterPackage.PackageType == PackageType.New || fighterPackage.PackageType == PackageType.Delete || !fighterPackage.FighterSettings.Compare(oldFighter.FighterSettings) || fighterPackage.FighterInfo.DisplayName != oldFighter.FighterInfo.DisplayName;
            _fighterService.UpdateFighterSettings(fighterPackage, updateSettings);
            // Update credits module
            if (changedDefinitions.Any(x => x.CosmeticType == CosmeticType.CreditsIcon))
            {
                _fighterService.UpdateCreditsModule(fighterPackage);
            }
            // Update trophies
            _fighterService.SaveFighterTrophies(fighterPackage, oldFighter);
            // Clean up inheritance
            fighterPackage = CleanupCosmeticInheritance(fighterPackage);
            // Set package type to update, in case it was a new package
            fighterPackage.PackageType = PackageType.Update;
        }

        /// <summary>
        /// Clean up cosmetic inheritance for a fighter package
        /// </summary>
        /// <param name="fighterPackage">Fighter package to update</param>
        /// <returns>Updated fighter package</returns>
        private FighterPackage CleanupCosmeticInheritance(FighterPackage fighterPackage)
        {
            // Remove cosmetics that were set to inherit from other ones
            fighterPackage.Cosmetics.Items.RemoveAll(x => fighterPackage.Cosmetics.InheritedStyles.Any(y => y.Key.BaseType == x.CosmeticType && y.Key.BaseStyle == x.Style));
            foreach (var costume in fighterPackage.Costumes)
            {
                costume.Cosmetics.RemoveAll(x => fighterPackage.Cosmetics.InheritedStyles.Any(y => y.Key.BaseType == x.CosmeticType && y.Key.BaseStyle == x.Style));
            }
            // Copy cosmetics that were inherited from
            foreach (var inheritedStyle in fighterPackage.Cosmetics.InheritedStyles)
            {
                var baseCosmetics = fighterPackage.Cosmetics.Items.Where(x => x.CosmeticType == inheritedStyle.Key.BaseType && x.Style == inheritedStyle.Value).ToList();
                foreach (var cosmetic in baseCosmetics)
                {
                    var newCosmetic = cosmetic.Copy();
                    newCosmetic.Style = inheritedStyle.Key.BaseStyle;
                    fighterPackage.Cosmetics.Add(newCosmetic);
                    foreach (var costume in fighterPackage.Costumes)
                    {
                        if (costume.Cosmetics.Any(x => x == cosmetic))
                        {
                            costume.Cosmetics.Add(newCosmetic);
                        }
                    }
                }
            }
            return fighterPackage;
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
                fighterPackage.FighterSettings.LLoadCharacterId = fighterPackage.FighterInfo.Ids.CSSSlotConfigId;
                fighterPackage.FighterSettings.SSESubCharacterId = fighterPackage.FighterInfo.Ids.CSSSlotConfigId;
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
                            costume.PacFiles.AddRange(GetPacFiles(pacFiles, fighterPackage, true));
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
                    fighterPackage.PacFiles.AddRange(GetPacFiles(pacFiles, fighterPackage, false));
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
                // Get trophies
                foreach(var type in typeof(TrophyType).GetDictionary<TrophyType>())
                {
                    var trophyPath = $"{path}\\Trophies\\{type.Value}";
                    if (_fileService.DirectoryExists(trophyPath))
                    {
                        var trophyJson = _fileService.ReadTextFile($"{trophyPath}\\Trophy.json");
                        if (!string.IsNullOrEmpty(trophyJson))
                        {
                            var trophy = JsonConvert.DeserializeObject<FighterTrophy>(trophyJson);
                            // Get BRRES
                            trophy.Trophy.BrresFile = _fileService.GetFiles(trophyPath, "*.brres", SearchOption.AllDirectories).FirstOrDefault();
                            // Get cosmetics
                            trophy.Trophy.Thumbnails = _cosmeticService.LoadCosmetics($"{trophyPath}\\Thumbnails");
                            // Check if trophy is new or existing
                            trophy = GetNewOrExistingTrophy(trophy);
                            fighterPackage.Trophies.Add(trophy);
                        }
                    }
                }
                // Copy trophy over if one is missing
                if (fighterPackage.Trophies.Count == 1)
                {
                    fighterPackage.Trophies.Add(new FighterTrophy 
                    {
                        Trophy = fighterPackage.Trophies.FirstOrDefault()?.Trophy, 
                        OldTrophy = fighterPackage.Trophies.FirstOrDefault()?.Trophy, 
                        Type = !fighterPackage.Trophies.Any(x => x.Type == TrophyType.AllStar) ? TrophyType.AllStar : TrophyType.Fighter
                    });
                }
                // Get install options
                var installOptionPath = $"{path}\\InstallOptions";
                if (_fileService.DirectoryExists(installOptionPath))
                {
                    fighterPackage.InstallOptions = new List<FighterInstallOption>();
                    foreach(var installOptionType in _fileService.GetDirectories(installOptionPath, "*", SearchOption.TopDirectoryOnly))
                    {
                        foreach(var option in _fileService.GetDirectories(installOptionType, "*", SearchOption.TopDirectoryOnly))
                        {
                            var json = Path.Combine(option, "InstallOption.json");
                            if (_fileService.FileExists(json))
                            {
                                var jsonText = _fileService.ReadTextFile(json);
                                var installOption = JsonConvert.DeserializeObject<FighterInstallOption>(jsonText);
                                installOption.File = _fileService.GetFiles(option, $"*.{installOption.Extension}").FirstOrDefault();
                                fighterPackage.InstallOptions.Add(installOption);
                            }
                        }
                    }
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
            // Export trophies
            var exportedTrophies = new List<Trophy>();
            foreach(var fighterTrophy in fighterPackage.Trophies)
            {
                // If already exported, don't export again
                if (exportedTrophies.Contains(fighterTrophy.Trophy))
                {
                    continue;
                }
                var trophyPath = $"{path}\\Trophies\\{fighterTrophy.Type}";
                var fighterTrophyJson = JsonConvert.SerializeObject(fighterTrophy, Formatting.Indented);
                _fileService.SaveTextFile($"{trophyPath}\\Trophy.json", fighterTrophyJson);
                // Export thumbnails
                _cosmeticService.ExportCosmetics($"{trophyPath}\\Thumbnails", fighterTrophy?.Trophy?.Thumbnails);
                // Export BRRES
                _fileService.CopyFile(fighterTrophy?.Trophy?.BrresFile, $"{trophyPath}\\{fighterTrophy?.Trophy?.Brres}.brres");
                exportedTrophies.Add(fighterTrophy.Trophy);
            }
            // Export install options
            var installOptionPath = $"{path}\\InstallOptions";
            foreach(var installOption in fighterPackage.InstallOptions)
            {
                // Save json
                var outDir = Path.Combine(installOptionPath, installOption.Type.ToString(), installOption.Name);
                var installOptionJson = JsonConvert.SerializeObject(installOption, Formatting.Indented);
                _fileService.SaveTextFile($"{outDir}\\InstallOption.json", installOptionJson);
                // Save file
                if (!string.IsNullOrEmpty(installOption.File))
                {
                    var installOptionFile = Path.Combine(outDir, Path.GetFileName(installOption.File));
                    _fileService.CopyFile(installOption.File, installOptionFile);
                }
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
            // Clean up cosmetic inheritance
            fighterPackage = CleanupCosmeticInheritance(fighterPackage);
        }

        /// <summary>
        /// Load legacy fighter package
        /// </summary>
        /// <param name="inFile">Zip file to load</param>
        /// <returns>Fighter package</returns>
        public FighterPackage LoadLegacyPackage(string inFile)
        {
            int? trophyId = null;
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
                var exConfigs = _fileService.GetFiles(Path.Combine(path, "EXConfigs"), "*.dat");
                fighterPackage.FighterInfo.FighterConfig = exConfigs.FirstOrDefault(x => Path.GetFileName(x).StartsWith("Fighter"));
                fighterPackage.FighterInfo.CosmeticConfig = exConfigs.FirstOrDefault(x => Path.GetFileName(x).StartsWith("Cosmetic"));
                fighterPackage.FighterInfo.CSSSlotConfig = exConfigs.FirstOrDefault(x => Path.GetFileName(x).StartsWith("CSSSlot"));
                fighterPackage.FighterInfo.SlotConfig = exConfigs.FirstOrDefault(x => Path.GetFileName(x).StartsWith("Slot"));
                fighterPackage.FighterInfo = _fighterService.GetFighterInfo(fighterPackage.FighterInfo, fighterPackage.FighterInfo.FighterConfig, fighterPackage.FighterInfo.CosmeticConfig, fighterPackage.FighterInfo.CSSSlotConfig, fighterPackage.FighterInfo.SlotConfig);
                // Set IDs to first available
                fighterPackage.FighterInfo = _fighterService.UpdateIdsToFirstUnused(fighterPackage.FighterInfo);
                // Get fighter settings
                var settingsPath = Path.Combine(path, "FighterSettings.txt");
                if (_fileService.FileExists(settingsPath))
                {
                    var iniData = _fileService.ParseIniFile(settingsPath);
                    if (iniData.TryGetValue("throwReleasePoint", out string throwRelease))
                    {
                        var throwReleasePoints = throwRelease.Split(',');
                        var resultX = double.TryParse(throwReleasePoints[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double x);
                        var resultY = double.TryParse(throwReleasePoints[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double y);
                        if (resultX && resultY)
                            fighterPackage.FighterSettings.ThrowReleasePoint = new Position(x, y);
                    }
                    if (iniData.TryGetValue("creditsThemeId", out string creditsThemeId) && uint.TryParse(creditsThemeId.Substring(2), NumberStyles.HexNumber, null, out uint creditsThemeIdValue))
                    {
                        fighterPackage.FighterInfo.CreditsThemeId = creditsThemeIdValue;
                    }
                    if (iniData.TryGetValue("lucarioBoneId", out string lucarioBoneId) && int.TryParse(lucarioBoneId.Substring(2), NumberStyles.HexNumber, null, out int lucarioBoneIdValue))
                    {
                        fighterPackage.FighterSettings.LucarioSettings.BoneIds = new List<int?> { lucarioBoneIdValue, lucarioBoneIdValue, lucarioBoneIdValue, lucarioBoneIdValue };
                    }
                    if (iniData.TryGetValue("lucarioKirbyEffectId", out string lucarioKirbyEffectId))
                    {
                        fighterPackage.FighterSettings.LucarioSettings.UseKirbyGfxFix = true;
                    }
                    if (iniData.TryGetValue("jigglypuffBoneId", out string jigglypuffBoneId) && int.TryParse(jigglypuffBoneId.Substring(2), NumberStyles.HexNumber, null, out int jigglypuffBoneIdValue))
                    {
                        fighterPackage.FighterSettings.JigglypuffSettings.BoneIds = new List<int?> { jigglypuffBoneIdValue, jigglypuffBoneIdValue, jigglypuffBoneIdValue, jigglypuffBoneIdValue };
                    }
                    if (iniData.TryGetValue("jigglypuffEFLSId", out string jigglypuffEFLSId) && int.TryParse(jigglypuffEFLSId.Substring(2), NumberStyles.HexNumber, null, out int jigglypuffEFLSIdValue))
                    {
                        fighterPackage.FighterSettings.JigglypuffSettings.EFLSId = jigglypuffEFLSIdValue;
                    }
                    if (iniData.TryGetValue("jigglypuffSfxIds", out string jigglypuffSfxIds))
                    {
                        var sfxIds = jigglypuffSfxIds.Split(',');
                        var sfxIdList = new List<int?>();
                        foreach (var id in sfxIds)
                        {
                            if (int.TryParse(id.Substring(2), NumberStyles.HexNumber, null, out int sfxId))
                            {
                                sfxIdList.Add(sfxId);
                            }
                            else
                            {
                                sfxIdList.Add(null);
                            }
                        }
                        fighterPackage.FighterSettings.JigglypuffSettings.SfxIds = sfxIdList;
                    }
                    if (iniData.TryGetValue("bowserBoneId", out string bowserBoneId) && int.TryParse(bowserBoneId.Substring(2), NumberStyles.HexNumber, null, out int bowserBoneIdValue))
                    {
                        fighterPackage.FighterSettings.BowserSettings.BoneId = bowserBoneIdValue;
                    }
                    if (iniData.TryGetValue("doorId", out string doorId) && uint.TryParse(doorId.Substring(2), NumberStyles.HexNumber, null, out uint doorIdValue))
                    {
                        fighterPackage.FighterSettings.DoorId = doorIdValue;
                    }
                    if (iniData.TryGetValue("trophyId", out string trophyIdNum) && int.TryParse(trophyIdNum.Substring(2), NumberStyles.HexNumber, null, out int trophyIdValue))
                    {
                        trophyId = trophyIdValue;
                    }
                }
                fighterPackage.FighterSettings.LLoadCharacterId = fighterPackage.FighterInfo.Ids.CSSSlotConfigId;
                fighterPackage.FighterSettings.SSESubCharacterId = fighterPackage.FighterInfo.Ids.CSSSlotConfigId;
                // Get cosmetics
                var cosmetics = GetLegacyCosmetics(Path.Combine(path, "BPs"), CosmeticType.BP, CosmeticType.BPName);
                cosmetics.AddRange(GetLegacyCostumeCosmetics(Path.Combine(path, "CSPs"), CosmeticType.CSP, "Result"));
                cosmetics.AddRange(GetLegacyCosmetics(Path.Combine(path, "CSSIcon"), CosmeticType.CSSIcon, CosmeticType.CSSIconName));
                cosmetics.AddRange(GetLegacyCosmetics(Path.Combine(path, "PortraitName"), CosmeticType.PortraitName, CosmeticType.PortraitName));
                cosmetics.AddRange(GetLegacyCosmetics(Path.Combine(path, "ReplayIcon"), CosmeticType.ReplayIcon, CosmeticType.ReplayIcon));
                cosmetics.AddRange(GetLegacyCostumeCosmetics(Path.Combine(path, "StockIcons"), CosmeticType.StockIcon, "P+"));
                // Get SSE unlock icon
                var vBrawlIcon = cosmetics.FirstOrDefault(x => x.CosmeticType == CosmeticType.CSSIcon && x.Style == "vBrawl");
                if (vBrawlIcon != null)
                {
                    var sseIcon = vBrawlIcon.Copy();
                    sseIcon.CosmeticType = CosmeticType.SSEUnlockIcon;
                    sseIcon.CostumeIndex = 0;
                    cosmetics.Add(sseIcon);
                }
                // Get franchise icon
                var franchiseIconFolders = _fileService.GetDirectories(Path.Combine(path, "FranchiseIcons"), "*", SearchOption.TopDirectoryOnly);
                var franchiseIcon = _fileService.GetFiles(franchiseIconFolders.FirstOrDefault(x => Path.GetFileName(x) == "Black"), "*.png").FirstOrDefault();
                var franchiseIconHd = _fileService.GetFiles(franchiseIconFolders.FirstOrDefault(x => Path.GetFileName(x) == "Transparent"), "*.png").FirstOrDefault();
                var franchiseIconModel = _fileService.GetFiles(franchiseIconFolders.FirstOrDefault(x => Path.GetFileName(x) == "Model"), "*.mdl0").FirstOrDefault();
                if (!string.IsNullOrEmpty(franchiseIcon))
                {
                    var newCosmetic = new Cosmetic
                    {
                        CosmeticType = CosmeticType.FranchiseIcon,
                        Style = "Icon",
                        ImagePath = franchiseIcon,
                        HDImagePath = _fileService.FileExists(franchiseIconHd) ? franchiseIconHd : string.Empty,
                        Image = _fileService.LoadImage(franchiseIcon),
                        HDImage = _fileService.FileExists(franchiseIconHd) ? _fileService.LoadImage(franchiseIconHd) : null,
                        ModelPath = franchiseIconModel,
                        SharesData = false
                    };
                    fighterPackage.Cosmetics.Add(newCosmetic);
                }
                // Add all cosmetics to package
                foreach (var cosmetic in cosmetics)
                {
                    fighterPackage.Cosmetics.Add(cosmetic);
                }
                // Inherit missing cosmetics that require it
                foreach (var definition in _settingsService.BuildSettings.CosmeticSettings)
                {
                    if (definition.AlwaysInheritStyle && !fighterPackage.Cosmetics.Items.Any(x => x.CosmeticType == definition.CosmeticType && x.Style == definition.Style))
                    {
                        var inheritedStyle = fighterPackage.Cosmetics.Items.FirstOrDefault(x => x.CosmeticType == definition.CosmeticType)?.Style;
                        if (!string.IsNullOrEmpty(inheritedStyle))
                        {
                            fighterPackage.Cosmetics.InheritedStyles.Add((definition.CosmeticType, definition.Style), inheritedStyle);
                        }
                    }
                }
                // Get all pac files
                var pacDirs = new List<string>();
                var pacPath = Path.Combine(path, "Fighter");
                var pacFileObjects = new List<FighterPacFile>();
                pacDirs.Add(pacPath);
                pacDirs.AddRange(_fileService.GetDirectories(pacPath, "*", SearchOption.TopDirectoryOnly).Where(x => x != "#Options"));
                foreach (var dir in pacDirs)
                {
                    var pacFiles = _fileService.GetFiles(dir, "*.pac");
                    pacFileObjects.AddRange(GetPacFiles(pacFiles, fighterPackage, true));
                }
                // Get option definition if it exists
                var defaultOptionSettingsFile = Path.Combine(pacPath, "OptionSettings.txt");
                if (_fileService.FileExists(defaultOptionSettingsFile))
                {
                    var defaultOptionSettings = _fileService.ParseIniFile(defaultOptionSettingsFile);
                    var defaultInstallOption = fighterPackage.InstallOptions.FirstOrDefault(x => x.Type == InstallOptionType.MovesetFile);
                    if (defaultInstallOption != null)
                    {
                        defaultInstallOption.Name = defaultOptionSettings.TryGetValue("name", out string name) ? name : string.Empty;
                        defaultInstallOption.Description = defaultOptionSettings.TryGetValue("description", out string description) ? description : string.Empty;
                    }
                }
                // Get alternate moveset files
                var optionPacPath = Path.Combine(pacPath, "#Options");
                if (_fileService.DirectoryExists(optionPacPath))
                {
                    foreach (var optionFolder in _fileService.GetDirectories(optionPacPath, "*", SearchOption.TopDirectoryOnly))
                    {
                        var optionSettingsFile = Path.Combine(optionFolder, "OptionSettings.txt");
                        if (_fileService.FileExists(optionSettingsFile))
                        {
                            var optionSettings = _fileService.ParseIniFile(optionSettingsFile);
                            var installOption = new FighterInstallOption();
                            installOption.Name = optionSettings.TryGetValue("name", out string name) ? name : string.Empty;
                            installOption.Description = optionSettings.TryGetValue("description", out string description) ? description : string.Empty;
                            installOption.Type = InstallOptionType.MovesetFile;
                            installOption.File = _fileService.GetFiles(optionFolder, $"*.{installOption.Extension}").FirstOrDefault();
                            fighterPackage.InstallOptions.Add(installOption);
                        }
                    }
                }
                // Get costumes
                var rootNode = _fileService.OpenFile(fighterPackage.FighterInfo.CSSSlotConfig);
                if (rootNode != null)
                {
                    foreach (var entry in rootNode.Children)
                    {
                        var costume = new Costume
                        {
                            Color = ((CSSCEntryNode)entry).Color,
                            CostumeId = ((CSSCEntryNode)entry).CostumeID
                        };
                        costume.Cosmetics = fighterPackage.Cosmetics.Items.Where(x => x.CostumeIndex == entry.Index + 1).ToList();
                        costume.PacFiles = pacFileObjects.Where(x => Path.GetFileNameWithoutExtension(x.FilePath).EndsWith(costume.CostumeId.ToString("D2"))).ToList();
                        fighterPackage.Costumes.Add(costume);
                    }
                    _fileService.CloseFile(rootNode);
                }
                // TODO: Get Kirby hat files?
                // Get non-costume pac files
                fighterPackage.PacFiles = pacFileObjects.Where(x => !fighterPackage.Costumes.SelectMany(y => y.PacFiles).Contains(x)).ToList();
                // Get module
                fighterPackage.Module = _fileService.GetFiles(Path.Combine(path, "Module"), "*.rel").FirstOrDefault();
                // Get ending pac files
                fighterPackage.EndingPacFiles = _fileService.GetFiles(Path.Combine(path, "Ending"), "*.pac");
                // Get other files
                fighterPackage.Soundbank = _fileService.GetFiles(Path.Combine(path, "Soundbank"), "*.sawnd").FirstOrDefault();
                fighterPackage.ClassicIntro = _fileService.GetFiles(Path.Combine(path, "ClassicIntro"), "*.brres").FirstOrDefault();
                fighterPackage.EndingMovie = _fileService.GetFiles(Path.Combine(path, "Ending"), "*.thp").FirstOrDefault();
                // Get soundbank options
                var soundbankFolder = Path.Combine(path, "Soundbank");
                var soundbankOptionSettingsFile = Path.Combine(soundbankFolder, "OptionSettings.txt");
                if (_fileService.FileExists(soundbankOptionSettingsFile))
                {
                    var soundbankOptionSettings = _fileService.ParseIniFile(soundbankOptionSettingsFile);
                    var defaultInstallOption = fighterPackage.InstallOptions.FirstOrDefault(x => x.Type == InstallOptionType.Sounbank);
                    if (defaultInstallOption != null)
                    {
                        defaultInstallOption.Name = soundbankOptionSettings.TryGetValue("name", out string name) ? name : string.Empty;
                        defaultInstallOption.Description = soundbankOptionSettings.TryGetValue("description", out string description) ? description : string.Empty;
                    }
                }
                // Get alternate soundbank options
                var soundbankOptionsFolder = Path.Combine(soundbankFolder, "#Options");
                if (_fileService.DirectoryExists(soundbankOptionsFolder))
                {
                    foreach (var optionFolder in _fileService.GetDirectories(soundbankOptionsFolder, "*", SearchOption.TopDirectoryOnly))
                    {
                        var optionSettingsFile = Path.Combine(optionFolder, "OptionSettings.txt");
                        if (_fileService.FileExists(optionSettingsFile))
                        {
                            var optionSettings = _fileService.ParseIniFile(optionSettingsFile);
                            var installOption = new FighterInstallOption();
                            installOption.Name = optionSettings.TryGetValue("name", out string name) ? name : string.Empty;
                            installOption.Description = optionSettings.TryGetValue("description", out string description) ? description : string.Empty;
                            installOption.Type = InstallOptionType.Sounbank;
                            installOption.File = _fileService.GetFiles(optionFolder, $"*.{installOption.Extension}").FirstOrDefault();
                            fighterPackage.InstallOptions.Add(installOption);
                        }
                    }
                }
                // Get victory and credits themes
                var victoryTheme = _fileService.GetFiles(Path.Combine(path, "VictoryTheme"), "*.brstm").FirstOrDefault();
                if (victoryTheme != null)
                {
                    fighterPackage.VictoryTheme = new TracklistSong
                    {
                        Name = fighterPackage.FighterInfo.DisplayName,
                        SongFile = victoryTheme,
                        SongPath = !string.IsNullOrEmpty(victoryTheme) ? $"Victory!/{Path.GetFileNameWithoutExtension(victoryTheme)}" : string.Empty,
                        SongId = fighterPackage.FighterInfo.VictoryThemeId ?? 0xF000
                    };
                }
                var creditsTheme = _fileService.GetFiles(Path.Combine(path, "CreditsTheme"), "*.brstm").FirstOrDefault();
                if (creditsTheme != null || fighterPackage.FighterInfo.CreditsThemeId != null)
                {
                    fighterPackage.CreditsTheme = new TracklistSong
                    {
                        Name = creditsTheme != null ? Path.GetFileNameWithoutExtension(creditsTheme) : fighterPackage.FighterInfo.DisplayName,
                        SongFile = creditsTheme,
                        SongPath = !string.IsNullOrEmpty(creditsTheme) ? $"Credits!/{Path.GetFileNameWithoutExtension(creditsTheme)}" : string.Empty,
                        SongId = fighterPackage.FighterInfo.CreditsThemeId ?? 0xF000
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
                // Get trophies
                var trophyPath = Path.Combine(path, "Trophy");
                if (_fileService.DirectoryExists(trophyPath))
                {
                    var trophy = new Trophy();
                    var trophySettingsPath = Path.Combine(trophyPath, "TrophySettings.txt");
                    if (_fileService.FileExists(trophySettingsPath))
                    {
                        // Get trophy data
                        var trophyIniData = _fileService.ParseIniFile(trophySettingsPath);
                        trophy.DisplayName = trophyIniData.TryGetValue("trophyName", out string trophyName) ? trophyName : string.Empty;
                        trophy.Description = trophyIniData.TryGetValue("description", out string description) ? "<color=E6E6E6FF>" + description.Replace("<br/>", "\r\n") + "</end>" : string.Empty;
                        trophy.GameName1 = trophyIniData.TryGetValue("gameName1", out string gameName1) ? gameName1 : string.Empty;
                        trophy.GameName2 = trophyIniData.TryGetValue("gameName2", out string gameName2) ? gameName2 : string.Empty;
                        trophy.GameIcon1 = trophyIniData.TryGetValue("gameIcon1", out string gameIcon1) && int.TryParse(gameIcon1, out int gameIcon1Index) ? gameIcon1Index : 0;
                        trophy.GameIcon2 = trophyIniData.TryGetValue("gameIcon2", out string gameIcon2) && int.TryParse(gameIcon2, out int gameIcon2Index) ? gameIcon2Index : 0;
                        trophy.SeriesIndex = trophyIniData.TryGetValue("seriesIndex", out string seriesIndex) && int.TryParse(seriesIndex, out int seriesIndexParsed) ? seriesIndexParsed : 0;
                        trophy.CategoryIndex = 23;
                    }
                    // Get BRRES
                    var trophyBrres = _fileService.GetFiles(trophyPath, "*.brres").FirstOrDefault();
                    if (trophyBrres != null)
                    {
                        trophy.Name = Path.GetFileNameWithoutExtension(trophyBrres);
                        trophy.Brres = Path.GetFileNameWithoutExtension(trophyBrres);
                        trophy.BrresFile = trophyBrres;
                    }
                    // Get thumbnail
                    var trophyThumbnail = _fileService.GetFiles(trophyPath, "*.png").FirstOrDefault();
                    trophy.Thumbnails.Add(new Cosmetic
                    {
                        Style = "vBrawl",
                        CosmeticType = CosmeticType.TrophyThumbnail,
                        ImagePath = trophyThumbnail,
                        Image = _fileService.LoadImage(trophyThumbnail)
                    });
                    // Add trophies
                    fighterPackage.Trophies.Add(new FighterTrophy { Trophy = trophy, Type = TrophyType.Fighter });
                    fighterPackage.Trophies.Add(new FighterTrophy { Trophy = trophy, Type = TrophyType.AllStar });
                    // Check if trophy is new or existing
                    foreach(var fighterTrophy in fighterPackage.Trophies)
                    {
                        GetNewOrExistingTrophy(fighterTrophy);
                    }
                }
                    
                fighterPackage.PackageType = PackageType.New;
                return fighterPackage;
            }
            return null;
        }

        /// <summary>
        /// Check if trophy is a custom trophy or not, and update it accordingly
        /// </summary>
        /// <param name="fighterTrophy">Fighter trophy to check</param>
        /// <returns>Updated fighter trophy</returns>
        private FighterTrophy GetNewOrExistingTrophy(FighterTrophy fighterTrophy)
        {
            var trophy = fighterTrophy;
            // Set custom trophy IDs to get a new ID
            if (trophy.Trophy.Ids.TrophyId >= 631)
            {
                trophy.Trophy.Ids.TrophyId = null;
            }
            if (trophy.Trophy.Ids.TrophyThumbnailId >= 631)
            {
                trophy.Trophy.Ids.TrophyThumbnailId = null;
            }
            _trophyService.GetUnusedTrophyIds(trophy.Trophy.Ids);
            // If NOT a custom trophy ID, set old trophy to matching trophy in build
            if (trophy.Trophy.Ids.TrophyId < 631)
            {
                var existingTrophy = _trophyService.LoadTrophyData(trophy.Trophy);
                trophy.OldTrophy = existingTrophy;
            }
            return trophy;
        }

        /// <summary>
        /// Get non-costume cosmetics for legacy fighter package
        /// </summary>
        /// <param name="path">Folder to get cosmetics from</param>
        /// <param name="cosmeticType">Type of cosmetic</param>
        /// <param name="nameType">Type of cosmetic for name</param>
        /// <returns>List of cosmetics</returns>
        private List<Cosmetic> GetLegacyCosmetics(string path, CosmeticType cosmeticType, CosmeticType nameType)
        {
            var cosmeticList = new List<Cosmetic>();
            var folders = new List<string>();
            var style = string.Empty;
            // This is to handle old replay icon folder structure
            if (cosmeticType == CosmeticType.ReplayIcon && _fileService.GetFiles(path, "*.png", SearchOption.TopDirectoryOnly).Any())
            {
                style = "P+";
                folders.Add(path);
            }
            else
            {
                folders = _fileService.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            }
            foreach (var folder in folders)
            {
                var images = _fileService.GetFiles(folder, "*.png");
                foreach (var image in images)
                {
                    var hdImagePath = Path.Combine(folder, "HD");
                    var hdImages = _fileService.GetFiles(hdImagePath, "*.png");
                    var hdImage = hdImages.Count > images.IndexOf(image) ? hdImages[images.IndexOf(image)] : null;
                    var newCosmetic = new Cosmetic
                    {
                        CosmeticType = cosmeticType,
                        Style = string.IsNullOrEmpty(style) ? Path.GetFileName(folder) : style,
                        ImagePath = image,
                        HDImagePath = _fileService.FileExists(hdImage) ? hdImage : string.Empty,
                        Image = _fileService.LoadImage(image),
                        HDImage = _fileService.FileExists(hdImage) ? _fileService.LoadImage(hdImage) : null,
                        InternalIndex = images.IndexOf(image),
                        CostumeIndex = images.IndexOf(image) + 1,
                        SharesData = false,
                        ColorSmashChanged = true
                    };
                    cosmeticList.Add(newCosmetic);
                }
                // Name
                var name = _fileService.GetFiles(Path.Combine(folder, "Name"), "*.png").FirstOrDefault();
                if (!string.IsNullOrEmpty(name))
                {
                    var hdImagePath = _fileService.GetFiles(Path.Combine(folder, "Name", "HD"), "*.png").FirstOrDefault();
                    var newCosmetic = new Cosmetic
                    {
                        CosmeticType = nameType,
                        Style = Path.GetFileName(folder),
                        ImagePath = name,
                        HDImagePath = hdImagePath,
                        Image = _fileService.LoadImage(name),
                        HDImage = _fileService.FileExists(hdImagePath) ? _fileService.LoadImage(hdImagePath) : null,
                        InternalIndex = 0,
                        CostumeIndex = 1,
                        SharesData = false,
                        ColorSmashChanged = true
                    };
                    cosmeticList.Add(newCosmetic);
                }
            }
            return cosmeticList;
        }

        /// <summary>
        /// Get costume cosmetics for legacy fighter package
        /// </summary>
        /// <param name="path">Folder to get cosmetics from</param>
        /// <param name="cosmeticType">Type of cosmetics</param>
        /// <param name="style">Style to use for cosmetics</param>
        /// <returns></returns>
        private List<Cosmetic> GetLegacyCostumeCosmetics(string path, CosmeticType cosmeticType, string style)
        {
            var cosmeticList = new List<Cosmetic>();
            var folders = _fileService.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            foreach (var folder in folders)
            {
                var images = _fileService.GetFiles(folder, "*.png");
                foreach (var image in images)
                {
                    var hdImagePath = Path.Combine(folder, "HD", Path.GetFileName(image));
                    var newCosmetic = new Cosmetic
                    {
                        CosmeticType = cosmeticType,
                        Style = style,
                        ImagePath = image,
                        HDImagePath = _fileService.FileExists(hdImagePath) ? hdImagePath : string.Empty,
                        Image = _fileService.LoadImage(image),
                        HDImage = _fileService.FileExists(hdImagePath) ? _fileService.LoadImage(hdImagePath) : null,
                        InternalIndex = cosmeticList.Count,
                        CostumeIndex = cosmeticList.Count + 1,
                        SharesData = image != images.LastOrDefault(),
                        ColorSmashChanged = true
                    };
                    cosmeticList.Add(newCosmetic);
                }
            }
            return cosmeticList;
        }

        /// <summary>
        /// Get fighter pac files from list of files
        /// </summary>
        /// <param name="files">Files to convert</param>
        /// <param name="fighterPackage">Fighter package to use</param>
        /// <param name="removeCostumeId">Remove costume ID</param>
        /// <returns>List of fighter pac files</returns>
        private List<FighterPacFile> GetPacFiles(List<string> files, FighterPackage fighterPackage, bool removeCostumeId = true)
        {
            var pacFiles = new List<FighterPacFile>();
            foreach (var file in files)
            {
                if (_fighterService.VerifyFighterPacName(Path.GetFileName(file), fighterPackage.FighterInfo.PacFileName, fighterPackage.FighterInfo.PacExtension))
                {
                    var newPacFile = _fighterService.GetFighterPacFile(file, fighterPackage.FighterInfo.PacFileName, fighterPackage.FighterInfo, removeCostumeId);
                    pacFiles.Add(newPacFile);
                }
                else if (_fighterService.VerifyFighterPacName(Path.GetFileName(file), $"Itm{fighterPackage.FighterInfo.PartialPacName}", fighterPackage.FighterInfo.PacExtension))
                {
                    var newPacFile = _fighterService.GetFighterPacFile(file, $"Itm{fighterPackage.FighterInfo.PartialPacName}", fighterPackage.FighterInfo, removeCostumeId);
                    pacFiles.Add(newPacFile);
                }
                else if (_fighterService.VerifyFighterPacName(Path.GetFileName(file), fighterPackage.FighterInfo.KirbyPacFileName, fighterPackage.FighterInfo.KirbyPacExtension))
                {
                    var newPacFile = _fighterService.GetFighterPacFile(file, fighterPackage.FighterInfo.KirbyPacFileName, fighterPackage.FighterInfo, removeCostumeId);
                    pacFiles.Add(newPacFile);
                }
            }
            return pacFiles;
        }
    }
}
