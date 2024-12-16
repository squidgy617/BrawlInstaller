using BrawlCrate.UI;
using BrawlInstaller.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace BrawlInstaller.ViewModels
{
    public interface IDialogViewModelBase
    {
        string Caption { get; set; }
        MessageBoxButton MessageBoxButton { get; set; }
        BitmapImage Image { get; set; }
        MessageBoxImage MessageIcon { get; set; }
        bool DialogResult { get; set; }
        event EventHandler OnRequestClose;
    }

    [Export(typeof(IDialogViewModelBase))]
    internal class DialogViewModelBase : ViewModelBase, IDialogViewModelBase
    {
        // Private properties
        private string _caption;
        private MessageBoxButton _messageBoxButton;
        private BitmapImage _image;
        private MessageBoxImage _messageIcon;
        private bool _dialogResult;

        // Commands
        public ICommand ConfirmCommand => new RelayCommand(param => Confirm());
        public ICommand CancelCommand => new RelayCommand(param => Cancel());

        // Events
        public event EventHandler OnRequestClose;

        // Importing constructor
        [ImportingConstructor]
        public DialogViewModelBase()
        {

        }

        // Properties
        public string Caption { get => _caption; set { _caption = value.Replace("\n", "\n\n"); OnPropertyChanged(nameof(Caption)); } }

        public MessageBoxButton MessageBoxButton { get => _messageBoxButton; set { _messageBoxButton = value; OnPropertyChanged(nameof(MessageBoxButton)); } }

        [DependsUpon(nameof(MessageBoxButton))]
        public string OkButtonCaption { get => MessageBoxButton == MessageBoxButton.OK || MessageBoxButton == MessageBoxButton.OKCancel ? "OK" : "Yes"; }

        [DependsUpon(nameof(MessageBoxButton))]
        public string CancelButtonCaption { get => MessageBoxButton == MessageBoxButton.OKCancel ? "Cancel" : "No"; }

        [DependsUpon(nameof(MessageBoxButton))]
        public Visibility CancelButtonVisibility { get => MessageBoxButton != MessageBoxButton.OK ? Visibility.Visible : Visibility.Collapsed; }

        public BitmapImage Image { get => _image; set { _image = value; OnPropertyChanged(nameof(Image)); } }

        [DependsUpon(nameof(Image))]
        public Visibility ImageVisbility { get => Image != null ? Visibility.Visible : Visibility.Collapsed; }

        public MessageBoxImage MessageIcon { get => _messageIcon; set { _messageIcon = value; OnPropertyChanged(nameof(MessageIcon)); } }

        [DependsUpon(nameof(MessageIcon))]
        public BitmapSource MessageIconSource
        {
            get
            {
                if (MessageIcon != MessageBoxImage.None)
                {
                    var windowIcon = (Icon)typeof(SystemIcons).GetProperty(MessageIcon.ToString(), BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
                    BitmapSource bs = Imaging.CreateBitmapSourceFromHIcon(windowIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    return bs;
                }
                else
                {
                    return null;
                }
            }
        }

        [DependsUpon(nameof(MessageIcon))]
        public Visibility MessageIconVisibility { get => MessageIcon == MessageBoxImage.None ? Visibility.Collapsed : Visibility.Visible; }

        public bool DialogResult { get => _dialogResult; set { _dialogResult = value; OnPropertyChanged(nameof(DialogResult)); } }

        public void Confirm()
        {
            DialogResult = true;
            OnRequestClose(this, new EventArgs());
        }

        public void Cancel()
        {
            DialogResult = false;
            OnRequestClose(this, new EventArgs());
        }
    }
}
