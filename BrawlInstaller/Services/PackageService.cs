using BrawlInstaller.Classes;
using BrawlInstaller.Enums;
using BrawlLib.Internal;
using BrawlLib.SSBB.ResourceNodes;
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

        /// <inheritdoc cref="PackageService.SaveFighter(FighterPackage)"/>
        void SaveFighter(FighterPackage fighterPackage);
    }
    [Export(typeof(IPackageService))]
    internal class PackageService : IPackageService
    {
        // Services
        IFileService _fileService { get; }
        ISettingsService _settingsService { get; }
        ICosmeticService _cosmeticService { get; }
        IFighterService _fighterService { get; }

        [ImportingConstructor]
        public PackageService(IFileService fileService, ISettingsService settingsService, ICosmeticService cosmeticService, IFighterService fighterService) 
        {
            _fileService = fileService;
            _settingsService = settingsService;
            _cosmeticService = cosmeticService;
            _fighterService = fighterService;
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

            // Get cosmetics
            var cosmetics = _cosmeticService.GetFighterCosmetics(fighterInfo.Ids);

            // Get costumes
            var costumes = _fighterService.GetFighterCostumes(fighterInfo);
            costumes = _fighterService.GetCostumeCosmetics(costumes, cosmetics);

            // Set up inheritance for styles
            var inheritedStyles = new Dictionary<(CosmeticType, string), string>();
            foreach(var cosmeticType in cosmetics.GroupBy(x => x.CosmeticType).Select(x => x.Key))
            {
                var typedCosmetics = cosmetics.Where(x => x.CosmeticType == cosmeticType);
                var groups = typedCosmetics.GroupBy(x => x.Style);
                foreach (var group in groups)
                {
                    var match = groups.Where(x => x.Key != group.Key).FirstOrDefault(x => group.Select(y => y.Texture?.MD5Str()).All(x.Select(y => y.Texture?.MD5Str()).Contains) && group.Count() == x.Count());
                    if (match != null && !inheritedStyles.Any(x => (x.Key == (cosmeticType, group.Key)) || x.Value == group.Key))
                    {
                        inheritedStyles.Add((cosmeticType, group.Key), match.Key);
                    }
                }
            }

            // Get fighter files
            fighterPackage.PacFiles = _fighterService.GetFighterFiles(fighterInfo.InternalName)?.Where(x => !costumes.SelectMany(y => y.PacFiles).Contains(x)).ToList();
            fighterPackage.PacFiles.AddRange(_fighterService.GetKirbyFiles(fighterInfo.InternalName)?.Where(x => !costumes.SelectMany(y => y.PacFiles).Contains(x)).ToList());
            fighterPackage.PacFiles.AddRange(_fighterService.GetItemFiles(fighterInfo.InternalName)?.Where(x => !costumes.SelectMany(y => y.PacFiles).Contains(x)).ToList());
            fighterPackage.Module = _fighterService.GetModule(fighterInfo.InternalName);
            fighterPackage.ExConfigs = new List<string>();

            // Set fighter info
            if (fighterInfo.FighterConfig != "")
                fighterPackage.ExConfigs.Add(fighterInfo.FighterConfig);
            if (fighterInfo.CosmeticConfig != "")
                fighterPackage.ExConfigs.Add(fighterInfo.CosmeticConfig);
            if (fighterInfo.CSSSlotConfig != "")
                fighterPackage.ExConfigs.Add(fighterInfo.CSSSlotConfig);
            if (fighterInfo.SlotConfig != "")
                fighterPackage.ExConfigs.Add(fighterInfo.SlotConfig);

            fighterPackage.Costumes = costumes;
            fighterPackage.FighterInfo = fighterInfo;
            fighterPackage.Cosmetics.Items = cosmetics;
            fighterPackage.Cosmetics.InheritedStyles = inheritedStyles;
            return fighterPackage;
        }

        /// <summary>
        /// Save fighter to build
        /// </summary>
        /// <param name="fighterPackage">Fighter package to save</param>
        public void SaveFighter(FighterPackage fighterPackage)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
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
            // Update pac files
            // TODO: only update pac files that have changed
            _fighterService.ImportFighterFiles(fighterPackage.PacFiles, fighterPackage.Costumes, fighterPackage.FighterInfo);
            // Update config
            // TODO: only update if costumes changed
            _fighterService.UpdateCostumeConfig(fighterPackage.FighterInfo, fighterPackage.Costumes);
            // Update module
            _fighterService.UpdateModule(fighterPackage.Module, fighterPackage.FighterInfo.Ids.FighterConfigId);
        }
    }
}
