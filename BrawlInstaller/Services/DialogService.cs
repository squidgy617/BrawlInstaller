using BrawlInstaller.Dialogs;
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
        /// <inheritdoc cref="DialogService.ShowMessage(string, string, MessageBoxImage)"/>
        bool ShowMessage(string text, string caption, MessageBoxImage image=MessageBoxImage.Information);

        /// <inheritdoc cref="DialogService.ShowMessage(string, string, MessageBoxButton, MessageBoxImage)"/>
        bool ShowMessage(string text, string caption, MessageBoxButton buttonType, MessageBoxImage image=MessageBoxImage.Information);

        /// <inheritdoc cref="DialogService.OpenFileDialog(string, string)"/>
        string OpenFileDialog(string title, string filter);

        /// <inheritdoc cref="DialogService.SaveFileDialog(string, string)"/>
        string SaveFileDialog(string title = "Save file", string filter = "");

        /// <inheritdoc cref="DialogService.OpenMultiFileDialog(string, string)"/>
        List<string> OpenMultiFileDialog(string title, string filter);

        /// <inheritdoc cref="DialogService.OpenStringInputDialog(string, string)"/>
        string OpenStringInputDialog(string title, string caption);
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
            return ShowMessage(text, caption, MessageBoxButton.OK, image);
        }

        /// <summary>
        /// Show configurable message dialog
        /// </summary>
        /// <param name="text">Text to display in message</param>
        /// <param name="caption">Caption to display in title bar</param>
        /// <param name="buttonType">Type of buttons to use in dialog</param>
        /// <param name="image">Image to show on dialog</param>
        /// <returns>Whether user gave a positive response to message</returns>
        public bool ShowMessage(string text, string caption, MessageBoxButton buttonType, MessageBoxImage image=MessageBoxImage.Information)
        {
            var result = MessageBox.Show(text, caption, buttonType, image);
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
        /// Open save file dialog
        /// </summary>
        /// <param name="title">Title to display on dialog</param>
        /// <param name="filter">File extension filter</param>
        /// <returns>Path to file</returns>
        public string SaveFileDialog(string title="Save file", string filter = "")
        {
            var dialog = new SaveFileDialog();
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

        /// <summary>
        /// Open dialog that accepts string input
        /// </summary>
        /// <returns>User string input</returns>
        public string OpenStringInputDialog(string title = "String Input", string caption = "Enter a string")
        {
            var dialog = new StringInputWindow();
            dialog.Title = title;
            dialog.Caption = caption;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                return dialog.ResponseText;
            }
            return null;
        }
    }
}
