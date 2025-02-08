using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static BrawlInstaller.ViewModels.MainControlsViewModel;

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
        public ICommand NewTrophyCommand => new RelayCommand(param => NewTrophy());

        [ImportingConstructor]
        public TrophyViewModel(ISettingsService settingsService, IFileService fileService, ITrophyService trophyService, ITrophyEditorViewModel trophyEditorViewModel)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _trophyService = trophyService;

            TrophyEditorViewModel = trophyEditorViewModel;

            GetTrophyList();

            WeakReferenceMessenger.Default.Register<UpdateTrophyListMessage>(this, (recipient, message) =>
            {
                GetTrophyList();
            });
            WeakReferenceMessenger.Default.Register<UpdateSettingsMessage>(this, (recipient, message) =>
            {
                GetTrophyList();
            });
            WeakReferenceMessenger.Default.Register<SettingsSavedMessage>(this, (recipient, message) =>
            {
                GetTrophyList();
            });
        }

        // ViewModels
        public ITrophyEditorViewModel TrophyEditorViewModel { get; }

        // Properties
        public ObservableCollection<Trophy> TrophyList { get => _trophyList; set { _trophyList = value; OnPropertyChanged(nameof(TrophyList)); } }
        public Trophy SelectedTrophy { get => _selectedTrophy; set { _selectedTrophy = value; OnPropertyChanged(nameof(SelectedTrophy)); } }

        // Methods
        public void LoadTrophy()
        {
            WeakReferenceMessenger.Default.Send(new LoadTrophyMessage(SelectedTrophy));
        }

        public void GetTrophyList()
        {
            TrophyList = new ObservableCollection<Trophy>(_trophyService.GetTrophyList());
            OnPropertyChanged(nameof(TrophyList));
        }

        public void NewTrophy()
        {
            var newTrophy = new Trophy();
            // Get new trophy IDs
            newTrophy.Ids.TrophyId = 631; // 631 is first EX trophy ID
            newTrophy.Ids.TrophyThumbnailId = 631;
            while (TrophyList.Select(x => x.Ids.TrophyId).Contains(newTrophy.Ids.TrophyId))
            {
                newTrophy.Ids.TrophyId++;
            }
            while (TrophyList.Select(x => x.Ids.TrophyThumbnailId).Contains(newTrophy.Ids.TrophyThumbnailId))
            {
                newTrophy.Ids.TrophyThumbnailId++;
            }
            WeakReferenceMessenger.Default.Send(new LoadTrophyMessage(newTrophy));
        }
    }

    // Messages
    public class LoadTrophyMessage : ValueChangedMessage<Trophy>
    {
        public LoadTrophyMessage(Trophy trophy) : base(trophy)
        {
        }
    }
}
