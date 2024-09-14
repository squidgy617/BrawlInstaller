using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace BrawlInstaller.ViewModels
{
    public interface IStageListViewModel
    {
        StageSlot SelectedStageSlot { get; }
    }

    [Export(typeof(IStageListViewModel))]
    internal class StageListViewModel : ViewModelBase, IStageListViewModel
    {
        // Private properties
        private List<StageList> _stageLists;
        private StageList _selectedStageList;
        private StagePage _selectedPage;
        private StageSlot _selectedStageSlot;
        private List<StageSlot> _stageTable;

        // Services
        IStageService _stageService { get; }

        // Commands
        public ICommand MoveUpCommand => new RelayCommand(param => MoveStageUp());
        public ICommand MoveDownCommand => new RelayCommand(param => MoveStageDown());
        public ICommand SaveStageListCommand => new RelayCommand(param => SaveStageList());

        [ImportingConstructor]
        public StageListViewModel(IStageService stageService)
        {
            _stageService = stageService;

            StageTable = _stageService.GetStageSlots();

            StageLists = _stageService.GetStageLists(StageTable);

            SelectedStageList = StageLists.FirstOrDefault();
        }

        // Properties
        public List<StageList> StageLists { get => _stageLists; set { _stageLists = value; OnPropertyChanged(nameof(StageLists)); } }

        [DependsUpon(nameof(StageLists))]
        public StageList SelectedStageList { get => _selectedStageList; set { _selectedStageList = value; OnPropertyChanged(nameof(SelectedStageList)); } }

        [DependsUpon(nameof(SelectedStageList))]
        public StagePage SelectedPage { get => _selectedPage; set { _selectedPage = value; OnPropertyChanged(nameof(SelectedPage)); } }

        [DependsUpon(nameof(SelectedPage))]
        public ObservableCollection<StageSlot> StageSlots { get => new ObservableCollection<StageSlot>(SelectedPage.StageSlots); }

        [DependsUpon(nameof(StageSlots))]
        public StageSlot SelectedStageSlot { get => _selectedStageSlot; set { _selectedStageSlot = value; OnPropertyChanged(nameof(SelectedStageSlot)); } }

        [DependsUpon(nameof(StageLists))]
        public List<StageSlot> StageTable { get => _stageTable; set { _stageTable = value; OnPropertyChanged(nameof(StageTable)); } }

        [DependsUpon(nameof(SelectedStageList))]
        [DependsUpon(nameof(StageTable))]
        public List<StageSlot> UnusedSlots { get => StageTable.Where(x => !SelectedStageList.Pages.SelectMany(z => z.StageSlots).ToList().Contains(x)).ToList(); }

        // Methods
        public void MoveStageUp()
        {
            SelectedPage.StageSlots.MoveUp(SelectedStageSlot);
            StageSlots.MoveUp(SelectedStageSlot);
            OnPropertyChanged(nameof(StageSlots));
        }

        public void MoveStageDown()
        {
            SelectedPage.StageSlots.MoveDown(SelectedStageSlot);
            StageSlots.MoveDown(SelectedStageSlot);
            OnPropertyChanged(nameof(StageSlots));
        }

        private void SaveStageList()
        {
            // TODO: Create one method to pass in the StageLists and StageTable, save them all in one go
            _stageService.SaveStageList(SelectedStageList);
        }
    }
}
