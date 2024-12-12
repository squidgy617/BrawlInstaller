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
    public interface INodeSelectorViewModel
    {
        string Caption { get; set; }
        MessageBoxButton MessageBoxButton { get; set; }
        BitmapImage Image { get; set; }
        MessageBoxImage MessageIcon { get; set; }
        bool DialogResult { get; set; }
        IEnumerable<ResourceNode> ListItems { get; set; }
        ResourceNode SelectedItem { get; set; }
        List<Type> AllowedTypes { get; set; }
        event EventHandler OnRequestClose;
    }

    [Export(typeof(INodeSelectorViewModel))]
    internal class NodeSelectorViewModel : ViewModelBase, INodeSelectorViewModel
    {
        // Private properties
        private string _caption;
        private MessageBoxButton _messageBoxButton;
        private BitmapImage _image;
        private MessageBoxImage _messageIcon;
        private bool _dialogResult;
        private IEnumerable<ResourceNode> _listItems;
        private ResourceNode _selectedItem;
        private List<Type> _allowedTypes;

        // Commands
        public ICommand ConfirmCommand => new RelayCommand(param => Confirm());
        public ICommand CancelCommand => new RelayCommand(param => Cancel());
        public ICommand SelectedItemChangedCommand => new RelayCommand(param => SelectedItemChanged(param));

        // Events
        public event EventHandler OnRequestClose;

        // Importing constructor
        [ImportingConstructor]
        public NodeSelectorViewModel()
        {

        }

        // Properties
        public string Caption { get => _caption; set { _caption = value; OnPropertyChanged(nameof(Caption)); } }

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

        public IEnumerable<ResourceNode> ListItems { get => _listItems; set { _listItems = value; OnPropertyChanged(nameof(ListItems)); } }

        [DependsUpon(nameof(ListItems))]
        public ResourceNode SelectedItem { get => _selectedItem; set { _selectedItem = value; OnPropertyChanged(nameof(SelectedItem)); } }

        public List<Type> AllowedTypes { get => _allowedTypes; set { _allowedTypes = value; OnPropertyChanged(nameof(AllowedTypes)); } }

        [DependsUpon(nameof(SelectedItem))]
        public bool ButtonEnabled { get => SelectedItem != null && (AllowedTypes == null || AllowedTypes.Contains(SelectedItem.GetType())); }

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

        public void SelectedItemChanged(object param)
        {
            SelectedItem = (ResourceNode)param;
            OnPropertyChanged(nameof(SelectedItem));
        }
    }
}
