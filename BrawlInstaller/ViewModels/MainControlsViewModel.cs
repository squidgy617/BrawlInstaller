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
    public interface IMainControlsViewModel
    {
        AppSettings AppSettings { get; }
    }

    [Export(typeof(IMainControlsViewModel))]
    internal class MainControlsViewModel : ViewModelBase, IMainControlsViewModel
    {
        // Private properties
        private AppSettings _appSettings;

        // Services
        ISettingsService _settingsService { get; }

        // Commands
        public ICommand RefreshCommand => new RelayCommand(param => RefreshSettings());

        [ImportingConstructor]
        public MainControlsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            // TODO: Change these!!
            _settingsService.AppSettings.BuildPath = "F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild\\Builds\\P+Ex\\";
            _settingsService.AppSettings.HDTextures = "F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild Launcher - For Netplay\\User\\Load\\Textures\\RSBE01";
            _settingsService.BuildSettings = _settingsService.LoadSettings($"{_settingsService.AppSettings.BuildPath}\\BuildSettings.json");

            AppSettings = _settingsService.AppSettings;
        }

        // Properties
        public AppSettings AppSettings { get => _appSettings; set { _appSettings = value; OnPropertyChanged(nameof(AppSettings)); } }

        // Methods
        private void RefreshSettings()
        {
            _settingsService.AppSettings = AppSettings;
            _settingsService.LoadSettings(AppSettings.BuildPath);
            _settingsService.LoadFighterInfoSettings();
            WeakReferenceMessenger.Default.Send(new UpdateSettingsMessage(AppSettings));
        }

        // Messages
        public class UpdateSettingsMessage : ValueChangedMessage<AppSettings>
        {
            public UpdateSettingsMessage(AppSettings appSettings) : base(appSettings)
            {
            }
        }
    }
}
