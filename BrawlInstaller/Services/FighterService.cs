using BrawlInstaller.Classes;
using BrawlInstaller.Enums;
using BrawlInstaller.StaticClasses;
using BrawlLib.SSBB;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace BrawlInstaller.Services
{
    public interface IFighterService
    {
        /// <inheritdoc cref="FighterService.GetFighterInfo(FighterInfo)"/>
        FighterInfo GetFighterInfo(FighterInfo fighterInfo);

        /// <inheritdoc cref="FighterService.GetFighterCostumes(FighterInfo)"/>
        List<Costume> GetFighterCostumes(FighterInfo fighterInfo);

        /// <inheritdoc cref="FighterService.GetCostumeCosmetics(List{Costume}, List{Cosmetic})"/>
        List<Costume> GetCostumeCosmetics(List<Costume> costumes, List<Cosmetic> cosmetics);

        /// <inheritdoc cref="FighterService.GetModule(string)"/>
        string GetModule(string internalName);

        /// <inheritdoc cref="FighterService.GetPacFiles(string)"/>
        List<string> GetPacFiles(string internalName);

        /// <inheritdoc cref="FighterService.GetItemFiles(string)"/>
        List<string> GetItemFiles(string internalName);

        /// <inheritdoc cref="FighterService.GetKirbyFiles(string)"/>
        List<string> GetKirbyFiles(string internalName);

        /// <inheritdoc cref="FighterService.GetAllFighterInfo()"/>
        List<FighterInfo> GetAllFighterInfo();

        /// <inheritdoc cref="FighterService.GetFighterFiles(FighterPackage)"/>
        FighterPackage GetFighterFiles(FighterPackage fighterPackage);

        /// <inheritdoc cref="FighterService.ImportFighterFiles(FighterPackage)"/>
        void ImportFighterFiles(FighterPackage fighterPackage);
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

        /// <summary>
        /// Get prefix of Ex configs associated with ID type
        /// </summary>
        /// <param name="type">Type of Ex config ID</param>
        /// <returns>Prefix for config name</returns>
        private string GetConfigPrefix(IdType type)
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

        /// <summary>
        /// Get ID from config name
        /// </summary>
        /// <param name="name">Ex config name</param>
        /// <param name="type">ID type used by config</param>
        /// <returns>Ex config ID</returns>
        private int GetConfigId(string name, IdType type)
        {
            var prefix = GetConfigPrefix(type);
            if (int.TryParse(name.Replace(prefix, ""), System.Globalization.NumberStyles.HexNumber, null, out int id))
                return id;
            return -1;
        }

        /// <summary>
        /// Get Ex config from ID
        /// </summary>
        /// <param name="id">ID of config</param>
        /// <param name="type">Type of ID</param>
        /// <returns>Path to Ex config</returns>
        private string GetExConfig(int id, IdType type)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
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

        /// <summary>
        /// Get all configs of type
        /// </summary>
        /// <param name="type">Type of configs to retrieve</param>
        /// <returns>List of configs as nodes</returns>
        private List<ResourceNode> GetExConfigs(IdType type)
        {
            var configs = new ConcurrentBag<ResourceNode>();
            var buildPath = _settingsService.AppSettings.BuildPath;
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

        /// <summary>
        /// Link Ex configs based on data and IDs
        /// </summary>
        /// <param name="fighterIds">Fighter IDs for configs</param>
        /// <returns>All IDs for a fighter</returns>
        private BrawlIds LinkExConfigs(BrawlIds fighterIds, List<ResourceNode> cosmeticConfigs, List<ResourceNode> cssSlotConfigs, List<ResourceNode> slotConfigs)
        {
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
            // If slot config ID is still missing, set it equal to fighter config ID
            if (fighterIds.SlotConfigId <= -1 && fighterIds.FighterConfigId >= 0x3F)
                fighterIds.SlotConfigId = fighterIds.FighterConfigId;
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
            // If cosmetic config ID is still missing, set it equal to slot config ID
            if (fighterIds.CosmeticConfigId <= -1 && fighterIds.SlotConfigId >= 0x3F)
                fighterIds.CosmeticConfigId = fighterIds.SlotConfigId;
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
            // If CSS slot config ID is still missing, set it equal to one of the other IDs
            if (fighterIds.CSSSlotConfigId <= -1 && (fighterIds.CosmeticConfigId >= 0x3F || fighterIds.SlotConfigId >= 0x3F))
                fighterIds.CSSSlotConfigId = fighterIds.CosmeticConfigId != -1 ? fighterIds.CosmeticConfigId : fighterIds.SlotConfigId;
            // Find fighter ID if missing
            if (fighterIds.SlotConfigId > -1 && fighterIds.FighterConfigId <= -1)
            {
                var foundConfig = slotConfigs.FirstOrDefault(x => ((SLTCNode)x).SetSlot && ((SLTCNode)x).Name.EndsWith(fighterIds.SlotConfigId.ToString("X2")));
                if (foundConfig != null)
                    fighterIds.FighterConfigId = Convert.ToInt32(((SLTCNode)foundConfig).CharSlot1);
            }

            // Return
            return fighterIds;
        }

        /// <summary>
        /// Get fighter info by fighter IDs
        /// </summary>
        /// <param name="fighterIds">IDs to get fighter info for</param>
        /// <returns>Fighter information</returns>
        private FighterInfo GetFighterInfo(BrawlIds fighterIds, List<ResourceNode> fighterConfigs, List<ResourceNode> cosmeticConfigs, List<ResourceNode> cssSlotConfigs, List<ResourceNode> slotConfigs)
        {
            var fighterInfo = new FighterInfo();
            fighterInfo.Ids = fighterIds;
            return GetFighterInfo(fighterInfo, fighterConfigs, cosmeticConfigs, cssSlotConfigs, slotConfigs);
        }

        /// <summary>
        /// Get fighter info for a specific fighter
        /// </summary>
        /// <param name="fighterIds">Fighter IDs</param>
        /// <returns>Fighter information</returns>
        public FighterInfo GetFighterInfo(FighterInfo fighterInfo)
        {
            // Open configs
            var fighterConfigs = GetExConfigs(IdType.FighterConfig);
            var cosmeticConfigs = GetExConfigs(IdType.CosmeticConfig);
            var cssSlotConfigs = GetExConfigs(IdType.CSSSlotConfig);
            var slotConfigs = GetExConfigs(IdType.SlotConfig);
            // Get fighter info
            fighterInfo = GetFighterInfo(fighterInfo, fighterConfigs, cosmeticConfigs, cssSlotConfigs, slotConfigs);
            // Close configs
            foreach (var config in fighterConfigs)
                _fileService.CloseFile(config);
            foreach (var config in slotConfigs)
                _fileService.CloseFile(config);
            foreach (var config in cosmeticConfigs)
                _fileService.CloseFile(config);
            foreach (var config in cssSlotConfigs)
                _fileService.CloseFile(config);
            return fighterInfo;
        }

        /// <summary>
        /// Get fighter info based on supplied configs
        /// </summary>
        /// <param name="fighterIds">Fighter IDs</param>
        /// <param name="fighterConfigs">All fighter configs to use</param>
        /// <param name="cosmeticConfigs">All cosmetic configs to use</param>
        /// <param name="cssSlotConfigs">All CSSslot configs to use</param>
        /// <param name="slotConfigs">All slot configs to use</param>
        /// <returns>Fighter info</returns>
        // TODO: get way more info
        private FighterInfo GetFighterInfo(FighterInfo fighterInfo, List<ResourceNode> fighterConfigs, List<ResourceNode> cosmeticConfigs, List<ResourceNode> cssSlotConfigs, List<ResourceNode> slotConfigs)
        {
            var fighterIds = LinkExConfigs(fighterInfo.Ids, cosmeticConfigs, cssSlotConfigs, slotConfigs);
            fighterInfo.CosmeticConfig = GetExConfig(fighterIds.CosmeticConfigId, IdType.CosmeticConfig);
            var rootNode = cosmeticConfigs.FirstOrDefault(x => x.FilePath == fighterInfo.CosmeticConfig);
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
                if (fighterInfo.EntryName == null)
                    fighterInfo.EntryName = coscNode.CharacterName;
            }
            fighterInfo.CSSSlotConfig = GetExConfig(fighterIds.CSSSlotConfigId, IdType.CSSSlotConfig);
            rootNode = cssSlotConfigs.FirstOrDefault(x => x.FilePath == fighterInfo.CSSSlotConfig);
            if (rootNode != null)
            {
                var csscNode = (CSSCNode)rootNode;
                if (fighterIds.SlotConfigId <= -1 && csscNode.SetPrimarySecondary)
                    fighterIds.CSSSlotConfigId = csscNode.CharSlot1;
                if (fighterIds.CosmeticConfigId <= -1 && csscNode.SetCosmeticSlot)
                    fighterIds.CosmeticConfigId = csscNode.CosmeticSlot;
            }
            fighterInfo.SlotConfig = GetExConfig(fighterIds.SlotConfigId, IdType.SlotConfig);
            rootNode = slotConfigs.FirstOrDefault(x => x.FilePath == fighterInfo.SlotConfig);
            if (rootNode != null)
            {
                var slotNode = (SLTCNode)rootNode;
                if (fighterIds.FighterConfigId <= -1 && slotNode.SetSlot)
                    fighterIds.FighterConfigId = Convert.ToInt32(slotNode.CharSlot1);
                fighterInfo.VictoryThemeId = slotNode.VictoryTheme;
            }
            fighterInfo.FighterConfig = GetExConfig(fighterIds.FighterConfigId, IdType.FighterConfig);
            rootNode = fighterConfigs.FirstOrDefault(x => x.FilePath == fighterInfo.FighterConfig);
            if (rootNode != null)
            {
                var fighterNode = (FCFGNode)rootNode;
                if (fighterInfo.InternalName == null)
                {
                    fighterInfo.InternalName = fighterNode.InternalFighterName;
                }
                if (fighterInfo.Ids.SoundbankId <= -1)
                {
                    fighterInfo.Ids.SoundbankId = (int)fighterNode.SoundBank;
                }
            }
            fighterInfo.Ids = fighterIds;
            return fighterInfo;
        }

        /// <summary>
        /// Get all fighter info in build
        /// </summary>
        /// <returns>List of all fighter info in build</returns>
        public List<FighterInfo> GetAllFighterInfo()
        {
            var fighterInfo = new ConcurrentBag<FighterInfo>();
            var fighterIds = new ConcurrentBag<BrawlIds>();
            // Get configs
            var fighterConfigs = GetExConfigs(IdType.FighterConfig);
            var cosmeticConfigs = GetExConfigs(IdType.CosmeticConfig);
            var cssSlotConfigs = GetExConfigs(IdType.CSSSlotConfig);
            var slotConfigs = GetExConfigs(IdType.SlotConfig);
            // Get fighter IDs
            Parallel.ForEach(fighterConfigs, fighterConfig =>
            {
                var id = GetConfigId(fighterConfig.Name, IdType.FighterConfig);
                if (id > -1)
                {
                    fighterIds.Add(new BrawlIds { FighterConfigId = id });
                }
            });
            // Get fighter info for each fighter
            Parallel.ForEach(fighterIds, fighterId =>
            {
                fighterInfo.Add(GetFighterInfo(fighterId, fighterConfigs, cosmeticConfigs, cssSlotConfigs, slotConfigs));
            });
            // Close configs
            foreach (var config in fighterConfigs)
                _fileService.CloseFile(config);
            foreach (var config in slotConfigs)
                _fileService.CloseFile(config);
            foreach (var config in cosmeticConfigs)
                _fileService.CloseFile(config);
            foreach (var config in cssSlotConfigs)
                _fileService.CloseFile(config);
            return fighterInfo.ToList();
        }

        /// <summary>
        /// Get name for fighter PAC file based on fighter info and costume ID
        /// </summary>
        /// <param name="node">Root node of PAC file</param>
        /// <param name="fighterInfo">Fighter info</param>
        /// <param name="costumeId">Costume ID for PAC file</param>
        /// <returns>Name to use for fighter PAC file</returns>
        private string GetFighterPacName(ResourceNode node, FighterInfo fighterInfo, int costumeId = -1)
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
                "AltZ",
                "Alt",
                "Param",
                "Brres"
            };
            var name = Path.GetFileNameWithoutExtension(node.FileName);
            var oldFighterName = name;
            // Remove prefix
            var prefix = new Regex("(Itm|Fit)");
            oldFighterName = prefix.Replace(oldFighterName, string.Empty, 1);
            // Remove numbers
            var numbers = new Regex("\\d");
            oldFighterName = numbers.Replace(oldFighterName, string.Empty);
            // Remove Kirby if fighter is not Kirby
            if (fighterInfo.InternalName.ToLower() != "kirby")
            {
                var kirby = new Regex("Kirby");
                oldFighterName = kirby.Replace(oldFighterName, string.Empty);
            }
            // Remove modifiers
            var index = 0;
            foreach (var modifier in modifierStrings)
            {
                var currentIndex = oldFighterName.LastIndexOf(modifier);
                if (currentIndex > -1 && (index == 0 || currentIndex < index))
                    index = currentIndex;
            }
            if (index > 0)
            {
                oldFighterName = oldFighterName.Substring(0, index);
            }
            // Replace first match of name with the new fighter name
            var regex = new Regex(oldFighterName);
            name = regex.Replace(name, fighterInfo.InternalName, 1);
            // Remove old costume ID
            var oldCostumeId = string.Concat(name.Where(char.IsNumber));
            if (!string.IsNullOrEmpty(oldCostumeId) && oldCostumeId.Length >= 2 && name.EndsWith(oldCostumeId.Substring(oldCostumeId.Length - 2, 2)))
            {
                name = name.Substring(0, name.Length - 2);
            }
            // Add new costume ID
            if (costumeId > -1)
                name += costumeId.ToString("D2");
            name += Path.GetExtension(node.FileName);
            if (name.ToLower() == node.FileName.ToLower())
                name = node.FileName;
            return name;
        }

        /// <summary>
        /// Import all PAC files associated with a fighter
        /// </summary>
        /// <param name="pacFiles">PAC files to import</param>
        /// <param name="costumes">Costumes to import PAC files for</param>
        /// <param name="fighterInfo">Fighter info</param>
        private List<ResourceNode> UpdatePacFiles(List<string> pacFiles, List<Costume> costumes, FighterInfo fighterInfo)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
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
                    // Update GFX if they are per-costume
                    var efNode = file.Children.FirstOrDefault(x => x.Name.StartsWith("ef_") && x.GetType() == typeof(ARCNode)
                    && ((ARCNode)x).FileType == ARCFileType.EffectData && Regex.Match(x.Name, ".+[X]\\d{2}$").Success);
                    if (efNode != null)
                    {
                        efNode.Name = efNode.Name.Substring(0, efNode.Name.Length - 2) + costume.CostumeId.ToString("D2");
                        // Update EFLS and REF nodes
                        foreach (var node in efNode.Children.Where(x => x.GetType() == typeof(EFLSNode) || x.GetType() == typeof(REFFNode)))
                        {
                            foreach(var child in node.Children)
                            {
                                if (Regex.Match(child.Name, ".+[X]\\d{2}$").Success)
                                {
                                    child.Name = child.Name.Substring(0, child.Name.Length - 2) + costume.CostumeId.ToString("D2");
                                }
                            }
                        }
                    }
                    // Add file to list
                    if (file != null)
                        files.Add((file, name));
                }
            }
            // Set all file paths
            foreach(var file in files)
            {
                var folder = file.name.Contains("Kirby") ? "kirby" : fighterInfo.InternalName;
                folder += file.name.StartsWith("Itm") ? "\\item" : string.Empty;
                file.node._origPath = $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\{folder}\\{file.name}";
            }
            return files.Select(x => x.node).ToList();
        }

        /// <summary>
        /// Remove all PAC files for specified fighter
        /// </summary>
        /// <param name="internalName">Internal name of fighter</param>
        private void RemovePacFiles(string internalName)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var settings = _settingsService.BuildSettings;
            var path = $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\{internalName}";
            RemoveFighterPacFiles(internalName, path);
            RemoveKirbyFiles(internalName);
            RemoveItemFiles(internalName);
        }

        /// <summary>
        /// Get all PAC files associated with fighter
        /// </summary>
        /// <param name="internalName">Internal name of fighter</param>
        /// <returns>List of PAC files</returns>
        public List<string> GetPacFiles(string internalName)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var settings = _settingsService.BuildSettings;
            var path = $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\{internalName}";
            return GetFighterPacFiles(internalName, path);
        }

        /// <summary>
        /// Get Kirby PAC files associated with fighter
        /// </summary>
        /// <param name="internalName">Internal name of fighter</param>
        /// <returns>List of Kirby PAC files</returns>
        public List<string> GetKirbyFiles(string internalName)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var settings = _settingsService.BuildSettings;
            var path = $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\kirby";
            return GetFighterPacFiles("Kirby" + internalName, path);
        }

        /// <summary>
        /// Remove regular PAC files associated with fighter
        /// </summary>
        /// <param name="name">Internal name of fighter</param>
        /// <param name="path">Path to remove PAC files from</param>
        private void RemoveFighterPacFiles(string name, string path)
        {
            var files = GetFighterPacFiles(name, path);
            foreach(var file in files)
            {
                _fileService.DeleteFile(file);
            }
        }

        /// <summary>
        /// Remove Kirby pac files associated with fighter
        /// </summary>
        /// <param name="name">Internal name of fighter</param>
        private void RemoveKirbyFiles(string name)
        {
            var files = GetKirbyFiles(name);
            foreach(var file in files)
            {
                _fileService.DeleteFile(file);
            }
        }

        /// <summary>
        /// Remove item pac files associated with fighter
        /// </summary>
        /// <param name="name">Internal name of fighter</param>
        private void RemoveItemFiles(string name)
        {
            var files = GetItemFiles(name);
            foreach(var file in files)
            {
                _fileService.DeleteFile(file);
            }
        }

        /// <summary>
        /// Get Kirby and fighter PAC files for fighter
        /// </summary>
        /// <param name="name">Internal name of fighter</param>
        /// <param name="path">Path to retrive files from</param>
        /// <returns>List of PAC files</returns>
        private List<string> GetFighterPacFiles(string name, string path)
        {
            var files = new List<string>();
            if (Directory.Exists(path))
                files = Directory.GetFiles(path, "*.pac").Where(x => Path.GetFileName(x).StartsWith($"Fit{name}", StringComparison.InvariantCultureIgnoreCase)).ToList();
            return files;
        }

        /// <summary>
        /// Get item files for fighter
        /// </summary>
        /// <param name="internalName">Internal name of fighter</param>
        /// <returns>List of item files</returns>
        public List<string> GetItemFiles(string internalName)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var settings = _settingsService.BuildSettings;
            var path = $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\{internalName}\\item";
            var files = new List<string>();
            if (Directory.Exists(path))
                files = Directory.GetFiles(path, "*.pac").ToList();
            return files;
        }

        /// <summary>
        /// Import updated fighter files
        /// </summary>
        /// <param name="fighterPackage">Fighter package to import files for</param>
        public void ImportFighterFiles(FighterPackage fighterPackage)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var settings = _settingsService.BuildSettings;
            // First, get fighter package with all of the old values
            var oldFighter = new FighterPackage();
            oldFighter.FighterInfo = new FighterInfo();
            oldFighter.FighterInfo.Ids = fighterPackage.FighterInfo.Ids.Copy();
            oldFighter.FighterInfo = GetFighterInfo(oldFighter.FighterInfo);
            oldFighter = GetFighterFiles(oldFighter);
            // Then, get new files for install, in case any of them would be deleted
            var pacFiles = UpdatePacFiles(fighterPackage.PacFiles, fighterPackage.Costumes, fighterPackage.FighterInfo);
            var module = _fileService.OpenFile(oldFighter.Module);
            var fighterConfig = _fileService.OpenFile(oldFighter.FighterInfo.FighterConfig);
            var cosmeticConfig = _fileService.OpenFile(oldFighter.FighterInfo.CosmeticConfig);
            var cssSlotConfig = _fileService.OpenFile(oldFighter.FighterInfo.CSSSlotConfig);
            var slotConfig = _fileService.OpenFile(oldFighter.FighterInfo.SlotConfig);
            var soundbank = _fileService.OpenFile(oldFighter.Soundbank);
            // Delete old files
            RemovePacFiles(oldFighter.FighterInfo.InternalName);
            DeleteModule(oldFighter.FighterInfo.InternalName);
            DeleteExConfigs(oldFighter.FighterInfo);
            DeleteSoundbank(oldFighter.FighterInfo.Ids.SoundbankId);
            // Import pac files
            foreach(var pacFile in pacFiles)
            {
                _fileService.SaveFile(pacFile);
                _fileService.CloseFile(pacFile);
            }
            // Update and import ex configs
            if (cssSlotConfig != null)
            {
                cssSlotConfig = UpdateCostumeConfig(cssSlotConfig, fighterPackage.Costumes);
                var configPath = $"{buildPath}\\{settings.FilePathSettings.BrawlEx}\\CSSSlotConfig\\CSSSlot{fighterPackage.FighterInfo.Ids.CSSSlotConfigId:X2}.dat";
                _fileService.SaveFileAs(cssSlotConfig, configPath);
                _fileService.CloseFile(cssSlotConfig);
            }
            if (fighterConfig != null)
            {
                fighterConfig = UpdateFighterConfig(fighterConfig, fighterPackage.FighterInfo);
                var configPath = $"{buildPath}\\{settings.FilePathSettings.BrawlEx}\\FighterConfig\\Fighter{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}.dat";
                _fileService.SaveFileAs(fighterConfig, configPath);
                _fileService.CloseFile(fighterConfig);
            }
            if (slotConfig != null)
            {
                var configPath = $"{buildPath}\\{settings.FilePathSettings.BrawlEx}\\SlotConfig\\Slot{fighterPackage.FighterInfo.Ids.SlotConfigId:X2}.dat";
                _fileService.SaveFileAs(slotConfig, configPath);
                _fileService.CloseFile(slotConfig);
            }
            if (cosmeticConfig != null)
            {
                var configPath = $"{buildPath}\\{settings.FilePathSettings.BrawlEx}\\CosmeticConfig\\Cosmetic{fighterPackage.FighterInfo.Ids.CosmeticConfigId:X2}.dat";
                _fileService.SaveFileAs(cosmeticConfig, configPath);
                _fileService.CloseFile(cosmeticConfig);
            }
            // Update and import module
            if (module != null)
            {
                module = UpdateModule((RELNode)module, fighterPackage.FighterInfo.Ids.FighterConfigId);
                var path = $"{_settingsService.AppSettings.BuildPath}\\{_settingsService.BuildSettings.FilePathSettings.Modules}\\ft_{fighterPackage.FighterInfo.InternalName.ToLower()}.rel";
                _fileService.SaveFileAs(module, path);
                _fileService.CloseFile(module);
            }
            // Rename and import soundbank
            if (soundbank != null)
            {
                var name = GetSoundbankName(fighterPackage.FighterInfo.Ids.SoundbankId);
                soundbank._origPath = Path.Combine(_settingsService.AppSettings.BuildPath, _settingsService.BuildSettings.FilePathSettings.SoundbankPath, name);
                _fileService.SaveFile(soundbank);
                _fileService.CloseFile(soundbank);
            }
        }

        /// <summary>
        /// Delete all ex configs associated with fighter
        /// </summary>
        /// <param name="fighterInfo">Fighter info</param>
        public void DeleteExConfigs(FighterInfo fighterInfo)
        {
            if (fighterInfo?.FighterConfig != null)
            {
                _fileService.DeleteFile(fighterInfo?.FighterConfig);
            }
            if (fighterInfo?.CosmeticConfig != null)
            {
                _fileService.DeleteFile(fighterInfo?.CosmeticConfig);
            }
            if (fighterInfo?.CSSSlotConfig != null)
            {
                _fileService.DeleteFile(fighterInfo?.CSSSlotConfig);
            }
            if (fighterInfo?.SlotConfig != null)
            {
                _fileService.DeleteFile(fighterInfo?.SlotConfig);
            }
        }

        /// <summary>
        /// Update CSSSlotConfig for fighter
        /// </summary>
        /// <param name="fighterInfo">Fighter info</param>
        /// <param name="costumes">Costumes to place in config</param>
        private ResourceNode UpdateCostumeConfig(ResourceNode rootNode, List<Costume> costumes)
        {
            if (rootNode != null)
            {
                rootNode.Children.RemoveAll(x => x != null);
                foreach(var costume in costumes)
                {
                    var newNode = new CSSCEntryNode
                    {
                        Parent = rootNode,
                        CostumeID = (byte)costume.CostumeId,
                        Color = costume.Color
                    };
                }
            }
            return rootNode;
        }

        /// <summary>
        /// Update fighter config
        /// </summary>
        /// <param name="rootNode">Config to update</param>
        /// <param name="fighterInfo">Fighter info</param>
        /// <returns>Updated config</returns>
        private ResourceNode UpdateFighterConfig(ResourceNode rootNode, FighterInfo fighterInfo)
        {
            if (rootNode != null)
            {
                var node = (FCFGNode) rootNode;
                node.SoundBank = (uint)fighterInfo.Ids.SoundbankId;
            }
            return rootNode;
        }

        /// <summary>
        /// Get fighter costumes
        /// </summary>
        /// <param name="fighterInfo">Fighter info</param>
        /// <returns>List of costumes</returns>
        public List<Costume> GetFighterCostumes(FighterInfo fighterInfo)
        {
            var costumes = new List<Costume>();
            if (fighterInfo.CSSSlotConfig != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
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
                            costume.PacFiles = GetPacFiles(fighterInfo.InternalName)
                                .Where(x => Path.GetFileNameWithoutExtension(x).EndsWith(costume.CostumeId.ToString("D2"))).ToList();
                            costume.PacFiles.AddRange(GetKirbyFiles(fighterInfo.InternalName)
                                .Where(x => Path.GetFileNameWithoutExtension(x).EndsWith(costume.CostumeId.ToString("D2"))).ToList());
                            costume.PacFiles.AddRange(GetItemFiles(fighterInfo.InternalName)
                                .Where(x => Path.GetFileNameWithoutExtension(x).EndsWith(costume.CostumeId.ToString("D2"))).ToList());
                        }
                        costumes.Add(costume);
                    }
                    _fileService.CloseFile(rootNode);
                }
            }
            return costumes;
        }

        /// <summary>
        /// Associate cosmetics with costumes
        /// </summary>
        /// <param name="costumes">Costumes</param>
        /// <param name="cosmetics">Cosmetics</param>
        /// <returns>List of costumes with cosmetics associated</returns>
        public List<Costume> GetCostumeCosmetics(List<Costume> costumes, List<Cosmetic> cosmetics)
        {
            foreach (var costume in costumes)
            {
                costume.Cosmetics = cosmetics.Where(x => x.CostumeIndex - 1 == costumes.IndexOf(costume)).ToList();
            }
            return costumes;
        }

        /// <summary>
        /// Get all fighter files for fighter package
        /// </summary>
        /// <param name="fighterPackage">Fighter package to retrieve files for</param>
        /// <returns>Fighter package with files</returns>
        public FighterPackage GetFighterFiles(FighterPackage fighterPackage)
        {
            // Get fighter info
            var fighterInfo = GetFighterInfo(fighterPackage.FighterInfo);

            // Get costumes
            var costumes = GetFighterCostumes(fighterInfo);

            // Get fighter files
            fighterPackage.PacFiles = GetPacFiles(fighterInfo.InternalName)?.Where(x => !costumes.SelectMany(y => y.PacFiles).Contains(x)).ToList();
            fighterPackage.PacFiles.AddRange(GetKirbyFiles(fighterInfo.InternalName)?.Where(x => !costumes.SelectMany(y => y.PacFiles).Contains(x)).ToList());
            fighterPackage.PacFiles.AddRange(GetItemFiles(fighterInfo.InternalName)?.Where(x => !costumes.SelectMany(y => y.PacFiles).Contains(x)).ToList());
            fighterPackage.Module = GetModule(fighterInfo.InternalName);
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

            // Get soundbank
            fighterPackage.Soundbank = GetSoundbank(fighterInfo.Ids.SoundbankId);

            fighterPackage.Costumes = costumes;
            fighterPackage.FighterInfo = fighterInfo;

            return fighterPackage;
        }

        /// <summary>
        /// Get name of soundbank by ID
        /// </summary>
        /// <param name="soundbankId">GetSoundbankName</param>
        /// <returns>Name of soundbank file</returns>
        private string GetSoundbankName(int soundbankId)
        {
            var name = _settingsService.BuildSettings.SoundSettings.SoundbankStyle == SoundbankStyle.InfoIndex
                        ? $"{soundbankId:X3}.sawnd" : $"{(soundbankId + 7):D}.sawnd";
            return name;
        }

        /// <summary>
        /// Get soundbank filepath by ID
        /// </summary>
        /// <param name="soundbankId">Soundbank ID</param>
        /// <returns>Soundbank file path</returns>
        private string GetSoundbank(int soundbankId)
        {
            if (soundbankId > -1)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var sfxPath = _settingsService.BuildSettings.FilePathSettings.SoundbankPath;
                var path = Path.Combine(buildPath, sfxPath);
                if (Directory.Exists(path))
                {
                    var name = GetSoundbankName(soundbankId);
                    var soundbank = Directory.GetFiles(path, name).FirstOrDefault();
                    if (soundbank != null)
                    {
                        return soundbank;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Delete soundbank by ID
        /// </summary>
        /// <param name="soundbankId">ID of soundbank to delete</param>
        private void DeleteSoundbank(int soundbankId)
        {
            var soundbank = GetSoundbank(soundbankId);
            if (soundbank != null)
            {
                _fileService.DeleteFile(soundbank);
            }
        }

        /// <summary>
        /// Delete fighter module by name
        /// </summary>
        /// <param name="internalName">Internal name of fighter</param>
        private void DeleteModule(string internalName)
        {
            var module = GetModule(internalName);
            if (module != null)
            {
                _fileService.DeleteFile(module);
            }
        }

        /// <summary>
        /// Get module for fighter
        /// </summary>
        /// <param name="internalName">Internal name for fighter</param>
        /// <returns>Fighter module</returns>
        public string GetModule(string internalName)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var settings = _settingsService.BuildSettings;
            var moduleFolder = $"{buildPath}\\{settings.FilePathSettings.Modules}";
            var module = $"{moduleFolder}\\ft_{internalName.ToLower()}.rel";
            if (Directory.Exists(moduleFolder))
                if (File.Exists(module))
                    return module;
            return null;
        }

        /// <summary>
        /// Update fighter module with new ID
        /// </summary>
        /// <param name="module">Module to update</param>
        /// <param name="fighterId">Fighter ID to insert</param>
        private RELNode UpdateModule(RELNode relNode, int fighterId)
        {
            var isExModule = false;
            if (relNode != null)
            {
                // First, check for Section [8] - indicates Ex module
                if (relNode.Sections.Length >= 9)
                {
                    var section = relNode.Sections[8];
                    if (section != null)
                    {
                        var sectionData = _fileService.ReadRawData(section);
                        // Next, check that first three digits are zeroes, otherwise its not a proper Ex module
                        isExModule = !sectionData.Take(3).Any(x => x != 0) && sectionData.Length >= 4;
                        // Update the fighter ID
                        if (isExModule)
                        {
                            // 4th position is fighter ID
                            // TODO: Make this configurable?
                            sectionData[3] = (byte)fighterId;
                            _fileService.ReplaceNodeRaw(section, sectionData);
                        }
                    }
                }
                // If it isn't an Ex module - either because we didn't find a Section [8] or it didn't match - treat it as a fighter module
                if (!isExModule)
                {
                    // Find matching fighter module ID locations
                    var matches = ModuleIdLocations.IdLocations.Where(x => x.ModuleName == relNode.Name);
                    if (matches.Any())
                    {
                        var match = matches.First();
                        // Check for Section [1]
                        if (relNode.Sections.Length >= 2)
                        {
                            var section = relNode.Sections[1];
                            if (section != null)
                            {
                                var sectionData = _fileService.ReadRawData(section);
                                // Replace data at each location with our ID
                                foreach(var location in match.Locations)
                                {
                                    // TODO: Instead of putting + 3 here, put it in the static class?
                                    sectionData[location + 3] = (byte)fighterId;
                                    _fileService.ReplaceNodeRaw(section, sectionData);
                                }
                            }
                        }
                    }
                }
            }
            return relNode;
        }
    }
}
