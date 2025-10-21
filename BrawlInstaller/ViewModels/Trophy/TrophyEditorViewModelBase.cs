using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Helpers;
using BrawlInstaller.Services;
using BrawlInstaller.StaticClasses;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static BrawlInstaller.ViewModels.MainControlsViewModel;

namespace BrawlInstaller.ViewModels
{
    public interface ITrophyEditorViewModelBase
    {
        Trophy Trophy { get; set; }
    }

    [Export(typeof(ITrophyEditorViewModelBase))]
    internal class TrophyEditorViewModelBase : ViewModelBase, ITrophyEditorViewModelBase
    {
        // Private properties
        private Trophy _trophy;
        private Trophy _oldTrophy;
        private CosmeticType _selectedCosmeticOption;
        private List<TrophyGameIcon> _gameIconList;
        private Dictionary<int, string> _trophySeries;
        private Dictionary<int, string> _trophyCategories;

        // Services
        ISettingsService _settingsService;
        IFileService _fileService;
        ITrophyService _trophyService;
        IDialogService _dialogService;

        // Commands
        public ICommand ReplaceThumbnailCommand => new RelayCommand(param => ReplaceCosmetic());
        public ICommand ReplaceHDThumbnailCommand => new RelayCommand(param => ReplaceHDCosmetic());
        public ICommand ClearThumbnailCommand => new RelayCommand(param => ClearCosmetic());
        public ICommand ClearHDThumbnailCommand => new RelayCommand(param => ClearHDCosmetic());
        public ICommand RefreshTrophyIdCommand => new RelayCommand(param => RefreshTrophyId());
        public ICommand RefreshThumbnailIdCommand => new RelayCommand(param => RefreshThumbnailId());

        [ImportingConstructor]
        public TrophyEditorViewModelBase(ISettingsService settingsService, IFileService fileService, ITrophyService trophyService, IDialogService dialogService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _trophyService = trophyService;
            _dialogService = dialogService;

            GameIconList = new List<TrophyGameIcon>();
        }

        // Properties
        public Trophy Trophy { get => _trophy; set { _trophy = value; OnPropertyChanged(nameof(Trophy)); } }
        public Trophy OldTrophy { get => _oldTrophy; set { _oldTrophy = value; OnPropertyChanged(nameof(OldTrophy)); } }
        public Dictionary<int, string> TrophySeries { get => _trophySeries; set { _trophySeries = value; OnPropertyChanged(nameof(TrophySeries)); } }
        public Dictionary<int, string> TrophyCategories { get => _trophyCategories; set { _trophyCategories = value; OnPropertyChanged(nameof(TrophyCategories)); } }
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
        public List<KeyValuePair<string, CosmeticType>> CosmeticOptions { get => DefaultCosmetics.DefaultTrophyCosmetics.Select(x => x.CosmeticType.GetKeyValuePair()).Distinct().ToList(); }

        [DependsUpon(nameof(CosmeticOptions))]
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(nameof(SelectedCosmeticOption)); } }

        [DependsUpon(nameof(SelectedCosmeticOption))]
        public Cosmetic Thumbnail { get => Trophy?.Thumbnails?.Items.FirstOrDefault(x => x.CosmeticType == SelectedCosmeticOption); }

        [DependsUpon(nameof(SelectedCosmeticOption))]
        public int? ThumbnailId { get => Trophy?.Ids?.TrophyThumbnailId; set { Trophy.Ids.TrophyThumbnailId = value; MarkThumbnailsChanged(); OnPropertyChanged(nameof(ThumbnailId)); } }

        // Methods
        public void LoadTrophy(Trophy trophy)
        {
            GameIconList = _trophyService.GetTrophyGameIcons();
            TrophySeries = _trophyService.GetTrophySeries();
            TrophyCategories = _trophyService.GetTrophyCategories();
            Trophy = _trophyService.LoadTrophyData(trophy);
            OldTrophy = Trophy.Copy();
            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;
            OnPropertyChanged(nameof(GameIconList));
            OnPropertyChanged(nameof(Trophy));
        }

        protected Cosmetic AddCosmetic()
        {
            var cosmetic = new Cosmetic
            {
                CosmeticType = SelectedCosmeticOption,
                Style = "vBrawl"
            };
            Trophy.Thumbnails.Add(cosmetic);
            return cosmetic;
        }

        public void ReplaceCosmetic()
        {
            var image = _dialogService.OpenFileDialog("Select image", "PNG image (.png)|*.png");
            if (!string.IsNullOrEmpty(image))
            {
                var bitmap = _fileService.LoadImage(image);
                var cosmetic = Thumbnail;
                if (Thumbnail == null)
                {
                    cosmetic = AddCosmetic();
                }
                cosmetic.Image = bitmap;
                cosmetic.ImagePath = image;
                cosmetic.Texture = null;
                cosmetic.Palette = null;
                Trophy.Thumbnails.ItemChanged(cosmetic);
                OnPropertyChanged(nameof(Thumbnail));
                OnPropertyChanged(nameof(Trophy));
            }
        }

        public void ReplaceHDCosmetic()
        {
            var image = _dialogService.OpenFileDialog("Select HD image", "PNG image (.png)|*.png");
            if (!string.IsNullOrEmpty(image))
            {
                var bitmap = _fileService.LoadImage(image);
                var cosmetic = Thumbnail;
                if (Thumbnail == null)
                {
                    cosmetic = AddCosmetic();
                }
                cosmetic.HDImage = bitmap;
                cosmetic.HDImagePath = image;
                Trophy.Thumbnails.ItemChanged(cosmetic);
                OnPropertyChanged(nameof(Thumbnail));
                OnPropertyChanged(nameof(Trophy));
            }
        }

        public void ClearCosmetic()
        {
            Trophy?.Thumbnails?.Remove(Thumbnail);
            OnPropertyChanged(nameof(Thumbnail));
            OnPropertyChanged(nameof(Trophy));
        }

        public void ClearHDCosmetic()
        {
            if (Thumbnail?.Image == null)
            {
                Trophy.Thumbnails.Remove(Thumbnail);
            }
            else
            {
                Thumbnail.HDImage = null;
                Thumbnail.HDImagePath = "";
                Trophy.Thumbnails.ItemChanged(Thumbnail);
            }
            OnPropertyChanged(nameof(Thumbnail));
            OnPropertyChanged(nameof(Trophy));
        }

        public void MarkThumbnailsChanged()
        {
            Trophy?.Thumbnails?.MarkAllChanged();
        }

        public void RefreshTrophyId()
        {
            if (_dialogService.ShowMessage("This will update your trophy's ID to the first available custom trophy ID in the build. Continue?", "Update Trophy ID", MessageBoxButton.YesNo))
            {
                var trophyIds = _trophyService.GetUnusedTrophyIds(new BrawlIds());
                Trophy.Ids.TrophyId = trophyIds.TrophyId;
                OnPropertyChanged(nameof(Trophy));
            }
        }

        public void RefreshThumbnailId()
        {
            if (_dialogService.ShowMessage("This will update your trophy's thumbnail ID to the first available custom thumbnail ID in the build. Continue?", "Update Trophy ID", MessageBoxButton.YesNo))
            {
                var trophyIds = _trophyService.GetUnusedTrophyIds(new BrawlIds());
                Trophy.Ids.TrophyThumbnailId = trophyIds.TrophyThumbnailId;
                OnPropertyChanged(nameof(Trophy));
            }
        }
    }
}
