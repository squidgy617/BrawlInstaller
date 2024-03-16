using BrawlInstaller.Classes;
using BrawlLib.Internal;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrawlLib.BrawlManagerLib.TextureContainer;

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
            var cosmetics = _cosmeticService.GetFighterCosmetics(fighterIds);
            foreach (var cosmetic in cosmetics)
            {
                cosmetic.Image.Save(cosmetic.CosmeticType.GetDisplayName() + cosmetic.Style + cosmetic.CostumeIndex.ToString() + ".png", ImageFormat.Png);
                Debug.Print(cosmetic.Texture.Name + " " + cosmetic.InternalIndex.ToString() + " " + cosmetic.CostumeIndex);
            }
            return fighterPackage;
        }
    }
}
