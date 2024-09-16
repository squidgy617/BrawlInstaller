using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Helpers;
using BrawlInstaller.Services;
using BrawlInstaller.StaticClasses;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
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
        StageInfo Stage { get; }
    }

    [Export(typeof(IStageListViewModel))]
    internal class StageListViewModel : ViewModelBase, IStageListViewModel
    {
        // Private properties
        private StageInfo _stage;
        private List<StageList> _stageLists;
        private StageList _selectedStageList;
        private StagePage _selectedPage;
        private StageSlot _selectedStageSlot;
        private List<StageSlot> _stageTable;
        private StageSlot _selectedUnusedSlot;

        // Services
        IStageService _stageService { get; }

        // Commands
        public ICommand MoveUpCommand => new RelayCommand(param => MoveStageUp());
        public ICommand MoveDownCommand => new RelayCommand(param => MoveStageDown());
        public ICommand SetStageUnusedCommand => new RelayCommand(param =>  SetStageUnused());
        public ICommand SetStageUsedCommand => new RelayCommand(param => SetStageUsed());
        public ICommand SaveStageListCommand => new RelayCommand(param => SaveStageList());
        public ICommand LoadStageCommand => new RelayCommand(param => LoadStage());

        [ImportingConstructor]
        public StageListViewModel(IStageService stageService)
        {
            _stageService = stageService;

            StageTable = _stageService.GetStageSlots();

            StageLists = _stageService.GetStageLists(StageTable);

            SelectedStageList = StageLists.FirstOrDefault();

            WeakReferenceMessenger.Default.Register<StageSavedMessage>(this, (recipient, message) =>
            {
                UpdateStageList(message);
            });
        }

        // Properties
        public StageInfo Stage { get => _stage; set { _stage = value; OnPropertyChanged(nameof(Stage)); } }
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

        [DependsUpon(nameof(UnusedSlots))]
        public StageSlot SelectedUnusedSlot { get => _selectedUnusedSlot; set { _selectedUnusedSlot = value; OnPropertyChanged(nameof(SelectedUnusedSlot)); } }

        // Methods
        public void UpdateStageList(StageSavedMessage message)
        {
            OnPropertyChanged(nameof(StageLists));
            SaveStageList();
        }

        public void MoveStageUp()
        {
            if (SelectedPage.StageSlots.FirstOrDefault() != SelectedStageSlot)
            {
                SelectedPage.StageSlots.MoveUp(SelectedStageSlot);
                StageSlots.MoveUp(SelectedStageSlot);
            }
            else if (SelectedPage != SelectedStageList.Pages.FirstOrDefault())
            {
                SelectedPage.StageSlots.Remove(SelectedStageSlot);
                StageSlots.Remove(SelectedStageSlot);
                var newPage = SelectedStageList.Pages[SelectedStageList.Pages.IndexOf(SelectedPage) - 1];
                newPage.StageSlots.Add(SelectedStageSlot);
                SelectedPage = newPage;
                OnPropertyChanged(nameof(SelectedPage));
                OnPropertyChanged(nameof(SelectedStageSlot));
            }
            OnPropertyChanged(nameof(StageSlots));
        }

        public void MoveStageDown()
        {
            if (SelectedPage.StageSlots.LastOrDefault() != SelectedStageSlot)
            {
                SelectedPage.StageSlots.MoveDown(SelectedStageSlot);
                StageSlots.MoveDown(SelectedStageSlot);
            }
            else if (SelectedPage != SelectedStageList.Pages.LastOrDefault())
            {
                SelectedPage.StageSlots.Remove(SelectedStageSlot);
                StageSlots.Remove(SelectedStageSlot);
                var newPage = SelectedStageList.Pages[SelectedStageList.Pages.IndexOf(SelectedPage) + 1];
                newPage.StageSlots.Insert(0, SelectedStageSlot);
                SelectedPage = newPage;
                OnPropertyChanged(nameof(SelectedPage));
                OnPropertyChanged(nameof(SelectedStageSlot));
            }
            OnPropertyChanged(nameof(StageSlots));
        }

        public void SetStageUnused()
        {
            if (SelectedStageSlot != null)
            {
                SelectedPage.StageSlots.Remove(SelectedStageSlot);
                OnPropertyChanged(nameof(StageLists));
                OnPropertyChanged(nameof(SelectedStageSlot));
                OnPropertyChanged(nameof(UnusedSlots));
                OnPropertyChanged(nameof(StageSlots));
            }
        }

        public void SetStageUsed()
        {
            if (SelectedUnusedSlot != null)
            {
                SelectedPage.StageSlots.Add(SelectedUnusedSlot);
                OnPropertyChanged(nameof(StageLists));
                OnPropertyChanged(nameof(SelectedStageSlot));
                OnPropertyChanged(nameof(UnusedSlots));
                OnPropertyChanged(nameof(StageSlots));
            }
        }

        private void SaveStageList()
        {
            _stageService.SaveStageLists(StageLists, StageTable);
        }

        public void LoadStage()
        {
            if (SelectedStageSlot != null)
            {
                using (new CursorWait())
                {
                    var stage = new StageInfo();
                    Stage = stage;
                    stage.Slot = SelectedStageSlot;
                    stage = _stageService.GetStageData(stage);
                    WeakReferenceMessenger.Default.Send(new StageLoadedMessage(stage));
                }
            }
        }

        public void NewStage()
        {
            // Get new stage ID
            var stageId = 1;
            var stageCosmeticId = 1;
            var stageIdList = StageLists.SelectMany(x => x.Pages).SelectMany(x => x.StageSlots).Select(x => x.StageIds).Concat(UnusedSlots.Select(x => x.StageIds));
            while (stageIdList.Select(x => x.StageId).Contains(stageId) || ReservedIds.ReservedStageIds.Contains(stageId))
            {
                stageId++;
            }
            while (stageIdList.Select(x => x.StageCosmeticId).Contains(stageCosmeticId) || ReservedIds.ReservedStageCosmeticIds.Contains(stageId))
            {
                stageCosmeticId++;
            }
            var stage = new StageInfo();
            stage.Slot.StageIds.StageId = stageId;
            stage.Slot.StageIds.StageCosmeticId = stageCosmeticId;
            WeakReferenceMessenger.Default.Send(new StageLoadedMessage(stage));
        }
    }

    // Messages
    public class StageLoadedMessage : ValueChangedMessage<StageInfo>
    {
        public StageLoadedMessage(StageInfo stage) : base(stage)
        {
        }
    }
}
