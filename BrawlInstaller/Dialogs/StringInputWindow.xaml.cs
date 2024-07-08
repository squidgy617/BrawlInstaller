using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace BrawlInstaller.Dialogs
{
    /// <summary>
    /// Interaction logic for StringInputWindow.xaml
    /// </summary>
    public partial class StringInputWindow : Window
    {
        public StringInputWindow()
        {
            InitializeComponent();
        }

        public string ResponseText
        {
            get { return textBox.Text; }
            set { textBox.Text = value; }
        }

        public string Caption
        {
            get { return caption.Content.ToString(); }
            set { caption.Content = value; }
        }

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
