using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrawlInstaller.ViewModels
{
    public interface ITrophyEditorViewModel
    {
        Trophy Trophy { get; set; }
    }

    [Export(typeof(ITrophyEditorViewModel))]
    internal class TrophyEditorViewModel : ViewModelBase, ITrophyEditorViewModel
    {
        // Private properties
        private Trophy _trophy;

        // Services
        ISettingsService _settingsService;
        IFileService _fileService;
        ITrophyService _trophyService;

        // Commands

        [ImportingConstructor]
        public TrophyEditorViewModel(ISettingsService settingsService, IFileService fileService, ITrophyService trophyService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _trophyService = trophyService;

            WeakReferenceMessenger.Default.Register<LoadTrophyMessage>(this, (recipient, message) =>
            {
                LoadTrophy(message);
            });
        }

        // Properties
        public Trophy Trophy { get => _trophy; set { _trophy = value; OnPropertyChanged(nameof(Trophy)); } }

        // Methods
        public void LoadTrophy(LoadTrophyMessage message)
        {
            var trophy = message.Value;
            Trophy = _trophyService.LoadTrophyData(trophy);
            OnPropertyChanged(nameof(Trophy));
        }
    }
}
