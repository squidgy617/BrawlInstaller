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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using static BrawlInstaller.ViewModels.MainControlsViewModel;

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
        private int _selectedStageIndex;
        private ObservableCollection<StageSlot> _stageTable;
        private StageSlot _selectedStageTableEntry;
        private List<int> _incompleteStageIds;

        // Services
        IStageService _stageService { get; }
        ICosmeticService _cosmeticService { get; }
        IDialogService _dialogService { get; }

        // Commands
        public ICommand MoveUpCommand => new RelayCommand(param => MoveStageUp());
        public ICommand MoveDownCommand => new RelayCommand(param => MoveStageDown());
        public ICommand AddStageToListCommand => new RelayCommand(param =>  AddStageToList());
        public ICommand RemoveStageFromListCommand => new RelayCommand(param => RemoveStageFromList());
        public ICommand SaveStageListCommand => new RelayCommand(param => SaveStageList());
        public ICommand LoadStageCommand => new RelayCommand(param => LoadStage(param));
        public ICommand NewStageCommand => new RelayCommand(param =>  NewStage());

        [ImportingConstructor]
        public StageListViewModel(IStageService stageService, ICosmeticService cosmeticService, IDialogService dialogService)
        {
            _stageService = stageService;
            _cosmeticService = cosmeticService;
            _dialogService = dialogService;

            StageTable = new ObservableCollection<StageSlot>(_stageService.GetStageSlots());

            StageLists = _stageService.GetStageLists(StageTable.ToList());

            SelectedStageList = StageLists.FirstOrDefault();

            IncompleteStageIds = _stageService.GetIncompleteStageIds();

            WeakReferenceMessenger.Default.Register<StageSavedMessage>(this, (recipient, message) =>
            {
                UpdateStageList(message);
            });

            WeakReferenceMessenger.Default.Register<StageDeletedMessage>(this, (recipient, message) =>
            {
                DeleteStageSlot(message);
            });

            WeakReferenceMessenger.Default.Register<UpdateSettingsMessage>(this, (recipient, message) =>
            {
                UpdateSettings();
            });
            WeakReferenceMessenger.Default.Register<SettingsSavedMessage>(this, (recipient, message) =>
            {
                UpdateSettings();
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
        public ObservableCollection<StageSlot> StageSlots { get => SelectedPage != null ? new ObservableCollection<StageSlot>(SelectedPage.StageSlots) : new ObservableCollection<StageSlot>(); }

        [DependsUpon(nameof(SelectedStageIndex))]
        [DependsUpon(nameof(StageSlots))]
        public StageSlot SelectedStageSlot { get => _selectedStageSlot; set { _selectedStageSlot = value; OnPropertyChanged(nameof(SelectedStageSlot)); } }

        [DependsUpon(nameof(SelectedStageSlot))]
        [DependsUpon(nameof(StageSlots))]
        public int SelectedStageIndex { get => _selectedStageIndex; set { _selectedStageIndex = value; OnPropertyChanged(nameof(SelectedStageIndex)); } }

        [DependsUpon(nameof(StageLists))]
        public ObservableCollection<StageSlot> StageTable { get => _stageTable; set { _stageTable = value; OnPropertyChanged(nameof(StageTable)); } }

        [DependsUpon(nameof(SelectedStageList))]
        [DependsUpon(nameof(StageTable))]
        public List<StageSlot> UnusedSlots { get => StageTable.Where(x => !SelectedStageList?.Pages?.SelectMany(z => z.StageSlots).ToList().Contains(x) == true).ToList(); }

        [DependsUpon(nameof(StageTable))]
        public StageSlot SelectedStageTableEntry { get => _selectedStageTableEntry; set { _selectedStageTableEntry = value; OnPropertyChanged(nameof(SelectedStageTableEntry)); } }

        public List<int> IncompleteStageIds { get => _incompleteStageIds; set { _incompleteStageIds = value; OnPropertyChanged(nameof(IncompleteStageIds)); } }

        // Methods
        public void UpdateStageList(StageSavedMessage message)
        {
            // If new stage, add it to the table
            var stage = message.Value;
            if (!StageTable.Select(x => x.StageIds).Select(x => x.StageId).Contains(stage.Slot.StageIds.StageId))
            {
                StageTable.Add(stage.Slot);
            }
            OnPropertyChanged(nameof(StageTable));
            OnPropertyChanged(nameof(StageLists));
            SaveStageList();
        }

        public void DeleteStageSlot(StageDeletedMessage message)
        {
            var stageSlot = message.Value;
            foreach(var slot in StageTable.Where(x => x.StageIds.StageId == stageSlot.StageIds.StageId && x.StageIds.StageCosmeticId == stageSlot.StageIds.StageCosmeticId).ToList())
            {
                StageTable.Remove(slot);
            }
            foreach(var list in StageLists)
            {
                foreach(var page in list.Pages)
                {
                    foreach (var slot in page.StageSlots.Where(x => x.StageIds.StageId == stageSlot.StageIds.StageId && x.StageIds.StageCosmeticId == stageSlot.StageIds.StageCosmeticId).ToList())
                    {
                        page.StageSlots.Remove(slot);
                    }
                }
            }
            OnPropertyChanged(nameof(StageTable));
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

        public void RemoveStageFromList()
        {
            if (SelectedStageIndex > -1)
            {
                SelectedPage.StageSlots.RemoveAt(SelectedStageIndex);
                SelectedStageIndex = -1;
                SelectedStageSlot = null;
                OnPropertyChanged(nameof(StageLists));
                OnPropertyChanged(nameof(SelectedStageSlot));
                OnPropertyChanged(nameof(SelectedStageTableEntry));
                OnPropertyChanged(nameof(UnusedSlots));
                OnPropertyChanged(nameof(StageSlots));
            }
        }

        public void AddStageToList()
        {
            if (SelectedStageTableEntry != null)
            {
                SelectedPage.StageSlots.Add(SelectedStageTableEntry);
                OnPropertyChanged(nameof(StageLists));
                OnPropertyChanged(nameof(SelectedStageSlot));
                OnPropertyChanged(nameof(SelectedStageTableEntry));
                OnPropertyChanged(nameof(UnusedSlots));
                OnPropertyChanged(nameof(StageSlots));
            }
        }

        private void SaveStageList()
        {
            _stageService.SaveStageLists(StageLists, StageTable.ToList());
            _dialogService.ShowMessage("Stage lists saved.", "Saved");
        }

        public void LoadStage(object param)
        {
            if (param != null)
            {
                var selectedSlot = (StageSlot)param;
                using (new CursorWait())
                {
                    var stage = new StageInfo();
                    Stage = stage;
                    stage.Slot = selectedSlot;
                    stage = _stageService.GetStageData(stage);
                    // TODO: Do we need to copy stage on load? Right now just useful for debugging
                    var stageCopy = stage.Copy();
                    WeakReferenceMessenger.Default.Send(new StageLoadedMessage(stageCopy));
                }
            }
        }

        public void NewStage()
        {
            // Get new stage ID
            // Start at 64 because that's where custom stages are supposed to start
            var stageId = 64;
            var stageCosmeticId = 1;
            var stageIdList = StageTable.Select(x => x.StageIds).ToList();
            while (stageIdList.Select(x => x.StageId).Contains(stageId) || ReservedIds.ReservedStageIds.Contains(stageId) || IncompleteStageIds.Contains(stageId))
            {
                stageId++;
            }
            while (stageIdList.Select(x => x.StageCosmeticId).Contains(stageCosmeticId) || ReservedIds.ReservedStageCosmeticIds.Contains(stageCosmeticId))
            {
                stageCosmeticId++;
            }
            var stage = new StageInfo();
            Stage = stage;
            stage.Slot = new StageSlot { Name = "New Stage", Index = StageTable.Count };
            stage.Slot.StageIds.StageId = stageId;
            stage.Slot.StageIds.StageCosmeticId = stageCosmeticId;
            // Include a default stage entry
            stage.StageEntries.Add(new StageEntry());
            stage.AllParams.Add(stage.StageEntries[0].Params);
            // This is done to populate selectable cosmetics
            stage.Cosmetics.Items = _cosmeticService.GetStageCosmetics(stage.Slot.StageIds);
            WeakReferenceMessenger.Default.Send(new StageLoadedMessage(stage));
        }

        private void UpdateSettings()
        {
            StageTable = new ObservableCollection<StageSlot>(_stageService.GetStageSlots());
            StageLists = _stageService.GetStageLists(StageTable.ToList());
            SelectedStageList = StageLists.FirstOrDefault();
            SelectedPage = SelectedStageList?.Pages?.FirstOrDefault();
            IncompleteStageIds = _stageService.GetIncompleteStageIds();
            OnPropertyChanged(nameof(StageTable));
            OnPropertyChanged(nameof(StageLists));
            OnPropertyChanged(nameof(SelectedPage));
            OnPropertyChanged(nameof(SelectedStageList));
            OnPropertyChanged(nameof(IncompleteStageIds));
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
