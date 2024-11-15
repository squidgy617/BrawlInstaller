using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrawlInstaller.ViewModels
{
    public interface IFighterInfoViewModel
    {

    }

    [Export(typeof(IFighterInfoViewModel))]
    internal class FighterInfoViewModel : ViewModelBase, IFighterInfoViewModel
    {
        // Private properties
        private FighterInfo _selectedFighterInfo;

        // Services
        IFighterService _fighterService { get; }
        ISettingsService _settingsService { get; }

        // Commands
        public ICommand AddFighterCommand => new RelayCommand(param => AddFighter());
        public ICommand RemoveFighterCommand => new RelayCommand(param => RemoveFighter());
        public ICommand RefreshFightersCommand => new RelayCommand(param => GetFighters());
        public ICommand LoadFightersCommand => new RelayCommand(param => LoadFighters());
        public ICommand SaveFightersCommand => new RelayCommand(param => SaveFighters());
        public ICommand MoveUpCommand => new RelayCommand(param => MoveUp());
        public ICommand MoveDownCommand => new RelayCommand(param => MoveDown());

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public FighterInfoViewModel(IFighterService fighterService, ISettingsService settingsService)
        {
            _fighterService = fighterService;
            _settingsService = settingsService;

            GetFighters();

            WeakReferenceMessenger.Default.Register<UpdateFighterListMessage>(this, (recipient, message) =>
            {
                UpdateFighterList(message);
            });
        }

        // Properties
        public ObservableCollection<FighterInfo> FighterInfoList { get => new ObservableCollection<FighterInfo>(_settingsService.FighterInfoList); }

        [DependsUpon(nameof(FighterInfoList))]
        public FighterInfo SelectedFighterInfo { get => _selectedFighterInfo; set { _selectedFighterInfo = value; OnPropertyChanged(nameof(SelectedFighterInfo)); } }

        // Methods
        private void AddFighter()
        {
            var newFighter = new FighterInfo 
            { 
                Ids = new BrawlIds 
                { 
                    FighterConfigId = 0,
                    SlotConfigId = 0,
                    CSSSlotConfigId = 0,
                    CosmeticConfigId = 0,
                    CosmeticId = 0,
                    FranchiseId = 0
                }
            };
            _settingsService.FighterInfoList.Add(newFighter);
            SelectedFighterInfo = newFighter;
            OnPropertyChanged(nameof(FighterInfoList));
            OnPropertyChanged(nameof(SelectedFighterInfo));
            WeakReferenceMessenger.Default.Send(new UpdateFighterListMessage(_settingsService.FighterInfoList));
        }

        private void RemoveFighter()
        {
            _settingsService.FighterInfoList.Remove(SelectedFighterInfo);
            OnPropertyChanged(nameof(FighterInfoList));
            OnPropertyChanged(nameof(SelectedFighterInfo));
            WeakReferenceMessenger.Default.Send(new UpdateFighterListMessage(_settingsService.FighterInfoList));
        }

        private void GetFighters()
        {
            var list = _settingsService.LoadFighterInfoSettings();
            _settingsService.FighterInfoList = list;
            OnPropertyChanged(nameof(FighterInfoList));
            OnPropertyChanged(nameof(SelectedFighterInfo));
            WeakReferenceMessenger.Default.Send(new UpdateFighterListMessage(_settingsService.FighterInfoList));
        }

        private void LoadFighters()
        {
            var currentFighterList = FighterInfoList.ToList();
            var newFighterList = new List<FighterInfo>();
            var fighterList = _fighterService.GetAllFighterInfo();
            // Get only fighters where all configs could be found
            var completeFighters = fighterList.Where(x => x.FighterConfig != "" && x.CosmeticConfig != "" && x.CSSSlotConfig != "" && x.SlotConfig != "").ToList();
            foreach (var fighter in completeFighters)
            {
                // Only add fighters if there is no entry with the same fighter ID
                var fighterMatch = currentFighterList.FirstOrDefault(x => x.Ids.FighterConfigId == fighter.Ids.FighterConfigId
                && x.Ids.SlotConfigId == fighter.Ids.SlotConfigId && x.Ids.CSSSlotConfigId == fighter.Ids.CSSSlotConfigId
                && x.Ids.CosmeticConfigId == fighter.Ids.CosmeticConfigId);

                if (fighterMatch == null)
                {
                    newFighterList.Add(fighter);
                }
                // Otherwise, replace the match
                else
                {
                    currentFighterList.Insert(currentFighterList.IndexOf(fighterMatch), fighter);
                    currentFighterList.Remove(fighterMatch);
                }
            }
            currentFighterList.AddRange(newFighterList.OrderBy(x => x.Ids.FighterConfigId));
            _settingsService.FighterInfoList = currentFighterList;
            OnPropertyChanged(nameof(FighterInfoList));
            WeakReferenceMessenger.Default.Send(new UpdateFighterListMessage(_settingsService.FighterInfoList));
        }

        // TODO: Include default fighter info stuff, make another pass at IDs that differ between builds
        private void SaveFighters() 
        {
            _settingsService.SaveFighterInfoSettings(FighterInfoList.ToList());
        }

        private void MoveUp()
        {
            var selected = SelectedFighterInfo;
            _settingsService.FighterInfoList.MoveUp(SelectedFighterInfo);
            SelectedFighterInfo = selected;
            OnPropertyChanged(nameof(FighterInfoList));
            WeakReferenceMessenger.Default.Send(new UpdateFighterListMessage(_settingsService.FighterInfoList));
        }

        private void MoveDown()
        {
            var selected = SelectedFighterInfo;
            _settingsService.FighterInfoList.MoveDown(SelectedFighterInfo);
            SelectedFighterInfo = selected;
            OnPropertyChanged(nameof(FighterInfoList));
            WeakReferenceMessenger.Default.Send(new UpdateFighterListMessage(_settingsService.FighterInfoList));
        }

        private void UpdateFighterList(UpdateFighterListMessage message)
        {
            OnPropertyChanged(nameof(FighterInfoList));
        }
    }
}
