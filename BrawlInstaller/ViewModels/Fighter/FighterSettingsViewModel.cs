﻿using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using BrawlInstaller.StaticClasses;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static BrawlLib.SSBB.ResourceNodes.FCFGNode;

namespace BrawlInstaller.ViewModels
{
    public interface IFighterSettingsViewModel
    {
        FighterPackage FighterPackage { get; }
    }

    [Export(typeof(IFighterSettingsViewModel))]
    internal class FighterSettingsViewModel : ViewModelBase, IFighterSettingsViewModel
    {
        //TODO: warn user that sword trails will not be changed when switching between custom and vanilla Effect.pacs

        // Private properties
        private FighterPackage _fighterPackage;
        private List<FighterInfo> _fighterInfoList;

        // Services
        IDialogService _dialogService { get; }
        IFighterService _fighterService { get; }
        ISettingsService _settingsService { get; }

        // Commands
        public ICommand LoadKirbyHatCommand => new RelayCommand(param => LoadKirbyHat());
        public ICommand ClearKirbyHatCommand => new RelayCommand(param => ClearKirbyHat());

        // Importing constructor
        [ImportingConstructor]
        public FighterSettingsViewModel(IDialogService dialogService, IFighterService fighterService, ISettingsService settingsService)
        {
            _dialogService = dialogService;
            _fighterService = fighterService;
            _settingsService = settingsService;

            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadFighterSettings(message);
            });
        }

        // Properties
        public FighterPackage FighterPackage { get => _fighterPackage; set { _fighterPackage = value; OnPropertyChanged(nameof(FighterPackage)); } }
        public List<FighterInfo> FighterInfoList { get => _fighterInfoList; set { _fighterInfoList = value; OnPropertyChanged(nameof(FighterInfoList)); } }
        public Dictionary<string, int> FighterEffectPacs { get => EffectPacs.FighterEffectPacs; }

        [DependsUpon(nameof(FighterPackage))]
        public Dictionary<string, KirbyLoadFlags> KirbyLoadFlagOptions { get => typeof(KirbyLoadFlags).GetDictionary<KirbyLoadFlags>(); }

        // Methods
        public void LoadFighterSettings(FighterLoadedMessage message)
        {
            FighterPackage = message.Value;
            FighterInfoList = _settingsService.LoadFighterInfoSettings();
        }

        public void LoadKirbyHat()
        {
            var file = _dialogService.OpenFileDialog("Select Kirby Hat XML file", "XML file (.xml)|*.xml");
            if (!string.IsNullOrEmpty(file))
            {
                FighterPackage.FighterSettings.KirbyHatData = _fighterService.ConvertXMLToKirbyHatData(file);
                OnPropertyChanged(nameof(FighterPackage));
            }
        }

        public void ClearKirbyHat()
        {
            FighterPackage.FighterSettings.KirbyHatData = null;
            OnPropertyChanged(nameof(FighterPackage));
        }
    }
}
