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

        /// <inheritdoc cref="PackageService.ExportFighter(FighterPackage)"/>
        void ExportFighter(FighterPackage fighterPackage);
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
            fighterPackage.FighterInfo.EffectPacId = _fighterService.GetFighterEffectPacId(fighterPackage.PacFiles, fighterInfo.InternalName);
            fighterPackage.FighterInfo.OriginalEffectPacId = fighterPackage.FighterInfo.EffectPacId;
            fighterPackage.FighterInfo.KirbyEffectPacId = _fighterService.GetFighterEffectPacId(fighterPackage.PacFiles, $"Kirby{fighterInfo.InternalName}");
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
                    var match = groups.Where(x => x.Key != group.Key).FirstOrDefault(x => group.Select(y => y.Texture?.MD5Str()).All(x.Select(y => y.Texture?.MD5Str()).Contains) && group.Count() == x.Count());
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
            // Import fighter files
            _fighterService.ImportFighterFiles(fighterPackage);
            // Set original Effect.pac and soundbank ID to current
            fighterPackage.FighterInfo.OriginalEffectPacId = fighterPackage.FighterInfo.EffectPacId;
            fighterPackage.FighterInfo.OriginalKirbyEffectPacId = fighterPackage.FighterInfo.KirbyEffectPacId;
            fighterPackage.FighterInfo.OriginalSoundbankId = fighterPackage.FighterInfo.SoundbankId;
            fighterPackage.FighterInfo.OriginalKirbySoundbankId = fighterPackage.FighterInfo.KirbySoundbankId;
            // Update fighter settings
            _fighterService.UpdateFighterSettings(fighterPackage);
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
        /// Export fighter package to filesystem
        /// </summary>
        /// <param name="fighterPackage">Fighter package to export</param>
        public void ExportFighter(FighterPackage fighterPackage)
        {
            _cosmeticService.ExportCosmetics("FighterPackage", fighterPackage.Cosmetics);
        }
    }
}
