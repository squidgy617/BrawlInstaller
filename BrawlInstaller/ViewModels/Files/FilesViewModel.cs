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
        private ResourceNode _leftFileNode;
        private ResourceNode _rightFileNode;
        private FilePatch _filePatch;
        private NodeDef _selectedNode;

        // Services
        IPatchService _patchService;
        IDialogService _dialogService;
        IFileService _fileService;

        // Commands
        public ICommand CompareFilesCommand => new RelayCommand(param => CompareFiles());
        public ICommand ExportFilePatchCommand => new RelayCommand(param => ExportFilePatch());
        public ICommand OpenFilePatchCommand => new RelayCommand(param => OpenFilePatch());

        [ImportingConstructor]
        public FilesViewModel(IPatchService patchService, IDialogService dialogService, IFileService fileService)
        {
            _patchService = patchService;
            _dialogService = dialogService;
            _fileService = fileService;
        }

        // Properties
        public string LeftFilePath { get => _leftFilePath; set { _leftFilePath = value; OnPropertyChanged(nameof(LeftFilePath)); } }
        public string RightFilePath { get => _rightFilePath; set { _rightFilePath = value;OnPropertyChanged(nameof(RightFilePath)); } }
        public ResourceNode LeftFileNode { get => _leftFileNode; set { _leftFileNode = value; OnPropertyChanged(nameof(LeftFileNode)); } }
        public ResourceNode RightFileNode { get => _rightFileNode; set { _rightFileNode = value; OnPropertyChanged(nameof(RightFileNode)); } }
        public string FileFilter { get => SupportedFilesHandler.GetAllSupportedFilter(true); }
        public FilePatch FilePatch { get => _filePatch; set { _filePatch = value; OnPropertyChanged(nameof(FilePatch)); } }

        [DependsUpon(nameof(FilePatch))]
        public List<NodeDef> NodeList { get => FilePatch?.NodeDefs ?? new List<NodeDef>(); } // TODO: Should this default to a list like this?
        public NodeDef SelectedNode { get => _selectedNode; set { _selectedNode = value; OnPropertyChanged(nameof(SelectedNode)); } }
        public ICommand SelectedItemChangedCommand => new RelayCommand(param => SelectedItemChanged(param));

        // Methods
        public void CompareFiles()
        {
            _dialogService.ShowProgressBar("Comparing", "Comparing files...");
            using (new CursorWait())
            {
                var rightFileNode = _fileService.OpenFile(RightFilePath);
                var leftFileNode = _fileService.OpenFile(LeftFilePath);
                if (rightFileNode != null && leftFileNode != null)
                {
                    Parallel.ForEach(NodeList.FlattenList().AsParallel(), node =>
                    {
                        _fileService.CloseFile(node.Node);
                    });
                    _fileService.CloseFile(RightFileNode);
                    _fileService.CloseFile(LeftFileNode);
                    RightFileNode = rightFileNode;
                    LeftFileNode = leftFileNode;
                    FilePatch = _patchService.CompareFiles(LeftFileNode, RightFileNode);
                }
            }
            _dialogService.CloseProgressBar();
            OnPropertyChanged(nameof(FilePatch));
        }

        // TODO: Possibly update context here so the editor can do a "Save" instead of just a "Save As" afterward
        public void ExportFilePatch()
        {
            var file = _dialogService.SaveFileDialog("Save file patch", "File patch file (.fpatch)|*.fpatch");
            if (!string.IsNullOrEmpty(file))
            {
                _patchService.ExportFilePatch(FilePatch, file);
                _dialogService.ShowMessage("Exported successfully.", "Success");
            }
        }

        public void OpenFilePatch()
        {
            var file = _dialogService.OpenFileDialog("Open file patch", "File patch file (.fpatch)|*.fpatch");
            if (!string.IsNullOrEmpty(file))
            {
                _dialogService.ShowProgressBar("Loading", "Loading file patch...");
                using (new CursorWait())
                {
                    FilePatch = _patchService.OpenFilePatch(file);
                    OnPropertyChanged(nameof(FilePatch));
                }
                _dialogService.CloseProgressBar();
            }
        }

        public void SelectedItemChanged(object param)
        {
            SelectedNode = (NodeDef)param;
            OnPropertyChanged(nameof(SelectedNode));
        }
    }
}
