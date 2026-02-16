using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Helpers;
using BrawlInstaller.Services;
using BrawlLib.SSBB;
using BrawlLib.SSBB.ResourceNodes;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static BrawlInstaller.ViewModels.MainControlsViewModel;

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
        private string _targetFilePath;
        private ResourceNode _leftFileNode;
        private ResourceNode _rightFileNode;
        private FilePatch _filePatch;
        private NodeDefViewModel _selectedNode;
        private BuildPatch _buildPatch;
        private BuildFilePatch _selectedBuildFilePatch;

        // Services
        IPatchService _patchService;
        IDialogService _dialogService;
        IFileService _fileService;
        ISettingsService _settingsService;

        // Commands
        public ICommand CompareFilesCommand => new RelayCommand(param => CompareFiles());
        public ICommand ExportFilePatchCommand => new RelayCommand(param => ExportFilePatch());
        public ICommand ExportBuildPatchCommand => new RelayCommand(param => ExportBuildPatch());
        public ICommand OpenFilePatchCommand => new RelayCommand(param => OpenFilePatch());
        public ICommand OpenBuildPatchCommand => new RelayCommand(param => OpenBuildPatch());
        public ICommand ApplyFilePatchCommand => new RelayCommand(param => ApplyFilePatch());
        public ICommand AddBuildPatchEntryCommand => new RelayCommand(param => AddBuildPatchEntry());
        public ICommand RemoveBuildPatchEntryCommand => new RelayCommand(param => RemoveBuildPatchEntry());
        public ICommand ApplyBuildPatchCommand => new RelayCommand(param => ApplyBuildPatch());

        [ImportingConstructor]
        public FilesViewModel(IPatchService patchService, IDialogService dialogService, IFileService fileService, ISettingsService settingsService)
        {
            _patchService = patchService;
            _dialogService = dialogService;
            _fileService = fileService;
            _settingsService = settingsService;

            BuildPatch = new BuildPatch();

            WeakReferenceMessenger.Default.Register<UpdateSettingsMessage>(this, (recipient, message) =>
            {
                UpdateSettings();
            });

            WeakReferenceMessenger.Default.Register<SettingsSavedMessage>(this, (recipient, message) =>
            {
                UpdateSettings();
            });
        }

        // Properties
        public bool BuildSettingsExist { get => _fileService.FileExists(_settingsService.BuildSettingsPath); }
        public bool BuildPathExists { get => !string.IsNullOrEmpty(_settingsService.AppSettings.BuildPath) && _fileService.DirectoryExists(_settingsService.AppSettings.BuildPath); }
        public string LeftFilePath { get => _leftFilePath; set { _leftFilePath = value; OnPropertyChanged(nameof(LeftFilePath)); } }
        public string RightFilePath { get => _rightFilePath; set { _rightFilePath = value;OnPropertyChanged(nameof(RightFilePath)); } }
        public string TargetFilePath { get => _targetFilePath; set { _targetFilePath = value; OnPropertyChanged(nameof(TargetFilePath)); } }
        public ResourceNode LeftFileNode { get => _leftFileNode; set { _leftFileNode = value; OnPropertyChanged(nameof(LeftFileNode)); } }
        public ResourceNode RightFileNode { get => _rightFileNode; set { _rightFileNode = value; OnPropertyChanged(nameof(RightFileNode)); } }
        public string FileFilter { get => SupportedFilesHandler.GetAllSupportedFilter(true); }
        public FilePatch FilePatch { get => _filePatch; set { _filePatch = value; OnPropertyChanged(nameof(FilePatch)); } }

        [DependsUpon(nameof(FilePatch))]
        public ObservableCollection<NodeDefViewModel> NodeList { get => FilePatch?.NodeDefs != null ? new ObservableCollection<NodeDefViewModel>(FilePatch.NodeDefs.ToViewModel()) : new ObservableCollection<NodeDefViewModel>(); } // TODO: Should this default to a list like this?
        public NodeDefViewModel SelectedNode { get => _selectedNode; set { _selectedNode = value; OnPropertyChanged(nameof(SelectedNode)); } }
        public ICommand SelectedItemChangedCommand => new RelayCommand(param => SelectedItemChanged(param));

        [DependsUpon(nameof(LeftFilePath))]
        [DependsUpon(nameof(RightFilePath))]
        public bool FilePathsEnabled { get => !string.IsNullOrEmpty(LeftFilePath) && !string.IsNullOrEmpty(RightFilePath); }

        public BuildPatch BuildPatch { get => _buildPatch; set { _buildPatch = value; OnPropertyChanged(nameof(BuildPatch)); } }

        [DependsUpon(nameof(BuildPatch))]
        public ObservableCollection<BuildFilePatch> BuildFilePatches { get => BuildPatch?.BuildFilePatches != null ? new ObservableCollection<BuildFilePatch>(BuildPatch.BuildFilePatches) : new ObservableCollection<BuildFilePatch>(); }

        [DependsUpon(nameof(BuildFilePatches))]
        public BuildFilePatch SelectedBuildFilePatch { get => _selectedBuildFilePatch; set { _selectedBuildFilePatch = value; OnPropertyChanged(nameof(SelectedBuildFilePatch)); } }

        // Methods
        private void UpdateSettings()
        {
            OnPropertyChanged(nameof(BuildSettingsExist));
            OnPropertyChanged(nameof(BuildPathExists));
        }

        public void CompareFiles()
        {
            if (string.IsNullOrEmpty(RightFilePath) || string.IsNullOrEmpty(LeftFilePath))
            {
                return;
            }
            _dialogService.ShowProgressBar("Comparing", "Comparing files...");
            using (new CursorWait())
            {
                var rightFileNode = _fileService.OpenFile(RightFilePath);
                var leftFileNode = _fileService.OpenFile(LeftFilePath);
                if (rightFileNode != null && leftFileNode != null)
                {
                    Parallel.ForEach(NodeList.ToNodeDefs().FlattenList().AsParallel(), node =>
                    {
                        _fileService.CloseFile(node.Node);
                    });
                    _fileService.CloseFile(RightFileNode);
                    _fileService.CloseFile(LeftFileNode);
                    RightFileNode = rightFileNode;
                    LeftFileNode = leftFileNode;
                    FilePatch = _patchService.CompareFiles(LeftFileNode, RightFileNode);
                    TargetFilePath = LeftFilePath;
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

        public void ExportBuildPatch()
        {
            if (BuildPatch.BuildFilePatches.Count <= 0)
            {
                _dialogService.ShowMessage("Build patch has no entries!", "Empty Build Patch", System.Windows.MessageBoxImage.Error);
                return;
            }
            else if (BuildPatch.BuildFilePatches.Any(x => string.IsNullOrEmpty(x.TargetPath) || (string.IsNullOrEmpty(x.FilePatchPath) && string.IsNullOrEmpty(x.FilePath))))
            {
                _dialogService.ShowMessage("Every entry must have a path and either a file or patch file!", "Missing Paths", System.Windows.MessageBoxImage.Error);
                return;
            }
            var file = _dialogService.SaveFileDialog("Save build patch", "Build patch file (.bpatch)|*.bpatch");
            if (!string.IsNullOrEmpty(file))
            {
                _patchService.ExportBuildPatch(BuildPatch, file);
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

        public void OpenBuildPatch()
        {
            var file = _dialogService.OpenFileDialog("Open build patch", "Build patch file (.bpatch)|*.bpatch");
            if (!string.IsNullOrEmpty(file))
            {
                _dialogService.ShowProgressBar("Loading", "Loading build patch...");
                using (new CursorWait())
                {
                    BuildPatch = _patchService.OpenBuildPatch(file);
                    OnPropertyChanged(nameof(BuildPatch));
                }
                _dialogService.CloseProgressBar();
            }
        }

        public void AddBuildPatchEntry()
        {
            BuildPatch.BuildFilePatches.Add(new BuildFilePatch());
            SelectedBuildFilePatch = BuildPatch.BuildFilePatches.LastOrDefault();
            OnPropertyChanged(nameof(BuildPatch));
            OnPropertyChanged(nameof(BuildFilePatches));
            OnPropertyChanged(nameof(SelectedBuildFilePatch));
        }

        public void RemoveBuildPatchEntry()
        {
            BuildPatch.BuildFilePatches.Remove(SelectedBuildFilePatch);
            OnPropertyChanged(nameof(BuildPatch));
            OnPropertyChanged(nameof(BuildFilePatches));
        }

        public void ApplyFilePatch()
        {
            if (!string.IsNullOrEmpty(TargetFilePath) && FilePatch != null)
            {
                _dialogService.ShowProgressBar("Applying", "Applying file patch...");
                using (new CursorWait())
                {
                    _patchService.ApplyFilePatch(FilePatch, TargetFilePath);
                }
                _dialogService.CloseProgressBar();
                _dialogService.ShowMessage("Changes applied successfully.", "Success");
            }
        }

        public void ApplyBuildPatch()
        {
            if (BuildPatch != null && !string.IsNullOrEmpty(_settingsService.AppSettings.BuildPath))
            {
                _fileService.StartBackup();
                _dialogService.ShowProgressBar("Applying", "Applying build patch...");
                using (new CursorWait())
                {
                    _patchService.ApplyBuildPatch(BuildPatch);
                }
                _dialogService.CloseProgressBar();
                _fileService.EndBackup();
                _dialogService.ShowMessage("Changes applied successfully.", "Success");
            }
        }

        public void SelectedItemChanged(object param)
        {
            SelectedNode = (NodeDefViewModel)param;
            OnPropertyChanged(nameof(SelectedNode));
        }
    }

    public class NodeDefViewModel : ViewModelBase
    {
        private NodeDef _nodeDef;
        private ObservableCollection<NodeDefViewModel> _children = new ObservableCollection<NodeDefViewModel>();
        private NodeDefViewModel _parent;
        private bool _isEnabled = true;

        public NodeDef NodeDef { get => _nodeDef; set { _nodeDef = value; OnPropertyChanged(nameof(NodeDef)); } }
        public ObservableCollection<NodeDefViewModel> Children { get => _children; set { _children = value; OnPropertyChanged(nameof(Children)); } }
        public NodeDefViewModel Parent { get => _parent; set { _parent = value; OnPropertyChanged(nameof(Parent)); } }
        public bool IsEnabled { get => _isEnabled; set { UpdateEnableState(value); } }
        public bool AllowForceAdd { get => NodeDef?.Change != NodeChangeType.None && NodeDef?.Change != NodeChangeType.Removed && !FilePatches.Folders.Contains(NodeDef?.NodeType); }
        public bool AllowReplaceAllContents { get => NodeDef?.Change == NodeChangeType.Container && NodeDef?.IsContainer() == true && !FilePatches.Folders.Contains(NodeDef?.NodeType); }

        private void UpdateEnableState(bool isEnabled, bool updateChildren = true)
        {
            _isEnabled = isEnabled;
            NodeDef.IsEnabled = isEnabled;
            if (updateChildren)
            {
                foreach (var child in Children)
                {
                    child.UpdateEnableState(isEnabled);
                }
            }
            if (isEnabled && Parent != null)
            {
                Parent.UpdateEnableState(isEnabled, false);
            }
            OnPropertyChanged(nameof(IsEnabled));
        }
    }
}
