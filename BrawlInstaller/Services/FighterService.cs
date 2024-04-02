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
        List<Costume> GetFighterCostumes(FighterInfo fighterInfo);
        List<Costume> GetCostumeCosmetics(List<Costume> costumes, List<Cosmetic> cosmetics);
        string GetModule(string internalName);
    }
    [Export(typeof(IFighterService))]
    internal class FighterService : IFighterService
    {
        // Services
        ISettingsService _settingsService { get; }
        IFileService _fileService { get; }

        [ImportingConstructor]
        public FighterService(ISettingsService settingsService, IFileService fileService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
        }

        // Methods
        public string GetConfigPrefix(IdType type)
        {
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
            return prefix;
        }
        public int GetConfigId(string name, IdType type)
        {
            var prefix = GetConfigPrefix(type);
            if (int.TryParse(name.Replace(prefix, ""), System.Globalization.NumberStyles.HexNumber, null, out int id))
                return id;
            return -1;
        }
        public string GetExConfig(int id, IdType type)
        {
            var buildPath = _settingsService.BuildPath;
            var settings = _settingsService.BuildSettings;
            var prefix = GetConfigPrefix(type);
            var directory = $"{buildPath}\\{settings.FilePathSettings.BrawlEx}\\{prefix}Config";
            if (Directory.Exists(directory))
            {
                var config = $"{directory}\\{prefix}{id:X2}.dat";
                if (File.Exists(config))
                    return config;
            }
            return "";
        }
        public List<ResourceNode> GetExConfigs(IdType type)
        {
            var configs = new List<ResourceNode>();
            var buildPath = _settingsService.BuildPath;
            var settings = _settingsService.BuildSettings;
            var prefix = GetConfigPrefix(type);
            var directory = $"{buildPath}\\{settings.FilePathSettings.BrawlEx}\\{prefix}Config";
            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory, $"{prefix}*.dat").ToList();
                foreach (var file in files)
                {
                    configs.Add(_fileService.OpenFile(file));
                }
            }
            return configs;
        }
        public FighterIds LinkExConfigs(FighterIds fighterIds)
        {
            var fighterConfigs = GetExConfigs(IdType.FighterConfig);
            var cosmeticConfigs = GetExConfigs(IdType.CosmeticConfig);
            var cssSlotConfigs = GetExConfigs(IdType.CSSSlotConfig);
            var slotConfigs = GetExConfigs(IdType.SlotConfig);
            // Find slot config ID if missing
            if (fighterIds.FighterConfigId > -1 && fighterIds.SlotConfigId <= -1)
            {
                var foundConfig = slotConfigs.FirstOrDefault(x => ((SLTCNode)x).SetSlot && ((SLTCNode)x).CharSlot1 == fighterIds.FighterConfigId);
                if (foundConfig != null)
                    fighterIds.SlotConfigId = GetConfigId(foundConfig.Name, IdType.SlotConfig);
            }
            if (fighterIds.CosmeticConfigId > -1 && fighterIds.SlotConfigId <= -1)
            {
                var foundConfig = cosmeticConfigs.FirstOrDefault(x => ((COSCNode)x).Name.EndsWith(fighterIds.CosmeticConfigId.ToString("X2")) && ((COSCNode)x).HasSecondary);
                if (foundConfig != null)
                    fighterIds.SlotConfigId = ((COSCNode)foundConfig).CharSlot1;
            }
            if (fighterIds.CSSSlotConfigId > -1 && fighterIds.SlotConfigId <= -1)
            {
                var foundConfig = cssSlotConfigs.FirstOrDefault(x => ((CSSCNode)x).Name.EndsWith(fighterIds.CSSSlotConfigId.ToString("X2")) && ((CSSCNode)x).SetPrimarySecondary);
                if (foundConfig != null)
                    fighterIds.SlotConfigId = ((CSSCNode)foundConfig).CharSlot1;
            }
            // Find cosmetic config ID if missing
            if (fighterIds.SlotConfigId > -1 && fighterIds.CosmeticConfigId <= -1)
            {
                var foundConfig = cosmeticConfigs.FirstOrDefault(x => ((COSCNode)x).HasSecondary && ((COSCNode)x).CharSlot1 == fighterIds.SlotConfigId);
                if (foundConfig != null)
                    fighterIds.CosmeticConfigId = GetConfigId(foundConfig.Name, IdType.CosmeticConfig);
            }
            if (fighterIds.CSSSlotConfigId > -1 && fighterIds.CosmeticConfigId <= -1)
            {
                var foundConfig = cssSlotConfigs.FirstOrDefault(x => ((CSSCNode)x).Name.EndsWith(fighterIds.CSSSlotConfigId.ToString("X2")) && ((CSSCNode)x).SetCosmeticSlot);
                if (foundConfig != null)
                    fighterIds.CosmeticConfigId = ((CSSCNode)foundConfig).CosmeticSlot;
            }
            // Find CSS slot config ID if missing
            if (fighterIds.SlotConfigId > -1 && fighterIds.CSSSlotConfigId <= -1)
            {
                var foundConfig = cssSlotConfigs.FirstOrDefault(x => ((CSSCNode)x).SetPrimarySecondary && ((CSSCNode)x).CharSlot1 == fighterIds.SlotConfigId);
                if (foundConfig != null)
                    fighterIds.CSSSlotConfigId = GetConfigId(foundConfig.Name, IdType.CSSSlotConfig);
            }
            if (fighterIds.CosmeticConfigId > -1 && fighterIds.CSSSlotConfigId <= -1)
            {
                var foundConfig = cssSlotConfigs.FirstOrDefault(x => ((CSSCNode)x).SetCosmeticSlot && ((CSSCNode)x).CosmeticSlot == fighterIds.CosmeticConfigId);
                if (foundConfig != null)
                    fighterIds.CSSSlotConfigId = ((CSSCNode)foundConfig).CosmeticSlot;
            }
            // Find fighter ID if missing
            if (fighterIds.SlotConfigId > -1 && fighterIds.FighterConfigId <= -1)
            {
                var foundConfig = slotConfigs.FirstOrDefault(x => ((SLTCNode)x).SetSlot && ((SLTCNode)x).Name.EndsWith(fighterIds.SlotConfigId.ToString("X2")));
                if (foundConfig != null)
                    fighterIds.FighterConfigId = Convert.ToInt32(((SLTCNode)foundConfig).CharSlot1);
            }
            // If any IDs are still missing, set them equal to other configs
            if (fighterIds.SlotConfigId <= -1 && fighterIds.FighterConfigId > -1)
                fighterIds.SlotConfigId = fighterIds.FighterConfigId;
            if (fighterIds.CosmeticConfigId <= -1 && fighterIds.SlotConfigId > -1)
                fighterIds.CosmeticConfigId = fighterIds.SlotConfigId;
            if (fighterIds.CSSSlotConfigId <= -1 && (fighterIds.CosmeticConfigId > -1 || fighterIds.SlotConfigId > -1))
                fighterIds.CSSSlotConfigId = fighterIds.CosmeticConfigId != -1 ? fighterIds.CosmeticConfigId : fighterIds.SlotConfigId;

            // Clean up nodes
            foreach (var config in fighterConfigs)
                config.Dispose();
            foreach (var config in slotConfigs)
                config.Dispose();
            foreach (var config in cosmeticConfigs)
                config.Dispose();
            foreach (var config in cssSlotConfigs)
                config.Dispose();
            // Return
            return fighterIds;
        }
        // TODO: get way more info
        public FighterInfo GetFighterInfo(FighterIds fighterIds)
        {
            var fighterInfo = new FighterInfo();
            fighterIds = LinkExConfigs(fighterIds);
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
            if (rootNode != null)
            {
                var slotNode = (SLTCNode)rootNode;
                if (fighterIds.FighterConfigId <= -1 && slotNode.SetSlot)
                    fighterIds.FighterConfigId = Convert.ToInt32(slotNode.CharSlot1);
                fighterInfo.VictoryThemeId = slotNode.VictoryTheme;
                rootNode.Dispose();
            }
            fighterInfo.FighterConfig = GetExConfig(fighterIds.FighterConfigId, IdType.FighterConfig);
            rootNode = _fileService.OpenFile(fighterInfo.FighterConfig);
            if (rootNode != null)
            {
                var fighterNode = (FCFGNode)rootNode;
                if (fighterInfo.InternalName == null)
                    fighterInfo.InternalName = fighterNode.InternalFighterName;
                rootNode.Dispose();
            }
            fighterInfo.Ids = fighterIds;
            return fighterInfo;
        }

        public List<string> GetFighterFiles(string internalName)
        {
            var buildPath = _settingsService.BuildPath;
            var settings = _settingsService.BuildSettings;
            var path = $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\{internalName}";
            return GetPacFiles(internalName, path);
        }

        public List<string> GetKirbyFiles(string internalName)
        {
            var buildPath = _settingsService.BuildPath;
            var settings = _settingsService.BuildSettings;
            var path = $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\kirby";
            return GetPacFiles("Kirby" + internalName, path);
        }

        public List<string> GetPacFiles(string name, string path)
        {
            var files = new List<string>();
            if (Directory.Exists(path))
                files = Directory.GetFiles(path, "*.pac").Where(x => Path.GetFileName(x).StartsWith($"Fit{name}", StringComparison.InvariantCultureIgnoreCase)).ToList();
            return files;
        }

        public List<Costume> GetFighterCostumes(FighterInfo fighterInfo)
        {
            var costumes = new List<Costume>();
            if (fighterInfo.CSSSlotConfig != null)
            {
                var buildPath = _settingsService.BuildPath;
                var settings = _settingsService.BuildSettings;
                var configPath = $"{buildPath}\\{settings.FilePathSettings.BrawlEx}\\CSSSlotConfig";
                var fighterPath = $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\{fighterInfo.InternalName}";
                ResourceNode rootNode = null;

                if (Directory.Exists(configPath))
                    rootNode = _fileService.OpenFile(fighterInfo.CSSSlotConfig);
                if (rootNode != null)
                {
                    foreach (CSSCEntryNode entry in rootNode.Children)
                    {
                        var costume = new Costume
                        {
                            Color = entry.Color,
                            CostumeId = entry.CostumeID
                        };
                        if (Directory.Exists(fighterPath))
                        {
                            costume.PacFiles = GetFighterFiles(fighterInfo.InternalName)
                                .Where(x => Path.GetFileNameWithoutExtension(x).EndsWith(costume.CostumeId.ToString("D2"))).ToList();
                        }
                        costumes.Add(costume);
                    }
                }
            }
            return costumes;
        }

        public List<Costume> GetCostumeCosmetics(List<Costume> costumes, List<Cosmetic> cosmetics)
        {
            foreach (var costume in costumes)
            {
                costume.Cosmetics = cosmetics.Where(x => x.CostumeIndex - 1 == costumes.IndexOf(costume)).ToList();
            }
            return costumes;
        }

        public string GetModule(string internalName)
        {
            var buildPath = _settingsService.BuildPath;
            var settings = _settingsService.BuildSettings;
            var moduleFolder = $"{buildPath}\\{settings.FilePathSettings.Modules}";
            var module = $"{moduleFolder}\\ft_{internalName.ToLower()}.rel";
            if (Directory.Exists(moduleFolder))
                if (File.Exists(module))
                    return module;
            return null;
        }
    }
}
