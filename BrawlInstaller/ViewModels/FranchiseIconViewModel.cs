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
        List<FranchiseCosmetic> FranchiseIcons { get; }
        FranchiseCosmetic SelectedFranchiseIcon { get; }
    }

    [Export(typeof(IFranchiseIconViewModel))]
    internal class FranchiseIconViewModel : ViewModelBase, IFranchiseIconViewModel
    {
        // Private Properties
        private List<FranchiseCosmetic> _franchiseIcons;
        private FranchiseCosmetic _selectedFranchiseIcon;

        // Services
        ICosmeticService _cosmeticService;

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public FranchiseIconViewModel(ICosmeticService cosmeticService)
        {
            _cosmeticService = cosmeticService;
            FranchiseIcons = new List<FranchiseCosmetic>();

            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadIcons(message);
            });
        }

        // Properties
        public List<FranchiseCosmetic> FranchiseIcons { get => _franchiseIcons; set { _franchiseIcons = value; OnPropertyChanged(); } }
        public FranchiseCosmetic SelectedFranchiseIcon { get => _selectedFranchiseIcon; set { _selectedFranchiseIcon = value; OnPropertyChanged(); } }

        // Methods
        public void LoadIcons(FighterLoadedMessage message)
        {
            FranchiseIcons = _cosmeticService.GetFranchiseIcons();
            SelectedFranchiseIcon = FranchiseIcons.FirstOrDefault(x => x.Id == message.Value.FighterInfo.Ids.FranchiseId);
        }
    }
}
