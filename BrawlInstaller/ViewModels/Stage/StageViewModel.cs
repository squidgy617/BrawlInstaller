using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Helpers;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private StageInfo _stage;

        // Services
        IStageService _stageService { get; }

        // Commands
        public ICommand LoadStageCommand => new RelayCommand(param => LoadStage());

        [ImportingConstructor]
        public StageViewModel(IStageService stageService, IStageListViewModel stageListViewModel, IStageEditorViewModel stageEditorViewModel)
        {
            _stageService = stageService;
            StageListViewModel = stageListViewModel;
            StageEditorViewModel = stageEditorViewModel;
        }

        // ViewModels
        public IStageListViewModel StageListViewModel { get; }
        public IStageEditorViewModel StageEditorViewModel { get; }

        // Properties
        public StageInfo Stage { get =>  _stage; set { _stage = value; OnPropertyChanged(nameof(Stage)); } }

        // Methods
        public void LoadStage()
        {
            using (new CursorWait())
            {
                var stage = new StageInfo();
                stage.Slot = StageListViewModel.SelectedStageSlot;
                stage = _stageService.GetStageData(stage);
                WeakReferenceMessenger.Default.Send(new StageLoadedMessage(stage));
            }
        }
    }

    // Messages
    public class StageLoadedMessage : ValueChangedMessage<StageInfo>
    {
        public StageLoadedMessage(StageInfo stage) : base(stage)
        {
        }
    }
}
