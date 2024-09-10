using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BrawlInstaller.UserControls
{
    /// <summary>
    /// Interaction logic for FileBox.xaml
    /// </summary>
    public partial class FileBox : UserControl
    {
        public FileBox()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FileBox), new FrameworkPropertyMetadata
        {
            DefaultValue = string.Empty,
            PropertyChangedCallback = OnTextPropertyChanged,
            BindsTwoWayByDefault = true
        });
        private static void OnTextPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as FileBox;

            control.SetCurrentValue(TextProperty, e.NewValue);
        }
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetCurrentValue(TextProperty, value);
            }
        }
        public string Filter { get; set; }
        public string Title { get; set; } = "Select a file";
        public int TextBoxWidth { get; set; } = 60;

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Text = string.Empty;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = Title;
            dialog.Filter = Filter;

            var result = dialog.ShowDialog();
            if (result == true)
            {
                Text = dialog.FileName;
            }
        }
    }
}
