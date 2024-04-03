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

namespace BrawlInstaller.ViewModels
{
    public interface IFranchiseIconViewModel
    {
        List<FranchiseCosmetic> FranchiseIcons { get; set; }
        FranchiseCosmetic SelectedFranchiseIcon { get; set; }
    }

    [Export(typeof(IFranchiseIconViewModel))]
    internal class FranchiseIconViewModel : ViewModelBase, IFranchiseIconViewModel
    {
        private List<FranchiseCosmetic> _franchiseIcons;
        private FranchiseCosmetic _selectedFranchiseIcon;

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public FranchiseIconViewModel()
        {
            FranchiseIcons = new List<FranchiseCosmetic>();
        }

        // Properties
        public List<FranchiseCosmetic> FranchiseIcons { get => _franchiseIcons; set { _franchiseIcons = value; OnPropertyChanged(); } }
        public FranchiseCosmetic SelectedFranchiseIcon { get => _selectedFranchiseIcon; set { _selectedFranchiseIcon = value; OnPropertyChanged(); } }

        // Methods
    }
}
