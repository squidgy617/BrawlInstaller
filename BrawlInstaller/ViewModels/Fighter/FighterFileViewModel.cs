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
        ITracklistService _tracklistService { get; }
        ISettingsService _settingsService { get; }

        // Commands
        public ICommand AddPacFilesCommand => new RelayCommand(param => AddPacFiles());
        public ICommand RemovePacFileCommand => new RelayCommand(param => RemovePacFile());
        public ICommand AddEndingPacFilesCommand => new RelayCommand(param => AddEndingPacFiles());
        public ICommand RemoveEndingPacFileCommand => new RelayCommand(param => RemoveEndingPacFile());
        public ICommand UpdateTracklistSongFileCommand => new RelayCommand(param => UpdateTracklistSongFile((TracklistSong)param));
        public ICommand SelectVictoryThemeCommand => new RelayCommand(param =>  SelectVictoryTheme());
        public ICommand SelectCreditsThemeCommand => new RelayCommand(param => SelectCreditsTheme());
        public ICommand RefreshSoundbankIdCommand => new RelayCommand(param => RefreshSoundbankId());
        public ICommand RefreshKirbySoundbankIdCommand => new RelayCommand(param => RefreshKirbySoundbankId());
        public ICommand RemoveInstallOptionCommand => new RelayCommand(param => RemoveInstallOption(param));
        public ICommand AddInstallOptionCommand => new RelayCommand(param => AddInstallOption());
        public ICommand UpdateInstallOptionSelectionCommand => new RelayCommand(param => UpdateInstallOptionSelection(param));

        // Importing constructor
        [ImportingConstructor]
        public FighterFileViewModel(IDialogService dialogService, IFighterService fighterService, ITracklistService tracklistService, ISettingsService settingsService)
        {
            _dialogService = dialogService;
            _fighterService = fighterService;
            _tracklistService = tracklistService;
            _settingsService = settingsService;
            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadFighterFiles(message);
            });

            WeakReferenceMessenger.Default.Register<AttributesUpdatedMessage>(this, (recipient, message) =>
            {
                OnPropertyChanged(nameof(SoundbankId));
                OnPropertyChanged(nameof(KirbySoundbankId));
                if (FighterPackage?.VictoryTheme != null)
                    FighterPackage.VictoryTheme.SongId = FighterPackage.FighterInfo.VictoryThemeId;
                OnPropertyChanged(nameof(VictoryThemeId));
                OnPropertyChanged(nameof(SoundbankControlsEnabled));
                OnPropertyChanged(nameof(VictoryThemeIdEnabled));
                OnPropertyChanged(nameof(CreditsThemeIdEnabled));
            });
        }

        // Properties
        public FighterPackage FighterPackage { get => _fighterPackage; set { _fighterPackage = value; OnPropertyChanged(nameof(FighterPackage)); } }

        public List<FighterInfo> FighterInfoList { get => _settingsService.FighterInfoList; }

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
        public uint? OldSoundbankId { get => FighterPackage?.FighterInfo?.OriginalSoundbankId; set { FighterPackage.FighterInfo.OriginalSoundbankId = value; OnPropertyChanged(nameof(OldSoundbankId)); } }

        [DependsUpon(nameof(FighterPackage))]
        public uint? SoundbankId { get => FighterPackage?.FighterInfo?.SoundbankId; set { ChangedSoundbankId(FighterPackage.FighterInfo.OriginalSoundbankId, value); OnPropertyChanged(nameof(SoundbankId)); } }

        [DependsUpon(nameof(SoundbankId))]
        [DependsUpon(nameof(OldSoundbankId))]
        public bool SoundbankIdControlEnabled { get => OldSoundbankId == null || SoundbankId == null || OldSoundbankId >= 324; }

        [DependsUpon(nameof(FighterPackage))]
        public uint? OldKirbySoundbankId { get => FighterPackage?.FighterInfo?.OriginalKirbySoundbankId; set { FighterPackage.FighterInfo.OriginalKirbySoundbankId = value; OnPropertyChanged(nameof(OldKirbySoundbankId)); } }

        [DependsUpon(nameof(FighterPackage))]
        [DependsUpon(nameof(OldKirbySoundbankId))]
        public uint? KirbySoundbankId { get => FighterPackage?.FighterInfo?.KirbySoundbankId; set { ChangedKirbySoundbankId(FighterPackage.FighterInfo.OriginalKirbySoundbankId, value); OnPropertyChanged(nameof(KirbySoundbankId)); } }

        [DependsUpon(nameof(KirbySoundbankId))]
        public bool KirbySoundbankIdControlEnabled { get => OldKirbySoundbankId == null || KirbySoundbankId == null || OldKirbySoundbankId >= 324; }

        [DependsUpon(nameof(FighterPackage))]
        public uint? VictoryThemeId { get => FighterPackage?.VictoryTheme?.SongId; set { ChangedThemeId(FighterPackage?.VictoryTheme, value); OnPropertyChanged(nameof(VictoryThemeId)); } }

        [DependsUpon(nameof(FighterPackage))]
        public bool VictoryThemeIdEnabled { get => !_settingsService.BuildSettings.MiscSettings.VictoryThemesUseFighterIds && FighterPackage?.FighterInfo?.SlotAttributes != null; }

        [DependsUpon(nameof(FighterPackage))]
        public bool ReplaceVictoryThemeEnabled { get => !_settingsService.BuildSettings.MiscSettings.VictoryThemesUseFighterIds; }

        [DependsUpon(nameof(FighterPackage))]
        public uint? CreditsThemeId { get => FighterPackage?.CreditsTheme?.SongId; set { ChangedThemeId(FighterPackage?.CreditsTheme, value); OnPropertyChanged(nameof(CreditsThemeId)); } }

        [DependsUpon(nameof(FighterPackage))]
        public bool CreditsThemeIdEnabled { get => !_settingsService.BuildSettings.MiscSettings.CreditsThemesUseFighterIds; }

        [DependsUpon(nameof(FighterPackage))]
        public bool ReplaceCreditsThemeEnabled { get => !_settingsService.BuildSettings.MiscSettings.CreditsThemesUseFighterIds; }

        [DependsUpon(nameof(FighterPackage))]
        public Dictionary<string, FighterFileType> FighterFileTypes { get => typeof(FighterFileType).GetDictionary<FighterFileType>().ToDictionary(x => FighterPackage != null ? FighterPacFile.GetPrefix(x.Value, FighterPackage?.FighterInfo) : x.Key, x => x.Value); }

        public List<string> PacFileSuffixes { get => StaticClasses.PacFiles.PacFileSuffixes; }

        public Dictionary<string, InstallOptionType> InstallOptionTypes { get => typeof(InstallOptionType).GetDictionary<InstallOptionType>().ToDictionary(x => x.Key, x => x.Value); }

        [DependsUpon(nameof(FighterPackage))]
        public ObservableCollection<FighterInstallOption> InstallOptions { get => FighterPackage?.InstallOptions != null ? new ObservableCollection<FighterInstallOption>(FighterPackage.InstallOptions) : new ObservableCollection<FighterInstallOption>(); }

        [DependsUpon(nameof(SelectedPacFile))]
        public string SelectedSuffix { get => GetDisplaySuffix(SelectedPacFile?.Suffix); set { SelectedPacFile.Suffix = value; OnPropertyChanged(nameof(SelectedSuffix)); } }

        [DependsUpon(nameof(FighterPackage))]
        public List<string> ExtraSuffixes { get => FighterPackage?.FighterInfo?.IsKirby == true ? FighterInfoList.Where(x => !x.IsKirby).Select(x => x.PartialPacName).ToList() : new List<string>(); }

        // Methods
        private string GetDisplaySuffix(string suffix)
        {
            if (!string.IsNullOrEmpty(suffix) && suffix.StartsWith("$") && suffix.Substring(1).Length % 2 == 0)
            {
                suffix = suffix.Substring(1);
                var newSuffix = string.Empty;
                for (int i = 0; i < suffix.Length; i += 4)
                {
                    var hexPair = suffix.Substring(i, 4);
                    var byteArray = new byte[2];
                    byteArray[0] = Convert.ToByte(hexPair.Substring(0, 2), 16);
                    byteArray[1] = Convert.ToByte(hexPair.Substring(2, 2), 16);
                    newSuffix += Encoding.BigEndianUnicode.GetString(byteArray);
                }
                suffix = "$" + newSuffix;
            }
            return suffix;
        }

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
                song.SongId = 0xFF00;
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

        public void SelectVictoryTheme()
        {
            var result = SelectTracklistSong(_settingsService.BuildSettings.FilePathSettings.VictoryThemeTracklist, "Select a victory theme");
            if (result != null)
            {
                if (!_settingsService.BuildSettings.MiscSettings.VictoryThemesUseFighterIds)
                {
                    FighterPackage.VictoryTheme.Name = result.Name;
                    FighterPackage.VictoryTheme.SongId = result.SongId;
                    FighterPackage.VictoryTheme.ReplaceExisting = true;
                }
                FighterPackage.VictoryTheme.SongFile = result.SongFile;
                FighterPackage.VictoryTheme.SongPath = result.SongPath;
                OnPropertyChanged(nameof(FighterPackage));
            }
        }

        public void SelectCreditsTheme()
        {
            var result = SelectTracklistSong(_settingsService.BuildSettings.FilePathSettings.CreditsThemeTracklist, "Select a credits theme");
            if (result != null)
            {
                if (!_settingsService.BuildSettings.MiscSettings.CreditsThemesUseFighterIds)
                {
                    FighterPackage.CreditsTheme.Name = result.Name;
                    FighterPackage.CreditsTheme.SongId = result.SongId;
                    FighterPackage.CreditsTheme.ReplaceExisting = true;
                }
                FighterPackage.CreditsTheme.SongFile = result.SongFile;
                FighterPackage.CreditsTheme.SongPath = result.SongPath;
                OnPropertyChanged(nameof(FighterPackage));
            }
        }

        private TracklistSong SelectTracklistSong(string tracklist, string caption)
        {
            var songs = _tracklistService.GetAllTracklistSongs(tracklist);
            var result = _dialogService.OpenDropDownDialog(songs, "Name", "Select a song", caption);
            return result as TracklistSong;
        }

        private void RefreshSoundbankId()
        {
            SoundbankId = GetUnusedSoundbankId(SoundbankId, KirbySoundbankId);
        }

        private void RefreshKirbySoundbankId()
        {
            KirbySoundbankId = GetUnusedSoundbankId(KirbySoundbankId, SoundbankId);
        }

        private uint? GetUnusedSoundbankId(uint? currentId, uint? otherUsedId)
        {
            if (_dialogService.ShowMessage("This will update your fighter's soundbank ID to the first available custom soundbank ID in the build. Continue?", "Update Sounbank ID", MessageBoxButton.YesNo))
            {
                uint newSoundbankId = 324; // 324 is first custom soundbank ID
                var usedIds = _fighterService.GetUsedSoundbankIds();
                while (usedIds.Contains(newSoundbankId) || newSoundbankId == otherUsedId)
                {
                    newSoundbankId++;
                }
                return newSoundbankId;
            }
            return currentId;
        }

        private void UpdateInstallOptionSelection(object param)
        {
            var installOption = param as FighterInstallOption;
            OnPropertyChanged(nameof(InstallOptions));
        }

        private void RemoveInstallOption(object param)
        {
            var installOption = param as FighterInstallOption;
            if (installOption != null && FighterPackage.InstallOptions.Count(x => x.Type == installOption.Type) > 1)
            {
                FighterPackage.InstallOptions.Remove(installOption);
                OnPropertyChanged(nameof(InstallOptions));
            }
        }

        private void AddInstallOption()
        {
            FighterPackage.InstallOptions.Add(new FighterInstallOption());
            OnPropertyChanged(nameof(InstallOptions));
        }
    }
}
