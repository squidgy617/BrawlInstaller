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

        // Importing constructor
        [ImportingConstructor]
        public CheckListViewModel()
        {

        }

        // Properties
        public IEnumerable<CheckListItem> CheckListItems { get => _checkListItems; set { _checkListItems = value; OnPropertyChanged(nameof(CheckListItems)); } }
    }
}
