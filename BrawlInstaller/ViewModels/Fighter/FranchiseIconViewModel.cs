using BrawlInstaller.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.Windows.Input;
using BrawlLib.SSBB.ResourceNodes;
using System.Diagnostics;
using BrawlInstaller.Services;
using BrawlInstaller.Classes;
using BrawlLib.Internal;
using CommunityToolkit.Mvvm.Messaging;
using System.Drawing;

namespace BrawlInstaller.ViewModels
{
    public interface IFranchiseIconViewModel
    {
        CosmeticList FranchiseIcons { get; }
        Cosmetic SelectedFranchiseIcon { get; }
        ICommand SelectModelCommand { get; }
    }

    [Export(typeof(IFranchiseIconViewModel))]
    internal class FranchiseIconViewModel : ViewModelBase, IFranchiseIconViewModel
    {
        // TODO: Allow changing icon ID, can probably use a command OnChanged to validate that the ID isn't already used
        // Commands
        public ICommand SelectModelCommand => new RelayCommand(param => SelectModel());
        public ICommand ClearModelCommand => new RelayCommand(param => ClearModel());
        public ICommand ReplaceIconCommand => new RelayCommand(param => ReplaceIcon());
        public ICommand ReplaceHDIconCommand => new RelayCommand(param => ReplaceHDIcon());
        public ICommand ClearHDIconCommand => new RelayCommand(param => ClearHDIcon());

        // Private Properties
        private CosmeticList _franchiseIcons;
        private Cosmetic _selectedFranchiseIcon;

        // Services
        ICosmeticService _cosmeticService;
        IDialogService _dialogService;

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public FranchiseIconViewModel(ICosmeticService cosmeticService, IDialogService dialogService)
        {
            _cosmeticService = cosmeticService;
            _dialogService = dialogService;
            FranchiseIcons = new CosmeticList();

            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadIcons(message);
            });
        }

        // Properties
        public CosmeticList FranchiseIcons { get => _franchiseIcons; set { _franchiseIcons = value; OnPropertyChanged(nameof(FranchiseIcons)); } }
        public Cosmetic SelectedFranchiseIcon { get => _selectedFranchiseIcon; set { _selectedFranchiseIcon = value; OnPropertyChanged(nameof(SelectedFranchiseIcon)); } }

        // Methods
        public void LoadIcons(FighterLoadedMessage message)
        {
            FranchiseIcons = _cosmeticService.GetFranchiseIcons();
            SelectedFranchiseIcon = FranchiseIcons.Items.FirstOrDefault(x => x.Id == message.Value.FighterInfo.Ids.FranchiseId);
        }

        public void SelectModel()
        {
            var model = _dialogService.OpenFileDialog("Select a model", "MDL0 files (.mdl0)|*.mdl0");
            // Update the mode
            if (!string.IsNullOrEmpty(model))
            {
                SelectedFranchiseIcon.ModelPath = model;
                FranchiseIcons.ItemChanged(SelectedFranchiseIcon);
                SelectedFranchiseIcon.Model = null;
                SelectedFranchiseIcon.ColorSequence = null;
                OnPropertyChanged(nameof(SelectedFranchiseIcon));
            }
        }

        public void ClearModel()
        {
            SelectedFranchiseIcon.ModelPath = "";
            SelectedFranchiseIcon.Model = null;
            SelectedFranchiseIcon.ColorSequence = null;
            FranchiseIcons.ItemChanged(SelectedFranchiseIcon);
            OnPropertyChanged(nameof(SelectedFranchiseIcon));
        }

        // TODO: maybe we add cosmetics to the fighter package on change here instead of on install?
        public void ReplaceIcon()
        {
            var image = _dialogService.OpenFileDialog("Select an image", "PNG images (.png)|*.png");
            // Update the image
            if (!string.IsNullOrEmpty(image))
            {
                var bitmap = new Bitmap(image);
                SelectedFranchiseIcon.Image = bitmap.ToBitmapImage();
                SelectedFranchiseIcon.ImagePath = image;
                SelectedFranchiseIcon.Texture = null;
                SelectedFranchiseIcon.Palette = null;
                FranchiseIcons.ItemChanged(SelectedFranchiseIcon);
                OnPropertyChanged(nameof(SelectedFranchiseIcon));
            }
        }

        public void ReplaceHDIcon()
        {
            var image = _dialogService.OpenFileDialog("Select an image", "PNG images (.png)|*.png");
            // Update the image
            if (!string.IsNullOrEmpty(image))
            {
                var bitmap = new Bitmap(image);
                SelectedFranchiseIcon.HDImage = bitmap.ToBitmapImage();
                SelectedFranchiseIcon.HDImagePath = image;
                FranchiseIcons.ItemChanged(SelectedFranchiseIcon);
                OnPropertyChanged(nameof(SelectedFranchiseIcon));
            }
        }

        public void ClearHDIcon()
        {
            SelectedFranchiseIcon.HDImage = null;
            SelectedFranchiseIcon.HDImagePath = "";
            FranchiseIcons.ItemChanged(SelectedFranchiseIcon);
            OnPropertyChanged(nameof(SelectedFranchiseIcon));
        }
    }
}
