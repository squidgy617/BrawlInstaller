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
using BrawlInstaller.Classes;

namespace BrawlInstaller.ViewModels
{
    public interface ICheckListViewModel : IDialogViewModelBase
    {
        IEnumerable<CheckListItem> CheckListItems { get; set; }
    }

    [Export(typeof(ICheckListViewModel))]
    internal class CheckListViewModel : DialogViewModelBase, ICheckListViewModel
    {
        // Private properties
        private IEnumerable<CheckListItem> _checkListItems;
        private CheckListItem _hoveredItem;

        // Commands
        public ICommand ChangeHoveredItemCommand => new RelayCommand(param => ChangeHovered((CheckListItem)param));
        public ICommand ClearHoveredItemCommand => new RelayCommand(param => ClearHovered());

        // Importing constructor
        [ImportingConstructor]
        public CheckListViewModel()
        {

        }

        // Properties
        public IEnumerable<CheckListItem> CheckListItems { get => _checkListItems; set { _checkListItems = value; OnPropertyChanged(nameof(CheckListItems)); } }
        public CheckListItem HoveredItem { get => _hoveredItem; set { _hoveredItem = value; OnPropertyChanged(nameof(HoveredItem)); } }

        // Methods
        private void ChangeHovered(CheckListItem checkListItem)
        {
            HoveredItem = checkListItem; 
            OnPropertyChanged(nameof(HoveredItem));
        }

        private void ClearHovered()
        {
            HoveredItem = null;
            OnPropertyChanged(nameof(HoveredItem));
        }
    }
}
