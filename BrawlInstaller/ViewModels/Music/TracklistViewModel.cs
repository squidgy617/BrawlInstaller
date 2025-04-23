using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Helpers;
using BrawlInstaller.Services;
using BrawlLib.Internal.Audio;
using BrawlLib.SSBB.ResourceNodes;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        // These are for volume control
        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        // Private properties
        private ObservableCollection<TracklistOption> _tracklistOptions;
        private TracklistOption _selectedTracklistOption;
        private ObservableCollection<string> _tracklists;
        private string _selectedTracklist;
        private Tracklist _loadedTracklist;
        private ObservableCollection<TracklistSong> _tracklistSongs;
        private TracklistSong _selectedSong;
        private int _volume;

        // Sound player for playing sounds
        private SoundPlayer _soundPlayer = new SoundPlayer();

        // Services
        ISettingsService _settingsService;
        IFileService _fileService;
        ITracklistService _tracklistService;
        IDialogService _dialogService;
        IFighterService _fighterService;

        // Commands
        public ICommand LoadTracklistCommand => new RelayCommand(param => LoadTracklist());
        public ICommand NewTracklistCommand => new RelayCommand(param => NewTracklist());
        public ICommand PlaySongCommand => new RelayCommand(param => PlaySong());
        public ICommand StopSongCommand => new RelayCommand(param => StopSong());
        public ICommand SaveTracklistCommand => new RelayCommand(param => SaveTracklist());
        public ICommand DeleteTracklistCommand => new RelayCommand(param => DeleteTracklist());
        public ICommand MoveSongUpCommand => new RelayCommand(param => MoveSongUp());
        public ICommand MoveSongDownCommand => new RelayCommand(param => MoveSongDown());
        public ICommand AddSongCommand => new RelayCommand(param => AddSong());
        public ICommand RemoveSongCommand => new RelayCommand(param => RemoveSong());
        public ICommand AlignFighterIdsCommand => new RelayCommand(param => AlignSongIdsWithFighterIds());

        [ImportingConstructor]
        public TracklistViewModel(ISettingsService settingsService, IFileService fileService, ITracklistService tracklistService, IDialogService dialogService, IFighterService fighterService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _tracklistService = tracklistService;
            _dialogService = dialogService;
            _fighterService = fighterService;

            TracklistOptions = new ObservableCollection<TracklistOption>();
            Volume = 100;
            LoadedTracklist = null;
            RefreshTracklists();
            OnPropertyChanged(nameof(LoadedTracklist));

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
        public ObservableCollection<TracklistOption> TracklistOptions { get => _tracklistOptions; set { _tracklistOptions = value; OnPropertyChanged(nameof(TracklistOptions)); } }

        [DependsUpon(nameof(TracklistOptions))]
        public TracklistOption SelectedTracklistOption { get => _selectedTracklistOption; set { _selectedTracklistOption = value; OnPropertyChanged(nameof(SelectedTracklistOption)); } }

        [DependsUpon(nameof(SelectedTracklistOption))]
        public ObservableCollection<string> Tracklists { get => SelectedTracklistOption?.Tracklists; }

        [DependsUpon(nameof(Tracklists))]
        public string SelectedTracklist { get => _selectedTracklist; set { _selectedTracklist = value; OnPropertyChanged(nameof(SelectedTracklist)); } }

        public Tracklist LoadedTracklist { get => _loadedTracklist; set { _loadedTracklist = value; OnPropertyChanged(nameof(LoadedTracklist)); } }

        [DependsUpon(nameof(LoadedTracklist))]
        public ObservableCollection<TracklistSong> TracklistSongs { get => LoadedTracklist?.TracklistSongs != null ? new ObservableCollection<TracklistSong>(LoadedTracklist?.TracklistSongs) : new ObservableCollection<TracklistSong>(); }

        [DependsUpon(nameof(TracklistSongs))]
        public TracklistSong SelectedSong { get => _selectedSong; set { _selectedSong = value; StopSong(); OnPropertyChanged(nameof(SelectedSong)); } }

        [DependsUpon(nameof(SelectedSong))]
        public bool PlaybackVisible { get => _fileService.FileExists(SelectedSong?.SongFile); }

        public int Volume { get => _volume; set { _volume = value; AdjustVolume(value); OnPropertyChanged(nameof(Volume)); } }

        [DependsUpon(nameof(LoadedTracklist))]
        public bool AlignIdsEnabled { get => LoadedTracklist?.Name == _settingsService.BuildSettings.FilePathSettings.VictoryThemeTracklist 
                || LoadedTracklist?.Name == _settingsService.BuildSettings.FilePathSettings.CreditsThemeTracklist; }

        // Methods
        private void RefreshTracklists()
        {
            var selectedTracklistIndex = TracklistOptions.IndexOf(SelectedTracklistOption);
            TracklistOptions = new ObservableCollection<TracklistOption>();
            if (!string.IsNullOrEmpty(_settingsService.BuildSettings.FilePathSettings.TracklistPath))
            {
                TracklistOptions.Add(new TracklistOption("Standard", TracklistType.Standard, new ObservableCollection<string>(_tracklistService.GetTracklists())));
            }
            if (!string.IsNullOrEmpty(_settingsService.BuildSettings.FilePathSettings.NetplaylistPath))
            {
                TracklistOptions.Add(new TracklistOption("Netplay", TracklistType.Netplay, new ObservableCollection<string>(_tracklistService.GetTracklists(TracklistType.Netplay))));
            }
            if (selectedTracklistIndex != -1 && TracklistOptions.Count > selectedTracklistIndex)
            {
                SelectedTracklistOption = TracklistOptions[selectedTracklistIndex];
            }
            else
            {
                SelectedTracklistOption = TracklistOptions.FirstOrDefault();
            }
            OnPropertyChanged(nameof(TracklistOptions));
            OnPropertyChanged(nameof(Tracklists));
            OnPropertyChanged(nameof(SelectedTracklistOption));
        }

        private void UpdateSettings()
        {
            LoadedTracklist = null;
            RefreshTracklists();
            OnPropertyChanged(nameof(LoadedTracklist));
        }

        private void SettingsSaved()
        {
            LoadedTracklist = null;
            RefreshTracklists();
            OnPropertyChanged(nameof(LoadedTracklist));
        }

        private void NewTracklist()
        {
            var tracklist = new Tracklist { Name = "New_Tracklist" };
            LoadedTracklist = tracklist;
            OnPropertyChanged(nameof(LoadedTracklist));
        }

        private void LoadTracklist()
        {
            if (SelectedTracklist != null)
            {
                using (new CursorWait())
                {
                    var tracklist = _tracklistService.LoadTracklist(Path.GetFileNameWithoutExtension(SelectedTracklist), SelectedTracklistOption.TracklistType);
                    LoadedTracklist = tracklist.Copy();
                    OnPropertyChanged(nameof(LoadedTracklist));
                }
            }
        }

        private void PlaySong()
        {
            if (_soundPlayer.Stream == null)
            {
                var brstmNode = _fileService.OpenFile(SelectedSong.SongFile) as RSTMNode;
                if (brstmNode != null)
                {
                    if (brstmNode._audioSource != DataSource.Empty)
                    {
                        var bytes = WAV.ToByteArray(brstmNode.CreateStreams()[0]);
                        var stream = new MemoryStream(bytes);
                        _soundPlayer.Stream = stream;
                        _soundPlayer.PlayLooping();
                    }
                    _fileService.CloseFile(brstmNode);
                }
            }
        }

        private void StopSong()
        {
            _soundPlayer.Stop();
            _soundPlayer.Stream?.Dispose();
            _soundPlayer.Stream = null;
        }

        private void AdjustVolume(int value)
        {
            // Calculate the volume that's being set
            int NewVolume = ((ushort.MaxValue / 100) * value);
            // Set the same volume for both the left and the right channels
            uint NewVolumeAllChannels = (((uint)NewVolume & 0x0000ffff) | ((uint)NewVolume << 16));
            // Set the volume
            waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);
        }

        private void DeleteTracklist()
        {
            var result = _dialogService.ShowMessage("WARNING! You are about to delete the currently loaded tracklist. Are you sure?", "Delete Tracklist", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result)
            {
                var tracklistToDelete = new Tracklist { File = LoadedTracklist.File };
                SaveTracklist(tracklistToDelete);
                LoadedTracklist = null;
                OnPropertyChanged(nameof(LoadedTracklist));
                _dialogService.ShowMessage("Changes saved.", "Saved");
            }
        }

        private void SaveTracklist()
        {
            if (!ErrorValidate())
            {
                return;
            }
            LoadedTracklist = SaveTracklist(LoadedTracklist);
            OnPropertyChanged(nameof(LoadedTracklist));
            _dialogService.ShowMessage("Changes saved.", "Saved");
        }

        private Tracklist SaveTracklist(Tracklist tracklist)
        {
            // Copy tracklist before save
            var tracklistToSave = tracklist.Copy();

            // Get delete options
            var deleteOptions = _tracklistService.GetTracklistDeleteOptions(tracklistToSave);

            // Prompt user with delete options
            var checklistItems = new List<CheckListItem>();
            foreach (var deleteOption in deleteOptions)
            {
                checklistItems.Add(new CheckListItem(deleteOption, Path.GetFileName(deleteOption), deleteOption, false));
            }
            var items = new List<CheckListItem>();
            if (deleteOptions.Count > 0)
            {
                items = _dialogService.OpenCheckListDialog(checklistItems, "Select items to delete", "The following items were changed, but may be shared in other tracklists. Ensure they are not used in other tracklists and then select the items you would like to delete.");
            }
            var itemsToDelete = items.Where(x => x.IsChecked).Select(x => x.Item.ToString()).ToList();

            using (new CursorWait())
            {
                _fileService.StartBackup();
                // Save tracklist
                tracklistToSave = _tracklistService.SaveTracklist(tracklistToSave, itemsToDelete);

                // Save successful, so load saved tracklist
                tracklist = tracklistToSave;

                // Update tracklists
                RefreshTracklists();

                _fileService.EndBackup();
            }
            return tracklist;
        }

        private void MoveSongUp()
        {
            if (SelectedSong != null)
            {
                LoadedTracklist.TracklistSongs.MoveUp(SelectedSong);
                OnPropertyChanged(nameof(TracklistSongs));
            }
        }

        private void MoveSongDown()
        {
            if (SelectedSong != null)
            {
                LoadedTracklist.TracklistSongs.MoveDown(SelectedSong);
                OnPropertyChanged(nameof(TracklistSongs));
            }
        }

        private void AddSong()
        {
            if (TracklistSongs != null)
            {
                uint newId = 0x0000F000;
                while (LoadedTracklist.TracklistSongs.Select(x => x.SongId).ToList().Contains(newId))
                {
                    newId++;
                }
                var newSong = new TracklistSong { Name = "New_Song", SongId = newId };
                LoadedTracklist.TracklistSongs.Add(newSong);
                SelectedSong = newSong;
                OnPropertyChanged(nameof(TracklistSongs));
                OnPropertyChanged(nameof(SelectedSong));
            }
        }

        private void RemoveSong()
        {
            if (SelectedSong != null)
            {
                LoadedTracklist.TracklistSongs.Remove(SelectedSong);
                OnPropertyChanged(nameof(TracklistSongs));
                OnPropertyChanged(nameof(SelectedSong));
            }
        }

        private bool ErrorValidate()
        {
            var messages = new List<DialogMessage>();
            var result = true;
            if (LoadedTracklist.TracklistSongs.Any(x => !string.IsNullOrEmpty(x.SongFile) && string.IsNullOrEmpty(x.SongPath)))
            {
                messages.Add(new DialogMessage("Missing Song Paths", "One or more songs have a file, but a blank name/path. Add a path to these files to continue."));
                result = false;
            }
            if (LoadedTracklist.TracklistSongs.GroupBy(x => x.SongId).Any(x => x.Count() > 1))
            {
                messages.Add(new DialogMessage("Duplicate Song IDs", "One or more songs have the same song ID. Give all songs a unique song ID to continue."));
                result = false;
            }
            if (messages.Count > 0)
                _dialogService.ShowMessages("Errors have occurred that prevent your tracklist from saving.", "Errors", messages, MessageBoxButton.OK, MessageBoxImage.Error);
            return result;
        }

        private void AlignSongIdsWithFighterIds()
        {
            if (!_dialogService.ShowMessage("This will update all song IDs to align with fighter IDs like in vanilla Brawl. You will have a chance to review before saving. Continue?", "Align IDs", MessageBoxButton.YesNo))
            {
                return;
            }
            var fighterList = _fighterService.GetAllFighterInfo();
            // Get credits theme IDs
            var creditsTable = _fighterService.GetCreditsThemeIds();
            foreach(var fighter in fighterList)
            {
                var slotId = fighter.Ids.SlotConfigId;
                if (slotId != null && creditsTable.Count > slotId)
                {
                    var id = creditsTable[slotId.Value];
                    var result = uint.TryParse(id.Replace("0x", string.Empty), NumberStyles.HexNumber, null, out uint foundId);
                    if (result)
                    {
                        fighter.CreditsThemeId = foundId;
                    }
                }
            }
            // Iterate through songs and update IDs
            var newSongs = new List<TracklistSong>();
            foreach(var song in LoadedTracklist.TracklistSongs)
            {
                var fighters = new List<FighterInfo>();
                if (LoadedTracklist.Name == _settingsService.BuildSettings.FilePathSettings.VictoryThemeTracklist)
                {
                    fighters = fighterList.Where(x => x.VictoryThemeId == song.SongId && x.Ids.FighterConfigId != null).ToList();
                }
                else if (LoadedTracklist.Name == _settingsService.BuildSettings.FilePathSettings.CreditsThemeTracklist)
                {
                    fighters = fighterList.Where(x => x.CreditsThemeId == song.SongId && x.Ids.FighterConfigId != null).ToList();
                }
                if (fighters.Any())
                {
                    foreach(var fighter in fighters)
                    {
                        var newSong = song.Copy();
                        newSong.SongId = 0xFF00 + (uint)fighter.Ids.FighterConfigId;
                        newSong.Name = fighter.DisplayName;
                        newSongs.Add(newSong);
                    }
                }
                else
                {
                    newSongs.Add(song.Copy());
                }
            }
            LoadedTracklist.TracklistSongs = newSongs.OrderBy(x => x.SongId).ToList();
            OnPropertyChanged(nameof(LoadedTracklist));
            _dialogService.ShowMessage("Song IDs updated. To apply changes to your build, make sure to save the tracklist.", "Success");
        }
    }

    internal class TracklistOption
    {
        public string Name { get; set; }
        public ObservableCollection<string> Tracklists { get; set; } = new ObservableCollection<string>();
        public TracklistType TracklistType { get; set; }

        public TracklistOption(string name, TracklistType tracklistType, ObservableCollection<string> tracklists)
        {
            Name = name;
            Tracklists = tracklists;
            TracklistType = tracklistType;
        }
    }
}
