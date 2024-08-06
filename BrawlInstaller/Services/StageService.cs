﻿using BrawlInstaller.Classes;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.ResourceNodes.ProjectPlus;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface IStageService
    {
        /// <inheritdoc cref="StageService.GetStageLists()"/>
        List<StageList> GetStageLists();

        /// <inheritdoc cref="StageService.GetStageData(StageInfo)"/>
        StageInfo GetStageData(StageInfo stage);

        /// <inheritdoc cref="StageService.SaveStage(StageInfo)"/>
        void SaveStage(StageInfo stage);
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
            var cosmetics = _cosmeticService.GetStageCosmetics(stage.Slot.StageIds);
            var names = GetStageRandomNames();
            if (names.Count > stage.Slot.StageIds.StageCosmeticId)
            {
                stage.RandomName = names[stage.Slot.StageIds.StageCosmeticId];
            }
            else
            {
                stage.RandomName = string.Empty;
            }
            //stage.Cosmetics.Items = cosmetics.Where(x => !x.SelectionOption).ToList();
            //stage.Cosmetics.SelectableOptions = cosmetics.Where(x => x.SelectionOption).ToList();
            stage.Cosmetics.Items = cosmetics;
            return stage;
        }

        //TODO: probably split this up, ViewModel should call GetStageSlots first and pass it in here so that we can update both from ViewModel
        /// <summary>
        /// Get stage lists from build
        /// </summary>
        /// <returns>List of stage lists in build</returns>
        public List<StageList> GetStageLists()
        {
            var stageLists = new List<StageList>();
            // Get stage table
            var stageTable = GetStageSlots();
            // Iterate through each stage list file
            foreach(var stageListFile in _settingsService.BuildSettings.FilePathSettings.StageListPaths)
            {
                var filePath = $"{_settingsService.AppSettings.BuildPath}\\{stageListFile}";
                if (File.Exists(filePath))
                {
                    var stageList = new StageList { Name = Path.GetFileNameWithoutExtension(stageListFile) };
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
                    // Get unused stages
                    // TODO: Should this be done in ViewModel instead so it's easier to change?
                    var stageSlots = stageList.Pages.SelectMany(z => z.StageSlots).ToList();
                    stageList.UnusedSlots = stageTable.Where(x => !stageSlots.Contains(x)).ToList();
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
        private List<StageSlot> GetStageSlots()
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
                        StageEntries = new List<StageEntry>(),
                        Index = stageIds.IndexOf(idPair)
                    };
                    foreach(ASLSEntryNode entry in node.Children)
                    {
                        stageSlot.StageEntries.Add(new StageEntry
                        {
                            Name = entry.Name,
                            ButtonFlags = entry.ButtonFlags
                        });
                    }
                    stageSlots.Add(stageSlot);
                }
            }
            return stageSlots;
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
        /// Save changes to a stage
        /// </summary>
        /// <param name="stage">Stage to save</param>
        public void SaveStage(StageInfo stage)
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
        }
    }
}
