using BrawlInstaller.Classes;
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
        FighterPackage ExtractFighter(FighterIds fighterIds);
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
        public FighterPackage ExtractFighter(FighterIds fighterIds)
        {
            var fighterPackage = new FighterPackage();
            var fighterInfo = _fighterService.GetFighterInfo(fighterIds);
            var cosmetics = _cosmeticService.GetFighterCosmetics(fighterInfo.Ids);
            var costumes = _fighterService.GetFighterCostumes(fighterInfo);
            costumes = _fighterService.GetCostumeCosmetics(costumes, cosmetics);

            var fighterFiles = new List<FighterFiles>{
                new FighterFiles
                {
                    PacFiles = _fighterService.GetFighterFiles(fighterInfo.InternalName)?.Where(x => !costumes.SelectMany(y => y.PacFiles).Contains(x)).ToList(),
                    Module = _fighterService.GetModule(fighterInfo.InternalName),
                    ExConfigs = new List<string>(),
                    ItemFiles = _fighterService.GetItemFiles(fighterInfo.InternalName),
                    KirbyPacFiles = _fighterService.GetKirbyFiles(fighterInfo.InternalName)
                } 
            };
            if (fighterInfo.FighterConfig != "")
                fighterFiles.FirstOrDefault().ExConfigs.Add(fighterInfo.FighterConfig);
            if (fighterInfo.CosmeticConfig != "")
                fighterFiles.FirstOrDefault().ExConfigs.Add(fighterInfo.CosmeticConfig);
            if (fighterInfo.CSSSlotConfig != "")
                fighterFiles.FirstOrDefault().ExConfigs.Add(fighterInfo.CSSSlotConfig);
            if (fighterInfo.SlotConfig != "")
                fighterFiles.FirstOrDefault().ExConfigs.Add(fighterInfo.SlotConfig);
            //foreach (var costume in costumes )
            //{
            //    var costumePath = $"Cosmetics\\Costume{costume.CostumeId:D2}";
            //    foreach (var cosmetic in costume.Cosmetics)
            //    {
            //        if (!Directory.Exists(costumePath))
            //            Directory.CreateDirectory(costumePath);
            //        if (cosmetic.Image != null)
            //            cosmetic.Image.Save(costumePath + "\\" + cosmetic.CosmeticType.GetDisplayName() + cosmetic.Style + cosmetic.CostumeIndex.ToString() + ".png", ImageFormat.Png);
            //        if (cosmetic.Model != null)
            //            cosmetic.Model.Export(costumePath + "\\" + cosmetic.CosmeticType.GetDisplayName() + cosmetic.Style + ".mdl0");
            //        if (cosmetic.Texture != null)
            //            Debug.Print(cosmetic.Texture.Name + " " + cosmetic.InternalIndex.ToString() + " " + cosmetic.CostumeIndex);
            //    }
            //    if (costume.PacFiles != null)
            //        foreach(var file in costume.PacFiles)
            //        {
            //            File.Copy(file, $"{costumePath}\\{Path.GetFileName(file)}");
            //        }
            //}
            //foreach (var cosmetic in cosmetics.Where(x => x.CostumeIndex < 1))
            //{
            //    if (!Directory.Exists("Cosmetics"))
            //        Directory.CreateDirectory("Cosmetics");
            //    if (cosmetic.Image != null)
            //        cosmetic.Image.Save("Cosmetics\\" + cosmetic.CosmeticType.GetDisplayName() + cosmetic.Style + cosmetic.CostumeIndex.ToString() + ".png", ImageFormat.Png);
            //    if (cosmetic.Model != null)
            //        cosmetic.Model.Export("Cosmetics\\" + cosmetic.CosmeticType.GetDisplayName() + cosmetic.Style + ".mdl0");
            //    if (cosmetic.Texture != null)
            //        Debug.Print(cosmetic.Texture.Name + " " + cosmetic.InternalIndex.ToString() + " " + cosmetic.CostumeIndex);
            //}
            //if (!Directory.Exists("ExConfigs"))
            //    Directory.CreateDirectory("ExConfigs");
            //if (fighterInfo.FighterConfig != null)
            //    File.Copy(fighterInfo.FighterConfig, $"ExConfigs\\{Path.GetFileName(fighterInfo.FighterConfig)}");
            //if (fighterInfo.CosmeticConfig != null)
            //    File.Copy(fighterInfo.CosmeticConfig, $"ExConfigs\\{Path.GetFileName(fighterInfo.CosmeticConfig)}");
            //if (fighterInfo.CSSSlotConfig != null)
            //    File.Copy(fighterInfo.CSSSlotConfig, $"ExConfigs\\{Path.GetFileName(fighterInfo.CSSSlotConfig)}");
            //if (fighterInfo.SlotConfig != null)
            //    File.Copy(fighterInfo.SlotConfig, $"ExConfigs\\{Path.GetFileName(fighterInfo.SlotConfig)}");
            //if (!Directory.Exists("Module"))
            //    Directory.CreateDirectory("Module");
            //var module = _fighterService.GetModule(fighterInfo.InternalName);
            //if (module != null)
            //    File.Copy(module, $"Module\\{Path.GetFileName(module)}");
            fighterPackage.Costumes = costumes;
            fighterPackage.FighterInfo = fighterInfo;
            fighterPackage.Cosmetics = cosmetics;
            fighterPackage.FighterFiles = fighterFiles;
            return fighterPackage;
        }

        public void SaveFighter(FighterPackage fighterPackage)
        {
            var buildPath = _settingsService.BuildPath;
            // Only update cosmetics that have changed
            foreach(var definition in _settingsService.BuildSettings.CosmeticSettings.Where(x => fighterPackage.Cosmetics.Any(y => y.CosmeticType == x.CosmeticType
                && y.Style == x.Style
                && y.HasChanged)))
            {
                var cosmetics = fighterPackage.Cosmetics.Where(x => x.CosmeticType == definition.CosmeticType && x.Style == definition.Style).ToList();
                _cosmeticService.ImportCosmetics(definition, cosmetics, fighterPackage.FighterInfo.Ids.CosmeticId);
            }
            // TODO: Should this somehow be included in cosmetics? Should we actually treat franchise icons differently at all?
            if (fighterPackage.FranchiseIcon.HasChanged)
            {
                var franchiseIcons = new List<Cosmetic>
                {
                    fighterPackage.FranchiseIcon
                };
                _cosmeticService.ImportCosmetics(_settingsService.BuildSettings.CosmeticSettings.FirstOrDefault(x => x.CosmeticType == Enums.CosmeticType.FranchiseIcon && x.Style == "Model"),
                    franchiseIcons, fighterPackage.FranchiseIcon.Id ?? -1);
            }
        }
    }
}
