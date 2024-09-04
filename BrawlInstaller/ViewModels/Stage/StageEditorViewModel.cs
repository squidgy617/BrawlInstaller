using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static BrawlLib.SSBB.ResourceNodes.ProjectPlus.STEXNode;

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
        private Substage _selectedSubstage;

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

        [DependsUpon(nameof(SelectedStageEntry))]
        public ushort SelectedButtonFlags { get => SelectedStageEntry?.ButtonFlags ?? 0x0; set { SelectedStageEntry.ButtonFlags = value; OnPropertyChanged(nameof(SelectedButtonFlags)); } }

        [DependsUpon(nameof(SelectedButtonFlags))]
        public bool ButtonsEnabled { get => SelectedButtonFlags < 0x4000; }

        [DependsUpon(nameof(ButtonsEnabled))]
        [DependsUpon(nameof(SelectedButtonFlags))]
        public bool ListAltsEnabled { get => SelectedButtonFlags <= 0x0 || !ButtonsEnabled; }

        [DependsUpon(nameof(ListAltsEnabled))]
        public bool LAltsEnabled { get => ListAltsEnabled && (SelectedButtonFlags < 0x4000 || SelectedButtonFlags >= 0x8000); }

        [DependsUpon(nameof(ListAltsEnabled))]
        public bool RAltsEnabled { get => ListAltsEnabled && ((SelectedButtonFlags >= 0x4000 && SelectedButtonFlags < 0x8000) || SelectedButtonFlags <= 0x4000); }

        public Dictionary<string, VariantType> VariantTypes { get => typeof(VariantType).GetDictionary<VariantType>(); }

        [DependsUpon(nameof(SelectedStageEntry))]
        public Substage SelectedSubstage { get => _selectedSubstage; set { _selectedSubstage = value; OnPropertyChanged(nameof(SelectedSubstage)); } }

        // ViewModels
        public IStageCosmeticViewModel StageCosmeticViewModel { get; }

        // Methods
        public void LoadStage(StageLoadedMessage message)
        {
            Stage = message.Value;
            OnPropertyChanged(nameof(Stage));
        }

        // TODO: Separate the filepath textboxes from the name textboxes for pac files, module, etc - selecting a file updates the file path, changing the name
        // just sets what the name will be upon save
    }
}
