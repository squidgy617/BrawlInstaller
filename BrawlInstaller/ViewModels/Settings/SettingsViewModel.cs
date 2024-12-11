using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using static BrawlInstaller.ViewModels.MainControlsViewModel;
using static System.Net.Mime.MediaTypeNames;

namespace BrawlInstaller.ViewModels
{
    public interface ISettingsViewModel
    {
        BuildSettings BuildSettings { get; }
    }

    [Export(typeof(ISettingsViewModel))]
    internal class SettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        // Private properties
        private BuildSettings _buildSettings;
        private string _selectedSettingsOption;
        private CompositeCollection _filePathSettings;
        private FilePath _selectedStageListPath;
        private RosterFile _selectedRosterFile;
        private FilePath _selectedCodeFile;
        private InstallLocation _selectedRandomStageNameLocation;

        // Services
        ISettingsService _settingsService { get; }
        IFileService _fileService { get; }
        IDialogService _dialogService { get; }

        // Commands
        public ICommand SaveSettingsCommand => new RelayCommand(param => SaveSettings());
        public ICommand LoadSettingsCommand => new RelayCommand(param => LoadSettings());
        public ICommand ApplyDefaultSettingCommand => new RelayCommand(param => ApplyDefaultSetting());
        public ICommand AddStageListCommand => new RelayCommand(param => AddStageList());
        public ICommand RemoveStageListCommand => new RelayCommand(param => RemoveStageList());
        public ICommand AddRosterCommand => new RelayCommand(param => AddRosterFile());
        public ICommand RemoveRosterCommand => new RelayCommand(param => RemoveRosterFile());
        public ICommand AddCodeFileCommand => new RelayCommand(param => AddCodePath());
        public ICommand RemoveCodeFileCommand => new RelayCommand(param => RemoveCodePath());
        public ICommand AddRandomStageNameLocationCommand => new RelayCommand(param => AddRandomStageNameLocation());
        public ICommand RemoveRandomStageNameLocationCommand => new RelayCommand(param => RemoveRandomStageNameLocation());
        public ICommand SelectRandomStageNameLocationCommand => new RelayCommand(param => SelectRandomStageNameLocation());
        public ICommand ClearRandomStageNameNodeCommand => new RelayCommand(param => ClearRandomStageNameNode());

        [ImportingConstructor]
        public SettingsViewModel(ISettingsService settingsService, IFileService fileService, IDialogService dialogService, ICosmeticSettingsViewModel cosmeticSettingsViewModel, IFighterInfoViewModel fighterInfoViewModel)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _dialogService = dialogService;
            CosmeticSettingsViewModel = cosmeticSettingsViewModel;
            FighterInfoViewModel = fighterInfoViewModel;

            BuildSettings = _settingsService.BuildSettings.Copy();

            FilePathSettings = new CompositeCollection
            {
                new CollectionContainer() { Collection = BuildSettings.FilePathSettings.FilePaths },
                new CollectionContainer() { Collection = BuildSettings.FilePathSettings.AsmPaths }
            };

            SelectedSettingsOption = DefaultSettingsOptions.FirstOrDefault();

            WeakReferenceMessenger.Default.Register<UpdateSettingsMessage>(this, (recipient, message) =>
            {
                UpdateSettings();
            });

            WeakReferenceMessenger.Default.Send(new SettingsLoadedMessage(BuildSettings));
        }

        // ViewModels
        public ICosmeticSettingsViewModel CosmeticSettingsViewModel { get; }
        public IFighterInfoViewModel FighterInfoViewModel { get; }

        // Properties
        public AppSettings AppSettings { get => _settingsService.AppSettings; }
        public BuildSettings BuildSettings { get => _buildSettings; set { _buildSettings = value; OnPropertyChanged(nameof(BuildSettings)); } }
        public List<string> DefaultSettingsOptions { get => new List<string> { "ProjectPlus" }; }
        public string SelectedSettingsOption { get => _selectedSettingsOption; set { _selectedSettingsOption = value; OnPropertyChanged(nameof(SelectedSettingsOption)); } }

        [DependsUpon(nameof(BuildSettings))]
        public CompositeCollection FilePathSettings { get => _filePathSettings; set { _filePathSettings = value; OnPropertyChanged(nameof(FilePathSettings)); } }

        [DependsUpon(nameof(BuildSettings))]
        public ObservableCollection<FilePath> StageListPaths { get => BuildSettings.FilePathSettings.StageListPaths != null ? new ObservableCollection<FilePath>(BuildSettings.FilePathSettings.StageListPaths) : new ObservableCollection<FilePath>(); }

        [DependsUpon(nameof(StageListPaths))]
        public FilePath SelectedStageListPath { get => _selectedStageListPath; set { _selectedStageListPath = value; OnPropertyChanged(nameof(SelectedStageListPath)); } }

        [DependsUpon(nameof(BuildSettings))]
        public ObservableCollection<RosterFile> RosterFiles { get => BuildSettings.FilePathSettings.RosterFiles != null ? new ObservableCollection<RosterFile>(BuildSettings.FilePathSettings.RosterFiles) : new ObservableCollection<RosterFile>(); }

        [DependsUpon(nameof(RosterFiles))]
        public RosterFile SelectedRosterFile { get => _selectedRosterFile; set { _selectedRosterFile = value; OnPropertyChanged(nameof(SelectedRosterFile)); } }

        [DependsUpon(nameof(BuildSettings))]
        public ObservableCollection<FilePath> CodeFilePaths { get => BuildSettings.FilePathSettings.CodeFilePaths != null ? new ObservableCollection<FilePath>(BuildSettings.FilePathSettings.CodeFilePaths) : new ObservableCollection<FilePath>(); }

        [DependsUpon(nameof(CodeFilePaths))]
        public FilePath SelectedCodeFilePath { get => _selectedCodeFile; set { _selectedCodeFile = value; OnPropertyChanged(nameof(SelectedCodeFilePath)); } }

        [DependsUpon(nameof(BuildSettings))]
        public ObservableCollection<InstallLocation> RandomStageNamesLocations { get => BuildSettings.FilePathSettings.RandomStageNamesLocations != null ? new ObservableCollection<InstallLocation>(BuildSettings.FilePathSettings.RandomStageNamesLocations) : new ObservableCollection<InstallLocation>(); }

        [DependsUpon(nameof(RandomStageNamesLocations))]
        public InstallLocation SelectedRandomStageNameLocation { get => _selectedRandomStageNameLocation; set { _selectedRandomStageNameLocation = value; OnPropertyChanged(nameof(SelectedRandomStageNameLocation)); } }

        public List<string> ExtensionOptions { get => new List<string> { "brres", "pac" }; }

        public Dictionary<string, SoundbankStyle> SoundbankStyleOptions { get => typeof(SoundbankStyle).GetDictionary<SoundbankStyle>(); }

        // Methods

        // TODO: Should these save to a different location?
        private void SaveSettings()
        {
            _settingsService.SaveSettings(BuildSettings, $"{_settingsService.AppSettings.BuildPath}\\BuildSettings.json");
        }

        private void LoadSettings()
        {
            BuildSettings = _settingsService.LoadSettings($"{_settingsService.AppSettings.BuildPath}\\BuildSettings.json");
        }

        private void UpdateSettings()
        {
            LoadSettings();
            _settingsService.BuildSettings = BuildSettings;
            OnPropertyChanged(nameof(BuildSettings));
            WeakReferenceMessenger.Default.Send(new SettingsLoadedMessage(BuildSettings));
        }

        private void ApplyDefaultSetting()
        {
            var json = GetSelectedSettings("BuildSettings.json");
            BuildSettings = JsonConvert.DeserializeObject<BuildSettings>(json);
            OnPropertyChanged(nameof(BuildSettings));
            WeakReferenceMessenger.Default.Send(new SettingsLoadedMessage(BuildSettings));
        }

        private string GetSelectedSettings(string file)
        {
            var json = string.Empty;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"BrawlInstaller.Resources.DefaultSettings.{SelectedSettingsOption}.{file}"))
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    json = streamReader.ReadToEnd();
                }
            }
            return json;
        }

        public void AddStageList()
        {
            var stageLists = BuildSettings.FilePathSettings.StageListPaths;
            stageLists.Add(new FilePath(FileType.StageListFile, ""));
            OnPropertyChanged(nameof(StageListPaths));
        }

        public void RemoveStageList()
        {
            var stageLists = BuildSettings.FilePathSettings.StageListPaths;
            if (stageLists.Count > 0 && SelectedStageListPath != null)
            {
                stageLists.Remove(SelectedStageListPath);
                OnPropertyChanged(nameof(StageListPaths));
            }
        }

        public void AddRosterFile()
        {
            var rosterFiles = BuildSettings.FilePathSettings.RosterFiles;
            rosterFiles.Add(new RosterFile());
            OnPropertyChanged(nameof(RosterFiles));
        }

        public void RemoveRosterFile()
        {
            var rosterFiles = BuildSettings.FilePathSettings.RosterFiles;
            if (rosterFiles.Count > 0 && SelectedRosterFile != null)
            {
                rosterFiles.Remove(SelectedRosterFile);
                OnPropertyChanged(nameof(RosterFiles));
            }
        }

        public void AddCodePath()
        {
            var codeFilePaths = BuildSettings.FilePathSettings.CodeFilePaths;
            codeFilePaths.Add(new FilePath(FileType.GCTCodeFile, ""));
            OnPropertyChanged(nameof(CodeFilePaths));
        }

        public void RemoveCodePath()
        {
            var codeFilePaths = BuildSettings.FilePathSettings.CodeFilePaths;
            if (codeFilePaths.Count > 0 && SelectedCodeFilePath != null)
            {
                codeFilePaths.Remove(SelectedCodeFilePath);
                OnPropertyChanged(nameof(CodeFilePaths));
            }
        }

        public void AddRandomStageNameLocation()
        {
            var randomStageNameLocations = BuildSettings.FilePathSettings.RandomStageNamesLocations;
            randomStageNameLocations.Add(new InstallLocation());
            OnPropertyChanged(nameof(RandomStageNamesLocations));
        }

        public void RemoveRandomStageNameLocation()
        {
            var randomStageNameLocations = BuildSettings.FilePathSettings.RandomStageNamesLocations;
            if (randomStageNameLocations.Count > 0 && SelectedRandomStageNameLocation != null)
            {
                randomStageNameLocations.Remove(SelectedRandomStageNameLocation);
                OnPropertyChanged(nameof(RandomStageNamesLocations));
            }
        }

        public void SelectRandomStageNameLocation()
        {
            if (SelectedRandomStageNameLocation != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var rootNode = _fileService.OpenFile(Path.Combine(buildPath, SelectedRandomStageNameLocation.FilePath));
                if (rootNode != null)
                {
                    var nodes = _fileService.GetNodes(rootNode);
                    var result = _dialogService.OpenNodeSelectorDialog(nodes, "Select Node", "Select node containing random stage names");
                    if (result != null)
                    {
                        SelectedRandomStageNameLocation.NodePath = result.TreePath;
                        OnPropertyChanged(nameof(RandomStageNamesLocations));
                    }
                    _fileService.CloseFile(rootNode);
                }
            }
        }

        public void ClearRandomStageNameNode()
        {
            if (SelectedRandomStageNameLocation != null)
            {
                SelectedRandomStageNameLocation.NodePath = string.Empty;
                OnPropertyChanged(nameof(RandomStageNamesLocations));
            }
        }
    }

    // Messages
    public class SettingsLoadedMessage : ValueChangedMessage<BuildSettings>
    {
        public SettingsLoadedMessage(BuildSettings buildSettings) : base(buildSettings)
        {
        }
    }
}
