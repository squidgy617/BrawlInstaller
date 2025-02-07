using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
using BrawlInstaller.StaticClasses;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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
        private List<TrophyGameIcon> _gameIconList;

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

            GameIconList = new List<TrophyGameIcon>();

            WeakReferenceMessenger.Default.Register<LoadTrophyMessage>(this, (recipient, message) =>
            {
                LoadTrophy(message);
            });
        }

        // Properties
        public Trophy Trophy { get => _trophy; set { _trophy = value; OnPropertyChanged(nameof(Trophy)); } }
        public Dictionary<string, int> TrophySeries { get => Trophies.Series; }
        public Dictionary<string, int> TrophyCategories { get => Trophies.Categories; }
        public List<TrophyGameIcon> GameIconList { get => _gameIconList; set { _gameIconList = value; OnPropertyChanged(nameof(GameIconList)); } }

        [DependsUpon(nameof(Trophy))]
        public int? SelectedGameIcon1 { get => Trophy?.GameIcon1; set { Trophy.GameIcon1 = (value ?? 0); OnPropertyChanged(nameof(SelectedGameIcon1)); } }

        [DependsUpon(nameof(Trophy))]
        public int? SelectedGameIcon2 { get => Trophy?.GameIcon2; set { Trophy.GameIcon2 = (value ?? 0); OnPropertyChanged(nameof(SelectedGameIcon2)); } }

        [DependsUpon(nameof(SelectedGameIcon1))]
        public BitmapImage GameIcon1 { get => GameIconList.FirstOrDefault(x => x.Id == Trophy?.GameIcon1)?.Image; }

        [DependsUpon(nameof(SelectedGameIcon2))]
        public BitmapImage GameIcon2 { get => GameIconList.FirstOrDefault(x => x.Id == Trophy?.GameIcon2)?.Image; }

        [DependsUpon(nameof(Trophy))]
        public Cosmetic Thumbnail { get => Trophy?.Thumbnails?.Items.FirstOrDefault(); }

        // Methods
        public void LoadTrophy(LoadTrophyMessage message)
        {
            GameIconList = _trophyService.GetTrophyGameIcons();
            var trophy = message.Value;
            Trophy = _trophyService.LoadTrophyData(trophy);
            OnPropertyChanged(nameof(GameIconList));
            OnPropertyChanged(nameof(Trophy));
        }
    }
}
