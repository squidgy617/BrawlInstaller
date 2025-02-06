using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface ITrophyService
    {
        /// <inheritdoc cref="TrophyService.GetTrophyList()"/>
        List<Trophy> GetTrophyList();

        /// <inheritdoc cref="TrophyService.LoadTrophyData(Trophy)"/>
        Trophy LoadTrophyData(Trophy trophy);
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
            return trophy;
        }
    }
}
