using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BrawlInstaller.Services
{
    public interface ITrophyService
    {
        /// <inheritdoc cref="TrophyService.GetTrophyList()"/>
        List<Trophy> GetTrophyList();

        /// <inheritdoc cref="TrophyService.LoadTrophyData(Trophy)"/>
        Trophy LoadTrophyData(Trophy trophy);

        /// <inheritdoc cref="TrophyService.GetTrophyGameIcons()"/>
        List<TrophyGameIcon> GetTrophyGameIcons();

        /// <inheritdoc cref="TrophyService.SaveTrophy(Trophy, Trophy, bool)"/>
        Trophy SaveTrophy(Trophy trophy, Trophy oldTrophy, bool addTrophy = true);

        /// <inheritdoc cref="TrophyService.SaveTrophy(Trophy, Trophy, bool)"/>
        void SaveTrophyList(List<Trophy> trophyList);

        /// <inheritdoc cref="TrophyService.GetUnusedTrophyIds(BrawlIds)"/>
        BrawlIds GetUnusedTrophyIds(BrawlIds ids);
    }

    [Export(typeof(ITrophyService))]
    internal class TrophyService : ITrophyService
    {
        // Services
        ISettingsService _settingsService { get; }
        IFileService _fileService { get; }
        ICosmeticService _cosmeticService { get; }

        [ImportingConstructor]
        public TrophyService(ISettingsService settingsService, IFileService fileService, ICosmeticService cosmeticService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _cosmeticService = cosmeticService;
        }

        // Methods

        /// <summary>
        /// Get list of trophies in build
        /// </summary>
        /// <returns>List of trophies</returns>
        public List<Trophy> GetTrophyList()
        {
            var trophyList = new List<Trophy>();
            var trophyPath = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyLocation.Path);
            var nodePath = _settingsService.BuildSettings.FilePathSettings.TrophyLocation.NodePath;
            if (!string.IsNullOrEmpty(trophyPath))
            {
                var rootNode = _fileService.OpenFile(trophyPath);
                if (rootNode != null)
                {
                    var trophyNodeList = rootNode.FindChild(nodePath) as TyDataListNode;
                    if (trophyNodeList != null)
                    {
                        trophyList = trophyNodeList.Children.Select(x => ((TyDataListEntryNode)x).ToTrophy()).ToList();
                    }
                    _fileService.CloseFile(rootNode);
                }
            }
            return trophyList;
        }

        /// <summary>
        /// Load full data for a trophy
        /// </summary>
        /// <param name="trophy">Trophy to load full data for</param>
        /// <returns>Trophy</returns>
        public Trophy LoadTrophyData(Trophy trophy)
        {
            var trophyPath = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyLocation.Path);
            var trophyNamePath = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyNames);
            var trophyDescriptionPath = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyDescriptions);
            var nodePath = _settingsService.BuildSettings.FilePathSettings.TrophyLocation.NodePath;
            // Get trophy data
            var rootNode = _fileService.OpenFile(trophyPath);
            if (rootNode != null )
            {
                var trophyNodeList = rootNode.FindChild(nodePath) as TyDataListNode;
                if (trophyNodeList != null)
                {
                    var trophyNode = trophyNodeList.Children.FirstOrDefault(x => ((TyDataListEntryNode)x).Id == trophy.Ids.TrophyId && ((TyDataListEntryNode)x).Name == trophy.Name) as TyDataListEntryNode;
                    if (trophyNode != null)
                    {
                        trophy = trophyNode.ToTrophy();
                    }
                }
                _fileService.CloseFile(rootNode);
            }
            // Get trophy name
            var namesNode = _fileService.OpenFile(trophyNamePath) as MSBinNode; 
            if (namesNode != null)
            {
                if (trophy.NameIndex != null && namesNode._strings.Count > trophy.NameIndex)
                {
                    trophy.DisplayName = namesNode._strings[trophy.NameIndex.Value];
                }
                // Get game names
                if (trophy.GameIndex != null && namesNode._strings.Count > trophy.GameIndex)
                {
                    var gameString = namesNode._strings[trophy.GameIndex.Value];
                    var gameNames = gameString.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
                    trophy.GameName1 = gameNames.FirstOrDefault();
                    trophy.GameName2 = gameNames.Count > 1 ? gameNames.LastOrDefault() : string.Empty;
                }
                _fileService.CloseFile(namesNode);
            }
            // Get trophy description
            var descriptionNode = _fileService.OpenFile(trophyDescriptionPath) as MSBinNode;
            if (descriptionNode != null)
            {
                if (trophy.DescriptionIndex != null && descriptionNode._strings.Count > trophy.DescriptionIndex)
                {
                    trophy.Description = descriptionNode._strings[trophy.DescriptionIndex.Value];
                }
                _fileService.CloseFile(descriptionNode);
            }
            // Get BRRES
            var trophyBrresPath = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyBrresLocation);
            var files = _fileService.GetFiles(trophyBrresPath, "*.brres");
            var brres = files.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == trophy.Brres);
            trophy.BrresFile = brres;
            // Get thumbnail
            var cosmetics = _cosmeticService.GetTrophyCosmetics(trophy.Ids);
            trophy.Thumbnails.Items = cosmetics;
            return trophy;
        }

        /// <summary>
        /// Get trophy game icons in build
        /// </summary>
        /// <returns>List of trophy game icons</returns>
        public List<TrophyGameIcon> GetTrophyGameIcons()
        {
            var gameIcons = new List<TrophyGameIcon>();
            var trophyGameIconPath = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyGameIconsLocation.Path);
            var rootNode = _fileService.OpenFile(trophyGameIconPath);
            if (rootNode != null)
            {
                var pat0 = rootNode.FindChild(_settingsService.BuildSettings.FilePathSettings.TrophyGameIconsLocation.NodePath);
                if (pat0 != null)
                {
                    foreach(PAT0TextureEntryNode child in pat0.Children)
                    {
                        var newGameIcon = new TrophyGameIcon();
                        newGameIcon.Id = (int)child.FrameIndex;
                        newGameIcon.Image = child.GetImage(0)?.ToBitmapImage();
                        gameIcons.Add(newGameIcon);
                    }
                }
                _fileService.CloseFile(rootNode);
            }
            return gameIcons;
        }

        /// <summary>
        /// Save a trophy to the build
        /// </summary>
        /// <param name="trophy">Trophy to save</param>
        /// <param name="oldTrophy">Old trophy to remove/replace</param>
        /// <param name="addTrophy">Whether or not to add the new trophy</param>
        /// <returns>Updated trophy</returns>
        public Trophy SaveTrophy(Trophy trophy, Trophy oldTrophy, bool addTrophy = true)
        {
            var trophyPath = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyLocation.Path);
            var trophyNamePath = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyNames);
            var trophyDescriptionPath = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyDescriptions);
            var nodePath = _settingsService.BuildSettings.FilePathSettings.TrophyLocation.NodePath;

            // Get old trophy indexes and stuff to ensure correctness
            var rootNode = _fileService.OpenFile(trophyPath);
            if (rootNode != null)
            {
                var trophyNodeList = rootNode.FindChild(nodePath) as TyDataListNode;
                if (trophyNodeList != null)
                {
                    if (oldTrophy != null)
                    {
                        var trophyNode = trophyNodeList.Children.FirstOrDefault(x => ((TyDataListEntryNode)x).Id == oldTrophy.Ids.TrophyId && ((TyDataListEntryNode)x).Name == oldTrophy.Name) as TyDataListEntryNode;
                        if (trophyNode != null)
                        {
                            oldTrophy.Ids.TrophyId = trophyNode.Id;
                            oldTrophy.Ids.TrophyThumbnailId = trophyNode.ThumbnailIndex;
                            oldTrophy.NameIndex = trophyNode.NameIndex;
                            oldTrophy.GameIndex = trophyNode.GameIndex;
                            oldTrophy.DescriptionIndex = trophyNode.DescriptionIndex;
                            oldTrophy.SeriesIndex = trophyNode.SeriesIndex;
                            oldTrophy.CategoryIndex = trophyNode.CategoryIndex;
                        }
                    }
                }
            }

            // Save cosmetics
            if (trophy != null)
            {
                var changedDefinitions = _settingsService.BuildSettings.CosmeticSettings.Where(x => trophy.Thumbnails.ChangedItems.Any(y => y.CosmeticType == x.CosmeticType)).ToList();
                _cosmeticService.ImportCosmetics(changedDefinitions, trophy.Thumbnails, trophy.Ids);
            }

            // If old trophy thumbnail ID was different, remove old trophy cosmetics
            if (oldTrophy?.Ids?.TrophyThumbnailId != null && oldTrophy?.Ids?.TrophyThumbnailId != trophy?.Ids?.TrophyThumbnailId)
            {
                var changedDefinitions = _settingsService.BuildSettings.CosmeticSettings.Where(x => oldTrophy.Thumbnails.Items.Any(y => y.CosmeticType == x.CosmeticType)).ToList();
                _cosmeticService.ImportCosmetics(changedDefinitions, new CosmeticList(), oldTrophy.Ids);
            }

            // Open BRRES in case it gets deleted
            var brres = _fileService.OpenFile(trophy?.BrresFile);
            // Replace old BRRES
            if (!string.IsNullOrEmpty(_settingsService.BuildSettings.FilePathSettings.TrophyBrresLocation))
            {
                if (!string.IsNullOrEmpty(oldTrophy?.BrresFile) && oldTrophy.BrresFile.Contains(_settingsService.AppSettings.BuildPath))
                {
                    _fileService.DeleteFile(oldTrophy?.BrresFile);
                }
                if (addTrophy && trophy != null)
                {
                    var trophyBrresFolder = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyBrresLocation);
                    var newTrophyBrresPath = Path.Combine(trophyBrresFolder, $"{trophy.Brres}.brres");
                    if (brres != null)
                    {
                        brres._origPath = newTrophyBrresPath;
                        trophy.BrresFile = newTrophyBrresPath;
                        _fileService.SaveFile(brres);
                    }
                }
            }
            _fileService.CloseFile(brres);

            // Update trophy name
            var nameIndex = ReplaceMsBinString(addTrophy ? trophy?.DisplayName : null, trophyNamePath, trophy?.NameIndex, oldTrophy?.NameIndex);

            // Update trophy game names
            var gameNameString = !string.IsNullOrEmpty(trophy?.GameName2) ? $"{trophy?.GameName1}\r\n{trophy?.GameName2}" : trophy?.GameName1;
            var gameIndex = ReplaceMsBinString(addTrophy ? gameNameString : null, trophyNamePath, trophy?.GameIndex - (nameIndex == null ? 1 : 0), oldTrophy?.GameIndex - (nameIndex == null ? 1 : 0));

            // Update trophy description
            var descriptionIndex = ReplaceMsBinString(addTrophy ? trophy?.Description : null, trophyDescriptionPath, trophy?.DescriptionIndex, oldTrophy?.DescriptionIndex);

            // Update trophy values
            if (trophy != null)
            {
                trophy.NameIndex = nameIndex;
                trophy.GameIndex = gameIndex;
                trophy.DescriptionIndex = descriptionIndex;
            }

            // Update trophy data
            if (rootNode != null)
            {
                var trophyNodeList = rootNode.FindChild(nodePath) as TyDataListNode;
                if (trophyNodeList != null)
                {
                    var index = -1;
                    // Delete old trophy node if it exists
                    if (oldTrophy != null)
                    {
                        var trophyNode = trophyNodeList.Children.FirstOrDefault(x => ((TyDataListEntryNode)x).Id == oldTrophy.Ids.TrophyId && ((TyDataListEntryNode)x).Name == oldTrophy.Name) as TyDataListEntryNode;
                        if (trophyNode != null)
                        {
                            index = trophyNode.Index;
                            trophyNodeList.RemoveChild(trophyNode);
                        }
                    }
                    // Insert new trophy node
                    if (trophy != null && addTrophy)
                    {
                        var newTrophyNode = trophy.ToNode();
                        // Get new index if necessary
                        if (index == -1)
                        {
                            index = trophyNodeList.Children.Count > 1 ? trophyNodeList.Children.Count - 1 : trophyNodeList.Children.Count;
                        }
                        trophyNodeList.InsertChild(newTrophyNode, index);
                    }
                    // If trophy was deleted, update other trophies' indexes
                    foreach(TyDataListEntryNode trophyNode in trophyNodeList.Children)
                    {
                        if (trophy?.NameIndex == null && trophyNode.NameIndex > oldTrophy.NameIndex)
                        {
                            trophyNode.NameIndex--;
                            if (trophyNode.NameIndex >= oldTrophy.GameIndex)
                            {
                                trophyNode.NameIndex--;
                            }
                        }
                        if (trophy?.GameIndex == null && trophyNode.GameIndex > oldTrophy.GameIndex)
                        {
                            trophyNode.GameIndex--;
                            if (trophyNode.GameIndex >= oldTrophy.NameIndex)
                            {
                                trophyNode.GameIndex--;
                            }
                        }
                        if (trophy?.DescriptionIndex == null && trophyNode.DescriptionIndex > oldTrophy.DescriptionIndex)
                        {
                            trophyNode.DescriptionIndex--;
                        }
                    }
                    _fileService.SaveFile(rootNode);
                }
                _fileService.CloseFile(rootNode);
            }
            return trophy;
        }

        /// <summary>
        /// Replace trophy-related string in MsBin file
        /// </summary>
        /// <param name="newString">String to insert</param>
        /// <param name="file">File to replace string in</param>
        /// <param name="index">Index to place new string</param>
        /// <param name="oldIndex">Index of old string</param>
        /// <returns>Index of new trophy string</returns>
        private int? ReplaceMsBinString(string newString, string file, int? index, int? oldIndex)
        {
            var rootNode = _fileService.OpenFile(file) as MSBinNode;
            if (rootNode != null)
            {
                // Delete old string
                if (oldIndex != null && rootNode._strings.Count > oldIndex)
                {
                    rootNode._strings.RemoveAt((int)oldIndex);
                }
                // Get new index if necessary
                if (index == null)
                {
                    if (oldIndex != null)
                    {
                        index = oldIndex;
                    }
                    else
                    {
                        index = rootNode._strings.Count;
                    }
                }
                // Insert new string
                if (newString != null)
                {
                    rootNode._strings.Insert((int)index, newString);
                }
                else
                {
                    index = null;
                }
                rootNode.IsDirty = true;
                _fileService.SaveFile(rootNode);
                _fileService.CloseFile(rootNode);
            }
            return index;
        }

        /// <summary>
        /// Save a list of trophies
        /// </summary>
        /// <param name="trophyList">Trophies to save</param>
        public void SaveTrophyList(List<Trophy> trophyList)
        {
            var trophyPath = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyLocation.Path);
            var nodePath = _settingsService.BuildSettings.FilePathSettings.TrophyLocation.NodePath;
            var rootNode = _fileService.OpenFile(trophyPath);
            if (rootNode != null)
            {
                var trophyNodeList = rootNode.FindChild(nodePath) as TyDataListNode;
                if (trophyNodeList != null)
                {
                    trophyNodeList.Children.Clear();
                    foreach(var trophy in trophyList)
                    {
                        trophyNodeList.AddChild(trophy.ToNode());
                    }
                    _fileService.SaveFile(rootNode);
                }
                _fileService.CloseFile(rootNode);
            }
        }

        /// <summary>
        /// Get first unused trophy IDs
        /// </summary>
        /// <param name="ids">BrawlIds to update</param>
        /// <returns>Updated IDs</returns>
        public BrawlIds GetUnusedTrophyIds(BrawlIds ids)
        {
            if (ids != null)
            {
                List<Trophy> trophyList = null;
                // Get IDs if they aren't there
                if (ids.TrophyId == null)
                {
                    trophyList = GetTrophyList();
                    ids.TrophyId = 631; // 631 is first custom trophy ID
                    while (trophyList.Any(x => x.Ids.TrophyId == ids.TrophyId))
                    {
                        ids.TrophyId++;
                    }
                }
                if (ids.TrophyThumbnailId == null)
                {
                    if (trophyList == null)
                    {
                        trophyList = GetTrophyList();
                    }
                    ids.TrophyThumbnailId = 631; // 631 is first custom trophy ID
                    while (trophyList.Any(x => x.Ids.TrophyThumbnailId == ids.TrophyThumbnailId))
                    {
                        ids.TrophyThumbnailId++;
                    }
                }
            }
            return ids;
        }
    }
}
