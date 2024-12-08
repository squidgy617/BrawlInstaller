using BrawlLib.Internal.Windows.Forms.Ookii.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace BrawlInstaller.UserControls
{
    /// <summary>
    /// Interaction logic for FileBox.xaml
    /// </summary>
    public partial class FileBox : System.Windows.Controls.UserControl
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

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(string), typeof(FileBox), new FrameworkPropertyMetadata
        {
            DefaultValue = string.Empty,
            PropertyChangedCallback = OnFilterPropertyChanged,
            BindsTwoWayByDefault = true
        });

        public static readonly DependencyProperty ExcludePathProperty = DependencyProperty.Register("ExcludePath", typeof(string), typeof(FileBox), new FrameworkPropertyMetadata
        {
            DefaultValue = string.Empty,
            PropertyChangedCallback = OnExcludePathPropertyChanged,
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

        private static void OnFilterPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as FileBox;

            control.SetCurrentValue(FilterProperty, e.NewValue);
        }

        public string Filter
        {
            get
            {
                return (string)GetValue(FilterProperty);
            }
            set
            {
                SetCurrentValue(FilterProperty, value);
            }
        }

        private static void OnExcludePathPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as FileBox;

            control.SetCurrentValue(ExcludePathProperty, e.NewValue);
        }

        public string ExcludePath
        {
            get
            {
                return (string)GetValue(ExcludePathProperty);
            }
            set
            {
                SetCurrentValue(ExcludePathProperty, value);
            }
        }

        public string Title { get; set; } = "Select a file";
        public int TextBoxWidth { get; set; } = 60;
        public bool IsReadOnly { get; set; } = true;

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Text = string.Empty;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Filter))
            {
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.Title = Title;
                dialog.Filter = Filter;

                var result = dialog.ShowDialog();
                if (result == true)
                {
                    Text = Path.GetFullPath(dialog.FileName);
                    if (!string.IsNullOrEmpty(ExcludePath))
                    {
                        Text = Text.Replace(Path.GetFullPath(ExcludePath), "");
                    }
                }
            }
            else
            {
                var dialog = new VistaFolderBrowserDialog();
                dialog.Description = Title;

                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    Text = Path.GetFullPath(dialog.SelectedPath);
                    if (!string.IsNullOrEmpty(ExcludePath))
                    {
                        Text = Text.Replace(Path.GetFullPath(ExcludePath), "");
                    }
                }
            }
        }
    }
}
