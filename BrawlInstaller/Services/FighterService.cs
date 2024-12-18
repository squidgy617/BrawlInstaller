using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.StaticClasses;
using BrawlLib.SSBB;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.ResourceNodes.ProjectPlus;
using BrawlLib.SSBB.Types;
using lKHM;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Effects;
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

        /// <inheritdoc cref="FighterService.GetPacFiles(string, bool)"/>
        List<FighterPacFile> GetPacFiles(string internalName, bool removeCostumeId = true);

        /// <inheritdoc cref="FighterService.GetKirbyFiles(string, bool)"/>
        List<FighterPacFile> GetKirbyFiles(string internalName, bool removeCostumeId = true);

        /// <inheritdoc cref="FighterService.GetAllFighterInfo()"/>
        List<FighterInfo> GetAllFighterInfo();

        /// <inheritdoc cref="FighterService.GetFighterAttributes(FighterInfo, List{string})"/>
        FighterInfo GetFighterAttributes(FighterInfo fighterInfo, List<string> exConfigs);

        /// <inheritdoc cref="FighterService.GetFighterFiles(FighterPackage)"/>
        FighterPackage GetFighterFiles(FighterPackage fighterPackage);

        /// <inheritdoc cref="FighterService.ImportFighterFiles(FighterPackage)"/>
        void ImportFighterFiles(FighterPackage fighterPackage);

        /// <inheritdoc cref="FighterService.UpdateFighterSettings(FighterPackage)"/>
        void UpdateFighterSettings(FighterPackage fighterPackage);

        /// <inheritdoc cref="FighterService.GetFighterSettings(FighterPackage)"/>
        FighterSettings GetFighterSettings(FighterPackage fighterPackage);

        /// <inheritdoc cref="FighterService.ConvertXMLToKirbyHatData(string)"/>
        HatInfoPack ConvertXMLToKirbyHatData(string xmlPath);

        /// <inheritdoc cref="FighterService.GetFighterPacName(FighterPacFile, FighterInfo, bool)"/>
        FighterPacFile GetFighterPacName(FighterPacFile pacFile, FighterInfo fighterInfo, bool removeCostumeId = true);

        /// <inheritdoc cref="FighterService.GetFighterEffectPacId(List{FighterPacFile}, string)"/>
        int? GetFighterEffectPacId(List<FighterPacFile> pacFiles, string internalName);

        /// <inheritdoc cref="FighterService.UpdateCreditsModule(FighterPackage)"/>
        void UpdateCreditsModule(FighterPackage fighterPackage);

        /// <inheritdoc cref="FighterService.GetFighterPacFile(string, string, string, bool)"/>
        FighterPacFile GetFighterPacFile(string filePath, string name, string folderName, bool removeCostumeId = true);

        /// <inheritdoc cref="FighterService.GetRosters()"/>
        List<Roster> GetRosters();

        /// <inheritdoc cref="FighterService.SaveRosters(List{Roster})"/>
        void SaveRosters(List<Roster> rosters);

        /// <inheritdoc cref="FighterService.UpdateNullIdsToFirstUnused(FighterInfo)"/>
        FighterInfo UpdateNullIdsToFirstUnused(FighterInfo fighterInfo);

        /// <inheritdoc cref="FighterService.UpdateIdsToFirstUnused(FighterInfo)"/>
        FighterInfo UpdateIdsToFirstUnused(FighterInfo fighterInfo);

        /// <inheritdoc cref="FighterService.GetUsedFighterIds()"/>
        List<BrawlId> GetUsedFighterIds();

        /// <inheritdoc cref="FighterService.GetUsedInternalNames()"/>
        List<string> GetUsedInternalNames();

        /// <inheritdoc cref="FighterService.UpdatePacFiles(FighterPackage)"/>
        List<ResourceNode> UpdatePacFiles(FighterPackage fighterPackage);
    }
    [Export(typeof(IFighterService))]
    internal class FighterService : IFighterService
    {
        // Services
        ISettingsService _settingsService { get; }
        IFileService _fileService { get; }
        ITracklistService _tracklistService { get; }
        ICodeService _codeService { get; }
        IPsaService _psaService { get; }

        [ImportingConstructor]
        public FighterService(ISettingsService settingsService, IFileService fileService, ITracklistService tracklistService, ICodeService codeService, IPsaService psaService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _tracklistService = tracklistService;
            _codeService = codeService;
            _psaService = psaService;
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
        private int? GetConfigId(string name, IdType type)
        {
            var prefix = GetConfigPrefix(type);
            if (int.TryParse(name.Replace(prefix, ""), NumberStyles.HexNumber, null, out int id))
                return id;
            return null;
        }

        /// <summary>
        /// Get Ex config from ID
        /// </summary>
        /// <param name="id">ID of config</param>
        /// <param name="type">Type of ID</param>
        /// <returns>Path to Ex config</returns>
        private string GetExConfig(int? id, IdType type)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var settings = _settingsService.BuildSettings;
            var prefix = GetConfigPrefix(type);
            var directory = $"{buildPath}\\{settings.FilePathSettings.BrawlEx}\\{prefix}Config";
            var config = $"{directory}\\{prefix}{id:X2}.dat";
            if (_fileService.FileExists(config))
            {
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
            var files = _fileService.GetFiles(directory, $"{prefix}*.dat").AsParallel().ToList();
            Parallel.ForEach(files, file =>
            {
                configs.Add(_fileService.OpenFile(file));
            });
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
            if (fighterIds.FighterConfigId != null && fighterIds.SlotConfigId == null)
            {
                var foundConfig = slotConfigs.FirstOrDefault(x => ((SLTCNode)x).SetSlot && ((SLTCNode)x).CharSlot1 == fighterIds.FighterConfigId);
                if (foundConfig != null)
                    fighterIds.SlotConfigId = GetConfigId(foundConfig.Name, IdType.SlotConfig);
            }
            if (fighterIds.CosmeticConfigId != null && fighterIds.SlotConfigId == null)
            {
                var foundConfig = cosmeticConfigs.FirstOrDefault(x => ((COSCNode)x).Name.EndsWith(fighterIds.CosmeticConfigId?.ToString("X2")) && ((COSCNode)x).HasSecondary);
                if (foundConfig != null)
                    fighterIds.SlotConfigId = ((COSCNode)foundConfig).CharSlot1;
            }
            if (fighterIds.CSSSlotConfigId != null && fighterIds.SlotConfigId == null)
            {
                var foundConfig = cssSlotConfigs.FirstOrDefault(x => ((CSSCNode)x).Name.EndsWith(fighterIds.CSSSlotConfigId?.ToString("X2")) && ((CSSCNode)x).SetPrimarySecondary);
                if (foundConfig != null)
                    fighterIds.SlotConfigId = ((CSSCNode)foundConfig).CharSlot1;
            }
            // If slot config ID is still missing, set it equal to fighter config ID
            if (fighterIds.SlotConfigId == null && fighterIds.FighterConfigId >= 0x3F)
                fighterIds.SlotConfigId = fighterIds.FighterConfigId;
            // Find cosmetic config ID if missing
            if (fighterIds.SlotConfigId != null && fighterIds.CosmeticConfigId == null)
            {
                var foundConfig = cosmeticConfigs.FirstOrDefault(x => ((COSCNode)x).HasSecondary && ((COSCNode)x).CharSlot1 == fighterIds.SlotConfigId);
                if (foundConfig != null)
                    fighterIds.CosmeticConfigId = GetConfigId(foundConfig.Name, IdType.CosmeticConfig);
            }
            if (fighterIds.CSSSlotConfigId != null && fighterIds.CosmeticConfigId == null)
            {
                var foundConfig = cssSlotConfigs.FirstOrDefault(x => ((CSSCNode)x).Name.EndsWith(fighterIds.CSSSlotConfigId?.ToString("X2")) && ((CSSCNode)x).SetCosmeticSlot);
                if (foundConfig != null)
                    fighterIds.CosmeticConfigId = ((CSSCNode)foundConfig).CosmeticSlot;
            }
            // If cosmetic config ID is still missing, set it equal to slot config ID
            if (fighterIds.CosmeticConfigId == null && fighterIds.SlotConfigId >= 0x3F)
                fighterIds.CosmeticConfigId = fighterIds.SlotConfigId;
            // Find CSS slot config ID if missing
            if (fighterIds.SlotConfigId != null && fighterIds.CSSSlotConfigId == null)
            {
                var foundConfig = cssSlotConfigs.FirstOrDefault(x => ((CSSCNode)x).SetPrimarySecondary && ((CSSCNode)x).CharSlot1 == fighterIds.SlotConfigId);
                if (foundConfig != null)
                    fighterIds.CSSSlotConfigId = GetConfigId(foundConfig.Name, IdType.CSSSlotConfig);
            }
            if (fighterIds.CosmeticConfigId != null && fighterIds.CSSSlotConfigId == null)
            {
                var foundConfig = cssSlotConfigs.FirstOrDefault(x => ((CSSCNode)x).SetCosmeticSlot && ((CSSCNode)x).CosmeticSlot == fighterIds.CosmeticConfigId);
                if (foundConfig != null)
                    fighterIds.CSSSlotConfigId = ((CSSCNode)foundConfig).CosmeticSlot;
            }
            // If CSS slot config ID is still missing, set it equal to one of the other IDs
            if (fighterIds.CSSSlotConfigId == null && (fighterIds.CosmeticConfigId >= 0x3F || fighterIds.SlotConfigId >= 0x3F))
                fighterIds.CSSSlotConfigId = fighterIds.CosmeticConfigId != -1 ? fighterIds.CosmeticConfigId : fighterIds.SlotConfigId;
            // Find fighter ID if missing
            if (fighterIds.SlotConfigId != null && fighterIds.FighterConfigId == null)
            {
                var foundConfig = slotConfigs.FirstOrDefault(x => ((SLTCNode)x).SetSlot && ((SLTCNode)x).Name.EndsWith(fighterIds.SlotConfigId?.ToString("X2")));
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
        /// <param name="fighterInfo">Fighter info</param>
        /// <param name="fighterConfigs">All fighter configs to use</param>
        /// <param name="cosmeticConfigs">All cosmetic configs to use</param>
        /// <param name="cssSlotConfigs">All CSSslot configs to use</param>
        /// <param name="slotConfigs">All slot configs to use</param>
        /// <returns>Fighter info</returns>
        // TODO: get way more info
        private FighterInfo GetFighterInfo(FighterInfo fighterInfo, List<ResourceNode> fighterConfigs, List<ResourceNode> cosmeticConfigs, List<ResourceNode> cssSlotConfigs, List<ResourceNode> slotConfigs)
        {
            // For IDs, we only fill them if they have not already been filled, but for everything else we fill it if we find it.
            // This way, if we can't find a file, we default to what the fighter info already has, otherwise we pull from the source of truth we find.
            var fighterIds = LinkExConfigs(fighterInfo.Ids, cosmeticConfigs, cssSlotConfigs, slotConfigs);
            fighterInfo.CosmeticConfig = GetExConfig(fighterIds.CosmeticConfigId, IdType.CosmeticConfig);
            var rootNode = cosmeticConfigs.FirstOrDefault(x => x.FilePath == fighterInfo.CosmeticConfig);
            if (rootNode != null)
            {
                var coscNode = (COSCNode)rootNode;
                if (fighterIds.SlotConfigId == null && coscNode.HasSecondary)
                    fighterIds.SlotConfigId = coscNode.CharSlot1;
                fighterIds.CosmeticId = coscNode.CosmeticID;
                fighterIds.FranchiseId = coscNode.FranchiseIconID + 1;
                fighterInfo.DisplayName = coscNode.CharacterName;
                fighterInfo.EntryName = coscNode.CharacterName;
                fighterInfo.CosmeticAttributes = coscNode.ToCosmeticAttributes();
            }
            fighterInfo.CSSSlotConfig = GetExConfig(fighterIds.CSSSlotConfigId, IdType.CSSSlotConfig);
            rootNode = cssSlotConfigs.FirstOrDefault(x => x.FilePath == fighterInfo.CSSSlotConfig);
            if (rootNode != null)
            {
                var csscNode = (CSSCNode)rootNode;
                if (fighterIds.SlotConfigId == null && csscNode.SetPrimarySecondary)
                    fighterIds.CSSSlotConfigId = csscNode.CharSlot1;
                if (fighterIds.CosmeticConfigId == null && csscNode.SetCosmeticSlot)
                    fighterIds.CosmeticConfigId = csscNode.CosmeticSlot;
                fighterInfo.CSSSlotAttributes = csscNode.ToCSSSlotAttributes();
            }
            fighterInfo.SlotConfig = GetExConfig(fighterIds.SlotConfigId, IdType.SlotConfig);
            rootNode = slotConfigs.FirstOrDefault(x => x.FilePath == fighterInfo.SlotConfig);
            if (rootNode != null)
            {
                var slotNode = (SLTCNode)rootNode;
                if (fighterIds.FighterConfigId == null && slotNode.SetSlot)
                    fighterIds.FighterConfigId = Convert.ToInt32(slotNode.CharSlot1);
                fighterInfo.VictoryThemeId = slotNode.VictoryTheme;
                fighterInfo.SlotAttributes = slotNode.ToSlotAttributes();
            }
            fighterInfo.FighterConfig = GetExConfig(fighterIds.FighterConfigId, IdType.FighterConfig);
            rootNode = fighterConfigs.FirstOrDefault(x => x.FilePath == fighterInfo.FighterConfig);
            if (rootNode != null)
            {
                var fighterNode = (FCFGNode)rootNode;
                fighterInfo.InternalName = fighterNode.FighterName;
                fighterInfo.SoundbankId = fighterNode.SoundBank;
                fighterInfo.OriginalSoundbankId = fighterNode.SoundBank;
                fighterInfo.KirbySoundbankId = fighterNode.KirbySoundBank;
                fighterInfo.OriginalKirbySoundbankId = fighterNode.KirbySoundBank;
                fighterInfo.KirbyLoadType = fighterNode.KirbyLoadType;
                fighterInfo.FighterAttributes = fighterNode.ToFighterAttributes();
            }
            fighterInfo.Ids = fighterIds;
            fighterInfo.EndingId = GetEndingId(fighterInfo.Ids.CosmeticConfigId);
            // Get masquerade file
            if (!string.IsNullOrEmpty(_settingsService.BuildSettings.FilePathSettings.MasqueradePath))
            {
                var path = $"{_settingsService.AppSettings.BuildPath}\\{_settingsService.BuildSettings.FilePathSettings.MasqueradePath}\\{fighterInfo.Ids.MasqueradeId:D2}.masq";
                if (_fileService.FileExists(path))
                {
                    fighterInfo.Masquerade = path;
                }
            }
            // TODO: Commented because it slows things down, find a way to speed this up, maybe get all slot IDs in advance at the start?
            //fighterInfo.CreditsThemeId = GetCreditsTheme(fighterInfo.Ids.SlotConfigId)?.SongId;
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
        /// Get fighter attributes from ex configs and apply to fighter info
        /// </summary>
        /// <param name="fighterInfo">Fighter info to apply attributes to</param>
        /// <param name="exConfigs">Configs to get attributes from</param>
        /// <returns>Fighter info with attributes</returns>
        public FighterInfo GetFighterAttributes(FighterInfo fighterInfo, List<string> exConfigs)
        {
            foreach (var config in exConfigs)
            {
                var rootNode = _fileService.OpenFile(config);
                if (rootNode != null)
                {
                    if (rootNode.GetType() == typeof(FCFGNode))
                    {
                        fighterInfo.FighterAttributes = ((FCFGNode)rootNode).ToFighterAttributes();
                    }
                    else if (rootNode.GetType() == typeof(SLTCNode))
                    {
                        fighterInfo.SlotAttributes = ((SLTCNode)rootNode).ToSlotAttributes();
                    }
                    else if (rootNode.GetType() == typeof(COSCNode))
                    {
                        fighterInfo.CosmeticAttributes = ((COSCNode)rootNode).ToCosmeticAttributes();
                    }
                    else if (rootNode.GetType() == typeof(CSSCNode))
                    {
                        fighterInfo.CSSSlotAttributes = ((CSSCNode)rootNode).ToCSSSlotAttributes();
                    }
                    _fileService.CloseFile(rootNode);
                }
            }
            return fighterInfo;
        }

        /// <summary>
        /// Get name for fighter PAC file based on fighter info and costume ID
        /// </summary>
        /// <param name="pacFile">PAC file</param>
        /// <param name="fighterInfo">Fighter info</param>
        /// <param name="removeCostumeId">Remove costume ID from pac name</param>
        /// <returns>Name to use for fighter PAC file</returns>
        public FighterPacFile GetFighterPacName(FighterPacFile pacFile, FighterInfo fighterInfo, bool removeCostumeId=true)
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
                "Brres",
                "Fake"
            };
            var name = Path.GetFileNameWithoutExtension(pacFile.FilePath);
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
            // TODO: We check for Kirby to ensure we don't overwrite filenames for Kirby's other fighter files, is there a better way to do this?
            if (fighterInfo.InternalName.ToLower() != "kirby")
            {
                var regex = new Regex(oldFighterName);
                name = regex.Replace(name, fighterInfo.InternalName, 1);
            }
            return GetFighterPacFile(pacFile.FilePath, oldFighterName, oldFighterName, removeCostumeId);
        }

        /// <summary>
        /// Get fighter pac file object from file
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="name">Internal name of fighter</param>
        /// <param name="folderName">Name of PAC file root folder</param>
        /// <returns>Fighter pac file object</returns>
        public FighterPacFile GetFighterPacFile(string filePath, string name, string folderName, bool removeCostumeId=true)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(folderName))
            {
                var pacFile = new FighterPacFile { FilePath = filePath };
                // If files are gotten from a subdirectory, set folder name
                var dir = Path.GetFileName(Path.GetDirectoryName(filePath));
                if (dir != folderName && (dir != "kirby" && folderName != "kirby"))
                {
                    pacFile.Subdirectory = dir;
                }
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var nameLocation = fileName.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                if (nameLocation > -1)
                {
                    // Remove old costume ID
                    var oldCostumeId = string.Concat(fileName.Where(char.IsNumber));
                    if (removeCostumeId && !string.IsNullOrEmpty(oldCostumeId) && oldCostumeId.Length >= 2 && fileName.EndsWith(oldCostumeId.Substring(oldCostumeId.Length - 2, 2)))
                    {
                        fileName = fileName.Substring(0, fileName.Length - 2);
                    }
                    pacFile.Prefix = fileName.Substring(0, nameLocation);
                    if (nameLocation + name.Length < fileName.Length)
                    {
                        pacFile.Suffix = fileName.Substring(nameLocation + name.Length);
                    }
                }
                return pacFile;
            }
            return null;
        }

        /// <summary>
        /// Update credits module for fighter
        /// </summary>
        /// <param name="fighterPackage">Fighter package</param>
        public void UpdateCreditsModule(FighterPackage fighterPackage)
        {
            var fighterInfo = fighterPackage.FighterInfo;
            var buildPath = _settingsService.AppSettings.BuildPath;
            if (_settingsService.BuildSettings.MiscSettings.UpdateCreditsModule && fighterInfo.Ids.SlotConfigId != null)
            {
                var modulePath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.CreditsModule);
                var rootNode = _fileService.OpenFile(modulePath);
                if (rootNode != null)
                {
                    // Update section 7
                    var module = (RELNode)rootNode;
                    if (module.Sections.Count() >= 7)
                    {
                        var section = module.Sections[7];
                        var sectionData = _fileService.ReadRawData(section);
                        // Slot ID + 0x200 is position to modify
                        var fighterPosition = 0x200 + fighterInfo.Ids.SlotConfigId.Value;
                        if (sectionData.Length > fighterPosition)
                        {
                            // Insert cosmetic config ID
                            sectionData[fighterPosition] = fighterPackage.PackageType != PackageType.Delete ? (byte)fighterInfo.Ids.CosmeticConfigId : (byte)0;
                            _fileService.ReplaceNodeRaw(section, sectionData);
                            _fileService.SaveFile(rootNode);
                        }
                    }
                    _fileService.CloseFile(rootNode);
                }
            }
        }

        /// <summary>
        /// Import all PAC files associated with a fighter
        /// </summary>
        /// <param name="pacFiles">PAC files to import</param>
        /// <param name="costumes">Costumes to import PAC files for</param>
        /// <param name="fighterInfo">Fighter info</param>
        public List<ResourceNode> UpdatePacFiles(FighterPackage fighterPackage)
        {
            var pacFiles = fighterPackage.PacFiles;
            var costumes = fighterPackage.Costumes;
            var fighterInfo = fighterPackage.FighterInfo;
            var buildPath = _settingsService.AppSettings.BuildPath;
            var settings = _settingsService.BuildSettings;
            var files = new List<(ResourceNode node, string name, FighterPacFile pacFile)>();
            foreach(var pacFile in pacFiles)
            {
                var file = _fileService.OpenFile(pacFile.FilePath);
                var name = pacFile.Prefix + fighterInfo.InternalName + pacFile.Suffix + ".pac";
                if (file != null)
                {
                    files.Add((file, name, pacFile));
                    // If main PAC file or item PAC file, update GFX and SFX IDs
                    var regex = Regex.Match(name.ToLower(), $"[itm{fighterInfo.InternalName.ToLower()}]\\d+[param]");
                    if (name.ToLower() == ("Fit" + fighterInfo.InternalName + ".pac").ToLower() || regex.Success)
                    {
                        // Update GFX IDs if they've changed
                        if (fighterPackage.FighterInfo.EffectPacId != null && fighterPackage.FighterInfo.OriginalEffectPacId != null && 
                            fighterPackage.FighterInfo.EffectPacId != fighterPackage.FighterInfo.OriginalEffectPacId)
                        {
                            UpdateEffectPac(file, (int)fighterPackage.FighterInfo.EffectPacId, (int)fighterPackage.FighterInfo.OriginalEffectPacId);
                        }
                        // Update SFX IDs if they've changed
                        if (fighterPackage.FighterInfo.SoundbankId != null && fighterPackage.FighterInfo.OriginalSoundbankId != null &&
                            fighterPackage.FighterInfo.SoundbankId != fighterPackage.FighterInfo.OriginalSoundbankId)
                        {
                            UpdateSFXIds(file, (int)fighterPackage.FighterInfo.SoundbankId, (int)fighterPackage.FighterInfo.OriginalSoundbankId);
                        }
                    }
                    // If Kirby PAC file, update GFX IDs
                    if (name.ToLower() == ("FitKirby" + fighterInfo.InternalName + ".pac").ToLower())
                    {
                        // Update GFX IDs if they've changed
                        if (fighterPackage.FighterInfo.KirbyEffectPacId != null && fighterPackage.FighterInfo.OriginalKirbyEffectPacId != null && 
                            fighterPackage.FighterInfo.KirbyEffectPacId != fighterPackage.FighterInfo.OriginalKirbyEffectPacId)
                        {
                            UpdateEffectPac(file, (int)fighterPackage.FighterInfo.KirbyEffectPacId, (int)fighterPackage.FighterInfo.OriginalKirbyEffectPacId);
                        }
                        // Update SFX IDs if they've changed
                        if (fighterPackage.FighterInfo.KirbySoundbankId != null && fighterPackage.FighterInfo.OriginalKirbySoundbankId != null &&
                            fighterPackage.FighterInfo.KirbySoundbankId != fighterPackage.FighterInfo.OriginalKirbySoundbankId)
                        {
                            UpdateSFXIds(file, (int)fighterPackage.FighterInfo.KirbySoundbankId, (int)fighterPackage.FighterInfo.OriginalKirbySoundbankId);
                        }
                    }
                }
            }
            foreach(var costume in costumes)
            {
                foreach(var pacFile in costume.PacFiles)
                {
                    var file = _fileService.OpenFile(pacFile.FilePath);
                    var name = pacFile.Prefix + fighterInfo.InternalName + pacFile.Suffix + costume.CostumeId.ToString("D2") + ".pac";
                    // Update GFX if they are per-costume
                    var efNode = file.Children.FirstOrDefault(x => x.Name.StartsWith("ef_") && x.GetType() == typeof(ARCNode)
                    && ((ARCNode)x).FileType == ARCFileType.EffectData && Regex.Match(x.Name, ".+[X]\\d{2}$").Success);
                    if (efNode != null)
                    {
                        // Update Effect.pac name
                        var effectPacName = string.Empty;
                        // Differentiate between Kirby and non-Kirby Effect.pac IDs to ensure everything is named properly
                        if (pacFile.Prefix.ToLower() != "fitkirby")
                        {
                            effectPacName = GetEffectPacName(fighterPackage.FighterInfo.EffectPacId);
                        }
                        else
                        {
                            effectPacName = GetEffectPacName(fighterPackage.FighterInfo.KirbyEffectPacId);
                        }
                        efNode.Name = effectPacName + "X" + costume.CostumeId.ToString("D2");
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
                        files.Add((file, name, pacFile));
                }
            }
            // Set all file paths
            foreach(var file in files)
            {
                var folder = file.name.Contains("Kirby") ? "kirby" : fighterInfo.InternalName;
                folder += $"\\{file.pacFile.Subdirectory}";
                if (string.IsNullOrEmpty(file.pacFile.SavePath))
                {
                    file.node._origPath = $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\{folder}\\{file.name}";
                }
                else
                {
                    file.node._origPath = file.pacFile.SavePath;
                    file.pacFile.SavePath = string.Empty;
                }
                file.pacFile.FilePath = file.node._origPath;
            }
            return files.Select(x => x.node).ToList();
        }

        /// <summary>
        /// Get Effect.pac name from ID
        /// </summary>
        /// <param name="effectPacId">EFfect.pac ID</param>
        /// <returns>Name of Effect.pac</returns>
        private string GetEffectPacName(int? effectPacId)
        {
            // Check dictionary for Effect.pac
            if (effectPacId != null)
            {
                var effectPacEntry = EffectPacs.FighterEffectPacs.FirstOrDefault(x => x.Value == effectPacId);
                if (effectPacEntry.Key != null)
                {
                    return effectPacEntry.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// Update Effect.pac
        /// </summary>
        /// <param name="rootNode">Root node of Effect.pac</param>
        /// <param name="effectPacId">Effect.pac ID to update to</param>
        /// <param name="oldEffectPacId">Effect.pac ID to replace</param>
        private void UpdateEffectPac(ResourceNode rootNode, int effectPacId, int oldEffectPacId)
        {
            // Update GFX IDs
            UpdateGFXIds(rootNode, effectPacId, oldEffectPacId);

            // Find and update effect ARC
            var effectPac = GetEffectPacNode(rootNode);
            if (effectPac != null)
            {
                effectPac.Name = GetEffectPacName(effectPacId);
            }

            // If custom Effect.pac ID , find trace textures and rename them
            if (effectPacId >= 311 && oldEffectPacId >= 311)
            {
                var nodes = rootNode.GetChildrenRecursive();
                foreach (var node in nodes)
                {
                    var regex = Regex.Match(node.Name, "TexCustom\\d+Trace\\d+");
                    if (node.GetType() == typeof(TEX0Node) && regex.Success)
                    {
                        var newTraceId = effectPacId - 311;
                        var newTraceName = Regex.Replace(node.Name, "TexCustom\\d+Trace", $"Tex{newTraceId:X2}Trace");
                        node.Name = newTraceName;
                    }
                }
            }
        }

        /// <summary>
        /// Update GFX IDs for fighter PAC file
        /// </summary>
        /// <param name="rootNode">Root node of fighter PAC file</param>
        /// <param name="effectPacId">New Effect.pac ID</param>
        /// <param name="oldEffectPacId">Old Effect.pac ID</param>
        private void UpdateGFXIds(ResourceNode rootNode, int effectPacId, int oldEffectPacId)
        {
            // Find moveset data node
            var dataNode = rootNode.FindChild("Misc Data [0]");
            if (dataNode != null)
            {
                _psaService.UpdateGFXIds(dataNode, effectPacId, oldEffectPacId);
            }
        }

        /// <summary>
        /// Update SFX IDs for fighter PAC file
        /// </summary>
        /// <param name="rootNode">Root node of fighter PAC file</param>
        /// <param name="soundbankId">New soundbank ID</param>
        /// <param name="oldSoundbankId">Old soundbank ID</param>
        private void UpdateSFXIds(ResourceNode rootNode, int soundbankId, int oldSoundbankId)
        {
            // Find moveset data node
            var dataNode = rootNode.FindChild("Misc Data [0]");
            if (dataNode != null)
            {
                _psaService.UpdateSFXIds(dataNode, soundbankId, oldSoundbankId);
            }
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
        }

        /// <summary>
        /// Get all PAC files associated with fighter
        /// </summary>
        /// <param name="internalName">Internal name of fighter</param>
        /// <param name="removeCostumeId">Remove costume ID from pac name</param>
        /// <returns>List of PAC files</returns>
        public List<FighterPacFile> GetPacFiles(string internalName, bool removeCostumeId = true)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var settings = _settingsService.BuildSettings;
            var path = $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\{internalName}";
            var files = GetFighterPacFiles(internalName, path, removeCostumeId);
            var folders = _fileService.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            // Get files from subdirectories
            foreach (var folder in folders)
            {
                files.AddRange(GetFighterPacFiles(internalName, folder, removeCostumeId));
            }
            return files;
        }

        /// <summary>
        /// Get Kirby PAC files associated with fighter
        /// </summary>
        /// <param name="internalName">Internal name of fighter</param>
        /// <param name="removeCostumeId">Remove costume ID from pac name</param>
        /// <returns>List of Kirby PAC files</returns>
        public List<FighterPacFile> GetKirbyFiles(string internalName, bool removeCostumeId=true)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var settings = _settingsService.BuildSettings;
            var path = $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\kirby";
            var files = new List<FighterPacFile>();
            foreach (var file in _fileService.GetFiles(path, "*.pac").Where(x => Path.GetFileName(x).StartsWith($"FitKirby{internalName}", StringComparison.InvariantCultureIgnoreCase)).ToList())
            {
                var pacFile = GetFighterPacFile(file, internalName, internalName, removeCostumeId);
                if (pacFile != null)
                {
                    files.Add(pacFile);
                }
            }
            return files;
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
                _fileService.DeleteFile(file.FilePath);
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
                _fileService.DeleteFile(file.FilePath);
            }
        }

        /// <summary>
        /// Get Kirby and fighter PAC files for fighter
        /// </summary>
        /// <param name="name">Internal name of fighter</param>
        /// <param name="path">Path to retrive files from</param>
        /// <param name="removeCostumeId">Remove costume ID from pac names</param>
        /// <returns>List of PAC files</returns>
        private List<FighterPacFile> GetFighterPacFiles(string name, string path, bool removeCostumeId = true)
        {
            var files = new List<FighterPacFile>();
            foreach (var file in _fileService.GetFiles(path, "*.pac").Where(x => Path.GetFileName(x).StartsWith($"Fit{name}", StringComparison.InvariantCultureIgnoreCase) 
            || Path.GetFileName(x).StartsWith($"Itm{name}", StringComparison.InvariantCultureIgnoreCase)).ToList())
            {
                var pacFile = GetFighterPacFile(file, name, name, removeCostumeId);
                if (pacFile != null)
                {
                    files.Add(pacFile);
                }
            }
            return files;
        }

        /// <summary>
        /// Convert Kirby hat XML to Kirby hat data
        /// </summary>
        /// <param name="xmlPath">Path to XML file</param>
        /// <returns>Parsed Kirby hat data</returns>
        public HatInfoPack ConvertXMLToKirbyHatData(string xmlPath)
        {
            // Parse XML
            var dictionary = new SortedDictionary<uint, HatInfoPack>();
            HatXMLParser.parseHatsFromXML(xmlPath, dictionary);
            // Return first item
            return dictionary.FirstOrDefault().Value;
        }

        /// <summary>
        /// Get Kirby hat data for fighter
        /// </summary>
        /// <param name="fighterId">Fighter ID</param>
        /// <returns>Hat data</returns>
        private HatInfoPack GetKirbyHatData(int? fighterId)
        {
            // Open rel
            HatInfoPack hatData = null;
            if (_settingsService.BuildSettings.MiscSettings.InstallKirbyHats && fighterId != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var modulePath = _settingsService.BuildSettings.FilePathSettings.Modules;
                var path = Path.Combine(buildPath, modulePath, "ft_kirby.rel");
                var rootNode = _fileService.OpenFile(path);
                if (rootNode != null)
                {
                    // Load hat entries
                    var hatManager = new KirbyHatManager();
                    var result = hatManager.loadHatEntriesFromREL((RELNode)rootNode);
                    if (result)
                    {
                        // Check if hat exists
                        var foundHat = hatManager.fighterIDToInfoPacks.ContainsKey((uint)fighterId);
                        if (foundHat)
                        {
                            // Load hat data
                            hatData = hatManager.fighterIDToInfoPacks[(uint)fighterId];
                        }
                    }
                    _fileService.CloseFile(rootNode);
                }
            }
            return hatData;
        }

        /// <summary>
        /// Update Kirby hat data for specified fighter
        /// </summary>
        /// <param name="kirbyHatData">Kirby hat data to use. If null, hat will be removed.</param>
        /// <param name="fighterId">Fighter ID to update</param>
        private void UpdateKirbyHatData(HatInfoPack kirbyHatData, int? fighterId)
        {
            if (_settingsService.BuildSettings.MiscSettings.InstallKirbyHats && fighterId != null)
            {
                // Open rel
                var buildPath = _settingsService.AppSettings.BuildPath;
                var modulePath = _settingsService.BuildSettings.FilePathSettings.Modules;
                var path = Path.Combine(buildPath, modulePath, "ft_kirby.rel");
                var rootNode = _fileService.OpenFile(path);
                if (rootNode != null)
                {
                    // Load hat entries
                    var hatManager = new KirbyHatManager();
                    var result = hatManager.loadHatEntriesFromREL((RELNode)rootNode);
                    if (result)
                    {
                        // Search for hat
                        var foundHat = hatManager.fighterIDToInfoPacks.ContainsKey((uint)fighterId);
                        if (foundHat)
                        {
                            // Replace hat if it exists and new one is provided
                            if (kirbyHatData != null)
                            {
                                hatManager.fighterIDToInfoPacks[(uint)fighterId] = kirbyHatData;
                            }
                            // If hat passed is null, remove existing hat
                            else
                            {
                                hatManager.eraseHat((uint)fighterId);
                            }
                        }
                        // If hat doesn't exist and is not null, add new one
                        else if (kirbyHatData != null)
                        {
                            hatManager.fighterIDToInfoPacks.Add((uint)fighterId, kirbyHatData);
                        }
                        // Write to rel
                        hatManager.writeTablesToREL((RELNode)rootNode);
                    }
                    _fileService.SaveFile(rootNode);
                    _fileService.CloseFile(rootNode);
                }
            }
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
            var pacFiles = UpdatePacFiles(fighterPackage);
            var module = _fileService.OpenFile(fighterPackage.Module);
            var masquerade = _fileService.OpenFile(fighterPackage.FighterInfo.Masquerade);
            var soundbank = _fileService.OpenFile(fighterPackage.Soundbank);
            var kirbySoundbank = _fileService.OpenFile(fighterPackage.KirbySoundbank);
            var victoryTheme = _fileService.OpenFile(fighterPackage.VictoryTheme.SongFile);
            var creditsTheme = _fileService.OpenFile(fighterPackage.CreditsTheme.SongFile);
            var classicIntro = _fileService.OpenFile(fighterPackage.ClassicIntro);
            var endingFiles = new List<ResourceNode>();
            foreach(var endingFile in fighterPackage.EndingPacFiles)
            {
                var file = _fileService.OpenFile(endingFile);
                if (file != null)
                {
                    endingFiles.Add(file);
                }
            }
            var endingMovie = _fileService.OpenFile(fighterPackage.EndingMovie);
            // Delete old files
            RemovePacFiles(oldFighter.FighterInfo.InternalName);
            DeleteModule(oldFighter.FighterInfo.InternalName);
            DeleteExConfigs(oldFighter.FighterInfo);
            DeleteSoundbank(oldFighter.FighterInfo.SoundbankId);
            DeleteSoundbank(oldFighter.FighterInfo.KirbySoundbankId);
            DeleteClassicIntro(oldFighter.FighterInfo.Ids.CosmeticId);
            DeleteEndingPacFiles(oldFighter.FighterInfo.EndingId);
            DeleteEndingMovie(oldFighter.FighterInfo.InternalName);
            _fileService.DeleteFile(oldFighter.FighterInfo.Masquerade);
            // Delete some files only if user chose to
            if (fighterPackage.FighterDeleteOptions.DeleteVictoryTheme)
            {
                DeleteVictoryTheme(oldFighter.VictoryTheme.SongId);
            }
            if (fighterPackage.FighterDeleteOptions.DeleteCreditsTheme)
            {
                DeleteCreditsTheme(oldFighter.CreditsTheme.SongId);
            }
            // Import pac files
            foreach(var pacFile in pacFiles)
            {
                _fileService.SaveFile(pacFile);
                _fileService.CloseFile(pacFile);
            }
            // Update and import ex configs
            var updateLinks = fighterPackage.PackageType == PackageType.New;
            var cssSlotConfig = fighterPackage.FighterInfo.CSSSlotAttributes?.ToCSSCNode(fighterPackage.FighterInfo, updateLinks);
            if (cssSlotConfig != null)
            {
                cssSlotConfig = UpdateCostumeConfig(cssSlotConfig, fighterPackage.Costumes);
                var configPath = $"{buildPath}\\{settings.FilePathSettings.BrawlEx}\\CSSSlotConfig\\CSSSlot{fighterPackage.FighterInfo.Ids.CSSSlotConfigId:X2}.dat";
                _fileService.SaveFileAs(cssSlotConfig, configPath);
                _fileService.CloseFile(cssSlotConfig);
            }
            var fighterConfig = fighterPackage.FighterInfo.FighterAttributes?.ToFCFGNode(fighterPackage.FighterInfo);
            if (fighterConfig != null)
            {
                var configPath = $"{buildPath}\\{settings.FilePathSettings.BrawlEx}\\FighterConfig\\Fighter{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}.dat";
                _fileService.SaveFileAs(fighterConfig, configPath);
                _fileService.CloseFile(fighterConfig);
            }
            var slotConfig = fighterPackage.FighterInfo.SlotAttributes?.ToSLTCNode(fighterPackage.FighterInfo, updateLinks);
            if (slotConfig != null)
            {
                var configPath = $"{buildPath}\\{settings.FilePathSettings.BrawlEx}\\SlotConfig\\Slot{fighterPackage.FighterInfo.Ids.SlotConfigId:X2}.dat";
                _fileService.SaveFileAs(slotConfig, configPath);
                _fileService.CloseFile(slotConfig);
            }
            var cosmeticConfig = fighterPackage.FighterInfo.CosmeticAttributes?.ToCOSCNode(fighterPackage.FighterInfo, updateLinks);
            if (cosmeticConfig != null)
            {
                var configPath = $"{buildPath}\\{settings.FilePathSettings.BrawlEx}\\CosmeticConfig\\Cosmetic{fighterPackage.FighterInfo.Ids.CosmeticConfigId:X2}.dat";
                _fileService.SaveFileAs(cosmeticConfig, configPath);
                _fileService.CloseFile(cosmeticConfig);
            }
            // Update and import masquerade
            if (masquerade != null)
            {
                masquerade = UpdateCostumeConfig((MasqueradeNode)masquerade, fighterPackage.Costumes);
                var configPath = $"{buildPath}\\{settings.FilePathSettings.MasqueradePath}\\{fighterPackage.FighterInfo.Ids.MasqueradeId:D2}.masq";
                _fileService.SaveFileAs(masquerade, configPath);
                _fileService.CloseFile(masquerade);
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
                var name = GetSoundbankName(fighterPackage.FighterInfo.SoundbankId);
                soundbank._origPath = Path.Combine(_settingsService.AppSettings.BuildPath, _settingsService.BuildSettings.FilePathSettings.SoundbankPath, name);
                _fileService.SaveFile(soundbank);
                _fileService.CloseFile(soundbank);
            }
            // Rename and import Kirby soundbank
            if (kirbySoundbank != null)
            {
                var name = GetSoundbankName(fighterPackage.FighterInfo.KirbySoundbankId);
                kirbySoundbank._origPath = Path.Combine(_settingsService.AppSettings.BuildPath, _settingsService.BuildSettings.FilePathSettings.SoundbankPath, name);
                _fileService.SaveFile(kirbySoundbank);
                _fileService.CloseFile(kirbySoundbank);
            }
            // Import victory theme
            fighterPackage.FighterInfo.VictoryThemeId = ImportVictoryTheme(fighterPackage.VictoryTheme, victoryTheme);
            // Import credits theme
            fighterPackage.FighterInfo.CreditsThemeId = ImportCreditsTheme(fighterPackage.CreditsTheme, creditsTheme, fighterPackage.FighterInfo);
            // Import classic intro
            fighterPackage.ClassicIntro = ImportClassicIntro(classicIntro, fighterPackage.FighterInfo.Ids.CosmeticId);
            // Import ending files
            fighterPackage.FighterInfo.EndingId = ImportEndingPacFiles(endingFiles, fighterPackage);
            // Import ending movie
            fighterPackage.EndingMovie = ImportEndingMovie(endingMovie, fighterPackage.FighterInfo.InternalName);
        }

        /// <summary>
        /// Update fighter settings in build
        /// </summary>
        /// <param name="fighterPackage">Fighter package to update settings for</param>
        public void UpdateFighterSettings(FighterPackage fighterPackage)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var fighterSettings = fighterPackage.FighterSettings;

            // Update Kirby hat
            UpdateKirbyHatData(fighterSettings.KirbyHatData, fighterPackage.FighterInfo.Ids.FighterConfigId);

            // Update throw release point
            UpdateThrowReleaseTable(fighterPackage.FighterInfo, fighterPackage.FighterSettings.ThrowReleasePoint);

            // Update L-Load
            UpdateLLoadTable(fighterPackage);

            // Update Slot Ex table
            UpdateExSlotsTable(fighterPackage);

            // Update SSE settings
            UpdateSSEModule(fighterPackage);

            // Update fighter specific settings
            UpdateFighterSpecificSettings(fighterPackage);
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
        /// <param name="rootNode">Root node of file</param>
        /// <param name="costumes">Costumes to place in config</param>
        private CSSCNode UpdateCostumeConfig(CSSCNode rootNode, List<Costume> costumes)
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
        /// Update Masquerade for fighter
        /// </summary>
        /// <param name="rootNode">Root node of file</param>
        /// <param name="costumes">Costumes to place in config</param>
        private ResourceNode UpdateCostumeConfig(MasqueradeNode rootNode, List<Costume> costumes)
        {
            if (rootNode != null)
            {
                rootNode.Children.RemoveAll(x => x != null);
                foreach (var costume in costumes)
                {
                    var newNode = new MasqueradeEntryNode
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
        /// Get fighter costumes
        /// </summary>
        /// <param name="fighterInfo">Fighter info</param>
        /// <returns>List of costumes</returns>
        public List<Costume> GetFighterCostumes(FighterInfo fighterInfo)
        {
            var costumes = new List<Costume>();
            var buildPath = _settingsService.AppSettings.BuildPath;
            var settings = _settingsService.BuildSettings;
            var configPath = string.Empty;
            Type entryType = null;
            if (!string.IsNullOrEmpty(fighterInfo.CSSSlotConfig))
            {
                
                configPath = fighterInfo.CSSSlotConfig;
                entryType = typeof(CSSCEntryNode);
            }
            else if (!string.IsNullOrEmpty(fighterInfo.Masquerade))
            {
                configPath = fighterInfo.Masquerade;
                entryType = typeof(MasqueradeEntryNode);
            }
            if (entryType != null)
            {
                var fighterPath = $"{buildPath}\\{settings.FilePathSettings.FighterFiles}\\{fighterInfo.InternalName}";
                ResourceNode rootNode = null;

                rootNode = _fileService.OpenFile(configPath);
                if (rootNode != null)
                {
                    foreach (var entry in rootNode.Children)
                    {
                        var costume = new Costume
                        {
                            Color = entryType == typeof(CSSCEntryNode) ? ((CSSCEntryNode)entry).Color : ((MasqueradeEntryNode)entry).Color,
                            CostumeId = entryType == typeof(CSSCEntryNode) ? ((CSSCEntryNode)entry).CostumeID : ((MasqueradeEntryNode)entry).CostumeID
                        };
                        if (_fileService.DirectoryExists(fighterPath))
                        {
                            costume.PacFiles = GetPacFiles(fighterInfo.InternalName)
                                .Where(x => Path.GetFileNameWithoutExtension(x.FilePath).EndsWith(costume.CostumeId.ToString("D2"))).ToList();
                            costume.PacFiles.AddRange(GetKirbyFiles(fighterInfo.InternalName)
                                .Where(x => Path.GetFileNameWithoutExtension(x.FilePath).EndsWith(costume.CostumeId.ToString("D2"))).ToList());
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
            fighterPackage.PacFiles = GetPacFiles(fighterInfo.InternalName, false)?.Where(x => !costumes.SelectMany(y => y.PacFiles).Select(z => z.FilePath).Contains(x.FilePath)).ToList();
            fighterPackage.PacFiles.AddRange(GetKirbyFiles(fighterInfo.InternalName, false)?.Where(x => !costumes.SelectMany(y => y?.PacFiles).Select(z => z?.FilePath).Contains(x?.FilePath)).ToList());
            fighterPackage.Module = GetModule(fighterInfo.InternalName);

            // Set fighter info
            if (fighterInfo.Masquerade != "")
                fighterPackage.MasqueradeFile = fighterInfo.Masquerade;

            // Get soundbank
            fighterPackage.Soundbank = GetSoundbank(fighterInfo.SoundbankId);

            // Get Kirby soundbank
            fighterPackage.KirbySoundbank = GetSoundbank(fighterInfo.KirbySoundbankId);

            // Get victory theme
            fighterPackage.VictoryTheme = GetVictoryTheme(fighterInfo.VictoryThemeId);

            // Get credits theme
            fighterPackage.CreditsTheme = GetCreditsTheme(fighterInfo.Ids.SlotConfigId);

            // Get classic intro
            fighterPackage.ClassicIntro = GetClassicIntro(fighterInfo.Ids.CosmeticId);

            // Get ending files
            fighterPackage.EndingPacFiles = GetEndingPacFiles(fighterInfo.EndingId);

            // Get ending movie
            fighterPackage.EndingMovie = GetEndingMovie(fighterInfo.InternalName);

            fighterPackage.Costumes = costumes;
            fighterPackage.FighterInfo = fighterInfo;

            return fighterPackage;
        }

        /// <summary>
        /// Get all fighter setting data for fighter
        /// </summary>
        /// <param name="fighterPackage">Fighter package to retrieve settings for</param>
        /// <returns></returns>
        public FighterSettings GetFighterSettings(FighterPackage fighterPackage)
        {
            var fighterSettings = fighterPackage.FighterSettings;

            // Get Kirby hat data
            fighterSettings.KirbyHatData = GetKirbyHatData(fighterPackage.FighterInfo.Ids.FighterConfigId);

            // Get throw release point
            fighterSettings.ThrowReleasePoint = GetThrowReleasePoint(fighterPackage.FighterInfo.Ids.FighterConfigId);

            // Get L-load
            fighterSettings.LLoadCharacterId = GetLLoad(fighterPackage.FighterInfo.Ids.CSSSlotConfigId);

            // Get Ex slots
            fighterSettings.ExSlotIds = GetExSlots(fighterPackage.FighterInfo.Ids.CSSSlotConfigId);

            // Get SSE settings
            fighterPackage = GetSSESettings(fighterPackage);

            // Get fighter-specific settings
            fighterSettings = GetFighterSpecificSettings(fighterPackage);

            return fighterSettings;
        }

        /// <summary>
        /// Get throw release point by fighter ID
        /// </summary>
        /// <param name="fighterId">Fighter ID</param>
        /// <returns>Throw release point</returns>
        private Position GetThrowReleasePoint(int? fighterId)
        {
            var throwRelease = new Position(0.0, 0.0);
            if (fighterId != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var asmPath = _settingsService.BuildSettings.FilePathSettings.ThrowReleaseAsmFile;
                var codePath = Path.Combine(buildPath, asmPath);
                var code = _codeService.ReadCode(codePath);
                var table = _codeService.ReadTable(code, "ThrowReleaseTable:");
                if (table.Count > fighterId * 2)
                {
                    var x = table[fighterId.Value * 2];
                    var y = "0.0";
                    if (table.Count > (fighterId * 2) + 1)
                    {
                        y = table[(fighterId.Value * 2) + 1];
                    }
                    throwRelease.X = Convert.ToDouble(x);
                    throwRelease.Y = Convert.ToDouble(y);
                }
            }
            return throwRelease;
        }

        /// <summary>
        /// Get Ex slots for fighter
        /// </summary>
        /// <param name="cssSlotId">CSS slot ID of fighter</param>
        /// <returns>Ex slot IDs for fighter</returns>
        private List<uint> GetExSlots(int? cssSlotId)
        {
            var exSlots = new List<uint>();
            if (cssSlotId != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var asmPath = _settingsService.BuildSettings.FilePathSettings.SlotExAsmFile;
                var codePath = Path.Combine(buildPath, asmPath);
                var code = _codeService.ReadCode(codePath);
                var table = _codeService.ReadTable(code, "Table:");
                if (table.Count > cssSlotId)
                {
                    var fullString = table[cssSlotId.Value].Replace("0x", "");
                    exSlots = Enumerable.Range(0, fullString.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToUInt32(fullString.Substring(x, 2), 16))
                         .ToList();
                }
            }
            return exSlots;
        }

        /// <summary>
        /// Get L-load ID for fighter
        /// </summary>
        /// <param name="cssSlotId">CSS slot ID of fighter</param>
        /// <returns>CSS slot ID of fighter L-load</returns>
        private int? GetLLoad(int? cssSlotId)
        {
            int? id = null;
            if (cssSlotId != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var asmPath = _settingsService.BuildSettings.FilePathSettings.LLoadAsmFile;
                var codePath = Path.Combine(buildPath, asmPath);
                var code = _codeService.ReadCode(codePath);
                var table = _codeService.ReadTable(code, ".GOTO->Table_Skip");
                if (table.Count > cssSlotId)
                {
                    id = Convert.ToInt32(table[cssSlotId.Value].Replace("0x", ""), 16);
                }
            }
            return id;
        }

        /// <summary>
        /// Update SlotEx table for fighter
        /// </summary>
        /// <param name="fighterPackage">Fighter package to update</param>
        private void UpdateExSlotsTable(FighterPackage fighterPackage)
        {
            var cssSlotId = fighterPackage.FighterInfo.Ids.CSSSlotConfigId;
            if (cssSlotId != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var asmPath = _settingsService.BuildSettings.FilePathSettings.SlotExAsmFile;
                var codePath = Path.Combine(buildPath, asmPath);
                var code = _codeService.ReadCode(codePath);
                var table = _codeService.ReadTable(code, "Table:");
                // Convert to ASM table
                var fighterInfoTable = _settingsService.FighterInfoList;
                var asmTable = new List<AsmTableEntry>();
                foreach (var entry in table)
                {
                    var comment = fighterInfoTable.FirstOrDefault(x => x.Ids.CSSSlotConfigId == asmTable.Count)?.DisplayName;
                    var newEntry = new AsmTableEntry
                    {
                        Item = entry,
                        Comment = !string.IsNullOrEmpty(comment) ? comment : "Unknown"
                    };
                    asmTable.Add(newEntry);
                }
                // Update fighter slot
                if (asmTable.Count > cssSlotId + 1)
                {
                    var exSlots = fighterPackage.FighterSettings.ExSlotIds;
                    asmTable[cssSlotId.Value].Item = $"0x{exSlots[0]:X2}{exSlots[1]:X2}{exSlots[2]:X2}{exSlots[3]:X2}";
                    asmTable[cssSlotId.Value].Comment = fighterPackage.FighterInfo.DisplayName;
                }
                // Write table
                code = _codeService.ReplaceTable(code, "Table:", asmTable, DataSize.Word, 4);
                _fileService.SaveTextFile(codePath, code);
            }
        }

        /// <summary>
        /// Update L-load table for fighter
        /// </summary>
        /// <param name="fighterPackage">Fighter package to update L-load table for</param>
        private void UpdateLLoadTable(FighterPackage fighterPackage)
        {
            var cssSlotId = fighterPackage.FighterInfo.Ids.CSSSlotConfigId;
            if (cssSlotId != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var asmPath = _settingsService.BuildSettings.FilePathSettings.LLoadAsmFile;
                var codePath = Path.Combine(buildPath, asmPath);
                var code = _codeService.ReadCode(codePath);
                var table = _codeService.ReadTable(code, ".GOTO->Table_Skip");
                // Convert to ASM table
                var fighterInfoTable = _settingsService.FighterInfoList;
                var asmTable = new List<AsmTableEntry>();
                foreach (var entry in table)
                {
                    var comment = fighterInfoTable.FirstOrDefault(x => x.Ids.CSSSlotConfigId == asmTable.Count)?.DisplayName;
                    var newEntry = new AsmTableEntry
                    {
                        Item = entry,
                        Comment = !string.IsNullOrEmpty(comment) ? comment : "Unknown"
                    };
                    asmTable.Add(newEntry);
                }
                // Update fighter slot
                if (asmTable.Count > cssSlotId + 1)
                {
                    asmTable[cssSlotId.Value].Item = $"0x{fighterPackage.FighterSettings.LLoadCharacterId:X2}";
                    asmTable[cssSlotId.Value].Comment = fighterPackage.FighterInfo.DisplayName;
                }
                // Write table
                code = _codeService.ReplaceTable(code, ".GOTO->Table_Skip", asmTable, DataSize.Byte, 4);
                _fileService.SaveTextFile(codePath, code);
            }
        }

        /// <summary>
        /// Update throw release point for fighter
        /// </summary>
        /// <param name="fighterInfo">Fighter info for fighter</param>
        /// <param name="throwReleasePoint">Throw release point to update</param>
        private void UpdateThrowReleaseTable(FighterInfo fighterInfo, Position throwReleasePoint)
        {
            var fighterId = fighterInfo.Ids.FighterConfigId;
            if (fighterId != null)
            {
                // Get table
                var buildPath = _settingsService.AppSettings.BuildPath;
                var asmPath = _settingsService.BuildSettings.FilePathSettings.ThrowReleaseAsmFile;
                var codePath = Path.Combine(buildPath, asmPath);
                var code = _codeService.ReadCode(codePath);
                var table = _codeService.ReadTable(code, "ThrowReleaseTable:");
                // Convert to ASM table
                var fighterInfoTable = _settingsService.FighterInfoList;
                var asmTable = new List<AsmTableEntry>();
                foreach (var entry in table)
                {
                    var comment = fighterInfoTable.FirstOrDefault(x => x.Ids.FighterConfigId * 2 == asmTable.Count)?.DisplayName;
                    var newEntry = new AsmTableEntry
                    {
                        Item = entry,
                        Comment = !string.IsNullOrEmpty(comment) ? comment : asmTable.Count % 2 == 0 ? "Unknown" : string.Empty
                    };
                    asmTable.Add(newEntry);
                }
                // Update fighter slot
                if (asmTable.Count > (fighterId * 2) + 1)
                {
                    var x = $"{throwReleasePoint.X}";
                    x = x.Contains(".") ? x : x += ".0";
                    asmTable[fighterId.Value * 2].Item = x;
                    asmTable[fighterId.Value * 2].Comment = fighterInfo.DisplayName;
                    var y = $"{throwReleasePoint.Y}";
                    y = y.Contains(".") ? y : y += ".0";
                    asmTable[(fighterId.Value * 2) + 1].Item = y;
                }
                // Write table
                code = _codeService.ReplaceTable(code, "ThrowReleaseTable:", asmTable, DataSize.Float, 2, 12);
                _fileService.SaveTextFile(codePath, code);
            }
        }

        /// <summary>
        /// Get ending movie for fighter
        /// </summary>
        /// <param name="internalName">Internal fighter name</param>
        /// <returns>Ending movie path</returns>
        private string GetEndingMovie(string internalName)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var moviePath = _settingsService.BuildSettings.FilePathSettings.MoviePath;
            var path = Path.Combine(buildPath, moviePath, $"End_{internalName}.thp");
            if (_fileService.FileExists(path))
            {
                return path;
            }
            return null;
        }

        /// <summary>
        /// Delete ending movie for fighter
        /// </summary>
        /// <param name="internalName">Internal name of fighter</param>
        private void DeleteEndingMovie(string internalName)
        {
            var file = GetEndingMovie(internalName);
            if (file != null)
            {
                _fileService.DeleteFile(file);
            }
        }

        /// <summary>
        /// Import ending movie for fighter
        /// </summary>
        /// <param name="rootNode">Root node of movie file</param>
        /// <param name="internalName">Internal name of fighter</param>
        private string ImportEndingMovie(ResourceNode rootNode, string internalName)
        {
            if (rootNode != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var moviePath = _settingsService.BuildSettings.FilePathSettings.MoviePath;
                var path = Path.Combine(buildPath, moviePath, $"End_{internalName}.thp");
                rootNode._origPath = path;
                _fileService.SaveFile(rootNode);
                _fileService.CloseFile(rootNode);
                return path;
            }
            return null;
        }

        /// <summary>
        /// Get ending ID for fighter
        /// </summary>
        /// <param name="cosmeticConfigId">Cosmetic config ID of fighter</param>
        /// <returns>Ending ID</returns>
        private int GetEndingId(int? cosmeticConfigId)
        {
            if (cosmeticConfigId != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var endingAsm = _settingsService.BuildSettings.FilePathSettings.EndingAsmFile;
                var path = Path.Combine(buildPath, endingAsm);
                var code = _codeService.ReadCode(path);
                var table = _codeService.ReadTable(code, "ENDINGTABLE:");
                if (table.Count > cosmeticConfigId)
                {
                    var result = int.TryParse(table[cosmeticConfigId.Value], out int endingId);
                    if (result)
                    {
                        return endingId;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Get ending pac files for fighter
        /// </summary>
        /// <param name="endingId">Ending ID for fighter</param>
        /// <returns>List of ending pac files</returns>
        private List<string> GetEndingPacFiles(int endingId)
        {
            var pacFiles = new List<string>();
            var buildPath = _settingsService.AppSettings.BuildPath;
            var endingPath = _settingsService.BuildSettings.FilePathSettings.EndingPath;
            var path = Path.Combine(buildPath, endingPath);
            if (_fileService.DirectoryExists(path))
            {
                pacFiles = _fileService.GetFiles(path, $"EndingAll{endingId:D2}.pac").ToList();
                pacFiles.AddRange(_fileService.GetFiles(path, $"EndingSimple{endingId:D2}.pac").ToList());
            }
            return pacFiles;
        }

        /// <summary>
        /// Delete ending pac files for fighter
        /// </summary>
        /// <param name="endingId">Ending ID for fighter</param>
        private void DeleteEndingPacFiles(int endingId)
        {
            var files = GetEndingPacFiles(endingId);
            foreach(var file in files)
            {
                _fileService.DeleteFile(file);
            }
        }

        /// <summary>
        /// Import ending pac files for fighter
        /// </summary>
        /// <param name="endingPacFiles">Opened files to import</param>
        /// <param name="fighterPackage">Fighter package to use</param>
        /// <returns>New ending ID</returns>
        private int ImportEndingPacFiles(List<ResourceNode> endingPacFiles, FighterPackage fighterPackage)
        {
            var fighterInfo = fighterPackage.FighterInfo;
            var cosmeticConfigId = fighterInfo.Ids.CosmeticConfigId;
            var endingId = fighterInfo.EndingId;
            if (cosmeticConfigId != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var endingAsm = _settingsService.BuildSettings.FilePathSettings.EndingAsmFile;
                var path = Path.Combine(buildPath, endingAsm);
                var code = _codeService.ReadCode(path);
                var table = _codeService.ReadTable(code, "ENDINGTABLE:");
                // Convert to AsmTable
                var fighterInfoTable = _settingsService.FighterInfoList;
                var asmTable = new List<AsmTableEntry>();
                foreach (var entry in table)
                {
                    var comment = fighterInfoTable.FirstOrDefault(x => x.Ids.CosmeticConfigId == asmTable.Count)?.DisplayName;
                    var newEntry = new AsmTableEntry
                    {
                        Item = entry,
                        Comment = !string.IsNullOrEmpty(comment) ? comment : "Unknown"
                    };
                    asmTable.Add(newEntry);
                }
                // Update ending ID if it's already used
                if (asmTable.Where(x => asmTable.IndexOf(x) != cosmeticConfigId).Any(x => int.Parse(x.Item) == endingId) && 
                    fighterPackage.PackageType != PackageType.Delete)
                {
                    while (asmTable.Where(x => asmTable.IndexOf(x) != cosmeticConfigId).Any(x => int.Parse(x.Item) == endingId))
                    {
                        endingId++;
                    }
                    fighterInfo.EndingId = endingId;
                }
                // If fighter is being deleted, set to -1
                else if (fighterPackage.PackageType == PackageType.Delete)
                {
                    fighterInfo.EndingId = -1;
                }
                // Update fighter slot
                if (asmTable.Count > cosmeticConfigId)
                {
                    asmTable[cosmeticConfigId.Value].Item = $"{fighterInfo.EndingId:D}";
                    asmTable[cosmeticConfigId.Value].Comment = fighterInfo.DisplayName;
                }
                // Write table
                code = _codeService.ReplaceTable(code, "ENDINGTABLE:", asmTable, DataSize.Byte, 8);
                _fileService.SaveTextFile(path, code);
                // Update and import pac files
                if (endingPacFiles.Count > 0)
                {
                    var endingPath = _settingsService.BuildSettings.FilePathSettings.EndingPath;
                    var savePath = Path.Combine(buildPath, endingPath);
                    foreach (var file in endingPacFiles)
                    {
                        // Update texture names
                        var texturePrefix = "A";
                        var filePrefix = "All";
                        if (file.RootNode.Name.StartsWith("EndingSimple"))
                        {
                            texturePrefix = "S";
                            filePrefix = "Simple";
                        }
                        var nodes = file.GetChildrenRecursive();
                        foreach (var node in nodes)
                        {
                            var regex = new Regex("MenEndpictures(A|S)\\d{4}");
                            if (!node.Name.EndsWith("0000"))
                            {
                                node.Name = regex.Replace(node.Name, $"MenEndpictures{texturePrefix}{endingId:D4}");
                            }
                        }
                        // Save
                        file._origPath = Path.Combine(savePath, $"Ending{filePrefix}{endingId:D2}.pac");
                        _fileService.SaveFile(file);
                        _fileService.CloseFile(file);
                    }
                }
            }
            return endingId;
        }

        /// <summary>
        /// Get Effect.pac ID for fighter
        /// </summary>
        /// <param name="pacFiles">PAC files to get ID from</param>
        /// <param name="internalName">Internal name of fighter</param>
        /// <returns>Effect.pac ID of fighter, null if ID not found</returns>
        public int? GetFighterEffectPacId(List<FighterPacFile> pacFiles, string internalName)
        {
            int? effectId = null;
            var pacFile = pacFiles.OrderByDescending(x => x.Subdirectory == "ex").FirstOrDefault(x => Path.GetFileNameWithoutExtension(x.FilePath).ToLower() == $"fit{internalName.ToLower()}");
            if (pacFile != null)
            {
                var rootNode = _fileService.OpenFile(pacFile.FilePath);
                if (rootNode != null)
                {
                    var effectNode = GetEffectPacNode(rootNode);
                    if (effectNode != null)
                    {
                        // Check dictionary for Effect.pac ID
                        var result = EffectPacs.FighterEffectPacs.TryGetValue(effectNode.Name, out int id);
                        if (result)
                        {
                            effectId = id;
                        }
                    }
                    _fileService.CloseFile(rootNode);
                }
            }

            return effectId;
        }

        /// <summary>
        /// Get Effect.pac node in fighter PAC file
        /// </summary>
        /// <param name="rootNode">Root node of fighter PAC file</param>
        /// <returns>Effect.pac node</returns>
        private ARCNode GetEffectPacNode(ResourceNode rootNode)
        {
            var node = rootNode.Children.FirstOrDefault(x => x.GetType() == typeof(ARCNode) && ((ARCNode)x).FileType == ARCFileType.EffectData
                    && x.Name.StartsWith("ef_"));
            if (node != null)
            {
                return (ARCNode)node;
            }
            return null;
        }

        /// <summary>
        /// Get classic intro filepath for fighter
        /// </summary>
        /// <param name="cosmeticId">Cosmetic ID of fighter</param>
        /// <returns>Path to classic intro</returns>
        private string GetClassicIntro(int? cosmeticId)
        {
            if (cosmeticId != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var classicPath = _settingsService.BuildSettings.FilePathSettings.ClassicIntroPath;
                var path = Path.Combine(buildPath, classicPath);
                var filePath = Path.Combine(path, $"chr{(cosmeticId + 1):D4}.brres");
                if (_fileService.FileExists(filePath))
                {
                    return filePath;
                }
            }
            return null;
        }

        /// <summary>
        /// Delete classic intro for fighter
        /// </summary>
        /// <param name="cosmeticId">Cosmetic ID of fighter</param>
        private void DeleteClassicIntro(int? cosmeticId)
        {
            var classicIntro = GetClassicIntro(cosmeticId);
            _fileService.DeleteFile(classicIntro);
        }

        /// <summary>
        /// Import classic intro for fighter
        /// </summary>
        /// <param name="rootNode">Root node of opened classic intro file</param>
        /// <param name="cosmeticId">Cosmetic ID of fighter</param>
        private string ImportClassicIntro(ResourceNode rootNode, int? cosmeticId)
        {
            var path = string.Empty;
            if (rootNode != null && cosmeticId != null)
            {
                var id = cosmeticId + 1;
                // Rename nodes
                var allNodes = rootNode.GetChildrenRecursive();
                foreach (var node in allNodes)
                {
                    var regex = new Regex("ItrSimpleChr\\d{4}");
                    if (regex.IsMatch(node.Name))
                    {
                        var name = regex.Replace(node.Name, $"ItrSimpleChr{id:D4}", 1);
                        if (name != node.Name)
                            node.Name = name;
                    }
                    regex = new Regex("GmSimpleChr\\d+");
                    if (regex.IsMatch(node.Name))
                    {
                        var name = regex.Replace(node.Name, $"GmSimpleChr{id:D2}", 1);
                        if (name != node.Name)
                            node.Name = name;
                    }
                    regex = new Regex("GmSimpleChrEy\\d+");
                    if (regex.IsMatch(node.Name))
                    {
                        var name = regex.Replace(node.Name, $"GmSimpleChrEy{id:D2}", 1);
                        if (name != node.Name)
                            node.Name = name;
                    }
                }
                // Save file
                var buildPath = _settingsService.AppSettings.BuildPath;
                var introPath = _settingsService.BuildSettings.FilePathSettings.ClassicIntroPath;
                path = Path.Combine(buildPath, introPath, $"chr{id:D4}.brres");
                rootNode._origPath = path;
                _fileService.SaveFile(rootNode);
                _fileService.CloseFile(rootNode);
            }
            return path;
        }

        /// <summary>
        /// Get victory theme by song ID
        /// </summary>
        /// <param name="songId">Song ID to retrieve</param>
        /// <returns>Tracklist song object</returns>
        private TracklistSong GetVictoryTheme(uint? songId)
        {
            return _tracklistService.GetTracklistSong(songId, _settingsService.BuildSettings.FilePathSettings.VictoryThemeTracklist);
        }

        /// <summary>
        /// Delete victory theme by song ID
        /// </summary>
        /// <param name="songId">Song ID to delete</param>
        private void DeleteVictoryTheme(uint? songId)
        {
            _tracklistService.DeleteTracklistSong(songId, _settingsService.BuildSettings.FilePathSettings.VictoryThemeTracklist);
        }

        /// <summary>
        /// Import victory theme for fighter
        /// </summary>
        /// <param name="tracklistSong">Tracklist song object to import</param>
        /// <param name="brstmNode">BRSTM node to import</param>
        /// <returns>ID of added song</returns>
        private uint ImportVictoryTheme(TracklistSong tracklistSong, ResourceNode brstmNode)
        {
            return _tracklistService.ImportTracklistSong(tracklistSong, _settingsService.BuildSettings.FilePathSettings.VictoryThemeTracklist, brstmNode);
        }

        /// <summary>
        /// Get credits theme by slot ID
        /// </summary>
        /// <param name="slotId">Slot ID</param>
        /// <returns>Credits theme</returns>
        private TracklistSong GetCreditsTheme(int? slotId)
        {
            var creditsTheme = new TracklistSong();
            if (slotId != null)
            {
                // Read the credits theme table
                var codePath = Path.Combine(_settingsService.AppSettings.BuildPath, _settingsService.BuildSettings.FilePathSettings.CreditsThemeAsmFile);
                var code = _codeService.ReadCode(codePath);
                var table = _codeService.ReadTable(code, "ClassicResultsTable:");
                // Ensure slot ID is within range of table
                if (table.Count > slotId)
                {
                    var id = table[slotId.Value];
                    // Find matching table entry
                    var result = uint.TryParse(id.Replace("0x", string.Empty), NumberStyles.HexNumber, null, out uint foundId);
                    if (result)
                    {
                        creditsTheme = _tracklistService.GetTracklistSong(foundId, _settingsService.BuildSettings.FilePathSettings.CreditsThemeTracklist);
                    }
                }
            }
            return creditsTheme;
        }

        /// <summary>
        /// Delete credits theme by song ID
        /// </summary>
        /// <param name="songId">Song ID to delete</param>
        private void DeleteCreditsTheme(uint? songId)
        {
            _tracklistService.DeleteTracklistSong(songId, _settingsService.BuildSettings.FilePathSettings.CreditsThemeTracklist);
        }

        /// <summary>
        /// Import credits theme for fighter
        /// </summary>
        /// <param name="tracklistSong">Tracklist song object to import</param>
        /// <param name="brstmNode">BRSTM node to import</param>
        /// <returns>ID of added song</returns>
        private uint ImportCreditsTheme(TracklistSong tracklistSong, ResourceNode brstmNode, FighterInfo fighterInfo)
        {
            var slotId = fighterInfo.Ids.SlotConfigId;
            if (slotId != null)
            {
                var id = _tracklistService.ImportTracklistSong(tracklistSong, _settingsService.BuildSettings.FilePathSettings.CreditsThemeTracklist, brstmNode);
                // Read the credits theme table
                var codePath = Path.Combine(_settingsService.AppSettings.BuildPath, _settingsService.BuildSettings.FilePathSettings.CreditsThemeAsmFile);
                var code = _codeService.ReadCode(codePath);
                var table = _codeService.ReadTable(code, "ClassicResultsTable:");
                // Convert to AsmTable
                var fighterInfoTable = _settingsService.FighterInfoList;
                var asmTable = new List<AsmTableEntry>();
                foreach (var entry in table)
                {
                    var comment = fighterInfoTable.FirstOrDefault(x => x.Ids.SlotConfigId == asmTable.Count)?.DisplayName;
                    var newEntry = new AsmTableEntry
                    {
                        Item = entry,
                        Comment = !string.IsNullOrEmpty(comment) ? comment : "Unknown"
                    };
                    asmTable.Add(newEntry);
                }
                // Update fighter slot
                if (asmTable.Count > slotId)
                {
                    asmTable[slotId.Value].Item = $"0x{tracklistSong.SongId:X4}";
                    asmTable[slotId.Value].Comment = fighterInfo.DisplayName;
                }
                // Write table
                code = _codeService.ReplaceTable(code, "ClassicResultsTable:", asmTable, DataSize.Halfword, 4);
                _fileService.SaveTextFile(codePath, code);
                return id;
            }
            return tracklistSong.SongId;
        }

        /// <summary>
        /// Get name of soundbank by ID
        /// </summary>
        /// <param name="soundbankId">GetSoundbankName</param>
        /// <returns>Name of soundbank file</returns>
        private string GetSoundbankName(uint? soundbankId)
        {
            if (soundbankId != null)
            {
                var increment = _settingsService.BuildSettings.SoundSettings.SoundbankIncrement;
                var format = _settingsService.BuildSettings.SoundSettings.SoundbankFormat;
                var name = $"{((uint)soundbankId + increment).ToString(format)}.sawnd";
                return name;
            }
            return string.Empty;
        }

        /// <summary>
        /// Get soundbank filepath by ID
        /// </summary>
        /// <param name="soundbankId">Soundbank ID</param>
        /// <returns>Soundbank file path</returns>
        private string GetSoundbank(uint? soundbankId)
        {
            if (soundbankId != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var sfxPath = _settingsService.BuildSettings.FilePathSettings.SoundbankPath;
                var path = Path.Combine(buildPath, sfxPath);
                if (_fileService.DirectoryExists(path))
                {
                    var name = GetSoundbankName(soundbankId);
                    var soundbank = _fileService.GetFiles(path, name).FirstOrDefault();
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
        private void DeleteSoundbank(uint? soundbankId)
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
            var module = $"{moduleFolder}\\ft_{internalName?.ToLower()}.rel";
            if (_fileService.FileExists(module))
            {
                return module;
            }
            return null;
        }

        /// <summary>
        /// Get SSE settings for fighter
        /// </summary>
        /// <param name="fighterPackage">Fighter package to retrieve settings for</param>
        /// <returns>Fighter package with SSE settings</returns>
        private FighterPackage GetSSESettings(FighterPackage fighterPackage)
        {
            if (_settingsService.BuildSettings.MiscSettings.SubspaceEx && fighterPackage.FighterInfo.Ids.CSSSlotConfigId != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var modulePath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.SSEModule);
                var rootNode = _fileService.OpenFile(modulePath);
                if (rootNode != null)
                {
                    var relNode = (RELNode)rootNode;
                    if (relNode.Sections.Count() > 8)
                    {
                        var section = relNode.Sections[8];
                        if (section != null)
                        {
                            var sectionData = _fileService.ReadRawData(section);
                            // Get unlock conditions
                            var unlockPosition = ((fighterPackage.FighterInfo.Ids.CSSSlotConfigId.Value - 0x2A) * 4) + 0x178;
                            if (fighterPackage.FighterInfo.Ids.CSSSlotConfigId > 0x2A && sectionData.Length > unlockPosition)
                            {
                                var unlockBytes = sectionData.Skip(unlockPosition).Take(4).ToArray();
                                fighterPackage.FighterSettings.DoorId = Convert.ToUInt32(BitConverter.ToString(unlockBytes, 0, 4).Replace("-", ""), 16);
                            }
                            // Get sub character
                            var subcharacterPosition = fighterPackage.FighterInfo.Ids.CSSSlotConfigId.Value + 0x84;
                            if (sectionData.Length > subcharacterPosition)
                            {
                                fighterPackage.FighterSettings.SSESubCharacterId = sectionData[subcharacterPosition];
                            }
                        }
                    }
                    _fileService.CloseFile(rootNode);
                }
            }
            return fighterPackage;
        }

        /// <summary>
        /// Update SSE module settings for fighter
        /// </summary>
        /// <param name="fighterPackage">Fighter package to update</param>
        private void UpdateSSEModule(FighterPackage fighterPackage)
        {
            if (_settingsService.BuildSettings.MiscSettings.SubspaceEx && fighterPackage.FighterInfo.Ids.CSSSlotConfigId != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var modulePath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.SSEModule);
                var rootNode = _fileService.OpenFile(modulePath);
                if (rootNode != null)
                {
                    var relNode = (RELNode)rootNode;
                    if (relNode.Sections.Count() > 8)
                    {
                        var section = relNode.Sections[8];
                        if (section != null)
                        {
                            byte[] sectionData = _fileService.ReadRawData(section);
                            // Set unlock conditions
                            var unlockPosition = ((fighterPackage.FighterInfo.Ids.CSSSlotConfigId.Value - 0x2A) * 4) + 0x178;
                            if (fighterPackage.FighterInfo.Ids.CSSSlotConfigId > 0x2A && sectionData.Length > unlockPosition)
                            {
                                byte[] newBytes = BitConverter.GetBytes(fighterPackage.FighterSettings.DoorId).Reverse().ToArray();
                                newBytes.CopyTo(sectionData, unlockPosition);
                            }
                            // Set sub character
                            var subcharacterPosition = fighterPackage.FighterInfo.Ids.CSSSlotConfigId.Value + 0x84;
                            if (sectionData.Length > subcharacterPosition && fighterPackage.FighterSettings.SSESubCharacterId != null)
                            {
                                sectionData[subcharacterPosition] = (byte)fighterPackage.FighterSettings.SSESubCharacterId;
                            }
                            // Add fighter if it is not already present
                            // TODO: Test this
                            var fighterCount = sectionData[0];
                            var fighterTable = sectionData.Skip(2).Take(126).ToList(); // There are 126 slots in sora_adv_stage.rel
                            if (_settingsService.BuildSettings.MiscSettings.InstallToSse && fighterPackage.PackageType == PackageType.New && !fighterTable.Contains((byte)fighterPackage.FighterInfo.Ids.SlotConfigId))
                            {
                                sectionData[fighterTable.IndexOf(0) + 2] = (byte)fighterPackage.FighterInfo.Ids.SlotConfigId;
                                sectionData[0] = (byte)(fighterCount + 1);
                            }
                            _fileService.ReplaceNodeRaw(section, sectionData);
                        }
                    }
                    _fileService.SaveFile(rootNode);
                    _fileService.CloseFile(rootNode);
                }
            }
        }

        /// <summary>
        /// Update fighter module with new ID
        /// </summary>
        /// <param name="module">Module to update</param>
        /// <param name="fighterId">Fighter ID to insert</param>
        private RELNode UpdateModule(RELNode relNode, int? fighterId)
        {
            var isExModule = false;
            if (relNode != null && fighterId != null)
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

        /// <summary>
        /// Get all rosters in build
        /// </summary>
        /// <returns>List of rosters</returns>
        public List<Roster> GetRosters()
        {
            var rosters = new List<Roster>();
            foreach(var rosterFile in _settingsService.BuildSettings.FilePathSettings.RosterFiles)
            {
                var path = Path.Combine(_settingsService.AppSettings.BuildPath, rosterFile.FilePath);
                var rootNode = _fileService.OpenFile(path);
                if (rootNode != null)
                {
                    var roster = new Roster();
                    roster.Name = rootNode.Name;
                    roster.FilePath = rosterFile.FilePath;
                    roster.AddNewCharacters = rosterFile.AddNewCharacters;
                    // Get CSS entries
                    var csNode = rootNode.Children.FirstOrDefault(x => x.GetType() == typeof(RSTCGroupNode) && ((RSTCGroupNode)x).GroupType == "Character Select");
                    if (csNode != null)
                    {
                        foreach(RSTCEntryNode child in csNode.Children)
                        {
                            var newEntry = new RosterEntry
                            {
                                Id = child.FighterID,
                                Name = _settingsService.FighterInfoList.FirstOrDefault(x => x.Ids.CSSSlotConfigId == child.FighterID)?.DisplayName ?? child.Name,
                                InCss = true,
                                InRandom = false
                            };
                            roster.Entries.Add(newEntry);
                        }
                    }
                    // Get random entries
                    var randomNode = rootNode.Children.FirstOrDefault(x => x.GetType() == typeof(RSTCGroupNode) && ((RSTCGroupNode)x).GroupType == "Random Character List");
                    if (randomNode != null)
                    {
                        foreach (RSTCEntryNode child in randomNode.Children)
                        {
                            // Add an entry if one isn't already in the roster
                            var entry = roster.Entries.FirstOrDefault(x => x.Id == child.FighterID);
                            if (entry == null)
                            {
                                var newEntry = new RosterEntry
                                {
                                    Id = child.FighterID,
                                    Name = _settingsService.FighterInfoList.FirstOrDefault(x => x.Ids.CSSSlotConfigId == child.FighterID)?.DisplayName ?? child.Name,
                                    InCss = false,
                                    InRandom = true
                                };
                                roster.Entries.Add(newEntry);
                            }
                            // Otherwise, update existing entry
                            else
                            {
                                entry.InRandom = true;
                            }
                        }
                    }
                    rosters.Add(roster);
                    _fileService.CloseFile(rootNode);
                }
            }
            return rosters;
        }

        /// <summary>
        /// Save a list of rosters to the build
        /// </summary>
        /// <param name="rosters">List of rosters to save</param>
        public void SaveRosters(List<Roster> rosters)
        {
            foreach (var roster in rosters)
            {
                // TODO: Create a whole new RSTCNode instead of opening existing one.
                // For some reason new RSTCNodes save as RawDataNodes, but this should be fixable somehow
                var file = _fileService.OpenFile(Path.Combine(_settingsService.AppSettings.BuildPath, roster.FilePath));
                if (file != null)
                {
                    var rosterFile = (RSTCNode)file;
                    rosterFile.cssList.Children.RemoveAll(x => x.GetType() == typeof(RSTCEntryNode));
                    rosterFile.randList.Children.RemoveAll(x => x.GetType() == typeof(RSTCEntryNode));
                    foreach (var entry in roster.Entries)
                    {
                        if (entry.InCss)
                        {
                            rosterFile.cssList.AddChild(new RSTCEntryNode { FighterID = (byte)entry.Id });
                        }
                        if (entry.InRandom)
                        {
                            rosterFile.randList.AddChild(new RSTCEntryNode { FighterID = (byte)entry.Id });
                        }
                    }
                    rosterFile.cssList.Name = rosterFile.cssList.GroupType.ToString();
                    rosterFile.randList.Name = rosterFile.cssList.GroupType.ToString();
                    rosterFile.AddChild(rosterFile.cssList);
                    rosterFile.AddChild(rosterFile.randList);
                    _fileService.SaveFileAs(rosterFile, Path.Combine(_settingsService.AppSettings.BuildPath, roster.FilePath));
                    _fileService.CloseFile(rosterFile);
                }
            }
        }

        /// <summary>
        /// Update null IDs in fighter info to first unused IDs
        /// </summary>
        /// <param name="fighterInfo">Fighter info to update</param>
        /// <returns>Fighter info with updated IDs</returns>
        public FighterInfo UpdateNullIdsToFirstUnused(FighterInfo fighterInfo)
        {
            var newIds = GetUnusedFighterIds();
            if (fighterInfo.Ids.FighterConfigId == null)
            {
                fighterInfo.Ids.FighterConfigId = newIds.ConfigId;
            }
            if (fighterInfo.Ids.CosmeticConfigId == null)
            {
                fighterInfo.Ids.CosmeticConfigId = newIds.ConfigId;
            }
            if (fighterInfo.Ids.SlotConfigId == null)
            {
                fighterInfo.Ids.SlotConfigId = newIds.ConfigId;
            }
            if (fighterInfo.Ids.CSSSlotConfigId == null)
            {
                fighterInfo.Ids.CSSSlotConfigId = newIds.ConfigId;
            }
            if (fighterInfo.Ids.CosmeticId == null)
            {
                fighterInfo.Ids.CosmeticId = newIds.CosmeticId;
            }
            return fighterInfo;
        }

        /// <summary>
        /// Update fighter info to use first available IDs
        /// </summary>
        /// <param name="fighterInfo">Fighter info to update</param>
        /// <returns>Updated fighter info</returns>
        public FighterInfo UpdateIdsToFirstUnused(FighterInfo fighterInfo)
        {
            // Set IDs to first available
            fighterInfo.Ids.FighterConfigId = null;
            fighterInfo.Ids.CosmeticConfigId = null;
            fighterInfo.Ids.SlotConfigId = null;
            fighterInfo.Ids.CSSSlotConfigId = null;
            fighterInfo.Ids.CosmeticId = null;
            fighterInfo = UpdateNullIdsToFirstUnused(fighterInfo);
            return fighterInfo;
        }

        /// <summary>
        /// Get first unused IDs in build
        /// </summary>
        /// <returns>First unused IDs</returns>
        private (int ConfigId, int CosmeticId) GetUnusedFighterIds()
        {
            var configId = 0x3F; // 0x3F is first custom Ex config ID
            var cosmeticId = 0; // TODO: Should this start at 110? 109 is last ID used by game
            var idList = GetUsedFighterIds();
            var usedIds = idList.Where(x => IdCategories.FighterConfigTypes.Contains(x.Type)).Select(x => x.Id);
            // Get config ID
            while (usedIds.Contains(configId))
            {
                configId++;
            }
            usedIds = idList.Where(x => x.Type == IdType.Cosmetic).Select(x => x.Id);
            while (usedIds.Contains(cosmeticId))
            {
                cosmeticId++;
            }
            return (configId, cosmeticId);
        }

        /// <summary>
        /// Get used fighter IDs in build
        /// </summary>
        /// <returns>List of used fighter IDs</returns>
        public List<BrawlId> GetUsedFighterIds()
        {
            var buildFighters = GetAllFighterInfo();
            var settingsFighters = _settingsService.FighterInfoList;
            // Get used IDs
            var usedIds = buildFighters.SelectMany(x => x.Ids.Ids.Where(y => y.Id != null && IdCategories.FighterIdTypes.Contains(y.Type)));
            usedIds = usedIds.Concat(settingsFighters.SelectMany(x => x.Ids.Ids.Where(y => y.Id != null && IdCategories.FighterIdTypes.Contains(y.Type))));
            // Get reserved config IDs
            var reservedIds = new List<BrawlId>();
            foreach(var id in ReservedIds.ReservedFighterConfigIds)
            {
                reservedIds.Add(new BrawlId { Id = id, Type = IdType.FighterConfig });
            }
            // Get reserved cosmetic IDs
            foreach (var id in ReservedIds.ReservedFighterCosmeticIds)
            {
                reservedIds.Add(new BrawlId { Id = id, Type = IdType.Cosmetic });
            }
            usedIds = usedIds.Concat(reservedIds).ToList();
            usedIds = usedIds.Distinct();
            return usedIds.ToList();
        }

        /// <summary>
        /// Get used internal names of fighters
        /// </summary>
        /// <returns>Used internal names</returns>
        public List<string> GetUsedInternalNames()
        {
            var buildFighterFolders = _fileService.GetDirectories(_settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.FighterFiles), "*", SearchOption.TopDirectoryOnly);
            var settingsFighters = _settingsService.FighterInfoList;
            var usedNames = settingsFighters.Select(x => x.InternalName.ToLower());
            usedNames = usedNames.Concat(buildFighterFolders.Select(x => Path.GetFileName(x).ToLower()));
            usedNames = usedNames.Concat(ReservedIds.ReservedInternalNames);
            return usedNames.ToList();
        }

        #region Fighter-Specific Settings

        /// <summary>
        /// Get fighter settings that are only for clones of specific characters
        /// </summary>
        /// <param name="fighterPackage">Fighter package to get settings for</param>
        /// <returns>Fighter settings</returns>
        private FighterSettings GetFighterSpecificSettings(FighterPackage fighterPackage)
        {
            // Get fighter specific settings
            var fighterSettings = fighterPackage.FighterSettings;
            var buildPath = _settingsService.AppSettings.BuildPath;
            var codePath = _settingsService.BuildSettings.FilePathSettings.FighterSpecificAsmFile;
            var path = Path.Combine(buildPath, codePath);
            var code = _codeService.ReadCode(path);
            if (!string.IsNullOrEmpty(code))
            {
                // Get Lucario Aura Sphere Bone ID settings
                var macroList = new List<string> { "80AA9D60", "80AA9D98", "80AAA768", "80AAA7A0" };
                for (int i = 0; i < 4; i++)
                {
                    var lucarioBoneMacro = _codeService.GetMacro(code, macroList[i], $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", 1, "BoneIDFixA");
                    if (lucarioBoneMacro != null)
                    {
                        fighterSettings.LucarioSettings.BoneIds[i] = Convert.ToInt32(lucarioBoneMacro.Parameters[2].Replace("0x", ""), 16);
                    }
                }
                // Get Lucario GFX fix settings
                var lucarioGfxMacro = _codeService.GetMacro(code, "80AA95B8", $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", 0, "GFXFix");
                if (lucarioGfxMacro != null)
                {
                    fighterSettings.LucarioSettings.UseGfxFix = true;
                }
                // Get Lucario Aura Sphere Kirby GFX Settings
                var lucarioKirbyGfxMacro = _codeService.GetMacro(code, "80AA95AC", $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", 0, "GFXFix");
                if (lucarioKirbyGfxMacro != null)
                {
                    fighterSettings.LucarioSettings.UseKirbyGfxFix = true;
                }
                // Get Samus GFX fix settings
                var samusGfxMacro = _codeService.GetMacro(code, "80A0AAA8", $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", 0, "GFXFix");
                if (samusGfxMacro != null)
                {
                    fighterSettings.SamusSettings.UseGfxFix = true;
                }
                // Get Samus Kirby GFX Settings
                var samusKirbyGfxMacro = _codeService.GetMacro(code, "80A0AB1C", $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", 0, "GFXFix");
                if (samusKirbyGfxMacro != null)
                {
                    fighterSettings.SamusSettings.UseKirbyGfxFix = true;
                }
                // Get Jigglypuff rollout bone settings
                macroList = new List<string> { "80AD0B20", "80ACC0C4", "80ACC9C4", "80ACD178" };
                for (int i = 0; i < 4; i++)
                {
                    var jigglypuffBoneMacro = _codeService.GetMacro(code, macroList[i], $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", 0, "CloneBones");
                    if (jigglypuffBoneMacro != null)
                    {
                        fighterSettings.JigglypuffSettings.BoneIds[i] = Convert.ToInt32(jigglypuffBoneMacro.Parameters[1].Replace("0x", ""), 16);
                    }
                }
                // Get Jigglypuff GFX fix settings
                var jigglypuffGfxMacro = _codeService.GetMacro(code, "80ACB67C", $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", 0, "CloneGFX");
                if (jigglypuffGfxMacro != null)
                {
                    fighterSettings.JigglypuffSettings.EFLSId = Convert.ToInt32(jigglypuffGfxMacro.Parameters[2].Replace("0x", ""), 16);
                }
                // Get Jigglypuff SFX settings
                var jigglypuffSfxAddresses = new List<string> { "80ACAE3C", "80ACAE60", "80ACF704", "80ACA09C" };
                for(var i = 0; i < 4; i++)
                {
                    var jigglypuffSfxMacro = _codeService.GetMacro(code, jigglypuffSfxAddresses[i], $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", 0, "CloneSFX");
                    if (jigglypuffSfxMacro != null)
                    {
                        fighterSettings.JigglypuffSettings.SfxIds[i] = Convert.ToInt32(jigglypuffSfxMacro.Parameters[1].Replace("0x", ""), 16);
                    }
                }
                // Get Dedede fix settings
                var dededeFixMacro = _codeService.GetMacro(code, "80aa1544", $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", 0, "DededeFix");
                if (dededeFixMacro != null)
                {
                    fighterSettings.DededeSettings.UseFix = true;
                }
                // Get Bowser bone fix setings
                var bowserBoneMacro = _codeService.GetMacro(code, "80A391F8", $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", 0, "BoneIDFix");
                if (bowserBoneMacro != null)
                {
                    fighterSettings.BowserSettings.BoneId = Convert.ToInt32(bowserBoneMacro.Parameters[1].Replace("0x", ""), 16);
                }
            }
            return fighterSettings;
        }

        /// <summary>
        /// Update fighter specific settings in build
        /// </summary>
        /// <param name="fighterPackage">Fighter package to update settings for</param>
        private void UpdateFighterSpecificSettings(FighterPackage fighterPackage)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var fighterSettings = fighterPackage.FighterSettings;

            var codePath = _settingsService.BuildSettings.FilePathSettings.FighterSpecificAsmFile;
            var path = Path.Combine(buildPath, codePath);
            var code = _codeService.ReadCode(path);
            if (!string.IsNullOrEmpty(code))
            {
                // Update Lucario settings
                // Lucario Bone ID Fix
                var macroList = new List<string> { "80AA9D60", "80AA9D98", "80AAA768", "80AAA7A0" };
                var registerList = new List<string> { "r3", "r7", "r3", "r7" };
                for (int i = 0; i < 4; i++)
                {
                    if (fighterSettings.LucarioSettings?.BoneIds[i] != null)
                    {
                        var lucarioBoneMacro = new AsmMacro
                        {
                            MacroName = "BoneIDFixA",
                            Parameters = new List<string>
                            {
                                registerList[i],
                                $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}",
                                $"0x{fighterSettings.LucarioSettings.BoneIds[i]:X2}"
                            },
                            Comment = fighterPackage.FighterInfo.DisplayName
                        };
                        code = _codeService.InsertUpdateMacro(code, macroList[i], lucarioBoneMacro, 1, 3);
                    }
                    else
                    {
                        code = _codeService.RemoveMacro(code, macroList[i], $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", "BoneIDFixA", 1);
                    }
                }
                // Lucario GFX fix
                if (fighterSettings.LucarioSettings?.UseGfxFix == true && fighterPackage.FighterInfo.EffectPacId != null)
                {
                    var lucarioGfxMacro = new AsmMacro
                    {
                        MacroName = "GFXFix",
                        Parameters = new List<string>
                            {
                                $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}",
                                $"0x{fighterPackage.FighterInfo.EffectPacId:X2}"
                            },
                        Comment = fighterPackage.FighterInfo.DisplayName
                    };
                    code = _codeService.InsertUpdateMacro(code, "80AA95B8", lucarioGfxMacro, 0, 0);
                }
                else
                {
                    code = _codeService.RemoveMacro(code, "80AA95B8", $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", "GFXFix", 0);
                }
                // Lucario Kirby Hat GFX Fix
                if (fighterSettings.LucarioSettings?.UseKirbyGfxFix == true && fighterPackage.FighterInfo.KirbyEffectPacId != null)
                {
                    var lucarioKirbyGfxMacro = new AsmMacro
                    {
                        MacroName = "GFXFix",
                        Parameters = new List<string>
                        {
                            $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}",
                            $"0x{fighterPackage.FighterInfo.KirbyEffectPacId:X2}"
                        },
                        Comment = fighterPackage.FighterInfo.DisplayName + "Hat"
                    };
                    code = _codeService.InsertUpdateMacro(code, "80AA95AC", lucarioKirbyGfxMacro, 0, 7);
                }
                else
                {
                    code = _codeService.RemoveMacro(code, "80AA95AC", $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", "GFXFix", 0);
                }
                // Update Samus settings
                // Samus GFX Fix
                if (fighterSettings.SamusSettings?.UseGfxFix == true && fighterPackage.FighterInfo.EffectPacId != null)
                {
                    var samusGfxMacro = new AsmMacro
                    {
                        MacroName = "GFXFix",
                        Parameters = new List<string>
                            {
                                $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}",
                                $"0x{fighterPackage.FighterInfo.EffectPacId:X2}"
                            },
                        Comment = fighterPackage.FighterInfo.DisplayName
                    };
                    code = _codeService.InsertUpdateMacro(code, "80A0AAA8", samusGfxMacro, 0, 9);
                }
                else
                {
                    code = _codeService.RemoveMacro(code, "80A0AAA8", $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", "GFXFix", 0);
                }
                // Samus Kirby hat GFX Fix
                if (fighterSettings.SamusSettings?.UseKirbyGfxFix == true && fighterPackage.FighterInfo.KirbyEffectPacId != null)
                {
                    var samusKirbyGfxMacro = new AsmMacro
                    {
                        MacroName = "GFXFix",
                        Parameters = new List<string>
                        {
                            $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}",
                            $"0x{fighterPackage.FighterInfo.KirbyEffectPacId:X2}"
                        },
                        Comment = fighterPackage.FighterInfo.DisplayName + "Hat"
                    };
                    code = _codeService.InsertUpdateMacro(code, "80A0AB1C", samusKirbyGfxMacro, 0, 17);
                }
                else
                {
                    code = _codeService.RemoveMacro(code, "80A0AB1C", $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", "GFXFix", 0);
                }
                // Jigglypuff rollout bone fix
                foreach(var item in _jigglypuffCloneBoneList)
                {
                    if (fighterSettings.JigglypuffSettings?.BoneIds[item.BoneIdIndex] != null)
                    {
                        var jigglypuffBoneMacro = new AsmMacro
                        {
                            MacroName = "CloneBones",
                            Parameters = new List<string>
                            {
                                $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}",
                                $"0x{fighterSettings.JigglypuffSettings.BoneIds[item.BoneIdIndex]:X2}",
                                item.Register
                            },
                            Comment = fighterPackage.FighterInfo.DisplayName
                        };
                        code = _codeService.InsertUpdateMacro(code, item.Address, jigglypuffBoneMacro, 0, 0);
                    }
                    else
                    {
                        code = _codeService.RemoveMacro(code, item.Address, $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", "CloneBones", 0);
                    }
                }
                // Jigglypuff GFX Fix
                var cloneGfxList = new List<(string Register, string Address)>
                {
                    ("r29", "80ACB67C"),
                    ("r4", "80ACD1EC"),
                    ("r4", "80ACEE68")
                };
                if (fighterSettings.JigglypuffSettings?.EFLSId != null && fighterPackage.FighterInfo.EffectPacId != null)
                {
                    foreach(var item in cloneGfxList)
                    {
                        var jigglypuffGfxMacro = new AsmMacro
                        {
                            MacroName = "CloneGFX",
                            Parameters = new List<string>
                        {
                            $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}",
                            $"0x{fighterPackage.FighterInfo.EffectPacId:X2}",
                            $"0x{fighterSettings.JigglypuffSettings?.EFLSId:X2}",
                            item.Register
                        },
                            Comment = fighterPackage.FighterInfo.DisplayName
                        };
                        code = _codeService.InsertUpdateMacro(code, item.Address, jigglypuffGfxMacro, 0, 0);
                    }
                }
                else
                {
                    foreach(var item in cloneGfxList)
                    {
                        code = _codeService.RemoveMacro(code, item.Address, $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", "CloneGFX", 0);
                    }
                }
                // Jigglypuff SFX fix
                var cloneSfxList = new List<(string Register, string Address, int Index)>
                {
                    ("r4", "80ACAE3C", 1),
                    ("r4", "80ACAE60", 2),
                    ("r3", "80ACF704", 0),
                    ("r3", "80ACA09C", 0)
                };
                for (var i = 0; i < 4; i++)
                {
                    if (fighterSettings.JigglypuffSettings?.SfxIds[i] != null)
                    {
                        var jigglypuffSfxMacro = new AsmMacro
                        {
                            MacroName = "CloneSFX",
                            Parameters = new List<string>
                        {
                            $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}",
                            $"0x{fighterSettings.JigglypuffSettings.SfxIds[i]:X2}",
                            cloneSfxList[i].Register
                        },
                            Comment = fighterPackage.FighterInfo.DisplayName
                        };
                        code = _codeService.InsertUpdateMacro(code, cloneSfxList[i].Address, jigglypuffSfxMacro, 0, cloneSfxList[i].Index);
                    }
                    else
                    {
                        code = _codeService.RemoveMacro(code, cloneSfxList[i].Address, $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", "CloneSFX", 0);
                    }
                }
                // Dedede fixes
                var dededeAddresses = new List<string>
                {
                    "80aa1544",
                    "80aa0af0",
                    "80aa0d4c",
                    "8088f768",
                    "8088e758",
                    "8088f120",
                    "8088fa50",
                    "80890050"
                };
                if (fighterSettings.DededeSettings?.UseFix == true)
                {
                    foreach(var address in dededeAddresses)
                    {
                        var dededeFixMacro = new AsmMacro
                        {
                            MacroName = "DededeFix",
                            Parameters = new List<string>
                        {
                            $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}"
                        },
                            Comment = fighterPackage.FighterInfo.DisplayName
                        };
                        code = _codeService.InsertUpdateMacro(code, address, dededeFixMacro, 0, 0);
                    }
                }
                else
                {
                    foreach(var address in dededeAddresses)
                    {
                        code = _codeService.RemoveMacro(code, address, $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", "DededeFix", 0);
                    }
                }
                // Bowser fixes
                if (fighterSettings.BowserSettings?.BoneId != null)
                {
                    var bowserBoneIdFixMacro = new AsmMacro
                    {
                        MacroName = "BoneIDFix",
                        Parameters = new List<string>
                        {
                            $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}",
                            $"0x{fighterSettings.BowserSettings.BoneId:X2}"
                        },
                        Comment = fighterPackage.FighterInfo.DisplayName
                    };
                    code = _codeService.InsertUpdateMacro(code, "80A391F8", bowserBoneIdFixMacro, 0, 0);
                }
                else
                {
                    code = _codeService.RemoveMacro(code, "80A391F8", $"0x{fighterPackage.FighterInfo.Ids.FighterConfigId:X2}", "BoneIDFix", 0);
                }
            }
            _fileService.SaveTextFile(path, code);
        }

        private List<(int BoneIdIndex, string Register, string Address)> _jigglypuffCloneBoneList = new List<(int, string, string)>
        {
            (2, "r28", "80AC9F9C"),
            (2, "r27", "80ACA3A4"),
            (2, "r27", "80ACA414"),
            (2, "r31", "80ACA7E8"),
            (2, "r31", "80ACA858"),
            (2, "r28", "80ACAC2C"),
            (2, "r27", "80ACB050"),
            (2, "r27", "80ACB0c0"),
            (3, "r5", "80ACB6A0"),
            (2, "r31", "80ACB7BC"),
            (2, "r31", "80ACB81C"),
            (2, "r28", "80ACBE9C"),
            (1, "r4", "80ACC0C4"),
            (2, "r27", "80ACC9C4"),
            (2, "r26", "80ACCA34"),
            (3, "r28", "80ACD178"),
            (2, "r31", "80ACD53C"),
            (2, "r31", "80ACD5AC"),
            (2, "r30", "80ACD93C"),
            (1, "r4", "80ACDB60"),
            (2, "r28", "80ACE580"),
            (2, "r26", "80ACE5F0"),
            (2, "r29", "80ACEBF0"),
            (3, "r28", "80ACEDF4"),
            (1, "r4", "80ACF8B0"),
            (2, "r25", "80ACFCD4"),
            (2, "r26", "80ACFD94"),
            (2, "r31", "80AD02C8"),
            (2, "r31", "80AD0338"),
            (1, "r4", "80AD0AE0"),
            (0, "r28", "80AD0B20"),
            (1, "r4", "80AD0B5C"),
            (1, "r4", "80AD0D94"),
            (1, "r4", "80AD1404"),
            (2, "r28", "80AD1628"),
            (2, "r28", "80AD1698"),
            (2, "r31", "80AD17D8"),
            (2, "r31", "80AD1848")
        };

        #endregion Fighter-Specific Settings
    }
}
