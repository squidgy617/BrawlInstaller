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
    }

    // Messages
    public class SettingsLoadedMessage : ValueChangedMessage<BuildSettings>
    {
        public SettingsLoadedMessage(BuildSettings buildSettings) : base(buildSettings)
        {
        }
    }
}
