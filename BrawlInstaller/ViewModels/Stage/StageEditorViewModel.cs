﻿using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BrawlInstaller.ViewModels
{
    public interface IStageEditorViewModel
    {

    }

    [Export(typeof(IStageEditorViewModel))]
    internal class StageEditorViewModel : ViewModelBase, IStageEditorViewModel
    {
        // Private properties
        private StageInfo _stage;
        private StageEntry _selectedStageEntry;

        // Services
        IStageService _stageService { get; }

        [ImportingConstructor]
        public StageEditorViewModel(IStageService stageService, IStageCosmeticViewModel stageCosmeticViewModel)
        {
            _stageService = stageService;
            StageCosmeticViewModel = stageCosmeticViewModel;

            WeakReferenceMessenger.Default.Register<StageLoadedMessage>(this, (recipient, message) =>
            {
                LoadStage(message);
            });
        }

        // Properties
        public StageInfo Stage { get => _stage; set { _stage = value; OnPropertyChanged(nameof(Stage)); } }

        [DependsUpon(nameof(Stage))]
        public StageEntry SelectedStageEntry { get => _selectedStageEntry; set { _selectedStageEntry = value; OnPropertyChanged(nameof(SelectedStageEntry)); } }

        // ViewModels
        public IStageCosmeticViewModel StageCosmeticViewModel { get; }

        // Methods
        public void LoadStage(StageLoadedMessage message)
        {
            Stage = message.Value;
            OnPropertyChanged(nameof(Stage));
        }
    }
}
