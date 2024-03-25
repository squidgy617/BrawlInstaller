﻿using BrawlInstaller.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.Windows.Input;
using BrawlLib.SSBB.ResourceNodes;
using System.Diagnostics;
using BrawlInstaller.Services;
using BrawlInstaller.Classes;
using static BrawlInstaller.ViewModels.FighterViewModel;
using BrawlInstaller.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BrawlInstaller.ViewModels
{
    public interface IFighterViewModel
    {
        ICommand LoadCommand { get; }
        void LoadFighter();
        FighterPackage FighterPackage { get; }
        List<Costume> Costumes { get; }
        Costume SelectedCostume { get; set; }
        List<CosmeticType> CosmeticOptions { get; }
        CosmeticType SelectedCosmeticOption { get; set; }
        Cosmetic SelectedCosmetic { get; }
        List<string> Styles { get; }
        string SelectedStyle { get; }
    }

    [Export(typeof(IFighterViewModel))]
    internal class FighterViewModel : ViewModelBase, IFighterViewModel
    {
        // Private properties
        private List<Costume> _costumes;
        private Costume _selectedCostume;
        private List<CosmeticType> _cosmeticOptions;
        private CosmeticType _selectedCosmeticOption;
        private string _selectedStyle;

        // Services
        IExtractService _extractService { get; }
        ISettingsService _settingsService { get; }

        // Commands
        public ICommand LoadCommand
        {
            get
            {
                return new RelayCommand(param => LoadFighter());
            }
        }

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public FighterViewModel(IExtractService extractService, ISettingsService settingsService)
        {
            _extractService = extractService;
            _settingsService = settingsService;

            CosmeticOptions = new List<CosmeticType>();
            foreach (var option in Enum.GetValues(typeof(CosmeticType)))
            {
                CosmeticOptions.Add((CosmeticType)option);
            }
            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault();
        }

        // Properties
        public FighterPackage FighterPackage { get; set; }
        public List<Costume> Costumes { get => _costumes; set { _costumes = value; OnPropertyChanged(); } }
        public Costume SelectedCostume { get => _selectedCostume; set { _selectedCostume = value; OnPropertyChanged(); OnPropertyChanged("SelectedCosmetic"); OnPropertyChanged("Styles"); } }
        public List<CosmeticType> CosmeticOptions { get => _cosmeticOptions; set { _cosmeticOptions = value; OnPropertyChanged(); } }
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(); OnPropertyChanged("SelectedCosmetic"); OnPropertyChanged("Styles"); } }
        public Cosmetic SelectedCosmetic { get => SelectedCostume?.Cosmetics?.FirstOrDefault(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle); }
        public List<string> Styles { get => Costumes?.FirstOrDefault()?.Cosmetics?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style).Distinct().ToList(); }
        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(); OnPropertyChanged("SelectedCosmetic"); } }

        // Methods
        public void LoadFighter()
        {
            _settingsService.BuildPath = "F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild\\Builds\\P+Ex\\";
            _settingsService.BuildSettings = _settingsService.GetDefaultSettings();
            FighterPackage = _extractService.ExtractFighter(new FighterIds
            {
                FighterConfigId = 37,
                SlotConfigId = 39,
                CosmeticConfigId = 35,
                CSSSlotConfigId = 35
            });
            Costumes = FighterPackage.Costumes;
            SelectedCostume = Costumes.FirstOrDefault();
        }
    }
}
