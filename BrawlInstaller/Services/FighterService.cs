using BrawlInstaller.Classes;
using BrawlInstaller.Enums;
using BrawlLib.SSBB;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BrawlInstaller.Services
{
    public interface IFighterService
    {
        FighterInfo GetFighterInfo(FighterIds fighterIds);
        List<Costume> GetFighterCostumes(FighterInfo fighterInfo);
        List<Costume> GetCostumeCosmetics(List<Costume> costumes, List<Cosmetic> cosmetics);
        string GetModule(string internalName);
        List<string> GetFighterFiles(string internalName);
        List<string> GetItemFiles(string internalName);
        List<string> GetKirbyFiles(string internalName);
        void ImportFighterFiles(List<string> pacFiles, List<Costume> costumes, FighterInfo fighterInfo);
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
            var configs = new ConcurrentBag<ResourceNode>();
            var buildPath = _settingsService.BuildPath;
            var settings = _settingsService.BuildSettings;
            var prefix = GetConfigPrefix(type);
            var directory = $"{buildPath}\\{settings.FilePathSettings.BrawlEx}\\{prefix}Config";
            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory, $"{prefix}*.dat").AsParallel().ToList();
                Parallel.ForEach(files, file =>
                {
                    configs.Add(_fileService.OpenFile(file));
                });
            }
            return configs.ToList();
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
                _fileService.CloseFile(config);
            foreach (var config in slotConfigs)
                _fileService.CloseFile(config);
            foreach (var config in cosmeticConfigs)
                _fileService.CloseFile(config);
            foreach (var config in cssSlotConfigs)
                _fileService.CloseFile(config);
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
                _fileService.CloseFile(rootNode);
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
                _fileService.CloseFile(rootNode);
            }
            fighterInfo.SlotConfig = GetExConfig(fighterIds.SlotConfigId, IdType.SlotConfig);
            rootNode = _fileService.OpenFile(fighterInfo.SlotConfig);
            if (rootNode != null)
            {
                var slotNode = (SLTCNode)rootNode;
                if (fighterIds.FighterConfigId <= -1 && slotNode.SetSlot)
                    fighterIds.FighterConfigId = Convert.ToInt32(slotNode.CharSlot1);
                fighterInfo.VictoryThemeId = slotNode.VictoryTheme;
                _fileService.CloseFile(rootNode);
            }
            fighterInfo.FighterConfig = GetExConfig(fighterIds.FighterConfigId, IdType.FighterConfig);
            rootNode = _fileService.OpenFile(fighterInfo.FighterConfig);
            if (rootNode != null)
            {
                var fighterNode = (FCFGNode)rootNode;
                if (fighterInfo.InternalName == null)
                {
                    fighterInfo.InternalName = fighterNode.InternalFighterName;
                }
                _fileService.CloseFile(rootNode);
            }
            fighterInfo.Ids = fighterIds;
            return fighterInfo;
        }

        // Get name for fighter PAC file based on fighter info and costume ID
        public string GetFighterPacName(ResourceNode node, FighterInfo fighterInfo, int costumeId = -1)
        {
            // List of strings that can be found in pac file names
            var modifierStrings = new List<string>
            {
                "MotionEtc",
                "Motion",
                "Etc",
                "Final",
                "Spy",
                "Dark",
                "Result",
                "Entry",
                "AltR",
                "AltZ"
            };
            var name = Path.GetFileNameWithoutExtension(node.FileName);
            // Get modifiers
            var index = 0;
            foreach (var modifier in modifierStrings)
            {
                var currentIndex = name.LastIndexOf(modifier);
                if (currentIndex > -1 && (index == 0 || currentIndex < index))
                    index = currentIndex;
            }
            name = $"Fit{fighterInfo.InternalName}";
            // Add modifiers to name
            if (index > 0)
                name += Path.GetFileNameWithoutExtension(node.FileName).Substring(index);
            // Remove old costume ID
            var oldCostumeId = string.Concat(name.Where(char.IsNumber));
            if (!string.IsNullOrEmpty(oldCostumeId))
                name = name.Replace(oldCostumeId, "");
            // Add new costume ID
            if (costumeId > -1)
                name += costumeId.ToString("D2");
            name += Path.GetExtension(node.FileName);
            if (name.ToLower() == node.FileName.ToLower())
                name = node.FileName;
            return name;
        }

        public void ImportFighterFiles(List<string> pacFiles, List<Costume> costumes, FighterInfo fighterInfo)
        {
            var buildPath = _settingsService.BuildPath;
            var settings = _settingsService.BuildSettings;
            var files = new List<(ResourceNode node, string name)>();
            foreach(var path in pacFiles)
            {
                var file = _fileService.OpenFile(path);
                var name = GetFighterPacName(file, fighterInfo);
                if (file != null)
                    files.Add((file, name));
            }
            foreach(var costume in costumes)
            {
                foreach(var path in costume.PacFiles)
                {
                    var file = _fileService.OpenFile(path);
                    var name = GetFighterPacName(file, fighterInfo, costume.CostumeId);
                    // Add file to list
                    if (file != null)
                        files.Add((file, name));
                }
            }
            RemoveFighterFiles(fighterInfo.InternalName);
            foreach(var file in files)
            {
                _fileService.SaveFileAs(file.node, $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\{fighterInfo.InternalName}\\{file.name}");
            }
        }

        public void RemoveFighterFiles(string internalName)
        {
            var buildPath = _settingsService.BuildPath;
            var settings = _settingsService.BuildSettings;
            var path = $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\{internalName}";
            RemovePacFiles(internalName, path);
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

        public void RemovePacFiles(string name, string path)
        {
            var files = GetPacFiles(name, path);
            foreach(var file in files)
            {
                _fileService.DeleteFile(file);
            }
        }

        public List<string> GetPacFiles(string name, string path)
        {
            var files = new List<string>();
            if (Directory.Exists(path))
                files = Directory.GetFiles(path, "*.pac").Where(x => Path.GetFileName(x).StartsWith($"Fit{name}", StringComparison.InvariantCultureIgnoreCase)).ToList();
            return files;
        }

        public List<string> GetItemFiles(string internalName)
        {
            var buildPath = _settingsService.BuildPath;
            var settings = _settingsService.BuildSettings;
            var path = $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\{internalName}\\item";
            var files = new List<string>();
            if (Directory.Exists(path))
                files = Directory.GetFiles(path).ToList();
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
