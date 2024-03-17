using BrawlInstaller.Classes;
using BrawlInstaller.Enums;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface IFighterService
    {
        FighterInfo GetFighterInfo(FighterIds fighterIds);
    }
    [Export(typeof(IFighterService))]
    internal class FighterService : IFighterService
    {
        // Services
        ISettingsService _settingsService { get; }
        IFileService _fileService { get; }

        public FighterService(ISettingsService settingsService, IFileService fileService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
        }

        // Methods
        public string GetExConfig(int id, IdType type)
        {
            var buildPath = _settingsService.BuildPath;
            var settings = _settingsService.BuildSettings;
            string prefix = "";
            switch (type)
            {
                case IdType.FighterConfig:
                    prefix = "Fighter";
                    break;
                case IdType.CosmeticConfig:
                    prefix = "Cosmetic";
                    break;
                case IdType.CSSSlotConfig:
                    prefix = "CSSSlot";
                    break;
                case IdType.SlotConfig:
                    prefix = "Slot";
                    break;
            }
            var directory = $"{buildPath}\\{settings.FilePathSettings.BrawlEx}\\{prefix}Config";
            if (Directory.Exists(directory))
            {
                var config = $"{directory}\\{prefix}{id:D2}.dat";
                if (File.Exists(config))
                    return config;
            }
            return "";
        }
        // TODO: get way more info
        public FighterInfo GetFighterInfo(FighterIds fighterIds)
        {
            var fighterInfo = new FighterInfo();
            fighterInfo.CosmeticConfig = GetExConfig(fighterIds.CosmeticConfigId, IdType.CosmeticConfig);
            var rootNode = _fileService.OpenFile(fighterInfo.CosmeticConfig);
            if (rootNode != null)
            {
                var coscNode = (COSCNode)rootNode;
                if (fighterIds.SlotConfigId <= -1 && coscNode.HasSecondary)
                    fighterIds.SlotConfigId = coscNode.CharSlot1;
                if (fighterIds.CosmeticId <= -1)
                    fighterIds.CosmeticId = coscNode.CosmeticID;
                if (fighterIds.FranchiseId <= -1)
                    fighterIds.FranchiseId = coscNode.FranchiseIconID + 1;
                if (fighterInfo.DisplayName == null)
                    fighterInfo.DisplayName = coscNode.CharacterName;
                rootNode.Dispose();
            }
            fighterInfo.CSSSlotConfig = GetExConfig(fighterIds.CSSSlotConfigId, IdType.CSSSlotConfig);
            rootNode = _fileService.OpenFile(fighterInfo.CSSSlotConfig);
            // TODO: Get costumes?
            if (rootNode != null)
            {
                var csscNode = (CSSCNode)rootNode;
                if (fighterIds.SlotConfigId <= -1 && csscNode.SetPrimarySecondary)
                    fighterIds.CSSSlotConfigId = csscNode.CharSlot1;
                if (fighterIds.CosmeticConfigId <= -1 && csscNode.SetCosmeticSlot)
                    fighterIds.CosmeticConfigId = csscNode.CosmeticSlot;
                rootNode.Dispose();
            }
            fighterInfo.SlotConfig = GetExConfig(fighterIds.SlotConfigId, IdType.SlotConfig);
            rootNode = _fileService.OpenFile(fighterInfo.SlotConfig);
            if ( rootNode != null )
            {
                var slotNode = (SLTCNode)rootNode;
                if (fighterIds.FighterConfigId <= -1 && slotNode.SetSlot)
                    fighterIds.FighterConfigId = Convert.ToInt32(slotNode.CharSlot1);
                fighterInfo.VictoryTheme = slotNode.VictoryTheme;
                rootNode.Dispose();
            }
            fighterInfo.FighterConfig = GetExConfig(fighterIds.FighterConfigId, IdType.FighterConfig);
            rootNode = _fileService.OpenFile(fighterInfo.FighterConfig);
            if ( rootNode != null )
            {
                var fighterNode = (FCFGNode)rootNode;
                if (fighterInfo.InternalName == null)
                    fighterInfo.InternalName = fighterNode.InternalFighterName;
                rootNode.Dispose();
            }
            return fighterInfo;
        }
    }
}
