using BrawlInstaller.Classes;
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
    }

    [Export(typeof(IStageService))]
    internal class StageService : IStageService
    {
        // Services
        ICodeService _codeService;
        ISettingsService _settingsService;
        IFileService _fileService;

        [ImportingConstructor]
        public StageService(ICodeService codeService, ISettingsService settingsService, IFileService fileService) 
        {
            _codeService = codeService;
            _settingsService = settingsService;
            _fileService = fileService;
        }

        // Methods

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
    }
}
