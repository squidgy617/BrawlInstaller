using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
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
        private ObservableCollection<FighterInfo> _fighterInfoList;
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

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public FighterInfoViewModel(IFighterService fighterService, ISettingsService settingsService)
        {
            _fighterService = fighterService;
            _settingsService = settingsService;

            GetFighters();
        }

        // Properties
        public ObservableCollection<FighterInfo> FighterInfoList { get => _fighterInfoList; set { _fighterInfoList = value; OnPropertyChanged(nameof(FighterInfoList)); } }

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
            FighterInfoList.Add(newFighter);
            SelectedFighterInfo = newFighter;
            OnPropertyChanged(nameof(FighterInfoList));
            OnPropertyChanged(nameof(SelectedFighterInfo));
        }

        private void RemoveFighter()
        {
            FighterInfoList.Remove(SelectedFighterInfo);
            OnPropertyChanged(nameof(FighterInfoList));
            OnPropertyChanged(nameof(SelectedFighterInfo));
        }

        private void GetFighters()
        {
            var list = _settingsService.LoadFighterInfoSettings();
            FighterInfoList = new ObservableCollection<FighterInfo>(list);
            OnPropertyChanged(nameof(FighterInfoList));
            OnPropertyChanged(nameof(SelectedFighterInfo));
        }

        private void LoadFighters()
        {
            FighterInfoList = new ObservableCollection<FighterInfo>(_fighterService.GetAllFighterInfo());
        }

        // TODO: Include default fighter info stuff, make another pass at IDs that differ between builds
        private void SaveFighters() 
        {
            _settingsService.SaveFighterInfoSettings(FighterInfoList.ToList());
        }
    }
}
