using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Helpers;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static BrawlInstaller.ViewModels.MainControlsViewModel;

namespace BrawlInstaller.ViewModels
{
    public interface ITracklistViewModel
    {

    }

    [Export(typeof(ITracklistViewModel))]
    internal class TracklistViewModel : ViewModelBase, ITracklistViewModel
    {
        // Private properties
        private ObservableCollection<string> _tracklists;
        private string _selectedTracklist;
        private Tracklist _loadedTracklist;
        private ObservableCollection<TracklistSong> _tracklistSongs;
        private TracklistSong _selectedSong;

        // Services
        ISettingsService _settingsService;
        IFileService _fileService;
        ITracklistService _tracklistService;

        // Commands
        public ICommand LoadTracklistCommand => new RelayCommand(param => LoadTracklist());

        [ImportingConstructor]
        public TracklistViewModel(ISettingsService settingsService, IFileService fileService, ITracklistService tracklistService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _tracklistService = tracklistService;

            LoadedTracklist = null;
            Tracklists = new ObservableCollection<string>(_tracklistService.GetTracklists());
            OnPropertyChanged(nameof(LoadTracklist));
            OnPropertyChanged(nameof(Tracklists));

            WeakReferenceMessenger.Default.Register<UpdateSettingsMessage>(this, (recipient, message) =>
            {
                UpdateSettings();
            });
            WeakReferenceMessenger.Default.Register<SettingsSavedMessage>(this, (recipient, message) =>
            {
                SettingsSaved();
            });
        }

        // Properties
        public ObservableCollection<string> Tracklists { get => _tracklists; set {  _tracklists = value; OnPropertyChanged(nameof(Tracklists)); } }

        [DependsUpon(nameof(Tracklists))]
        public string SelectedTracklist { get => _selectedTracklist; set { _selectedTracklist = value; OnPropertyChanged(nameof(SelectedTracklist)); } }

        public Tracklist LoadedTracklist { get => _loadedTracklist; set { _loadedTracklist = value; OnPropertyChanged(nameof(LoadedTracklist)); } }

        [DependsUpon(nameof(LoadedTracklist))]
        public ObservableCollection<TracklistSong> TracklistSongs { get => LoadedTracklist?.TracklistSongs != null ? new ObservableCollection<TracklistSong>(LoadedTracklist?.TracklistSongs) : new ObservableCollection<TracklistSong>(); }

        [DependsUpon(nameof(TracklistSongs))]
        public TracklistSong SelectedSong { get => _selectedSong; set { _selectedSong = value; OnPropertyChanged(nameof(SelectedSong)); } }

        // Methods
        private void UpdateSettings()
        {
            LoadedTracklist = null;
            Tracklists = new ObservableCollection<string>(_tracklistService.GetTracklists());
            OnPropertyChanged(nameof(Tracklists));
            OnPropertyChanged(nameof(LoadedTracklist));
        }

        private void SettingsSaved()
        {
            LoadedTracklist = null;
            Tracklists = new ObservableCollection<string>(_tracklistService.GetTracklists());
            OnPropertyChanged(nameof(Tracklists));
            OnPropertyChanged(nameof(LoadedTracklist));
        }

        private void LoadTracklist()
        {
            if (SelectedTracklist != null)
            {
                using (new CursorWait())
                {
                    var tracklist = _tracklistService.LoadTracklist(Path.GetFileNameWithoutExtension(SelectedTracklist));
                    LoadedTracklist = tracklist.Copy();
                    OnPropertyChanged(nameof(LoadedTracklist));
                }
            }
        }
    }
}
