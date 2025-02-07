using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
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

        /// <inheritdoc cref="TrophyService.SaveTrophy(Trophy, Trophy)"/>
        Trophy SaveTrophy(Trophy trophy, Trophy oldTrophy);
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
                    trophy.GameNames = gameString.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
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
        /// <returns>Updated trophy</returns>
        public Trophy SaveTrophy(Trophy trophy, Trophy oldTrophy)
        {
            _fileService.StartBackup();

            var trophyPath = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyLocation.Path);
            var trophyNamePath = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyNames);
            var trophyDescriptionPath = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyDescriptions);
            var nodePath = _settingsService.BuildSettings.FilePathSettings.TrophyLocation.NodePath;

            // Save cosmetics
            var changedDefinitions = _settingsService.BuildSettings.CosmeticSettings.Where(x => trophy.Thumbnails.ChangedItems.Any(y => y.CosmeticType == x.CosmeticType)).ToList();
            _cosmeticService.ImportCosmetics(changedDefinitions, trophy.Thumbnails, trophy.Ids);

            // Open BRRES in case it gets deleted
            var brres = _fileService.OpenFile(trophy.BrresFile);
            // Replace old BRRES
            if (!string.IsNullOrEmpty(_settingsService.BuildSettings.FilePathSettings.TrophyBrresLocation))
            {
                _fileService.DeleteFile(oldTrophy.BrresFile);
                var trophyBrresFolder = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.TrophyBrresLocation);
                var newTrophyBrresPath = Path.Combine(trophyBrresFolder, $"{trophy.Brres}.brres");
                _fileService.CopyFile(trophy.BrresFile, newTrophyBrresPath);
            }
            _fileService.CloseFile(brres);

            // Update trophy name
            trophy.NameIndex = ReplaceMsBinString(trophy.DisplayName, trophyNamePath, trophy.NameIndex, oldTrophy.NameIndex);

            // Update trophy game names
            trophy.GameIndex = ReplaceMsBinString(string.Join("\r\n", trophy.GameNames), trophyNamePath, trophy.GameIndex, oldTrophy.GameIndex);

            // Update trophy description
            trophy.DescriptionIndex = ReplaceMsBinString(trophy.Description, trophyDescriptionPath, trophy.DescriptionIndex, oldTrophy.DescriptionIndex);

            // Update trophy data
            var rootNode = _fileService.OpenFile(trophyPath);
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
                    if (trophy != null)
                    {
                        var newTrophyNode = trophy.ToNode();
                        // Get new index if necessary
                        if (index == -1)
                        {
                            index = trophyNodeList.Children.Count;
                        }
                        trophyNodeList.InsertChild(newTrophyNode, index);
                    }
                    _fileService.SaveFile(rootNode);
                }
                _fileService.CloseFile(rootNode);
            }

            _fileService.EndBackup();
            return trophy;
        }

        /// <summary>
        /// Replace trophy-related string in MsBin file
        /// </summary>
        /// <param name="newString">String to insert</param>
        /// <param name="file">File to replace string in</param>
        /// <param name="index">Index to place new string</param>
        /// <param name="oldIndex">Index of old string</param>
        /// <returns>Updated trophy</returns>
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
                    index = rootNode._strings.Count;
                }
                // Insert new string
                rootNode._strings.Insert((int)index, newString);
                rootNode.IsDirty = true;
                _fileService.SaveFile(rootNode);
                _fileService.CloseFile(rootNode);
            }
            return index;
        }
    }
}
