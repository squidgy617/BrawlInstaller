using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel.Composition;
using BrawlInstaller.Common;
using System.Drawing;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Interop;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlInstaller.ViewModels
{
    public interface INodeSelectorViewModel : IDialogViewModelBase
    {
        IEnumerable<ResourceNode> ListItems { get; set; }
        ResourceNode SelectedItem { get; set; }
        List<Type> AllowedTypes { get; set; }
    }

    [Export(typeof(INodeSelectorViewModel))]
    internal class NodeSelectorViewModel : DialogViewModelBase, INodeSelectorViewModel
    {
        // Private properties
        private IEnumerable<ResourceNode> _listItems;
        private ResourceNode _selectedItem;
        private List<Type> _allowedTypes;

        // Commands
        public ICommand SelectedItemChangedCommand => new RelayCommand(param => SelectedItemChanged(param));

        // Importing constructor
        [ImportingConstructor]
        public NodeSelectorViewModel()
        {

        }

        // Properties
        public IEnumerable<ResourceNode> ListItems { get => _listItems; set { _listItems = value; OnPropertyChanged(nameof(ListItems)); } }

        [DependsUpon(nameof(ListItems))]
        public ResourceNode SelectedItem { get => _selectedItem; set { _selectedItem = value; OnPropertyChanged(nameof(SelectedItem)); } }

        public List<Type> AllowedTypes { get => _allowedTypes; set { _allowedTypes = value; OnPropertyChanged(nameof(AllowedTypes)); } }

        [DependsUpon(nameof(SelectedItem))]
        public bool ButtonEnabled { get => SelectedItem != null && (AllowedTypes == null || AllowedTypes.Contains(SelectedItem.GetType())); }

        public void SelectedItemChanged(object param)
        {
            SelectedItem = (ResourceNode)param;
            OnPropertyChanged(nameof(SelectedItem));
        }
    }
}
