using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlLib.BrawlManagerLib;
using BrawlLib.Internal;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.ResourceNodes.ProjectPlus;
using BrawlLib.Wii.Textures;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using static BrawlLib.BrawlManagerLib.TextureContainer;

namespace BrawlInstaller.Services
{
    public interface IStageService
    {
        /// <inheritdoc cref="StageService.GetStageLists(List{StageSlot})"/>
        List<StageList> GetStageLists(List<StageSlot> stageTable);

        /// <inheritdoc cref="StageService.GetIncompleteStageIds()"/>
        List<int> GetIncompleteStageIds();

        /// <inheritdoc cref="StageService.GetStageSlots()"/>
        List<StageSlot> GetStageSlots();

        /// <inheritdoc cref="StageService.GetStageData(StageInfo)"/>
        StageInfo GetStageData(StageInfo stage);

        /// <inheritdoc cref="StageService.SaveStageLists(List{StageList}, List{StageSlot})"/>
        void SaveStageLists(List<StageList> stageLists, List<StageSlot> stageTable);

        /// <inheritdoc cref="StageService.SaveStage(StageInfo, StageInfo, List{string}, bool)"/>
        void SaveStage(StageInfo stage, StageInfo oldStage, List<string> deleteOptions, bool updateRandomName = true);

        /// <inheritdoc cref="StageService.GetListAlt(string)"/>
        ListAlt GetListAlt(string binFile);
    }

    [Export(typeof(IStageService))]
    internal class StageService : IStageService
    {
        // Services
        ICodeService _codeService;
        ISettingsService _settingsService;
        IFileService _fileService;
        ICosmeticService _cosmeticService;

        // Private properties

        // Table starts at 0x104 and holds 256 pairs, so 512 total bytes
        private const int _rssStageTableStart = 0x104;
        private const int _rssStageTableLength = 512;
        // Pages start at 0x3C and are 40 bytes each
        private List<int> _pageLocations = new List<int> { 0x3C, 0x64, 0x8C, 0xB4, 0xDC };
        private const int _stagesPerPage = 39;

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
            // Get cosmetics
            var cosmetics = _cosmeticService.GetStageCosmetics(stage.Slot.StageIds);
            // Get name for RSS
            var names = GetStageRandomNames();
            if (stage.Slot.StageIds.StageCosmeticId != null && names.Count > stage.Slot.StageIds.StageCosmeticId)
            {
                stage.RandomName = names[stage.Slot.StageIds.StageCosmeticId.Value];
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
                var filePath = $"{_settingsService.AppSettings.BuildPath}\\{stageListFile.Path}";
                if (_fileService.FileExists(filePath))
                {
                    var stageList = new StageList { Name = Path.GetFileNameWithoutExtension(stageListFile.Path), FilePath = filePath.Replace(_settingsService.AppSettings.BuildPath, "") };
                    if (Path.GetExtension(filePath) == ".asm")
                    {
                        // Read all pages from stage list file
                        var fileText = _fileService.ReadTextFile(filePath);
                        var labels = new List<string> { "TABLE_1:", "TABLE_2:", "TABLE_3:", "TABLE_4:", "TABLE_5:" };
                        int pageNumber = 1;
                        foreach (var label in labels)
                        {
                            var page = new StagePage { PageNumber = pageNumber };
                            // Get indexes in table
                            var indexList = _codeService.ReadTable(fileText, label);
                            foreach (var index in indexList)
                            {
                                // Get IDs from index
                                if (int.TryParse(index.Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result))
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
                    }
                    else if (Path.GetExtension(filePath) == ".rss")
                    {
                        var rssData = _fileService.ReadAllBytes(filePath);
                        
                        int pageNumber = 1;
                        foreach(var pageLocation in _pageLocations)
                        {
                            var page = new StagePage { PageNumber = pageNumber };
                            // Get random toggles
                            page.RandomFlags = GetStageRandomToggles(rssData, pageNumber);
                            // Get hazard toggles
                            page.HazardFlags = GetStageHazardToggles(rssData, pageNumber);
                            // Get indexes in page
                            var indexList = new List<int>();
                            int stageCount = rssData[pageLocation];
                            var start = pageLocation + 1;
                            for(var i = start; i < start + stageCount; i++)
                            {
                                indexList.Add(rssData[i]);
                            }
                            foreach (var index in indexList)
                            {
                                var stageSlot = stageTable.FirstOrDefault(x => x.Index == index);
                                if (stageSlot != null)
                                {
                                    page.StageSlots.Add(stageSlot);
                                }
                            }
                            stageList.Pages.Add(page);
                            pageNumber++;
                        }
                    }
                    // Add stage list
                    stageLists.Add(stageList);
                }
            }
            return stageLists;
        }

        /// <summary>
        /// Get stage random toggles
        /// </summary>
        /// <param name="rssData">RSS file data</param>
        /// <param name="pageNumber">Page number to get toggles for</param>
        /// <returns>Stage indexes that are toggled</returns>
        private ulong GetStageRandomToggles(byte[] rssData, int pageNumber)
        {
            var randomToggleList = rssData.SubArray((pageNumber - 1) * 6, 6).Reverse().ToList(); // Each bitarray is 6 bytes long, and there is one for each page
            randomToggleList.AddRange(new byte[2]); // Pad bitarray so we can convert to int64
            var randomToggleBitmask = BitConverter.ToUInt64(randomToggleList.ToArray(), 0);
            return randomToggleBitmask;
        }

        /// <summary>
        /// Get stage hazard toggles
        /// </summary>
        /// <param name="rssData">RSS file data</param>
        /// <param name="pageNumber">Page number to get toggles for</param>
        /// <returns>Stage indexes that are toggled</returns>
        private ulong GetStageHazardToggles(byte[] rssData, int pageNumber)
        {
            var hazardToggleList = rssData.SubArray(((pageNumber - 1) * 6) + 30, 6).Reverse().ToList(); // Hazard toggles start at offset 30
            hazardToggleList.AddRange(new byte[2]);
            var hazardToggleBitmask = BitConverter.ToUInt64(hazardToggleList.ToArray(), 0);
            return hazardToggleBitmask;
        }

        /// <summary>
        /// Get all stage IDs that are missing a corresponding cosmetic ID
        /// </summary>
        /// <returns>List of stage IDs</returns>
        public List<int> GetIncompleteStageIds()
        {
            var ids = new List<int>();
            var path = $"{_settingsService.AppSettings.BuildPath}\\{_settingsService.BuildSettings.FilePathSettings.StageSlots}";
            var files = _fileService.GetFiles(path, "*.asl");
            foreach(var file in files)
            {
                var result = int.TryParse(Path.GetFileNameWithoutExtension(file), NumberStyles.HexNumber, null, out int newId);
                if (result)
                {
                    ids.Add(newId);
                }
            }
            return ids;
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
                    var existingParams = stage.AllParams.FirstOrDefault(x => x.Name == entry.Name);
                    var newEntry = new StageEntry
                    {
                        ButtonFlags = entry.ButtonFlags,
                        Params = existingParams ?? GetStageParams(entry.Name, stage.AllParams)
                    };
                    // Get bin file
                    if (stage.Slot.StageIds.StageId != null)
                    {
                        newEntry.ListAlt = GetListAlt(stage.Slot.StageIds.StageId.Value, newEntry);
                        // Update button flags for list alts, so that stage editor displays them correctly
                        entry.ButtonFlags = (ushort)(entry.ButtonFlags >= 0x8000 ? 0x8000 : entry.ButtonFlags >= 0x4000 ? 0x4000 : entry.ButtonFlags);
                    }
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
        /// <param name="entry">Stage entry</param>
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
                if (_fileService.DirectoryExists(folder))
                {
                    // Bin files correspond to button flags in alphabetical order (e.g. the first file in alphabetical order will be attached to button flag ending in 00
                    var index = GetButtonFlagIndex(entry);
                    var files = _fileService.GetFiles(folder, "*.bin");
                    string file = null;
                    if (index < files.Count())
                        file = files[index];
                    return file;
                }
            }
            return null;
        }

        /// <summary>
        /// Get list alt for stage entry
        /// </summary>
        /// <param name="stageId">ID of stage associated with stage entry</param>
        /// <param name="entry">Stage entry</param>
        /// <returns>List alt</returns>
        private ListAlt GetListAlt(int stageId, StageEntry entry)
        {
            var binFile = GetStageBinFile(stageId, entry);
            return GetListAlt(binFile);
        }

        /// <summary>
        /// Get list alt for stage entry
        /// </summary>
        /// <param name="binFile">Path to bin file</param>
        /// <returns>List alt</returns>
        public ListAlt GetListAlt(string binFile)
        {
            var listAlt = new ListAlt();
            if (!string.IsNullOrEmpty(binFile))
            {
                listAlt.BinFileName = Regex.Replace(Path.GetFileNameWithoutExtension(binFile), "st_\\d+_", "");
                listAlt.BinFilePath = binFile;
                var binData = _fileService.DecryptBinFile(binFile);
                // Get the encoding style
                var encodingByte = binData[0x6B]; // 0x6B is the preview pic setting normally, but we are hijacking the 8th bit of it to denote encoding
                var utf8Encoding = encodingByte.GetToggledBits().Contains(8); // If 8th bit is enabled, it's UTF8
                // Get the name
                var nameData = binData.AsSpan(0x70, 32).ToArray();
                if (utf8Encoding)
                {
                    listAlt.Name = Encoding.UTF8.GetString(nameData).Replace("\u0000", "");
                }
                else
                {
                    listAlt.Name = Encoding.BigEndianUnicode.GetString(nameData).Replace("\u0000", "");
                }
                // Get the image data
                var imageStart = BitConverter.ToInt32(binData.Skip(0x5C).Take(4).Reverse().ToArray(), 0) + 0x54; // position 0x5C + 0x54 is start of image data
                var imageEnd = BitConverter.ToInt32(binData.Skip(0x58).Take(4).Reverse().ToArray(), 0) + 0x34; // position 0x58 + 0x34 is end of image data
                var imageData = binData.AsSpan(imageStart, imageEnd - imageStart);
                var imageHeader = binData.Skip(imageStart).Take(4).ToArray();
                // If JPEG header is found, read as JPEG
                if (imageHeader.Compare(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }))
                {
                    using (MemoryStream stream = new MemoryStream(imageData.ToArray()))
                    {
                        var bitmap = Image.FromStream(stream, true, true);
                        var bitmapImage = ((Bitmap)bitmap).ToBitmapImage(ImageFormat.Jpeg);
                        listAlt.Image = bitmapImage;
                    }
                }
                // If TEX0 header is found, read as TEX0
                else if (imageHeader.Compare(new byte[] { 0x54, 0x45, 0x58, 0x30 }))
                {
                    var tex = imageData.ToArray().ToResourceNode() as TEX0Node;
                    listAlt.Image = tex.GetImage(0).ToBitmapImage();
                    // Get HD texture if available
                    if (!string.IsNullOrEmpty(tex?.DolphinTextureName) && _settingsService.AppSettings.ModifyHDTextures && !string.IsNullOrEmpty(_settingsService.AppSettings.HDTextures))
                    {
                        listAlt.HDImage = _cosmeticService.GetHDImage(tex.DolphinTextureName);
                    }
                }
            }
            return listAlt;
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
            var file = _fileService.GetFiles(path, $"{name}.param").FirstOrDefault();
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
                    stageParams.PacFile = _fileService.GetFiles(pacPath, $"STG{paramNode.StageName.ToUpper()}.pac").FirstOrDefault();
                    stageParams.ModuleFile = _fileService.GetFiles(modulePath, $"{paramNode.Module}").FirstOrDefault();
                    stageParams.TrackListFile = _fileService.GetFiles(tracklistPath, $"{paramNode.TrackList}.tlst").FirstOrDefault();
                    // TODO: handle soundbank styles here too?
                    stageParams.SoundBankFile = _fileService.GetFiles(soundbankPath, $"{paramNode.SoundBank:X3}_{stageParams.PacName}.sawnd").FirstOrDefault();
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
            if (_fileService.FileExists(pacPath))
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
            if (_fileService.FileExists(filePath))
            {
                if (Path.GetExtension(filePath) == ".asm")
                {
                    var fileText = _fileService.ReadTextFile(filePath);
                    var idList = _codeService.ReadTable(fileText, _settingsService.BuildSettings.FilePathSettings.StageTableLabel);
                    foreach (var id in idList)
                    {
                        var newIds = new BrawlIds();
                        var stageId = id.Substring(2, 2);
                        var cosmeticId = id.Substring(4, 2);
                        newIds.StageId = Convert.ToInt32(stageId, 16);
                        newIds.StageCosmeticId = Convert.ToInt32(cosmeticId, 16);
                        stageIds.Add(newIds);
                    }
                }
                else if (Path.GetExtension(filePath) == ".rss")
                {
                    var rssData = _fileService.ReadAllBytes(filePath);
                    
                    for (var i = _rssStageTableStart; i < (_rssStageTableStart + _rssStageTableLength); i += 2)
                    {
                        if (rssData[i] != 0 && rssData[i + 1] != 0)
                        {
                            var newIds = new BrawlIds();
                            newIds.StageId = rssData[i];
                            newIds.StageCosmeticId = rssData[i + 1];
                            stageIds.Add(newIds);
                        }
                    }
                }
            }
            return stageIds;
        }

        /// <summary>
        /// Get preset count for random stage select screen
        /// </summary>
        /// <returns>Preset count</returns>
        private int GetRSSPresetCount()
        {
            var presetCount = 7;
            if (!string.IsNullOrEmpty(_settingsService.BuildSettings.FilePathSettings.RSSFile))
            {
                var rssFile = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.RSSFile);
                if (!string.IsNullOrEmpty(rssFile))
                {
                    var rssText = _codeService.ReadCode(rssFile);
                    var alias = _codeService.GetCodeAlias(rssText, "NUM_PRESETS");
                    if (alias != null)
                    {
                        var valid = int.TryParse(alias.Value.Replace("0x", ""), NumberStyles.HexNumber, null, out int count);
                        if (valid)
                        {
                            presetCount = count;
                        }
                    }
                }
            }
            return presetCount;
        }

        /// <summary>
        /// Update names for stages on RSS
        /// </summary>
        /// <param name="stage"></param>
        private void SaveStageRandomName(StageInfo stage)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            // Get preset count
            var presetCount = GetRSSPresetCount();
            var randomStageLocations = _settingsService.BuildSettings.FilePathSettings.RandomStageNamesLocations;
            foreach(var location in randomStageLocations)
            {
                var path = $"{buildPath}\\{location.FilePath}";
                var rootNode = _fileService.OpenFile(path);
                if (rootNode != null && stage.Slot.StageIds.StageCosmeticId != null)
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
                        names[stage.Slot.StageIds.StageCosmeticId.Value + presetCount] = stage.RandomName;
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
            var presetCount = GetRSSPresetCount();
            var names = new List<string>();
            var buildPath = _settingsService.AppSettings.BuildPath;
            var randomStageLocation = _settingsService.BuildSettings.FilePathSettings.RandomStageNamesLocations.FirstOrDefault();
            var path = $"{buildPath}\\{randomStageLocation?.FilePath}";
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
        public void SaveStageInfo(StageInfo stage, StageInfo oldStageData, List<string> deleteOptions)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            // Get files for install
            // We do this before deleting old files in case the user kept an old file
            var files = OpenStageFiles(stage);
            // Delete old stage data
            DeleteStageEntries(oldStageData, deleteOptions);
            // Generate ASL and param files
            SaveStageEntries(stage);
            // Save files
            foreach(var file in files)
            {
                _fileService.SaveFile(file);
                _fileService.CloseFile(file);
            }
            // Update slot name
            stage.Slot.Name = stage.StageEntries.FirstOrDefault()?.Params?.Name ?? "Unknown";
        }

        /// <summary>
        /// Save params and ASL for stage
        /// </summary>
        /// <param name="stage">Stage to save</param>
        private void SaveStageEntries(StageInfo stage)
        {
            if (stage.StageEntries.Count > 0)
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
                    entry.Params.PacFile = installPath;
                }
                // Open soundbanks
                var soundbank = _fileService.OpenFile(entry.Params.SoundBankFile);
                if (soundbank != null)
                {
                    var installPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.SoundbankPath, $"{entry.Params.SoundBank:X3}_{entry.Params.PacName}.sawnd");
                    soundbank._origPath = installPath;
                    files.Add(soundbank);
                    entry.Params.SoundBankFile = installPath;
                }
                // Open modules
                var module = _fileService.OpenFile(entry.Params.ModuleFile);
                if (module != null)
                {
                    var installPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.Modules, $"{entry.Params.Module.ToLower()}");
                    module._origPath = installPath;
                    files.Add(module);
                    entry.Params.ModuleFile = installPath;
                }
                // Open tracklists
                var tracklist = _fileService.OpenFile(entry.Params.TrackListFile);
                if (tracklist != null)
                {
                    var installPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.TracklistPath, $"{entry.Params.TrackList}.tlst");
                    tracklist._origPath = installPath;
                    files.Add(tracklist);
                    entry.Params.TrackListFile = installPath;
                }
                // Open bin files
                if (entry.IsRAlt || entry.IsLAlt)
                {
                    var binFile = _fileService.OpenFile(entry.ListAlt.BinFilePath);
                    if (binFile != null && entry.ListAlt.Image != null)
                    {
                        UpdateBinFile(binFile, entry.ListAlt);
                        var index = GetButtonFlagIndex(entry);
                        var fileName = $"st_{index:D2}_{entry.ListAlt.BinFileName}.bin";
                        var folderLetter = entry.IsRAlt ? "R" : (entry.IsLAlt ? "L" : string.Empty);
                        var folderName = $"{stage.Slot.StageIds.StageId?.ToString("X2")}_{folderLetter}";
                        var installPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.StageAltListPath, folderName, fileName);
                        binFile._origPath = installPath;
                        files.Add(binFile);
                        entry.ListAlt.BinFilePath = installPath;
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
                        substage.PacFile = installPath;
                    }
                }
            }
            return files;
        }

        /// <summary>
        /// Update bin file with list alt parameters
        /// </summary>
        /// <param name="node">Node of bin file to update</param>
        /// <param name="listAlt">List alt to apply changes from</param>
        /// <returns>Updated node</returns>
        private void UpdateBinFile(ResourceNode node, ListAlt listAlt)
        {
            var data = _fileService.ReadRawData(node);
            if (data != null)
            {
                var originalData = _fileService.DecryptBinData(data);
                var decryptedData = originalData.ToArray();
                // Update encoding flag
                var currentEncoding = decryptedData[0x6B];
                if (_settingsService.BuildSettings.MiscSettings.BinUTF8Encoding)
                {
                    decryptedData[0x6B] = currentEncoding.EnableBit(7); // If we're using UTF8 encoding, enable the 8th bit to indicate such
                }
                else
                {
                    decryptedData[0x6B] = currentEncoding.DisableBit(7); // Otherwise, disable it, indicating UTF16/BigEndianUnicode
                }
                // Get name
                var nameStart = 0x70;
                var nameEnd = nameStart + 32;
                var name = new List<byte>();
                if (_settingsService.BuildSettings.MiscSettings.BinUTF8Encoding)
                {
                    name = Encoding.UTF8.GetBytes(listAlt.Name.ToCharArray()).ToList();
                }
                else
                {
                    name = Encoding.BigEndianUnicode.GetBytes(listAlt.Name.ToCharArray()).ToList();
                }
                // Pad name
                while (name.Count < 32)
                {
                    name.Add(0x0);
                }
                // Get beginning of file
                var fileStart = decryptedData.AsSpan(0, nameStart);
                // Get image location
                var imageStart = BitConverter.ToInt32(decryptedData.Skip(0x5C).Take(4).Reverse().ToArray(), 0) + 0x54; // position 0x5C + 0x54 is start of image data
                var imageEnd = BitConverter.ToInt32(decryptedData.Skip(0x58).Take(4).Reverse().ToArray(), 0) + 0x34; // position 0x58 + 0x34 is end of image data
                // Get current image
                var currentImageData = decryptedData.Skip(imageStart).Take(imageEnd - imageStart).ToArray();
                if (_settingsService.BuildSettings.MiscSettings.RGBA8Thumbnails)
                {
                    var currentTex = currentImageData.ToResourceNode() as TEX0Node;
                    // Delete HD texture if it exists
                    if (currentTex != null && !string.IsNullOrEmpty(_settingsService.AppSettings.HDTextures) && _settingsService.AppSettings.ModifyHDTextures)
                    {
                        _cosmeticService.DeleteHDTexture(currentTex);
                    }
                }
                // Get data between name and image
                var miscData = decryptedData.AsSpan(nameEnd, imageStart - nameEnd);
                // Get image data from our list alt
                var imageData = new byte[0];
                if (_settingsService.BuildSettings.MiscSettings.RGBA8Thumbnails)
                {
                    imageData = _cosmeticService.ImportBinThumbnail(listAlt.Image, listAlt.HDImage, WiiPixelFormat.RGBA8, new ImageSize(160, 120));
                }
                else
                {
                    imageData = listAlt.JpegData;
                }
                // Combine data leading up to image
                var partialData = fileStart.ToArray().Append(name.ToArray()).Append(miscData.ToArray());
                // Store image starting position
                var newImageStart = BitConverter.GetBytes(partialData.Length - 0x54).Reverse().ToArray(); // Starting position has 0x54 subtracted for some reason
                // Add image data
                var newData = partialData.Append(imageData).ToList();
                // If data is not multiple of 16, pad it
                while (newData.Count % 16 != 0)
                {
                    newData.Add(0);
                }
                // Store image ending position
                var newImageEnd = BitConverter.GetBytes(newData.Count - 0x34).Reverse().ToArray(); // Ending position has 0x34 subtracted for some reason
                // Pad with 32 0x0s
                newData.AddRange(Enumerable.Repeat((byte)0x0, 32).ToList());
                // Convert to byte array
                var finalData = newData.ToArray();
                // Update image length fields
                newImageEnd.CopyTo(finalData, 0x58); // Ending position
                newImageStart.CopyTo(finalData, 0x5C); // Starting position
                newImageEnd.CopyTo(finalData, 0x60); // Ending position (wide)
                // Encrypt
                if (!finalData.SequenceEqual(originalData))
                {
                    var encryptedData = _fileService.EncryptBinData(finalData);
                    _fileService.ReplaceNodeRaw(node, encryptedData);
                }
            }
        }

        /// <summary>
        /// Delete all stage entries from stage
        /// </summary>
        /// <param name="stage">Stage to remove from</param>
        /// <param name="deleteOptions">Selected items to delete</param>
        /// <returns>List of files that can be optionally deleted</returns>
        private void DeleteStageEntries(StageInfo stage, List<string> deleteOptions)
        {
            if (stage != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var paramPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.StageParamPath);
                var aslPath = Path.Combine(buildPath, _settingsService.BuildSettings.FilePathSettings.StageSlots);
                foreach (var stageEntry in stage.StageEntries)
                {
                    // Delete param files
                    var path = Path.Combine(paramPath, $"{stageEntry.Params.Name}.param");
                    _fileService.DeleteFile(path);
                    // Delete substage files
                    foreach (var substage in stageEntry.Params.Substages)
                    {
                        path = substage.PacFile;
                        _fileService.DeleteFile(path);
                    }
                    // Delete soundbank file
                    path = stageEntry.Params.SoundBankFile;
                    _fileService.DeleteFile(path);
                    // Delete bin file
                    path = stageEntry.ListAlt.BinFilePath;
                    _fileService.DeleteFile(path);
                    // Delete selected options
                    foreach (var file in deleteOptions)
                    {
                        _fileService.DeleteFile(file);
                    }
                    // Delete ASL file
                    path = Path.Combine(aslPath, $"{stage.Slot.StageIds.StageId:X2}.asl");
                    _fileService.DeleteFile(path);
                }
            }
        }

        /// <summary>
        /// Save changes to stage lists
        /// </summary>
        /// <param name="stageLists">Stage lists to save</param>
        /// <param name="stageTable">Stage table to save</param>
        public void SaveStageLists(List<StageList> stageLists, List<StageSlot> stageTable)
        {
            // Add dummy slots
            // TODO: Is this how we should handle this? Do we need this?
            var dummySlot = new StageSlot { Name = "NOTHING", StageIds = new BrawlIds { StageId = 0xFF, StageCosmeticId = 0x64 } };
            stageTable.Insert(41, dummySlot);
            stageTable.Insert(42, dummySlot);
            // Update stage table
            var stageTableAsm = stageTable.ConvertToAsmTable();
            var tableFilepath = $"{Path.Combine(_settingsService.AppSettings.BuildPath, _settingsService.BuildSettings.FilePathSettings.StageTablePath)}";
            if (_fileService.FileExists(tableFilepath))
            {
                if (Path.GetExtension(tableFilepath) == ".asm")
                {
                    var tableFileText = _codeService.ReadCode(tableFilepath);
                    tableFileText = _codeService.ReplaceTable(tableFileText, _settingsService.BuildSettings.FilePathSettings.StageTableLabel, stageTableAsm, DataSize.Halfword, 4);
                    _fileService.SaveTextFile(tableFilepath, tableFileText);
                }
                else if (Path.GetExtension(tableFilepath) == ".rss")
                {
                    var rssData = _fileService.ReadAllBytes(tableFilepath);
                    var bytes = stageTable.ToByteArray();
                    bytes.CopyTo(rssData, _rssStageTableStart);
                    _fileService.WriteAllBytes(tableFilepath, rssData);
                }
            }
            // Update indexes
            foreach (var stageSlot in stageTable)
            {
                stageSlot.Index = stageTable.IndexOf(stageSlot);
            }
            foreach(var stageList in stageLists)
            {
                var filePath = $"{_settingsService.AppSettings.BuildPath}\\{stageList.FilePath}";
                if (Path.GetExtension(filePath) == ".asm")
                {
                    var fileText = _codeService.ReadCode(filePath);
                    // Update stage table if it exists
                    fileText = _codeService.ReplaceTable(fileText, _settingsService.BuildSettings.FilePathSettings.StageTableLabel, stageTableAsm, DataSize.Halfword, 4);
                    foreach (var page in stageList.Pages)
                    {
                        // Update stage list
                        var pageEntriesAsm = page.ConvertToAsmTable();
                        fileText = _codeService.ReplaceTable(fileText, $"TABLE_{page.PageNumber}:", pageEntriesAsm, DataSize.Byte);
                        // Update memory allocations
                        var hookAddress = string.Empty;
                        switch (page.PageNumber)
                        {
                            case 1:
                                hookAddress = "806B929C";
                                break;
                            case 2:
                                hookAddress = "806B92A4";
                                break;
                            case 3:
                                hookAddress = "80496002";
                                break;
                            case 4:
                                hookAddress = "80496003";
                                break;
                            case 5:
                                hookAddress = "80496004";
                                break;
                        }
                        var hook = new AsmHook { Address = hookAddress, Instructions = new List<Instruction> { new Instruction { Text = $"byte {page.StageSlots.Count:D2}" } }, Comment = $"Page {page.PageNumber}" };
                        fileText = _codeService.ReplaceHook(hook, fileText);
                    }
                    // Update total stage count
                    var countHook = new AsmHook { Address = "800AF673", Instructions = new List<Instruction> { new Instruction { Text = $"byte {stageTable.Count(x => x != dummySlot):D2}" } }, Comment = "Stage Count" };
                    fileText = _codeService.ReplaceHook(countHook, fileText);
                    _fileService.SaveTextFile(filePath, fileText);
                }
                else if (Path.GetExtension(filePath) == ".rss")
                {
                    var rssData = _fileService.ReadAllBytes(filePath);
                    // Update stage table if it exists
                    var tableBytes = stageTable.ToByteArray();
                    tableBytes.CopyTo(rssData, _rssStageTableStart);

                    foreach (var page in stageList.Pages)
                    {
                        // Update stage page
                        var location = _pageLocations[stageList.Pages.IndexOf(page)];
                        var bytes = page.StageSlots.Select(x => (byte)x.Index).Concat(Enumerable.Repeat<byte>(0, _stagesPerPage - page.StageSlots.Count)).ToArray();
                        rssData[location] = (byte)page.StageSlots.Count;
                        bytes.CopyTo(rssData, location + 1);
                        // Update random toggles
                        var randomBytes = BitConverter.GetBytes(page.RandomFlags).SubArray(0, 6).Reverse().ToArray();
                        randomBytes.CopyTo(rssData, (page.PageNumber - 1) * 6);
                        // Update hazard toggles
                        var hazardBytes = BitConverter.GetBytes(page.HazardFlags).SubArray(0, 6).Reverse().ToArray();
                        hazardBytes.CopyTo(rssData, 30 + ((page.PageNumber - 1) * 6)); // Offset 30 is where hazard toggles start
                    }
                    _fileService.WriteAllBytes(filePath, rssData);
                }
            }
            // Remove dummy slots
            // TODO: Remove this if we don't need the dummy slots to begin with
            stageTable.RemoveAll(x => x == dummySlot);
            // Compile code
            _codeService.CompileCodes();
        }

        /// <summary>
        /// Save changes to a stage
        /// </summary>
        /// <param name="stage">Stage to save</param>
        /// <returns>List of delete options</returns>
        public void SaveStage(StageInfo stage, StageInfo oldStage, List<string> deleteOptions, bool updateRandomName = true)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            // Only update cosmetics that have changed
            var changedDefinitions = _settingsService.BuildSettings.CosmeticSettings.Where(x => stage.Cosmetics.ChangedItems
            .Any(y => y.CosmeticType == x.CosmeticType && y.Style == x.Style)).ToList();

            // Import cosmetics
            _cosmeticService.ImportCosmetics(changedDefinitions, stage.Cosmetics, stage.Slot.StageIds);

            // Save stage random name
            if (updateRandomName)
            {
                SaveStageRandomName(stage);
            }

            // TODO: Make sure to update stage list object to use the correct name if stage entries were renamed

            // Remove all params that aren't used
            stage.AllParams.RemoveAll(x => !stage.StageEntries.Select(y => y.Params).Contains(x));

            // Update button flags for list alts based on order they are in list
            var rListAlts = stage.StageEntries.Where(x => x.IsRAlt).ToList();
            var lListAlts = stage.StageEntries.Where(x => x.IsLAlt).ToList();
            var eventAlts = stage.StageEntries.Where(x => x.IsEventStage).ToList();
            foreach(var alt in rListAlts)
            {
                alt.ButtonFlags = (ushort)(0x4000 + rListAlts.IndexOf(alt));
            }
            foreach (var alt in lListAlts)
            {
                alt.ButtonFlags = (ushort)(0x8000 + lListAlts.IndexOf(alt));
            }
            foreach (var alt in eventAlts)
            {
                alt.ButtonFlags = (ushort)(0xC000 + eventAlts.IndexOf(alt));
            }
            SaveStageInfo(stage, oldStage, deleteOptions);
        }
    }
}
