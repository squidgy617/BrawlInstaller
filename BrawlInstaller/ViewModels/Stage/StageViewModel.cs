using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Helpers;
using BrawlInstaller.Services;
using BrawlInstaller.StaticClasses;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BrawlInstaller.ViewModels
{
    public interface IStageViewModel
    {

    }

    [Export(typeof(IStageViewModel))]
    internal class StageViewModel : ViewModelBase, IStageViewModel
    {
        // Private properties

        // Services
        IStageService _stageService { get; }
        IDialogService _dialogService { get; }
        IFileService _fileService { get; }

        // Commands

        [ImportingConstructor]
        public StageViewModel(IStageService stageService, IDialogService dialogService, IFileService fileService, IStageListViewModel stageListViewModel, IStageEditorViewModel stageEditorViewModel)
        {
            _stageService = stageService;
            _dialogService = dialogService;
            _fileService = fileService;
            StageListViewModel = stageListViewModel;
            StageEditorViewModel = stageEditorViewModel;
        }

        // ViewModels
        public IStageListViewModel StageListViewModel { get; }
        public IStageEditorViewModel StageEditorViewModel { get; }

        // Properties

        // Methods
    }
}
