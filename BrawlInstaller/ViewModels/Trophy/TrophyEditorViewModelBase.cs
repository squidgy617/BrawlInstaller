﻿using BrawlInstaller.Classes;
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
        private List<TrophyGameIcon> _gameIconList;

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

        [DependsUpon(nameof(Trophy))]
        public int? ThumbnailId { get => Trophy?.Ids?.TrophyThumbnailId; set { Trophy.Ids.TrophyThumbnailId = value; MarkThumbnailsChanged(); OnPropertyChanged(nameof(ThumbnailId)); } }

        // Methods
        public void LoadTrophy(Trophy trophy)
        {
            GameIconList = _trophyService.GetTrophyGameIcons();
            Trophy = _trophyService.LoadTrophyData(trophy);
            OldTrophy = Trophy.Copy();
            OnPropertyChanged(nameof(GameIconList));
            OnPropertyChanged(nameof(Trophy));
        }

        protected Cosmetic AddCosmetic()
        {
            var cosmetic = new Cosmetic
            {
                CosmeticType = CosmeticType.TrophyThumbnail,
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
                if (Thumbnail == null)
                {
                    AddCosmetic();
                }
                Thumbnail.Image = bitmap;
                Thumbnail.ImagePath = image;
                Thumbnail.Texture = null;
                Thumbnail.Palette = null;
                Trophy.Thumbnails.ItemChanged(Thumbnail);
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
                if (Thumbnail == null)
                {
                    AddCosmetic();
                }
                Thumbnail.HDImage = bitmap;
                Thumbnail.HDImagePath = image;
                Trophy.Thumbnails.ItemChanged(Thumbnail);
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
    }
}
