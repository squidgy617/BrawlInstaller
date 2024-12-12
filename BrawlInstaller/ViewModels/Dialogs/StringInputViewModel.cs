using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel.Composition;
using BrawlInstaller.Common;
using System.Windows.Input;
using System.Drawing;
using System.Reflection;
using System.Windows.Interop;

namespace BrawlInstaller.ViewModels
{
    public interface IStringInputViewModel : IDialogViewModelBase
    {
        string StringInput { get; set; }
    }

    [Export(typeof(IStringInputViewModel))]
    internal class StringInputViewModel : DialogViewModelBase, IStringInputViewModel
    {
        // Private properties
        private string _caption;
        private MessageBoxButton _messageBoxButton;
        private BitmapImage _image;
        private MessageBoxImage _messageIcon;
        private bool _dialogResult;
        private string _stringInput;

        // Importing constructor
        [ImportingConstructor]
        public StringInputViewModel()
        {

        }

        // Properties
        public string StringInput { get => _stringInput; set { _stringInput = value; OnPropertyChanged(nameof(StringInput)); } }
    }
}
