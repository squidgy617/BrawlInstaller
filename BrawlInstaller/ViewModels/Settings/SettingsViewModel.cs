using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        // Commands
        public ICommand SaveSettingsCommand => new RelayCommand(param => SaveSettings());
        public ICommand LoadSettingsCommand => new RelayCommand(param => LoadSettings());
        public ICommand ApplyDefaultSettingCommand => new RelayCommand(param => ApplyDefaultSetting());

        [ImportingConstructor]
        public SettingsViewModel(ISettingsService settingsService, ICosmeticSettingsViewModel cosmeticSettingsViewModel, IFighterInfoViewModel fighterInfoViewModel)
        {
            _settingsService = settingsService;
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
        public FilePath SelectedStageListPath { get => _selectedStageListPath; set { _selectedStageListPath = value; OnPropertyChanged(nameof(SelectedStageListPath)); } }

        [DependsUpon(nameof(BuildSettings))]
        public RosterFile SelectedRosterFile { get => _selectedRosterFile; set { _selectedRosterFile = value; OnPropertyChanged(nameof(SelectedRosterFile)); } }

        [DependsUpon(nameof(BuildSettings))]
        public FilePath SelectedCodeFilePath { get => _selectedCodeFile; set { _selectedCodeFile = value; OnPropertyChanged(nameof(SelectedCodeFilePath)); } }

        [DependsUpon(nameof(BuildSettings))]
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
    }

    // Messages
    public class SettingsLoadedMessage : ValueChangedMessage<BuildSettings>
    {
        public SettingsLoadedMessage(BuildSettings buildSettings) : base(buildSettings)
        {
        }
    }
}
