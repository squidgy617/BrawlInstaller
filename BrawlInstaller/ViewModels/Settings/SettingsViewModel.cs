using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

        // Services
        ISettingsService _settingsService { get; }

        // Commands
        public ICommand SaveSettingsCommand => new RelayCommand(param => SaveSettings());
        public ICommand LoadSettingsCommand => new RelayCommand(param => LoadSettings());

        [ImportingConstructor]
        public SettingsViewModel(ISettingsService settingsService, ICosmeticSettingsViewModel cosmeticSettingsViewModel)
        {
            _settingsService = settingsService;
            CosmeticSettingsViewModel = cosmeticSettingsViewModel;

            BuildSettings = _settingsService.BuildSettings;

            WeakReferenceMessenger.Default.Send(new SettingsLoadedMessage(BuildSettings));
        }

        // ViewModels
        public ICosmeticSettingsViewModel CosmeticSettingsViewModel { get; }

        // Properties
        public BuildSettings BuildSettings { get => _buildSettings; set { _buildSettings = value; OnPropertyChanged(nameof(BuildSettings)); } }

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
    }

    // Messages
    public class SettingsLoadedMessage : ValueChangedMessage<BuildSettings>
    {
        public SettingsLoadedMessage(BuildSettings buildSettings) : base(buildSettings)
        {
        }
    }
}
