using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.ViewModels
{
    public interface IStageListViewModel
    {

    }

    [Export(typeof(IStageListViewModel))]
    internal class StageListViewModel : ViewModelBase, IStageListViewModel
    {
        // Private properties
        private List<StageList> _stageLists;
        private StageList _selectedStageList;

        // Services
        IStageService _stageService { get; }

        [ImportingConstructor]
        public StageListViewModel(IStageService stageService)
        {
            _stageService = stageService;

            StageLists = _stageService.GetStageLists();

            SelectedStageList = StageLists.FirstOrDefault();
        }

        // Properties
        public List<StageList> StageLists { get => _stageLists; set { _stageLists = value; OnPropertyChanged(nameof(StageLists)); } }
        public StageList SelectedStageList { get => _selectedStageList; set { _selectedStageList = value; OnPropertyChanged(nameof(SelectedStageList)); } }
    }
}
