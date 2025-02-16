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
    public interface IRadioButtonViewModel : IDialogViewModelBase
    {
        IEnumerable<RadioButtonGroup> RadioButtonGroups { get; set; }
        IEnumerable<RadioButtonItem> RadioButtonItems { get; }
    }

    [Export(typeof(IRadioButtonViewModel))]
    internal class RadioButtonViewModel : DialogViewModelBase, IRadioButtonViewModel
    {
        // Private properties
        private IEnumerable<RadioButtonGroup> _radioButtonGroups;
        private RadioButtonItem _hoveredItem;

        // Commands
        public ICommand ChangeHoveredItemCommand => new RelayCommand(param => ChangeHovered((RadioButtonItem)param));
        public ICommand ClearHoveredItemCommand => new RelayCommand(param => ClearHovered());

        // Importing constructor
        [ImportingConstructor]
        public RadioButtonViewModel()
        {

        }

        // Properties
        public IEnumerable<RadioButtonGroup> RadioButtonGroups { get => _radioButtonGroups; set { _radioButtonGroups = value; OnPropertyChanged(nameof(RadioButtonGroups)); } }
        public IEnumerable<RadioButtonItem> RadioButtonItems { get => RadioButtonGroups.SelectMany(x => x.Items); }
        public RadioButtonItem HoveredItem { get => _hoveredItem; set { _hoveredItem = value; OnPropertyChanged(nameof(HoveredItem)); } }

        // Methods
        private void ChangeHovered(RadioButtonItem checkListItem)
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
