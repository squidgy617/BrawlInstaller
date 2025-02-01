﻿using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Helpers;
using BrawlInstaller.Services;
using BrawlLib.Internal.Audio;
using BrawlLib.SSBB.ResourceNodes;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
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
        // These are for volume control
        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        // Private properties
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

        // Commands
        public ICommand LoadTracklistCommand => new RelayCommand(param => LoadTracklist());
        public ICommand PlaySongCommand => new RelayCommand(param => PlaySong());
        public ICommand StopSongCommand => new RelayCommand(param => StopSong());

        [ImportingConstructor]
        public TracklistViewModel(ISettingsService settingsService, IFileService fileService, ITracklistService tracklistService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _tracklistService = tracklistService;

            Volume = 100;
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

        [DependsUpon(nameof(SelectedSong))]
        public bool PlaybackVisible { get => _fileService.FileExists(SelectedSong?.SongFile); }

        public int Volume { get => _volume; set { _volume = value; AdjustVolume(value); OnPropertyChanged(nameof(Volume)); } }

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

        private void PlaySong()
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

        private void StopSong()
        {
            _soundPlayer.Stop();
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
    }
}
