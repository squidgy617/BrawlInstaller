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

namespace BrawlInstaller.ViewModels
{
    public interface IFranchiseIconViewModel
    {
        TrackedList<Cosmetic> FranchiseIcons { get; }
        Cosmetic SelectedFranchiseIcon { get; }
        ICommand SelectModelCommand { get; }
    }

    [Export(typeof(IFranchiseIconViewModel))]
    internal class FranchiseIconViewModel : ViewModelBase, IFranchiseIconViewModel
    {
        // Commands
        public ICommand SelectModelCommand => new RelayCommand(param => SelectModel());
        public ICommand ClearModelCommand => new RelayCommand(param => ClearModel());

        // Private Properties
        private TrackedList<Cosmetic> _franchiseIcons;
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
            FranchiseIcons = new TrackedList<Cosmetic>();

            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadIcons(message);
            });
        }

        // Properties
        public TrackedList<Cosmetic> FranchiseIcons { get => _franchiseIcons; set { _franchiseIcons = value; OnPropertyChanged(nameof(FranchiseIcons)); } }
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
            // Update the image
            if (model != "")
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
    }
}
