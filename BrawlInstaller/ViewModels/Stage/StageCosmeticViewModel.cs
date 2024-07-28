using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BrawlInstaller.ViewModels
{
    public interface IStageCosmeticViewModel
    {
        Cosmetic SelectedCosmetic { get; }
    }

    [Export(typeof(IStageCosmeticViewModel))]
    internal class StageCosmeticViewModel : ViewModelBase, IStageCosmeticViewModel
    {
        // Private properties
        private StageInfo _stage;
        private ObservableCollection<KeyValuePair<string, CosmeticType>> _cosmeticOptions;
        private CosmeticType _selectedCosmeticOption;
        private string _selectedStyle;
        private Cosmetic _selectedCosmetic;

        // Services

        // Commands
        public ICommand ChangeCosmeticCommand => new RelayCommand(param => ChangeCosmetic(param));

        [ImportingConstructor]
        public StageCosmeticViewModel()
        {
            WeakReferenceMessenger.Default.Register<StageLoadedMessage>(this, (recipient, message) =>
            {
                LoadStage(message);
            });
        }

        // Properties
        public StageInfo Stage { get => _stage; set { _stage = value; OnPropertyChanged(nameof(Stage)); } }

        [DependsUpon(nameof(Stage))]
        public ObservableCollection<KeyValuePair<string, CosmeticType>> CosmeticOptions { get => _cosmeticOptions; set { _cosmeticOptions = value; OnPropertyChanged(nameof(CosmeticOptions)); } }

        [DependsUpon(nameof(CosmeticOptions))]
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(nameof(SelectedCosmeticOption)); } }

        [DependsUpon(nameof(SelectedCosmeticOption))]
        public List<string> Styles { get => Stage?.Cosmetics?.Items.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style).Distinct().ToList(); }

        [DependsUpon(nameof(Styles))]
        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(nameof(SelectedStyle)); } }

        [DependsUpon(nameof(SelectedStyle))]
        [DependsUpon(nameof(Stage))]
        public List<Cosmetic> SelectedCosmetics { get => Stage?.Cosmetics?.Items.Where(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle).ToList(); }

        [DependsUpon(nameof(SelectedCosmetics))]
        public Cosmetic SelectedCosmetic { 
            get => SelectedCosmetics?.FirstOrDefault(x => !x.SelectionOption) ?? SelectedCosmetics?.FirstOrDefault();
        }

        [DependsUpon(nameof(SelectedCosmetics))]
        public bool DisplayCosmeticSelect { get => SelectedCosmetics?.Count > 1; }

        // Methods
        public void LoadStage(StageLoadedMessage message)
        {
            var cosmeticOptions = new List<KeyValuePair<string, CosmeticType>>
            {
                CosmeticType.StagePreview.GetKeyValuePair(),
                CosmeticType.StageFranchiseIcon.GetKeyValuePair()
            };
            CosmeticOptions = new ObservableCollection<KeyValuePair<string, CosmeticType>>(cosmeticOptions);
            OnPropertyChanged(nameof(CosmeticOptions));
            Stage = message.Value;
            OnPropertyChanged(nameof(Stage));
            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;
            OnPropertyChanged(nameof(SelectedCosmeticOption));
            SelectedStyle = Styles.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedStyle));
        }

        public void ChangeCosmetic(object selectedCosmetic)
        {
            // If the user changes the selected cosmetic, flip the old selection to be an option and the new one to be selected
            var cosmetic = selectedCosmetic as Cosmetic;
            if (cosmetic != null)
            {
                // Only set SelectedCosmetic to an option if it actually exists
                if (SelectedCosmetic != null && SelectedCosmetic.CosmeticType == cosmetic.CosmeticType && SelectedCosmetic.Style == cosmetic.Style)
                {
                    SelectedCosmetic.SelectionOption = true;
                }
                cosmetic.SelectionOption = false;
                OnPropertyChanged(nameof(SelectedCosmetic));
            }
        }
    }
}
