﻿using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
        IDialogService _dialogService { get; }

        // Commands
        public ICommand MoveEntryUpCommand => new RelayCommand(param => MoveEntryUp());
        public ICommand MoveEntryDownCommand => new RelayCommand(param => MoveEntryDown());
        public ICommand MoveSubstageUpCommand => new RelayCommand(param => MoveSubstageUp());
        public ICommand MoveSubstageDownCommand => new RelayCommand(param => MoveSubstageDown());

        [ImportingConstructor]
        public StageEditorViewModel(IStageService stageService, IDialogService dialogService, IStageCosmeticViewModel stageCosmeticViewModel)
        {
            _stageService = stageService;
            _dialogService = dialogService;
            StageCosmeticViewModel = stageCosmeticViewModel;

            WeakReferenceMessenger.Default.Register<StageLoadedMessage>(this, (recipient, message) =>
            {
                LoadStage(message);
            });
        }

        // Properties
        public StageInfo Stage { get => _stage; set { _stage = value; OnPropertyChanged(nameof(Stage)); } }

        [DependsUpon(nameof(Stage))]
        public ObservableCollection<StageEntry> StageEntries { get => Stage?.StageEntries != null ? new ObservableCollection<StageEntry>(Stage.StageEntries) : new ObservableCollection<StageEntry>(); }

        [DependsUpon(nameof(Stage))]
        [DependsUpon(nameof(StageEntries))]
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
        public ObservableCollection<Substage> Substages { get => SelectedStageEntry?.Params?.Substages != null ? new ObservableCollection<Substage>(SelectedStageEntry.Params.Substages) : new ObservableCollection<Substage>(); }

        [DependsUpon(nameof(SelectedStageEntry))]
        [DependsUpon(nameof(Substages))]
        public Substage SelectedSubstage { get => _selectedSubstage; set { _selectedSubstage = value; OnPropertyChanged(nameof(SelectedSubstage)); } }

        [DependsUpon(nameof(SelectedButtonFlags))]
        public bool RListAlt { get => SelectedButtonFlags >= 0x4000 && SelectedButtonFlags < 0x8000; }

        [DependsUpon(nameof(SelectedButtonFlags))]
        public bool LListAlt { get => SelectedButtonFlags >= 0x8000; }

        [DependsUpon(nameof(LListAlt))]
        [DependsUpon(nameof(RListAlt))]
        public bool ListAlt { get => LListAlt || RListAlt; }

        // ViewModels
        public IStageCosmeticViewModel StageCosmeticViewModel { get; }

        // Methods
        public void LoadStage(StageLoadedMessage message)
        {
            Stage = message.Value;
            OnPropertyChanged(nameof(Stage));
        }

        private void MoveEntryUp()
        {
            Stage.StageEntries.MoveUp(SelectedStageEntry);
            StageEntries.MoveUp(SelectedStageEntry);
            OnPropertyChanged(nameof(SelectedStageEntry));
            OnPropertyChanged(nameof(StageEntries));
        }

        private void MoveEntryDown()
        {
            Stage.StageEntries.MoveDown(SelectedStageEntry);
            StageEntries.MoveDown(SelectedStageEntry);
            OnPropertyChanged(nameof(SelectedStageEntry));
            OnPropertyChanged(nameof(StageEntries));
        }

        private void MoveSubstageUp()
        {
            SelectedStageEntry.Params.Substages.MoveUp(SelectedSubstage);
            Substages.MoveUp(SelectedSubstage);
            OnPropertyChanged(nameof(SelectedSubstage));
            OnPropertyChanged(nameof(Substages));
        }

        private void MoveSubstageDown()
        {
            SelectedStageEntry.Params.Substages.MoveDown(SelectedSubstage);
            Substages.MoveDown(SelectedSubstage);
            OnPropertyChanged(nameof(SelectedSubstage));
            OnPropertyChanged(nameof(Substages));
        }
    }
}
