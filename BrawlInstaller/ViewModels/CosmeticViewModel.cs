using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
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
        ObservableCollection<KeyValuePair<string, CosmeticType>> CosmeticOptions { get; }
        CosmeticType SelectedCosmeticOption { get; set; }
        Cosmetic SelectedCosmetic { get; }
        List<string> Styles { get; }
        string SelectedStyle { get; }
    }

    [Export(typeof(ICosmeticViewModel))]
    internal class CosmeticViewModel : ViewModelBase, ICosmeticViewModel
    {
        // Private properties
        private List<Cosmetic> _cosmetics;
        private ObservableCollection<KeyValuePair<string, CosmeticType>> _cosmeticOptions;
        private CosmeticType _selectedCosmeticOption;
        private string _selectedStyle;

        // Services
        ISettingsService _settingsService { get; }

        // Importing constructor
        [ImportingConstructor]
        public CosmeticViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            CosmeticOptions = new ObservableCollection<KeyValuePair<string, CosmeticType>>
            {
                CosmeticType.CreditsIcon.GetKeyValuePair()
            };

            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;

            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadCosmetics(message);
            });
        }

        //Properties
        public List<Cosmetic> Cosmetics { get => _cosmetics; set { _cosmetics = value; OnPropertyChanged(nameof(Cosmetics)); } }

        [DependsUpon(nameof(Cosmetics))]
        public ObservableCollection<KeyValuePair<string, CosmeticType>> CosmeticOptions { get => _cosmeticOptions; set { _cosmeticOptions = value; OnPropertyChanged(nameof(CosmeticOptions)); } }
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(nameof(SelectedCosmeticOption)); } }

        [DependsUpon(nameof(Cosmetics))]
        [DependsUpon(nameof(SelectedStyle))]
        public Cosmetic SelectedCosmetic { get => Cosmetics?.FirstOrDefault(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle); }

        [DependsUpon(nameof(Cosmetics))]
        [DependsUpon(nameof(SelectedCosmeticOption))]
        public List<string> Styles { get => Cosmetics?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style).Distinct().ToList(); }
        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(nameof(SelectedStyle)); } }

        // Methods
        public void LoadCosmetics(FighterLoadedMessage message)
        {
            Cosmetics = message.Value.Cosmetics.Items;
            foreach (CosmeticType option in _settingsService.BuildSettings.CosmeticSettings.Where(x => x.IdType != IdType.Cosmetic && x.IdType != IdType.Franchise
            && x.IdType != IdType.Thumbnail && !CosmeticOptions.Select(y => y.Value).Contains(x.CosmeticType)).Select(x => x.CosmeticType).Distinct())
            {
                CosmeticOptions.Add(option.GetKeyValuePair());
            }
            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;
        }
    }
}
