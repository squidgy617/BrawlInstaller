using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.ViewModels
{
    public interface IStageEditorViewModel
    {

    }

    [Export(typeof(IStageEditorViewModel))]
    internal class StageEditorViewModel : ViewModelBase, IStageEditorViewModel
    {
        // Private properties
        private Stage _stage;

        // Services
        IStageService _stageService { get; }

        [ImportingConstructor]
        public StageEditorViewModel(IStageService stageService)
        {
            _stageService = stageService;

            WeakReferenceMessenger.Default.Register<StageLoadedMessage>(this, (recipient, message) =>
            {
                LoadStage(message);
            });
        }

        // Properties
        public Stage Stage { get => _stage; set { _stage = value; OnPropertyChanged(nameof(Stage)); } }

        // Methods
        public void LoadStage(StageLoadedMessage message)
        {
            Stage = message.Value;
        }
    }
}
