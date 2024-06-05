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
        CosmeticList FranchiseIcons { get; }
        Cosmetic SelectedFranchiseIcon { get; }
        ICommand SelectModelCommand { get; }
    }

    [Export(typeof(IFranchiseIconViewModel))]
    internal class FranchiseIconViewModel : ViewModelBase, IFranchiseIconViewModel
    {
        // Commands
        public ICommand SelectModelCommand
        {
            get
            {
                return new RelayCommand(param => SelectModel());
            }
        }

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
            SelectedFranchiseIcon = FranchiseIcons.Cosmetics.FirstOrDefault(x => x.Id == message.Value.FighterInfo.Ids.FranchiseId);
        }

        public void SelectModel()
        {
            var model = _dialogService.OpenFileDialog("Select a model", "MDL0 files (.mdl0)|*.mdl0");
            // Update the image
            if (model != "")
            {
                SelectedFranchiseIcon.ModelPath = model;
                FranchiseIcons.CosmeticChanged(SelectedFranchiseIcon);
                SelectedFranchiseIcon.Model = null;
                SelectedFranchiseIcon.ColorSequence = null;
                OnPropertyChanged(nameof(SelectedFranchiseIcon));
            }
        }
    }
}
