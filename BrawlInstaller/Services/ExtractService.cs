﻿using BrawlInstaller.Classes;
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
        IFighterService _fighterService { get; }

        [ImportingConstructor]
        public ExtractService(IFileService fileService, ISettingsService settingsService, ICosmeticService cosmeticService, IFighterService fighterService) 
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
            foreach (var cosmetic in cosmetics)
            {
                if (!Directory.Exists("Cosmetics"))
                    Directory.CreateDirectory("Cosmetics");
                if (cosmetic.Image != null)
                    cosmetic.Image.Save("Cosmetics\\" + cosmetic.CosmeticType.GetDisplayName() + cosmetic.Style + cosmetic.CostumeIndex.ToString() + ".png", ImageFormat.Png);
                if (cosmetic.Model != null)
                    cosmetic.Model.Export("Cosmetics\\" + cosmetic.CosmeticType.GetDisplayName() + cosmetic.Style + ".mdl0");
                if (cosmetic.Texture != null)
                    Debug.Print(cosmetic.Texture.Name + " " + cosmetic.InternalIndex.ToString() + " " + cosmetic.CostumeIndex);
            }
            return fighterPackage;
        }
    }
}
