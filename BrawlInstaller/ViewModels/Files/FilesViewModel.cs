using BrawlInstaller.Common;
using BrawlInstaller.Services;
using BrawlLib.SSBB;
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

        // Services
        IPatchService _patchService;

        // Commands
        public ICommand CompareFilesCommand => new RelayCommand(param => CompareFiles());

        [ImportingConstructor]
        public FilesViewModel(IPatchService patchService)
        {
            _patchService = patchService;
        }

        // Properties
        public string LeftFilePath { get => _leftFilePath; set { _leftFilePath = value; OnPropertyChanged(LeftFilePath); } }
        public string RightFilePath { get => _rightFilePath; set { _rightFilePath = value;OnPropertyChanged(RightFilePath); } }
        public string FileFilter { get => SupportedFilesHandler.GetAllSupportedFilter(true); }

        // Methods
        public void CompareFiles()
        {
            var filePatch = _patchService.CompareFiles(LeftFilePath, RightFilePath);
            return;
        }
    }
}
