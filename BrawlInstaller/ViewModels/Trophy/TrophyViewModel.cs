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

        // Services
        ISettingsService _settingsService;
        IFileService _fileService;
        ITrophyService _trophyService;

        // Commands

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
    }
}
