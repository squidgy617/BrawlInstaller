using BrawlInstaller.Classes;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface IExtractService
    {
        FighterPackage ExtractFighter(FighterIds fighterIds);
    }
    [Export(typeof(IExtractService))]
    internal class ExtractService : IExtractService
    {
        // Services
        IFileService _fileService { get; }
        ISettingsService _settingsService { get; }
        ICosmeticService _cosmeticService { get; }

        [ImportingConstructor]
        public ExtractService(IFileService fileService, ISettingsService settingsService, ICosmeticService cosmeticService) 
        {
            _fileService = fileService;
            _settingsService = settingsService;
            _cosmeticService = cosmeticService;
        }

        // Methods
        public FighterPackage ExtractFighter(FighterIds fighterIds)
        {
            var fighterPackage = new FighterPackage();
            var settings = _settingsService.BuildSettings;
            foreach (var CSP in settings.CosmeticSettings.CSPs)
            {
                var rootNode = _fileService.OpenFile(_cosmeticService.GetCosmeticPath(CSP, fighterIds.CosmeticId));
                var textures = _cosmeticService.GetCosmetics(CSP, rootNode, fighterIds.CosmeticId, false);
                foreach (var texture in textures)
                {
                    Debug.Print(texture.Texture.Name + " " + texture.InternalIndex.ToString());
                }
            }
            return fighterPackage;
        }
    }
}
