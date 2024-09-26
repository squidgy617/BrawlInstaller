﻿using BrawlInstaller.Common;
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
using System.Collections.ObjectModel;
using System.Windows;

namespace BrawlInstaller.ViewModels
{
    public interface IFranchiseIconViewModel
    {
        CosmeticList FranchiseIconList { get; }
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
        public ICommand RemoveIconCommand => new RelayCommand(param => RemoveIcon());

        // Private Properties
        private CosmeticList _franchiseIconList;
        private Cosmetic _selectedFranchiseIcon;

        // Services
        ICosmeticService _cosmeticService;
        IDialogService _dialogService;
        IFileService _fileService;

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public FranchiseIconViewModel(ICosmeticService cosmeticService, IDialogService dialogService, IFileService fileService)
        {
            _cosmeticService = cosmeticService;
            _dialogService = dialogService;
            _fileService = fileService;
            FranchiseIconList = new CosmeticList();

            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadIcons(message);
            });
        }

        // Properties
        public CosmeticList FranchiseIconList { get => _franchiseIconList; set { _franchiseIconList = value; OnPropertyChanged(nameof(FranchiseIcons)); } }

        [DependsUpon(nameof(FranchiseIconList))]
        public ObservableCollection<Cosmetic> FranchiseIcons { get => new ObservableCollection<Cosmetic>(FranchiseIconList.Items); }
        public Cosmetic SelectedFranchiseIcon { get => _selectedFranchiseIcon; set { _selectedFranchiseIcon = value; OnPropertyChanged(nameof(SelectedFranchiseIcon)); } }

        // Methods
        public void LoadIcons(FighterLoadedMessage message)
        {
            FranchiseIconList = _cosmeticService.GetFranchiseIcons();
            SelectedFranchiseIcon = FranchiseIcons.FirstOrDefault(x => x.Id == message.Value.FighterInfo.Ids.FranchiseId);
        }

        public void SelectModel()
        {
            var model = _dialogService.OpenFileDialog("Select a model", "MDL0 files (.mdl0)|*.mdl0");
            // Update the mode
            if (!string.IsNullOrEmpty(model))
            {
                SelectedFranchiseIcon.ModelPath = model;
                FranchiseIconList.ItemChanged(SelectedFranchiseIcon);
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
            FranchiseIconList.ItemChanged(SelectedFranchiseIcon);
            OnPropertyChanged(nameof(SelectedFranchiseIcon));
        }

        // TODO: maybe we add cosmetics to the fighter package on change here instead of on install?
        public void ReplaceIcon()
        {
            var image = _dialogService.OpenFileDialog("Select an image", "PNG images (.png)|*.png");
            // Update the image
            if (!string.IsNullOrEmpty(image))
            {
                var bitmap = _fileService.LoadImage(image);
                SelectedFranchiseIcon.Image = bitmap;
                SelectedFranchiseIcon.ImagePath = image;
                SelectedFranchiseIcon.Texture = null;
                SelectedFranchiseIcon.Palette = null;
                FranchiseIconList.ItemChanged(SelectedFranchiseIcon);
                OnPropertyChanged(nameof(SelectedFranchiseIcon));
            }
        }

        public void ReplaceHDIcon()
        {
            var image = _dialogService.OpenFileDialog("Select an image", "PNG images (.png)|*.png");
            // Update the image
            if (!string.IsNullOrEmpty(image))
            {
                var bitmap = _fileService.LoadImage(image);
                SelectedFranchiseIcon.HDImage = bitmap;
                SelectedFranchiseIcon.HDImagePath = image;
                FranchiseIconList.ItemChanged(SelectedFranchiseIcon);
                OnPropertyChanged(nameof(SelectedFranchiseIcon));
            }
        }

        public void ClearHDIcon()
        {
            SelectedFranchiseIcon.HDImage = null;
            SelectedFranchiseIcon.HDImagePath = "";
            FranchiseIconList.ItemChanged(SelectedFranchiseIcon);
            OnPropertyChanged(nameof(SelectedFranchiseIcon));
        }

        public void RemoveIcon()
        {
            var accepted = _dialogService.ShowMessage("Removing a franchise icon will remove it from ANY characters that use it. Are you sure?", "Warning", MessageBoxButton.YesNo);
            if (accepted)
            {
                Cosmetic newSelection;
                if (FranchiseIcons.IndexOf(SelectedFranchiseIcon) > 0)
                {
                    newSelection = FranchiseIcons[FranchiseIcons.IndexOf(SelectedFranchiseIcon) - 1];
                }
                else
                {
                    newSelection = FranchiseIcons.FirstOrDefault();
                }
                FranchiseIconList.Remove(SelectedFranchiseIcon);
                SelectedFranchiseIcon = newSelection;
                OnPropertyChanged(nameof(SelectedFranchiseIcon));
                OnPropertyChanged(nameof(FranchiseIconList));
            }
        }
    }
}
