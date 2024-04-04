﻿using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using BrawlLib.Internal;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.ViewModels
{
    public interface ICostumeViewModel
    {
        List<Costume> Costumes { get; }
        Costume SelectedCostume { get; set; }
        List<KeyValuePair<string, CosmeticType>> CosmeticOptions { get; }
        CosmeticType SelectedCosmeticOption { get; set; }
        Cosmetic SelectedCosmetic { get; }
        List<string> Styles { get; }
        string SelectedStyle { get; }
        List<BrawlExColorID> Colors { get; }
        List<Cosmetic> CosmeticList { get; }
        Cosmetic SelectedCosmeticNode { get; set; }
    }

    [Export(typeof(ICostumeViewModel))]
    internal class CostumeViewModel : ViewModelBase, ICostumeViewModel
    {
        // Private properties
        private List<Costume> _costumes;
        private Costume _selectedCostume;
        private List<KeyValuePair<string, CosmeticType>> _cosmeticOptions;
        private CosmeticType _selectedCosmeticOption;
        private string _selectedStyle;
        private List<BrawlExColorID> _colors;
        private Cosmetic _selectedCosmeticNode;

        // Services
        ISettingsService _settingsService { get; }

        // Importing constructor
        [ImportingConstructor]
        public CostumeViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            CosmeticOptions = new List<KeyValuePair<string, CosmeticType>>();
            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;

            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadCostumes(message);
            });
        }

        // Properties
        public List<Costume> Costumes { get => _costumes; set { _costumes = value; OnPropertyChanged(); OnPropertyChanged(nameof(CosmeticOptions)); OnPropertyChanged(nameof(Styles)); OnPropertyChanged(nameof(CosmeticList)); } }
        public Costume SelectedCostume { get => _selectedCostume; set { _selectedCostume = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCosmetic)); OnPropertyChanged(nameof(SelectedCosmeticOption)); } }
        public List<KeyValuePair<string, CosmeticType>> CosmeticOptions { get => _cosmeticOptions; set { _cosmeticOptions = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCosmeticOption)); } }
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCosmetic)); OnPropertyChanged(nameof(Styles)); OnPropertyChanged(nameof(CosmeticList)); } }
        public Cosmetic SelectedCosmetic { get => SelectedCostume?.Cosmetics?.FirstOrDefault(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle); }
        public List<string> Styles { get => Costumes?.FirstOrDefault()?.Cosmetics?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style).Distinct().ToList(); }
        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCosmetic)); OnPropertyChanged(nameof(CosmeticList)); } }
        public List<BrawlExColorID> Colors { get => _colors; set { _colors = value; OnPropertyChanged(); } }
        public List<Cosmetic> CosmeticList
        {
            get => Costumes?.SelectMany(x => x.Cosmetics).OrderBy(x => x.InternalIndex)
                .Where(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle).ToList();
        }
        public Cosmetic SelectedCosmeticNode { get => _selectedCosmeticNode; set { _selectedCosmeticNode = value; OnPropertyChanged(); } }

        // Methods
        public void LoadCostumes(FighterLoadedMessage message)
        {
            Costumes = message.Value.Costumes;
            SelectedCostume = Costumes.FirstOrDefault();

            CosmeticOptions = new List<KeyValuePair<string, CosmeticType>>();
            //foreach (CosmeticType option in Enum.GetValues(typeof(CosmeticType)))
            foreach (CosmeticType option in _settingsService.BuildSettings.CosmeticSettings.Where(x => x.IdType == IdType.Cosmetic).Select(x => x.CosmeticType).Distinct())
            {
                CosmeticOptions.Add(option.GetKeyValuePair());
            }
            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;

            Colors = BrawlExColorID.Colors.ToList();
        }
    }
}
