using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Helpers;
using BrawlInstaller.Services;
using BrawlLib.SSBB;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrawlInstaller.ViewModels
{
    public interface IFilesViewModel
    {

    }

    [Export(typeof(IFilesViewModel))]
    internal class FilesViewModel : ViewModelBase, IFilesViewModel
    {
        // Private properties
        private string _leftFilePath;
        private string _rightFilePath;
        private List<NodeDef> _nodeList;
        private NodeDef _selectedNode;

        // Services
        IPatchService _patchService;
        IDialogService _dialogService;

        // Commands
        public ICommand CompareFilesCommand => new RelayCommand(param => CompareFiles());

        [ImportingConstructor]
        public FilesViewModel(IPatchService patchService, IDialogService dialogService)
        {
            _patchService = patchService;
            _dialogService = dialogService;
        }

        // Properties
        public string LeftFilePath { get => _leftFilePath; set { _leftFilePath = value; OnPropertyChanged(LeftFilePath); } }
        public string RightFilePath { get => _rightFilePath; set { _rightFilePath = value;OnPropertyChanged(RightFilePath); } }
        public string FileFilter { get => SupportedFilesHandler.GetAllSupportedFilter(true); }
        public List<NodeDef> NodeList { get => _nodeList; set { _nodeList = value; OnPropertyChanged(nameof(NodeList)); } }
        public NodeDef SelectedNode { get => _selectedNode; set { _selectedNode = value; OnPropertyChanged(nameof(SelectedNode)); } }
        public ICommand SelectedItemChangedCommand => new RelayCommand(param => SelectedItemChanged(param));

        // Methods
        public void CompareFiles()
        {
            _dialogService.ShowProgressBar("Comparing", "Comparing files...");
            using (new CursorWait())
            {
                NodeList = _patchService.CompareFiles(LeftFilePath, RightFilePath);
            }
            _dialogService.CloseProgressBar();
            OnPropertyChanged(nameof(NodeList));
        }

        public void SelectedItemChanged(object param)
        {
            SelectedNode = (NodeDef)param;
            OnPropertyChanged(nameof(SelectedNode));
        }
    }
}
