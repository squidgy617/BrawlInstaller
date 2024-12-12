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

namespace BrawlInstaller.ViewModels
{
    public interface IDropDownViewModel : IDialogViewModelBase
    {
        IEnumerable<object> ListItems { get; set; }
        object SelectedItem { get; set; }
        string DisplayMemberPath { get; set; }
    }

    [Export(typeof(IDropDownViewModel))]
    internal class DropDownViewModel : DialogViewModelBase, IDropDownViewModel
    {
        // Private properties
        private IEnumerable<object> _listItems;
        private object _selectedItem;
        private string _displayMemberPath;

        // Importing constructor
        [ImportingConstructor]
        public DropDownViewModel()
        {

        }

        // Properties
        public IEnumerable<object> ListItems { get => _listItems; set { _listItems = value; OnPropertyChanged(nameof(ListItems)); } }

        [DependsUpon(nameof(ListItems))]
        public object SelectedItem { get => _selectedItem; set { _selectedItem = value; OnPropertyChanged(nameof(SelectedItem)); } }

        public string DisplayMemberPath { get => _displayMemberPath; set { _displayMemberPath = value; OnPropertyChanged(nameof(DisplayMemberPath)); } }
    }
}
