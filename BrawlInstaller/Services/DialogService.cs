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
        List<string> OpenMultiFileDialog(string title, string filter);
    }
    [Export(typeof(IDialogService))]
    internal class DialogService : IDialogService
    {
        [ImportingConstructor]
        public DialogService() { }

        // Methods

        /// <summary>
        /// Show generic message dialog
        /// </summary>
        /// <param name="text">Text to display in message</param>
        /// <param name="caption">Caption to display in title bar</param>
        /// <param name="image">Image to show on dialog</param>
        /// <returns>Whether user gave a positive response to message</returns>
        public bool ShowMessage(string text, string caption, MessageBoxImage image=MessageBoxImage.Information)
        {
            var result = MessageBox.Show(text, caption, MessageBoxButton.OK, image);
            if (result == MessageBoxResult.Yes || result == MessageBoxResult.OK || result == MessageBoxResult.None)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Open file dialog
        /// </summary>
        /// <param name="title">Title to display on dialog</param>
        /// <param name="filter">File extension filter</param>
        /// <returns>File chosen by user</returns>
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

        /// <summary>
        /// Open multi-file dialog
        /// </summary>
        /// <param name="title">Title to display on dialog</param>
        /// <param name="filter">File extension filter</param>
        /// <returns>Files chosen by user</returns>
        public List<string> OpenMultiFileDialog(string title = "Select a file", string filter = "")
        {
            var dialog = new OpenFileDialog();
            dialog.Title = title;
            dialog.Filter = filter;
            dialog.Multiselect = true;

            var result = dialog.ShowDialog();
            if (result == true)
            {
                return dialog.FileNames.ToList();
            }
            return new List<string>();
        }
    }
}
