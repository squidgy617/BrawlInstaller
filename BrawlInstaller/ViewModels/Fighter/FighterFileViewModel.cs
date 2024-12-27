using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BrawlInstaller.ViewModels
{
    public interface IFighterFileViewModel
    {
        FighterPackage FighterPackage { get; }
    }

    [Export(typeof(IFighterFileViewModel))]
    internal class FighterFileViewModel : ViewModelBase, IFighterFileViewModel
    {
        // Private properties
        private FighterPackage _fighterPackage;
        private FighterPacFile _selectedPacFile;
        private string _selectedExConfig;
        private string _selectedEndingPacFile;

        // Services
        IDialogService _dialogService { get; }
        IFighterService _fighterService { get; }

        // Commands
        public ICommand AddPacFilesCommand => new RelayCommand(param => AddPacFiles());
        public ICommand RemovePacFileCommand => new RelayCommand(param => RemovePacFile());
        public ICommand AddEndingPacFilesCommand => new RelayCommand(param => AddEndingPacFiles());
        public ICommand RemoveEndingPacFileCommand => new RelayCommand(param => RemoveEndingPacFile());
        public ICommand UpdateTracklistSongFileCommand => new RelayCommand(param => UpdateTracklistSongFile((TracklistSong)param));

        // Importing constructor
        [ImportingConstructor]
        public FighterFileViewModel(IDialogService dialogService, IFighterService fighterService)
        {
            _dialogService = dialogService;
            _fighterService = fighterService;
            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadFighterFiles(message);
            });

            WeakReferenceMessenger.Default.Register<AttributesUpdatedMessage>(this, (recipient, message) =>
            {
                OnPropertyChanged(nameof(SoundbankControlsEnabled));
                OnPropertyChanged(nameof(VictoryThemeControlsEnabled));
            });
        }

        // Properties
        public FighterPackage FighterPackage { get => _fighterPackage; set { _fighterPackage = value; OnPropertyChanged(nameof(FighterPackage)); } }

        [DependsUpon(nameof(FighterPackage))]
        public FighterPacFile SelectedPacFile { get => _selectedPacFile; set { _selectedPacFile = value; OnPropertyChanged(nameof(SelectedPacFile)); } }

        [DependsUpon(nameof(FighterPackage))]
        public string SelectedExConfig { get => _selectedExConfig; set { _selectedExConfig = value; OnPropertyChanged(nameof(SelectedExConfig)); } }

        [DependsUpon(nameof(FighterPackage))]
        public string SelectedEndingPacFile { get => _selectedEndingPacFile; set { _selectedEndingPacFile = value; OnPropertyChanged(nameof(SelectedEndingPacFile)); } }

        [DependsUpon(nameof(FighterPackage))]
        public ObservableCollection<FighterPacFile> PacFiles { get => FighterPackage != null ? new ObservableCollection<FighterPacFile>(FighterPackage?.PacFiles) : new ObservableCollection<FighterPacFile>(); }

        [DependsUpon(nameof(FighterPackage))]
        public ObservableCollection<string> EndingPacFiles { get => FighterPackage != null ? new ObservableCollection<string>(FighterPackage?.EndingPacFiles) : new ObservableCollection<string>(); }

        [DependsUpon(nameof(FighterPackage))]
        public bool SoundbankControlsEnabled { get => FighterPackage?.FighterInfo?.FighterAttributes != null; }

        [DependsUpon(nameof(FighterPackage))]
        public uint? SoundbankId { get => FighterPackage?.FighterInfo?.SoundbankId; set { ChangedSoundbankId(FighterPackage.FighterInfo.OriginalSoundbankId, value); OnPropertyChanged(nameof(SoundbankId)); } }

        [DependsUpon(nameof(SoundbankId))]
        public bool SoundbankIdControlEnabled { get => FighterPackage?.FighterInfo?.OriginalSoundbankId == null || SoundbankId == null || SoundbankId >= 324; }

        [DependsUpon(nameof(FighterPackage))]
        public uint? KirbySoundbankId { get => FighterPackage?.FighterInfo?.KirbySoundbankId; set { ChangedKirbySoundbankId(FighterPackage.FighterInfo.OriginalKirbySoundbankId, value); OnPropertyChanged(nameof(KirbySoundbankId)); } }

        [DependsUpon(nameof(KirbySoundbankId))]
        public bool KirbySoundbankIdControlEnabled { get => FighterPackage?.FighterInfo?.OriginalKirbySoundbankId == null || KirbySoundbankId == null || KirbySoundbankId >= 324; }

        [DependsUpon(nameof(FighterPackage))]
        public bool VictoryThemeControlsEnabled { get => FighterPackage?.FighterInfo?.SlotAttributes != null; }

        [DependsUpon(nameof(FighterPackage))]
        public uint? VictoryThemeId { get => FighterPackage?.VictoryTheme?.SongId; set { ChangedThemeId(FighterPackage?.VictoryTheme, value); OnPropertyChanged(nameof(VictoryThemeId)); } }

        [DependsUpon(nameof(FighterPackage))]
        public uint? CreditsThemeId { get => FighterPackage?.CreditsTheme?.SongId; set { ChangedThemeId(FighterPackage?.CreditsTheme, value); OnPropertyChanged(nameof(CreditsThemeId)); } }

        [DependsUpon(nameof(FighterPackage))]
        public Dictionary<string, FighterFileType> FighterFileTypes { get => typeof(FighterFileType).GetDictionary<FighterFileType>().ToDictionary(x => FighterPackage != null ? FighterPacFile.GetPrefix(x.Value, FighterPackage?.FighterInfo) : x.Key, x => x.Value); }

        // Methods
        public void LoadFighterFiles(FighterLoadedMessage message)
        {
            FighterPackage = message.Value;
        }

        public void ChangedThemeId(TracklistSong tracklist, uint? songId)
        {
            if (tracklist != null)
            {
                if (songId != null)
                {
                    var idString = songId.ToString();
                    var result = uint.TryParse(idString.Replace("0x", string.Empty), NumberStyles.Number, null, out uint id);
                    if (result)
                    {
                        if (id < 0x0000F000)
                        {
                            tracklist.SongFile = null;
                            tracklist.SongPath = string.Empty;
                            OnPropertyChanged(nameof(FighterPackage));
                            _dialogService.ShowMessage("ID is less than minimum custom ID value of 0xF000. Tracklist entries will not be created for non-custom IDs. If you'd like to import a song, change the ID to 0xF000 or greater.", "Song Will Not Import");
                        }
                    }
                    tracklist.SongId = (uint)songId;
                }
            }
        }

        public void UpdateTracklistSongFile(TracklistSong song)
        {
            if (song.SongId < 0xF000)
            {
                song.SongId = 0xF000;
                OnPropertyChanged(nameof(FighterPackage));
            }
        }

        public void ChangedSoundbankId(uint? oldSoundbankId, uint? newSoundbankId)
        {
            if (oldSoundbankId != null && newSoundbankId != null && newSoundbankId < 324)
            {
                _dialogService.ShowMessage("Cannot change to a non-custom soundbank. Custom soundbanks start at 0x144 (324).", "Non-Custom Soundbank ID", MessageBoxImage.Error);
                FighterPackage.FighterInfo.SoundbankId = oldSoundbankId;
            }
            else
            {
                FighterPackage.FighterInfo.SoundbankId = newSoundbankId;
            }
        }

        public void ChangedKirbySoundbankId(uint? oldSoundbankId, uint? newSoundbankId)
        {
            if (oldSoundbankId != null && newSoundbankId != null && newSoundbankId < 324)
            {
                _dialogService.ShowMessage("Cannot change to a non-custom soundbank. Custom soundbanks start at 0x144 (324).", "Non-Custom Soundbank ID", MessageBoxImage.Error);
                FighterPackage.FighterInfo.KirbySoundbankId = oldSoundbankId;
            }
            else
            {
                FighterPackage.FighterInfo.KirbySoundbankId = newSoundbankId;
            }
        }

        public void AddPacFiles()
        {
            var files = _dialogService.OpenMultiFileDialog("Select pac files", "PAC files (.pac)|*.pac");
            foreach (var file in files)
            {
                var pacFile = new FighterPacFile { FilePath = file };
                pacFile = _fighterService.GetFighterPacName(pacFile, FighterPackage.FighterInfo, false);
                FighterPackage.PacFiles.Add(pacFile);
            }
            OnPropertyChanged(nameof(FighterPackage));
        }

        public void RemovePacFile()
        {
            FighterPackage.PacFiles.Remove(SelectedPacFile);
            OnPropertyChanged(nameof(FighterPackage));
        }

        public void AddEndingPacFiles()
        {
            var files = _dialogService.OpenMultiFileDialog("Select ending pac files", "PAC files (.pac)|*.pac");
            foreach (var file in files)
            {
                var pacFile = file;
                FighterPackage.EndingPacFiles.Add(pacFile);
            }
            OnPropertyChanged(nameof(FighterPackage));
        }

        public void RemoveEndingPacFile()
        {
            FighterPackage.EndingPacFiles.Remove(SelectedEndingPacFile);
            OnPropertyChanged(nameof(FighterPackage));
        }
    }
}
