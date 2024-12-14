using BrawlInstaller.ViewModels;
using BrawlLib.SSBB.ResourceNodes;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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

        /// <inheritdoc cref="DialogService.OpenDropDownDialog(IEnumerable{object}, string, string, string)"/>
        object OpenDropDownDialog(IEnumerable<object> list, string displayMemberPath, string title = "Select an item", string caption = "Select an item");

        /// <inheritdoc cref="DialogService.OpenNodeSelectorDialog(string, string, string, List{ResourceType})"/>
        string OpenNodeSelectorDialog(string filePath, string title = "Select an item", string caption = "Select an item", List<Type> allowedNodeTypes = null);
    }
    [Export(typeof(IDialogService))]
    internal class DialogService : IDialogService
    {
        // ViewModels
        IMessageViewModel _messageViewModel { get; }
        IStringInputViewModel _stringInputViewModel { get; }
        IDropDownViewModel _dropDownViewModel { get; }
        INodeSelectorViewModel _nodeSelectorViewModel { get; }

        // Services
        IFileService _fileService { get; }

        [ImportingConstructor]
        public DialogService(IFileService fileService, IMessageViewModel messageViewModel, IStringInputViewModel stringInputViewModel, IDropDownViewModel dropDownViewModel, INodeSelectorViewModel nodeSelectorViewModel) 
        {
            _fileService = fileService;
            _messageViewModel = messageViewModel;
            _stringInputViewModel = stringInputViewModel;
            _dropDownViewModel = dropDownViewModel;
            _nodeSelectorViewModel = nodeSelectorViewModel;
        }

        // Methods

        /// <summary>
        /// Generate a window with basic parameters
        /// </summary>
        /// <param name="title">Title for window</param>
        /// <returns>New window</returns>
        private Window GenerateWindow(string title = "Title")
        {
            var dialog = new Window { Width = 300, Height = 172, Title = title, SizeToContent = SizeToContent.WidthAndHeight, ResizeMode = ResizeMode.NoResize };
            return dialog;
        }

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
            var dialog = GenerateWindow(caption);
            _messageViewModel.Caption = text;
            _messageViewModel.MessageBoxButton = buttonType;
            _messageViewModel.MessageIcon = image;
            _messageViewModel.Image = bitmapImage;
            _messageViewModel.OnRequestClose += (s, e) => dialog.Close();
            dialog.Content = _messageViewModel;
            dialog.ShowDialog();
            return _messageViewModel.DialogResult;
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
            var dialog = GenerateWindow(title);
            _stringInputViewModel.Caption = caption;
            _stringInputViewModel.MessageBoxButton = MessageBoxButton.OKCancel;
            _stringInputViewModel.MessageIcon = MessageBoxImage.None;
            _stringInputViewModel.Image = null;
            _stringInputViewModel.StringInput = string.Empty;
            _stringInputViewModel.OnRequestClose += (s, e) => dialog.Close();
            dialog.Content = _stringInputViewModel;
            dialog.ShowDialog();
            if (_stringInputViewModel.DialogResult)
            {
                return _stringInputViewModel.StringInput;
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
        public object OpenDropDownDialog(IEnumerable<object> list, string displayMemberPath, string title = "Select an item", string caption = "Select an item")
        {
            var dialog = GenerateWindow(title);
            _dropDownViewModel.Caption = caption;
            _dropDownViewModel.MessageBoxButton = MessageBoxButton.OKCancel;
            _dropDownViewModel.MessageIcon = MessageBoxImage.None;
            _dropDownViewModel.Image = null;
            _dropDownViewModel.ListItems = list;
            _dropDownViewModel.DisplayMemberPath = displayMemberPath;
            _dropDownViewModel.OnRequestClose += (s, e) => dialog.Close();
            dialog.Content = _dropDownViewModel;
            dialog.ShowDialog();
            if (_dropDownViewModel.DialogResult)
            {
                return _dropDownViewModel.SelectedItem;
            }
            return null;
        }

        /// <summary>
        /// Open node selector dialog
        /// </summary>
        /// <param name="filePath">File to open node selector for</param>
        /// <param name="title">Title to display</param>
        /// <param name="caption">Caption to display</param>
        /// <param name="allowedNodeTypes">Node types allowed to be selected</param>
        /// <returns></returns>
        public string OpenNodeSelectorDialog(string filePath, string title = "Select an item", string caption = "Select an item", List<Type> allowedNodeTypes = null)
        {
            var rootNode = _fileService.OpenFile(filePath);
            if (rootNode != null)
            {
                var nodeList = new List<ResourceNode> { rootNode };
                var selectedNode = OpenNodeSelectorDialog(nodeList, title, caption, allowedNodeTypes);
                var nodePath = string.Empty;
                if (selectedNode != null)
                {
                    nodePath = selectedNode.TreePath;
                }
                _fileService.CloseFile(rootNode);
                return nodePath;
            }
            return string.Empty;
        }

        /// <summary>
        /// Open node selector dialog
        /// </summary>
        /// <param name="nodeList">List of nodes to display</param>
        /// <param name="title">Title</param>
        /// <param name="caption">Caption</param>
        /// <returns>Selected node</returns>
        private ResourceNode OpenNodeSelectorDialog(List<ResourceNode> nodeList, string title = "Select an item", string caption = "Select an item", List<Type> allowedNodeTypes = null)
        {
            var dialog = GenerateWindow(title);
            _nodeSelectorViewModel.Caption = caption;
            _nodeSelectorViewModel.MessageBoxButton = MessageBoxButton.OKCancel;
            _nodeSelectorViewModel.MessageIcon = MessageBoxImage.None;
            _nodeSelectorViewModel.Image = null;
            _nodeSelectorViewModel.ListItems = nodeList;
            _nodeSelectorViewModel.AllowedTypes = allowedNodeTypes;
            _nodeSelectorViewModel.OnRequestClose += (s, e) => dialog.Close();
            dialog.Content = _nodeSelectorViewModel;
            dialog.ShowDialog();
            if (_nodeSelectorViewModel.DialogResult)
            {
                return _nodeSelectorViewModel.SelectedItem;
            }
            return null;
        }
    }
}
