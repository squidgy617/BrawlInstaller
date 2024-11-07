using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using BrawlInstaller.StaticClasses;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.ViewModels
{
    public interface ICosmeticViewModel
    {
        List<Cosmetic> Cosmetics { get; }
        CosmeticType SelectedCosmeticOption { get; set; }
        Cosmetic SelectedCosmetic { get; }
        List<string> Styles { get; }
        string SelectedStyle { get; }
    }

    [Export(typeof(ICosmeticViewModel))]
    internal class CosmeticViewModel : ViewModelBase, ICosmeticViewModel
    {
        // Private properties
        private FighterPackage _fighterPackage;
        private List<Cosmetic> _cosmetics;
        private CosmeticType _selectedCosmeticOption;
        private string _selectedStyle;

        // Services
        ISettingsService _settingsService { get; }

        // Importing constructor
        [ImportingConstructor]
        public CosmeticViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;

            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadCosmetics(message);
            });
        }

        //Properties
        public FighterPackage FighterPackage { get => _fighterPackage; set { _fighterPackage = value; OnPropertyChanged(nameof(FighterPackage)); } }

        public List<Cosmetic> Cosmetics { get => _cosmetics; set { _cosmetics = value; OnPropertyChanged(nameof(Cosmetics)); } }

        [DependsUpon(nameof(Cosmetics))]
        public Dictionary<string, CosmeticType> CosmeticOptions { get => DefaultCosmetics.DefaultFighterCosmetics.Select(x => x.CosmeticType.GetKeyValuePair()).Distinct().ToList().ToDictionary(x => x.Key, x => x.Value); }
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(nameof(SelectedCosmeticOption)); } }

        [DependsUpon(nameof(Cosmetics))]
        [DependsUpon(nameof(SelectedStyle))]
        public Cosmetic SelectedCosmetic { get => Cosmetics?.FirstOrDefault(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle); }

        [DependsUpon(nameof(Cosmetics))]
        [DependsUpon(nameof(SelectedCosmeticOption))]
        public List<string> Styles
        {
            get => DefaultCosmetics.DefaultCostumeCosmetics?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style)
                .Concat(_settingsService.BuildSettings?.CosmeticSettings?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style) ?? new List<string>())
                .Concat(FighterPackage?.Cosmetics?.Items?.Where(y => y.CosmeticType == SelectedCosmeticOption)?.Select(y => y.Style) ?? new List<string>()).Distinct().ToList();
        }

        [DependsUpon(nameof(Styles))]
        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(nameof(SelectedStyle)); } }

        // Methods
        public void LoadCosmetics(FighterLoadedMessage message)
        {
            Cosmetics = message.Value.Cosmetics.Items;
            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;
        }
    }
}
