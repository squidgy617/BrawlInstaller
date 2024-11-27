using BrawlInstaller.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BrawlInstaller.Services
{
    public interface IDialogService
    {
        /// <inheritdoc cref="DialogService.ShowMessage(string, string, MessageBoxImage)"/>
        bool ShowMessage(string text, string caption, MessageBoxImage image=MessageBoxImage.Information);

        /// <inheritdoc cref="DialogService.ShowMessage(string, string, MessageBoxButton, MessageBoxImage, BitmapImage)"/>
        bool ShowMessage(string text, string caption, MessageBoxButton buttonType, MessageBoxImage image=MessageBoxImage.Information, BitmapImage bitmapImage=null);

        /// <inheritdoc cref="DialogService.OpenFileDialog(string, string)"/>
        string OpenFileDialog(string title, string filter);

        /// <inheritdoc cref="DialogService.SaveFileDialog(string, string)"/>
        string SaveFileDialog(string title = "Save file", string filter = "");

        /// <inheritdoc cref="DialogService.OpenMultiFileDialog(string, string)"/>
        List<string> OpenMultiFileDialog(string title, string filter);

        /// <inheritdoc cref="DialogService.OpenStringInputDialog(string, string)"/>
        string OpenStringInputDialog(string title, string caption);

        /// <inheritdoc cref="DialogService.OpenDropDownDialog<T>(object, string, string, string)"/>
        object OpenDropDownDialog<T>(IEnumerable<T> list, string displayMemberPath, string title = "Select an item", string caption = "Select an item");
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
        public bool ShowMessage(string text, string caption, MessageBoxButton buttonType, MessageBoxImage image=MessageBoxImage.Information, BitmapImage bitmapImage=null)
        {
            var dialog = new MessageWindow();
            dialog.Title = caption;
            dialog.Caption = text;
            dialog.MessageBoxButton = buttonType;
            dialog.MessageIcon = image;
            dialog.Image = bitmapImage;
            var result = dialog.ShowDialog();
            if (result == true)
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

        /// <summary>
        /// Open dialog that accepts a list of items
        /// </summary>
        /// <typeparam name="T">Type of items in list</typeparam>
        /// <param name="list">List of items</param>
        /// <param name="title">Title displayed in dialog</param>
        /// <param name="caption">Caption displayed in dialog</param>
        /// <returns></returns>
        public object OpenDropDownDialog<T>(IEnumerable<T> list, string displayMemberPath, string title = "Select an item", string caption = "Select an item")
        {
            var dialog = new DropDownWindow();
            dialog.Title = title;
            dialog.Message = caption;
            dialog.ListItems = list;
            dialog.DisplayMemberPath = displayMemberPath;
            dialog.SelectedItem = list.FirstOrDefault();
            var result = dialog.ShowDialog();
            if (result == true)
            {
                return dialog.SelectedItem;
            }
            return null;
        }
    }
}
