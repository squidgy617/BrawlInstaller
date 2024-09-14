using BrawlInstaller.Classes;
using BrawlInstaller.Enums;
using BrawlLib.BrawlManagerLib;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.ResourceNodes.ProjectPlus;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrawlInstaller.Services
{
    public interface IStageService
    {
        /// <inheritdoc cref="StageService.GetStageLists(List{StageSlot})"/>
        List<StageList> GetStageLists(List<StageSlot> stageTable);

        /// <inheritdoc cref="StageService.GetStageSlots()"/>
        List<StageSlot> GetStageSlots();

        /// <inheritdoc cref="StageService.GetStageData(StageInfo)"/>
        StageInfo GetStageData(StageInfo stage);

        /// <inheritdoc cref="StageService.SaveStageLists(List{StageList}, List{StageSlot})"/>
        void SaveStageLists(List<StageList> stageLists, List<StageSlot> stageTable);

        /// <inheritdoc cref="StageService.SaveStage(StageInfo)"/>
        List<string> SaveStage(StageInfo stage);
    }

    [Export(typeof(IStageService))]
    internal class StageService : IStageService
    {
        // Services
        ICodeService _codeService;
        ISettingsService _settingsService;
        IFileService _fileService;
        ICosmeticService _cosmeticService;

        [ImportingConstructor]
        public StageService(ICodeService codeService, ISettingsService settingsService, IFileService fileService, ICosmeticService cosmeticService) 
        {
            _codeService = codeService;
            _settingsService = settingsService;
            _fileService = fileService;
            _cosmeticService = cosmeticService;
        }

        // Methods

        /// <summary>
        /// Get data associated with a specific stage
        /// </summary>
        /// <param name="stage">Stage to load data for</param>
        /// <returns>Stage object with data</returns>
        public StageInfo GetStageData(StageInfo stage)
        {
            // TODO: Get list of tracklists and use dropdown to select them?
            // Get cosmetics
            var cosmetics = _cosmeticService.GetStageCosmetics(stage.Slot.StageIds);
            // Get name for RSS
            var names = GetStageRandomNames();
            if (names.Count > stage.Slot.StageIds.StageCosmeticId)
            {
                stage.RandomName = names[stage.Slot.StageIds.StageCosmeticId];
            }
            else
            {
                stage.RandomName = string.Empty;
            }
            // Get parameters and button flags
            LoadStageEntries(stage);
            stage.Cosmetics.Items = cosmetics;
            return stage;
        }

        /// <summary>
        /// Get stage lists from build
        /// </summary>
        /// <returns>List of stage lists in build</returns>
        public List<StageList> GetStageLists(List<StageSlot> stageTable)
        {
            var stageLists = new List<StageList>();
            // Iterate through each stage list file
            foreach(var stageListFile in _settingsService.BuildSettings.FilePathSettings.StageListPaths)
            {
                var filePath = $"{_settingsService.AppSettings.BuildPath}\\{stageListFile}";
                if (File.Exists(filePath))
                {
                    var stageList = new StageList { Name = Path.GetFileNameWithoutExtension(stageListFile), FilePath = filePath.Replace(_settingsService.AppSettings.BuildPath, "") };
                    // Read all pages from stage list file
                    var fileText = File.ReadAllText(filePath);
                    var labels = new List<string> { "TABLE_1:", "TABLE_2:", "TABLE_3:", "TABLE_4:", "TABLE_5:" };
                    int pageNumber = 1;
                    foreach (var label in labels)
                    {
                        var page = new StagePage { PageNumber = pageNumber };
                        // Get indexes in table
                        var indexList = _codeService.ReadTable(fileText, label);
                        foreach(var index in indexList)
                        {
                            // Get IDs from index
                            if(int.TryParse(index.Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result))
                            {
                                if (result >= 0)
                                {
                                    var stageSlot = stageTable.FirstOrDefault(x => x.Index == result);
                                    if (stageSlot != null)
                                    {
                                        page.StageSlots.Add(stageSlot);
                                    }
                                }
                            }
                        }
                        stageList.Pages.Add(page);
                        pageNumber++;
                    }
                    // Add stage list
                    stageLists.Add(stageList);
                }
            }
            return stageLists;
        }

        /// <summary>
        /// Get all stage slots from build
        /// </summary>
        /// <returns>List of stage slots</returns>
        public List<StageSlot> GetStageSlots()
        {
            var stageSlots = new List<StageSlot>();
            var stageIds = GetStageIds();
            foreach(var idPair in stageIds)
            {
                var path = $"{_settingsService.AppSettings.BuildPath}\\{_settingsService.BuildSettings.FilePathSettings.StageSlots}\\{idPair.StageId:X2}.asl";
                var node = _fileService.OpenFile(path);
                if (node != null)
                {
                    var stageSlot = new StageSlot
                    {
                        StageIds = idPair,
                        Index = stageIds.IndexOf(idPair),
                        Name = node.Children.FirstOrDefault()?.Name ?? "Unknown"
                    };
                    stageSlots.Add(stageSlot);
                    _fileService.CloseFile(node);
                }
            }
            return stageSlots;
        }

        /// <summary>
        /// Load stage entry parameters and button flags for a given stage
        /// </summary>
        /// <param name="stage">Stage to load data for</param>
        /// <returns>Updated stage data</returns>
        private StageInfo LoadStageEntries(StageInfo stage)
        {
            stage.StageEntries = new List<StageEntry>();
            var path = $"{_settingsService.AppSettings.BuildPath}\\{_settingsService.BuildSettings.FilePathSettings.StageSlots}\\{stage.Slot.StageIds.StageId:X2}.asl";
            var node = _fileService.OpenFile(path);
            if (node != null)
            {
                foreach (ASLSEntryNode entry in node.Children)
                {
                    var newEntry = new StageEntry
                    {
                        ButtonFlags = (ushort)(entry.ButtonFlags > 0x4000 ? 0x4000 : entry.ButtonFlags > 0x8000 ? 0x8000 : entry.ButtonFlags),
                        Params = GetStageParams(entry.Name, stage.AllParams)
                    };
                    // Get bin file
                    var binFile = GetStageBinFile(stage.Slot.StageIds.StageId, newEntry);
                    newEntry.BinFilePath = binFile;
                    newEntry.BinFileName = binFile != null ? Regex.Replace(Path.GetFileNameWithoutExtension(binFile), "st_\\d+_", "") : "Unknown";
                    // Add params to list if they are not already there
                    stage.StageEntries.Add(newEntry);
                    if (!stage.AllParams.Contains(newEntry.Params))
                    {
                        stage.AllParams.Add(newEntry.Params);
                    }
                }
                _fileService.CloseFile(node);
            }
            return stage;
        }

        /// <summary>
        /// Get the index of a list alt based on button flags
        /// </summary>
        /// <param name="buttonFlags">Button flags of stage entry</param>
        /// <returns>Index for list alt</returns>
        private int GetButtonFlagIndex(StageEntry entry)
        {
            var subtract = entry.IsRAlt ? 0x4000 : (entry.IsLAlt ? 0x8000 : 0);
            var result = entry.ButtonFlags - subtract;
            return result;
        }

        /// <summary>
        /// Get bin file associated with a stage entry
        /// </summary>
        /// <param name="stageId">ID of stage associated with stage entry</param>
        /// <param name="buttonFlags">Button flags associated with stage entry</param>
        /// <returns>Bin file path</returns>
        private string GetStageBinFile(int stageId, StageEntry entry)
        {
            if (entry.IsRAlt || entry.IsLAlt)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var altListPath = _settingsService.BuildSettings.FilePathSettings.StageAltListPath;
                var directoryPath = Path.Combine(buildPath, altListPath);
                var folderLetter = entry.IsRAlt ? "R" : (entry.IsLAlt ? "L" : string.Empty);
                var folderName = $"{stageId.ToString("X2")}_{folderLetter}";
                var folder = Path.Combine(directoryPath, folderName);
                if (Directory.Exists(folder))
                {
                    // Bin files correspond to button flags in alphabetical order (e.g. the first file in alphabetical order will be attached to button flag ending in 00
                    var index = GetButtonFlagIndex(entry);
                    var files = Directory.GetFiles(folder, "*.bin");
                    string file = null;
                    if (index < files.Count())
                        file = files[index];
                    return file;
                }
            }
            return null;
        }

        /// <summary>
        /// Get stage params from file or list
        /// </summary>
        /// <param name="name">Param name to search for</param>
        /// <param name="paramList">List of params to check</param>
        /// <returns>Stage parameters</returns>
        private StageParams GetStageParams(string name, List<StageParams> paramList)
        {
            var found = paramList.FirstOrDefault(x => x.Name == name);
            if (found != null)
            {
                return found;
            }
            else
            {
                return GetStageParams(name);
            }
        }

        /// <summary>
        /// Get stage parameters from param file name
        /// </summary>
        /// <param name="name">Param file name</param>
        /// <returns>Stage parameters</returns>
        private StageParams GetStageParams(string name)
        {
            var stageParams = new StageParams();
            var buildPath = _settingsService.AppSettings.BuildPath;
            var paramPath = _settingsService.BuildSettings.FilePathSettings.StageParamPath;
            var path = $"{buildPath}\\{paramPath}";
            var file = Directory.GetFiles(path, $"{name}.param").FirstOrDefault();
            if (file != null)
            {
                var rootNode = _fileService.OpenFile(file);
                if (rootNode != null)
                {
                    // Set params
                    var paramNode = (STEXNode)rootNode;
                    stageParams.Name = paramNode.Name;
                    stageParams.PacName = paramNode.StageName;
                    stageParams.EffectBank = paramNode.EffectBank;
                    stageParams.SoundBank = paramNode.SoundBank;
                    stageParams.Module = paramNode.Module;
                    stageParams.CharacterOverlay = paramNode.CharacterOverlay;
                    stageParams.MemoryAllocation = paramNode.MemoryAllocation;
                    stageParams.WildSpeed = paramNode.WildSpeed;
                    stageParams.IsDualLoad = paramNode.IsDualLoad;
                    stageParams.IsDualShuffle = paramNode.IsDualShuffle;
                    stageParams.IsOldSubStage = paramNode.IsOldSubstage;
                    stageParams.VariantType = paramNode.SubstageVarianceType;
                    stageParams.SubstageRange = paramNode.SubstageRange;
                    stageParams.IsFlat = paramNode.IsFlat;
                    stageParams.IsFixedCamera = paramNode.IsFixedCamera;
                    stageParams.IsSlowStart = paramNode.IsSlowStart;
                    foreach(var substage in rootNode.Children)
                    {
                        stageParams.Substages.Add(new Substage
                        {
                            Name = substage.Name,
                            PacFile = GetSubstagePath(paramNode.StageName, substage.Name)
                        });
                    }
                    // Get files
                    var pacPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.StagePacPath);
                    var modulePath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.Modules);
                    var tracklistPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.TracklistPath);
                    var soundbankPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.SoundbankPath);
                    stageParams.PacFile = Directory.GetFiles(pacPath, $"STG{paramNode.StageName.ToUpper()}.pac").FirstOrDefault();
                    stageParams.ModuleFile = Directory.GetFiles(modulePath, $"{paramNode.Module}").FirstOrDefault();
                    stageParams.TrackListFile = Directory.GetFiles(tracklistPath, $"{paramNode.TrackList}.tlst").FirstOrDefault();
                    stageParams.SoundBankFile = Directory.GetFiles(soundbankPath, $"{paramNode.SoundBank:X3}_{stageParams.PacName}.sawnd").FirstOrDefault();
                    _fileService.CloseFile(rootNode);
                }
            }
            return stageParams;
        }

        /// <summary>
        /// Get substage pac path if it exists
        /// </summary>
        /// <param name="mainPacName">Name of pac file for main stage</param>
        /// <param name="suffix">Suffix used for substage</param>
        /// <returns>Path to substage pac file</returns>
        private string GetSubstagePath(string mainPacName, string suffix)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var stagePath = _settingsService.BuildSettings.FilePathSettings.StagePacPath;
            var path = Path.Combine(buildPath, stagePath);
            var pacPath = $"{path}\\STG{mainPacName.ToUpper()}_{suffix.ToUpper()}.pac";
            if (File.Exists(pacPath))
            {
                return pacPath;
            }
            return null;
        }

        /// <summary>
        /// Get all stage IDs from build
        /// </summary>
        /// <returns>List of stage IDs</returns>
        private List<BrawlIds> GetStageIds()
        {
            var stageIds = new List<BrawlIds>();
            var filePath = $"{_settingsService.AppSettings.BuildPath}\\{_settingsService.BuildSettings.FilePathSettings.StageTablePath}";
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);
                var idList = _codeService.ReadTable(fileText, "TABLE_STAGES:");
                foreach(var id in idList)
                {
                    var newIds = new BrawlIds();
                    var stageId = id.Substring(2, 2);
                    var cosmeticId = id.Substring(4, 2);
                    newIds.StageId = Convert.ToInt32(stageId, 16);
                    newIds.StageCosmeticId = Convert.ToInt32(cosmeticId, 16);
                    stageIds.Add(newIds);
                }
            }
            return stageIds;
        }

        /// <summary>
        /// Update names for stages on RSS
        /// </summary>
        /// <param name="stage"></param>
        private void SaveStageRandomName(StageInfo stage)
        {
            // TODO: Load this from code in Random.asm
            var presetCount = 7;
            var buildPath = _settingsService.AppSettings.BuildPath;
            var randomStageLocations = _settingsService.BuildSettings.FilePathSettings.RandomStageNamesLocations;
            foreach(var location in randomStageLocations)
            {
                var path = $"{buildPath}\\{location.FilePath}";
                var rootNode = _fileService.OpenFile(path);
                if (rootNode != null)
                {
                    var namesNode = rootNode.FindChild(location.NodePath);
                    if (namesNode != null)
                    {
                        var names = ((MSBinNode)namesNode)._strings;
                        // If name list is shorter than stage cosmetic ID, fill in missing spots
                        while (names.Count - presetCount <= stage.Slot.StageIds.StageCosmeticId)
                        {
                            names.Add("_");
                        }
                        names[stage.Slot.StageIds.StageCosmeticId + presetCount] = stage.RandomName;
                        ((MSBinNode)namesNode)._strings = names;
                        namesNode.IsDirty = true;
                        _fileService.SaveFile(rootNode);
                    }
                    _fileService.CloseFile(rootNode);
                }
            }
        }

        /// <summary>
        /// Get names for stages on RSS
        /// </summary>
        /// <returns>List of stage names</returns>
        private List<string> GetStageRandomNames()
        {
            // TODO: Load this from code in Random.asm
            var presetCount = 7;
            var names = new List<string>();
            var buildPath = _settingsService.AppSettings.BuildPath;
            var randomStageLocation = _settingsService.BuildSettings.FilePathSettings.RandomStageNamesLocations.FirstOrDefault();
            var path = $"{buildPath}\\{randomStageLocation.FilePath}";
            var rootNode = _fileService.OpenFile(path);
            if (rootNode != null)
            {
                var namesNode = rootNode.FindChild(randomStageLocation.NodePath);
                if (namesNode != null)
                {
                    names = ((MSBinNode)namesNode)._strings;
                    names.RemoveRange(0, presetCount);
                }
                _fileService.CloseFile(rootNode);
            }
            return names;
        }

        /// <summary>
        /// Save changes to stage
        /// </summary>
        /// <param name="stage">The stage to save</param>
        /// <returns>List of files user can choose to delete</returns>
        public List<string> SaveStageInfo(StageInfo stage)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            // Get old stage data
            var oldStageData = new StageInfo
            {
                Slot = stage.Slot.Copy()
            };
            LoadStageEntries(oldStageData);
            // Get files for install
            // We do this before deleting old files in case the user kept an old file
            var files = OpenStageFiles(stage);
            // Delete old stage data
            var toDelete = DeleteStageEntries(oldStageData);
            // Generate ASL and param files
            SaveStageEntries(stage);
            // Save files
            foreach(var file in files)
            {
                _fileService.SaveFile(file);
                _fileService.CloseFile(file);
            }
            // Return optional delete files, only ones that don't still exist in stage entries
            var modules = stage.StageEntries.Select(x => x.Params.Module).ToList();
            var tracklists = stage.StageEntries.Select(x => x.Params.TrackList).ToList();
            return toDelete.Where(x => !modules.Contains(Path.GetFileName(x), StringComparer.OrdinalIgnoreCase) && !tracklists.Contains(Path.GetFileNameWithoutExtension(x), StringComparer.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Save params and ASL for stage
        /// </summary>
        /// <param name="stage">Stage to save</param>
        private void SaveStageEntries(StageInfo stage)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            // Generate new ASL
            var path = $"{_settingsService.AppSettings.BuildPath}\\{_settingsService.BuildSettings.FilePathSettings.StageSlots}\\{stage.Slot.StageIds.StageId:X2}.asl";
            var node = new ASLSNode();
            // Add entries to ASL
            foreach (var entry in stage.StageEntries)
            {
                var newEntry = new ASLSEntryNode
                {
                    ButtonFlags = entry.ButtonFlags,
                    Name = entry.Params.Name
                };
                node.AddChild(newEntry);
            }
            // Save ASL
            _fileService.SaveFileAs(node, path);
            _fileService.CloseFile(node);
            // Generate param files
            foreach (var param in stage.AllParams)
            {
                // Generate param file
                var newParam = param.ConvertToNode();
                // Save params
                var paramPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.StageParamPath, $"{param.Name}.param");
                _fileService.SaveFileAs(newParam, paramPath);
                _fileService.CloseFile(newParam);
            }
        }

        /// <summary>
        /// Open all files associated with a stage
        /// </summary>
        /// <param name="stage">Stage to open files for</param>
        /// <returns>List of files</returns>
        private List<ResourceNode> OpenStageFiles(StageInfo stage)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var files = new List<ResourceNode>();
            foreach (var entry in stage.StageEntries)
            {
                // Open pac files
                var pacFile = _fileService.OpenFile(entry.Params.PacFile);
                if (pacFile != null)
                {
                    // Set install path so it will save to the correct location
                    var installPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.StagePacPath, $"STG{entry.Params.PacName.ToUpper()}.pac");
                    pacFile._origPath = installPath;
                    files.Add(pacFile);
                }
                // Open soundbanks
                var soundbank = _fileService.OpenFile(entry.Params.SoundBankFile);
                if (soundbank != null)
                {
                    var installPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.SoundbankPath, $"{entry.Params.SoundBank:X3}_{entry.Params.PacName}.sawnd");
                    soundbank._origPath = installPath;
                    files.Add(soundbank);
                }
                // Open modules
                var module = _fileService.OpenFile(entry.Params.ModuleFile);
                if (module != null)
                {
                    var installPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.Modules, $"{entry.Params.Module.ToLower()}");
                    module._origPath = installPath;
                    files.Add(module);
                }
                // Open tracklists
                var tracklist = _fileService.OpenFile(entry.Params.TrackListFile);
                if (tracklist != null)
                {
                    var installPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.TracklistPath, $"{entry.Params.TrackList}.tlst");
                    tracklist._origPath = installPath;
                    files.Add(tracklist);
                }
                // Open bin files
                if (entry.IsRAlt || entry.IsLAlt)
                {
                    var binFile = _fileService.OpenFile(entry.BinFilePath);
                    if (binFile != null)
                    {
                        var index = GetButtonFlagIndex(entry);
                        var fileName = $"st_{index:D2}_{entry.BinFileName}.bin";
                        var folderLetter = entry.IsRAlt ? "R" : (entry.IsLAlt ? "L" : string.Empty);
                        var folderName = $"{stage.Slot.StageIds.StageId.ToString("X2")}_{folderLetter}";
                        var installPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.StageAltListPath, folderName, fileName);
                        binFile._origPath = installPath;
                        files.Add(binFile);
                    }
                }
                // Get substages for install
                foreach (var substage in entry.Params.Substages)
                {
                    var substageFile = _fileService.OpenFile(substage.PacFile);
                    if (substageFile != null)
                    {
                        var installPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.StagePacPath,
                            $"STG{entry.Params.PacName.ToUpper()}_{substage.Name}.pac");
                        substageFile._origPath = installPath;
                        files.Add(substageFile);
                    }
                }
            }
            return files;
        }

        /// <summary>
        /// Delete all stage entries from stage
        /// </summary>
        /// <param name="stage">Stage to remove from</param>
        /// <returns>List of files that can be optionally deleted</returns>
        private List<string> DeleteStageEntries(StageInfo stage)
        {
            var toDelete = new List<string>();
            var buildPath = _settingsService.AppSettings.BuildPath;
            var paramPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.StageParamPath);
            var aslPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.StageSlots);
            foreach(var stageEntry in stage.StageEntries)
            {
                // Delete param files
                var path = Path.Combine(paramPath, $"{stageEntry.Params.Name}.param");
                if (File.Exists(path))
                {
                    _fileService.DeleteFile(path);
                }
                // Delete pac files
                path = stageEntry.Params.PacFile;
                if (File.Exists(path))
                {
                    _fileService.DeleteFile(path);
                }
                // Delete substage files
                foreach(var substage in stageEntry.Params.Substages)
                {
                    path = substage.PacFile;
                    if (File.Exists(path))
                    {
                        _fileService.DeleteFile(path);
                    }
                }
                // Delete soundbank file
                path = stageEntry.Params.SoundBankFile;
                if (File.Exists(path))
                {
                    _fileService.DeleteFile(path);
                }
                // Delete bin file
                path = stageEntry.BinFilePath;
                if (File.Exists(path))
                {
                    _fileService.DeleteFile(path);
                }
                // Add modules and tracklists to delete options
                if (stageEntry.Params.ModuleFile != null && File.Exists(stageEntry.Params.ModuleFile) && !toDelete.Contains(stageEntry.Params.ModuleFile))
                {
                    toDelete.Add(stageEntry.Params.ModuleFile);
                }
                if (stageEntry.Params.TrackListFile != null && File.Exists(stageEntry.Params.TrackListFile) && !toDelete.Contains(stageEntry.Params.TrackListFile))
                {
                    toDelete.Add(stageEntry.Params.TrackListFile);
                }
                // Delete ASL file
                path = Path.Combine(aslPath, $"{stage.Slot.StageIds.StageId:X2}.asl");
                if (File.Exists(path))
                {
                    _fileService.DeleteFile(path);
                }
            }
            return toDelete;
        }

        /// <summary>
        /// Save changes to stage lists
        /// </summary>
        /// <param name="stageLists">Stage lists to save</param>
        /// <param name="stageTable">Stage table to save</param>
        public void SaveStageLists(List<StageList> stageLists, List<StageSlot> stageTable)
        {
            var stageTableText = stageTable.Select(x => $"0x{x.StageIds.StageId:X2}{x.StageIds.StageCosmeticId:X2}").ToList();
            // Update indexes
            foreach (var stageSlot in stageTable)
            {
                stageSlot.Index = stageTable.IndexOf(stageSlot);
            }
            foreach(var stageList in stageLists)
            {
                var filePath = $"{_settingsService.AppSettings.BuildPath}\\{stageList.FilePath}";
                var fileText = File.ReadAllText(filePath);
                foreach (var page in stageList.Pages)
                {
                    // Update stage table
                    fileText = _codeService.ReplaceTable(fileText, "TABLE_STAGES:", stageTableText, DataSize.Halfword, 4);
                    // TODO: Convert int to hex string in CodeService?
                    // Update stage list
                    var pageEntries = page.StageSlots.Select(x => $"0x{x.Index.ToString("X2")}").ToList();
                    fileText = _codeService.ReplaceTable(fileText, $"TABLE_{page.PageNumber}:", pageEntries, DataSize.Byte);
                }
                _fileService.SaveTextFile(filePath, fileText);
            }
        }

        /// <summary>
        /// Save changes to a stage
        /// </summary>
        /// <param name="stage">Stage to save</param>
        /// <returns>List of delete options</returns>
        public List<string> SaveStage(StageInfo stage)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            // Only update cosmetics that have changed
            var changedDefinitions = _settingsService.BuildSettings.CosmeticSettings.Where(x => stage.Cosmetics.ChangedItems
            .Any(y => y.CosmeticType == x.CosmeticType && y.Style == x.Style)).ToList();

            // Import cosmetics
            _cosmeticService.ImportCosmetics(changedDefinitions, stage.Cosmetics, stage.Slot.StageIds);

            // TODO: only do if name is changed
            // Save stage random name
            SaveStageRandomName(stage);

            // TODO: Make sure to update stage list object to use the correct name if stage entries were renamed

            // TODO: To save params, open the existing ASL file, loop through all params. Open the associated param files.
            // Gather up names of files that we might want to delete. Then delete each param file and each node in the ASL file and generate new ones with our
            // changes. Use the names we gathered up to prompt the user on what they might want to delete.
            // Perhaps could add something to check for unused files too.

            // Remove all params that aren't used
            stage.AllParams.RemoveAll(x => !stage.StageEntries.Select(y => y.Params).Contains(x));

            // Update button flags for list alts based on order they are in list
            var rListAlts = stage.StageEntries.Where(x => x.IsRAlt).ToList();
            var lListAlts = stage.StageEntries.Where(x => x.IsLAlt);
            foreach(var alt in rListAlts)
            {
                alt.ButtonFlags = (ushort)(0x4000 + rListAlts.IndexOf(alt));
            }
            foreach (var alt in lListAlts)
            {
                alt.ButtonFlags = (ushort)(0x8000 + rListAlts.IndexOf(alt));
            }

            return SaveStageInfo(stage);
        }
    }
}
