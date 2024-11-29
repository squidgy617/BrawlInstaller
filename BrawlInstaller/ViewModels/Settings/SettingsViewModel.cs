using BrawlInstaller.Classes;
using BrawlInstaller.Common;
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

        // Services
        ISettingsService _settingsService { get; }

        // Commands
        public ICommand SaveSettingsCommand => new RelayCommand(param => SaveSettings());
        public ICommand LoadSettingsCommand => new RelayCommand(param => LoadSettings());
        public ICommand ApplyDefaultSettingCommand => new RelayCommand(param => ApplyDefaultSetting());

        [ImportingConstructor]
        public SettingsViewModel(ISettingsService settingsService, ICosmeticSettingsViewModel cosmeticSettingsViewModel)
        {
            _settingsService = settingsService;
            CosmeticSettingsViewModel = cosmeticSettingsViewModel;

            BuildSettings = _settingsService.BuildSettings.Copy();

            SelectedSettingsOption = DefaultSettingsOptions.FirstOrDefault();

            WeakReferenceMessenger.Default.Register<UpdateSettingsMessage>(this, (recipient, message) =>
            {
                UpdateSettings();
            });

            WeakReferenceMessenger.Default.Send(new SettingsLoadedMessage(BuildSettings));
        }

        // ViewModels
        public ICosmeticSettingsViewModel CosmeticSettingsViewModel { get; }

        // Properties
        public BuildSettings BuildSettings { get => _buildSettings; set { _buildSettings = value; OnPropertyChanged(nameof(BuildSettings)); } }
        public List<string> DefaultSettingsOptions { get => new List<string> { "ProjectPlus" }; }
        public string SelectedSettingsOption { get => _selectedSettingsOption; set { _selectedSettingsOption = value; OnPropertyChanged(nameof(SelectedSettingsOption)); } }

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
