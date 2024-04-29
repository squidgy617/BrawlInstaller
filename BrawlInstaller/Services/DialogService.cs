using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BrawlInstaller.Services
{
    public interface IDialogService
    {
        bool ShowMessage(string text, string caption, MessageBoxImage image);
        string OpenFileDialog(string title, string filter);
    }
    [Export(typeof(IDialogService))]
    internal class DialogService : IDialogService
    {
        [ImportingConstructor]
        public DialogService() { }

        // Methods
        public bool ShowMessage(string text, string caption, MessageBoxImage image=MessageBoxImage.Information)
        {
            var result = MessageBox.Show(text, caption, MessageBoxButton.OK, image);
            if (result == MessageBoxResult.Yes || result == MessageBoxResult.OK || result == MessageBoxResult.None)
                return true;
            else
                return false;
        }

        public string OpenFileDialog(string title="Select a file", string filter = "")
        {
            var dialog = new OpenFileDialog();
            dialog.Title = title;
            dialog.Filter = filter;

            var result = dialog.ShowDialog();
            if (result == true)
            {
                return dialog.FileName;
            }
            return "";
        }
    }
}
