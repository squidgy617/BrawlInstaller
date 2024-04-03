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
using BrawlInstaller.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BrawlLib.Internal;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BrawlInstaller.ViewModels
{
    public interface IFighterViewModel
    {
        ICommand LoadCommand { get; }
        void LoadFighter();
        FighterPackage FighterPackage { get; }
        List<Costume> Costumes { get; }
        Costume SelectedCostume { get; set; }
        List<KeyValuePair<string, CosmeticType>> CosmeticOptions { get; }
        CosmeticType SelectedCosmeticOption { get; set; }
        Cosmetic SelectedCosmetic { get; }
        List<string> Styles { get; }
        string SelectedStyle { get; }
        List<BrawlExColorID> Colors { get; }
        FighterIdsViewModel FighterIds { get; set; }
        List<Cosmetic> CosmeticList { get; }
        Cosmetic SelectedCosmeticNode { get; set; }
    }

    [Export(typeof(IFighterViewModel))]
    internal class FighterViewModel : ViewModelBase, IFighterViewModel
    {
        // Private properties
        private List<Costume> _costumes;
        private Costume _selectedCostume;
        private List<KeyValuePair<string, CosmeticType>> _cosmeticOptions;
        private CosmeticType _selectedCosmeticOption;
        private string _selectedStyle;
        private List<BrawlExColorID> _colors;
        private FighterIdsViewModel _fighterIds;
        private Cosmetic _selectedCosmeticNode;

        // Services
        IExtractService _extractService { get; }
        ISettingsService _settingsService { get; }
        IDialogService _dialogService { get; }
        ICosmeticService _cosmeticService { get; }

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
        public FighterViewModel(IExtractService extractService, ISettingsService settingsService, IDialogService dialogService, ICosmeticService cosmeticService, IFranchiseIconViewModel franchiseIconViewModel)
        {
            _extractService = extractService;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _cosmeticService = cosmeticService;
            FranchiseIconViewModel = franchiseIconViewModel;

            _settingsService.BuildPath = "F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild\\Builds\\P+Ex\\";
            _settingsService.BuildSettings = _settingsService.GetDefaultSettings();

            CosmeticOptions = new List<KeyValuePair<string, CosmeticType>>();
            //foreach (CosmeticType option in Enum.GetValues(typeof(CosmeticType)))
            foreach (CosmeticType option in settingsService.BuildSettings.CosmeticSettings.Where(x => x.IdType == IdType.Cosmetic).Select(x => x.CosmeticType).Distinct())
            {
                CosmeticOptions.Add(option.GetKeyValuePair());
            }
            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;

            Colors = BrawlExColorID.Colors.ToList();

            FighterIds = new FighterIdsViewModel ();
        }

        // ViewModels
        public IFranchiseIconViewModel FranchiseIconViewModel { get; }

        // Properties
        public FighterPackage FighterPackage { get; set; }
        public List<Costume> Costumes { get => _costumes; set { _costumes = value; OnPropertyChanged(); OnPropertyChanged(nameof(CosmeticOptions)); OnPropertyChanged(nameof(Styles)); OnPropertyChanged(nameof(CosmeticList)); } }
        public Costume SelectedCostume { get => _selectedCostume; set { _selectedCostume = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCosmetic)); } }
        public List<KeyValuePair<string, CosmeticType>> CosmeticOptions { get => _cosmeticOptions; set { _cosmeticOptions = value; OnPropertyChanged(); } }
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCosmetic)); OnPropertyChanged(nameof(Styles)); OnPropertyChanged(nameof(CosmeticList)); } }
        public Cosmetic SelectedCosmetic { get => SelectedCostume?.Cosmetics?.FirstOrDefault(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle); }
        public List<string> Styles { get => Costumes?.FirstOrDefault()?.Cosmetics?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style).Distinct().ToList(); }
        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCosmetic)); OnPropertyChanged(nameof(CosmeticList)); } }
        public List<BrawlExColorID> Colors { get => _colors; set { _colors = value; OnPropertyChanged(); } }
        public FighterIdsViewModel FighterIds { get => _fighterIds; set { _fighterIds = value; OnPropertyChanged(); } }
        public List<Cosmetic> CosmeticList 
        { 
            get => Costumes?.SelectMany(x => x.Cosmetics).OrderBy(x => x.InternalIndex)
                .Where(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle).ToList();
        }
        public Cosmetic SelectedCosmeticNode { get => _selectedCosmeticNode; set { _selectedCosmeticNode = value; OnPropertyChanged(); } }

        // Methods
        public void LoadFighter()
        {
            FighterPackage = _extractService.ExtractFighter(new FighterIds
            {
                FighterConfigId = FighterIds.FighterConfigId ?? -1,
                CosmeticConfigId = FighterIds.CosmeticConfigId ?? -1,
                CSSSlotConfigId = FighterIds.CSSSlotConfigId ?? -1,
                SlotConfigId = FighterIds.SlotConfigId ?? -1,
                CosmeticId = FighterIds.CosmeticId ?? -1
                //FighterConfigId = 37,
                //SlotConfigId = 39,
                //CosmeticConfigId = 35,
                //CSSSlotConfigId = 35
            });
            Costumes = FighterPackage.Costumes;
            SelectedCostume = Costumes.FirstOrDefault();
            WeakReferenceMessenger.Default.Send(new FighterLoadedMessage(FighterPackage));
        }
    }

    // Mappings
    public class FighterIdsViewModel
    {
        public int? FighterConfigId { get; set; }
        public int? SlotConfigId { get; set; }
        public int? CSSSlotConfigId { get; set; }
        public int? CosmeticConfigId { get; set; }
        public int? CosmeticId { get; set; }
    }

    // Messages
    public class FighterLoadedMessage : ValueChangedMessage<FighterPackage>
    {
        public FighterLoadedMessage(FighterPackage fighterPackage) : base(fighterPackage)
        {
        }
    }
}
