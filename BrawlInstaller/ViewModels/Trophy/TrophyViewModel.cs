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
    public interface ITrophyViewModel
    {

    }

    [Export(typeof(ITrophyViewModel))]
    internal class TrophyViewModel : ViewModelBase, ITrophyViewModel
    {
        // Private properties
        private ObservableCollection<Trophy> _trophyList;
        private Trophy _selectedTrophy;

        // Services
        ISettingsService _settingsService;
        IFileService _fileService;
        ITrophyService _trophyService;

        // Commands
        public ICommand LoadTrophyCommand => new RelayCommand(param => LoadTrophy());

        [ImportingConstructor]
        public TrophyViewModel(ISettingsService settingsService, IFileService fileService, ITrophyService trophyService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _trophyService = trophyService;

            TrophyList = new ObservableCollection<Trophy>(_trophyService.GetTrophyList());
            OnPropertyChanged(nameof(TrophyList));
        }

        // Properties
        public ObservableCollection<Trophy> TrophyList { get => _trophyList; set { _trophyList = value; OnPropertyChanged(nameof(TrophyList)); } }
        public Trophy SelectedTrophy { get => _selectedTrophy; set { _selectedTrophy = value; OnPropertyChanged(nameof(SelectedTrophy)); } }

        // Methods
        public void LoadTrophy()
        {
            var test = _trophyService.LoadTrophyData(SelectedTrophy);
        }
    }
}
