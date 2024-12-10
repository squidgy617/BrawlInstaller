using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BrawlInstaller.Dialogs
{
    /// <summary>
    /// Interaction logic for NodeSelectorWindow.xaml
    /// </summary>
    public partial class NodeSelectorWindow : Window
    {
        public NodeSelectorWindow()
        {
            InitializeComponent();
        }

        public string Message
        {
            get { return message.Text.ToString(); }
            set { message.Text = value.Replace("\n", "\n\n"); }
        }

        public MessageBoxButton MessageBoxButton
        {
            set
            {
                if (value == MessageBoxButton.YesNo || value == MessageBoxButton.YesNoCancel)
                {
                    button.Content = "Yes";
                    cancelButton.Content = "No";
                }
                else
                {
                    button.Content = "OK";
                    cancelButton.Content = "Cancel";
                }
                if (value == MessageBoxButton.OK)
                {
                    cancelButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        public BitmapImage Image
        {
            set
            {
                image.Source = value;
                if (value != null)
                    image.Visibility = Visibility.Visible;
                else
                    image.Visibility = Visibility.Collapsed;
            }
        }

        public MessageBoxImage MessageIcon
        {
            set
            {
                var windowIcon = (Icon)typeof(SystemIcons).GetProperty(value.ToString(), BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
                BitmapSource bs = Imaging.CreateBitmapSourceFromHIcon(windowIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                icon.Source = bs;
                if (value != MessageBoxImage.None)
                    icon.Visibility = Visibility.Visible;
                else
                    icon.Visibility = Visibility.Collapsed;
            }
        }

        public IEnumerable ListItems
        {
            get { return treeView.Items; }
            set { treeView.ItemsSource = value; }
        }

        public object SelectedItem { get => treeView.SelectedItem; }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
