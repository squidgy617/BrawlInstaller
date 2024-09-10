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
using System.Drawing;
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
        private BuildSettings _buildSettings;

        // Services
        IDialogService _dialogService { get; }
        ISettingsService _settingsService { get; }

        // Commands
        public ICommand ChangeCosmeticCommand => new RelayCommand(param => ChangeCosmetic(param));
        public ICommand ReplaceCosmeticCommand => new RelayCommand(param => ReplaceCosmetic());
        public ICommand ReplaceHDCosmeticCommand => new RelayCommand(param => ReplaceHDCosmetic());
        public ICommand ClearCosmeticCommand => new RelayCommand(param =>  ClearCosmetic());
        public ICommand ClearHDCosmeticCommand => new RelayCommand(param => ClearHDCosmetic());
        public ICommand AddCosmeticOptionCommand => new RelayCommand(param => AddCosmeticOption());
        public ICommand AddStyleCommand => new RelayCommand(param => AddStyle());
        public ICommand RemoveStyleCommand => new RelayCommand(param => RemoveStyle());

        [ImportingConstructor]
        public StageCosmeticViewModel(IDialogService dialogService, ISettingsService settingsService)
        {
            _dialogService = dialogService;
            _settingsService = settingsService;

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

        // TODO: For everywhere that uses styles and cosmetic types, we should make it so that it reads from your build settings, the game files, and any packages
        // that are loaded to determine what options to list
        [DependsUpon(nameof(SelectedCosmeticOption))]
        public List<string> Styles { get => BuildSettings?.CosmeticSettings?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style)
                .Concat(Stage?.Cosmetics?.Items?.Where(y => y.CosmeticType == SelectedCosmeticOption)?.Select(y => y.Style) ?? new List<string>()).Distinct().ToList(); }

        [DependsUpon(nameof(Styles))]
        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(nameof(SelectedStyle)); } }

        [DependsUpon(nameof(SelectedStyle))]
        [DependsUpon(nameof(Stage))]
        public List<Cosmetic> SelectedCosmetics { get => Stage?.Cosmetics?.Items.Where(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle).ToList(); }

        [DependsUpon(nameof(SelectedCosmetics))]
        public Cosmetic SelectedCosmetic { 
            get => SelectedCosmetics?.FirstOrDefault(x => !x.SelectionOption) ?? SelectedCosmetics?.FirstOrDefault();
            set 
            { 
                var cosmetic = SelectedCosmetics?.FirstOrDefault(x => !x.SelectionOption) ?? SelectedCosmetics?.FirstOrDefault();
                cosmetic = value;
                OnPropertyChanged(nameof(SelectedCosmetic));
                OnPropertyChanged(nameof(SelectedCosmetics));
            }
        }

        [DependsUpon(nameof(SelectedCosmetics))]
        public bool DisplayCosmeticSelect { get => SelectedCosmetics?.Count > 1; }

        [DependsUpon(nameof(Stage))]
        public BuildSettings BuildSettings { get => _buildSettings; set { _buildSettings = value; OnPropertyChanged(nameof(BuildSettings)); } }

        // Methods
        public void LoadStage(StageLoadedMessage message)
        {
            BuildSettings = _settingsService.BuildSettings;
            OnPropertyChanged(nameof(BuildSettings));
            var cosmeticOptions = new List<KeyValuePair<string, CosmeticType>>
            {
                CosmeticType.StagePreview.GetKeyValuePair(),
                CosmeticType.StageFranchiseIcon.GetKeyValuePair(),
                CosmeticType.StageIcon.GetKeyValuePair()
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
            var oldCosmetic = SelectedCosmetic;
            if (cosmetic != null)
            {
                // Only set SelectedCosmetic to an option if it actually exists
                if (oldCosmetic != null && oldCosmetic.CosmeticType == cosmetic.CosmeticType && oldCosmetic.Style == cosmetic.Style)
                {
                    oldCosmetic.SelectionOption = true;
                    // Undo changes to cosmetic
                    Stage.Cosmetics.ChangedItems.Remove(oldCosmetic);
                }
                cosmetic.SelectionOption = false;
                // Change the cosmetic so it will save
                Stage.Cosmetics.ItemChanged(cosmetic);
                OnPropertyChanged(nameof(SelectedCosmetic));
            }
        }

        public void AddCosmeticOption()
        {
            if (SelectedCosmetic != null)
            {
                var selectedCosmetic = SelectedCosmetic;
                // TODO: Move this into a service?
                var newId = 0;
                while (Stage.Cosmetics.Items
                    .Where(x => x.Style == SelectedStyle && x.CosmeticType == SelectedCosmeticOption).Select(y => y.TextureId).ToList().Contains(newId)
                    || ReservedIds.ReservedStageCosmeticIds.Contains(newId))
                {
                    newId++;
                }
                var newCosmetic = new Cosmetic
                {
                    CosmeticType = SelectedCosmetic.CosmeticType,
                    Style = SelectedCosmetic.Style,
                    TextureId = newId,
                    SelectionOption = true
                };
                Stage.Cosmetics.Add(newCosmetic);
                OnPropertyChanged(nameof(SelectedCosmetics));
                OnPropertyChanged(nameof(SelectedCosmetic));
            }
        }

        private Cosmetic AddCosmetic()
        {
            var cosmetic = new Cosmetic
            {
                CosmeticType = SelectedCosmeticOption,
                InternalIndex = Stage.Cosmetics.Items.Where(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle)
                    .Max(x => x.InternalIndex) + 1,
                Style = SelectedStyle
            };
            Stage.Cosmetics.Add(cosmetic);
            return cosmetic;
        }

        private void AddStyle()
        {
            var styleName = _dialogService.OpenStringInputDialog("Style Name Input", "Enter the name for your new style");
            if (styleName != null && !Stage.Cosmetics.Items.Any(x => x.Style == styleName && x.CosmeticType == SelectedCosmeticOption))
            {
                var cosmetic = new Cosmetic
                {
                    CosmeticType = SelectedCosmeticOption,
                    Style = styleName,
                    // TODO: How to handle selectable cosmetics?
                    SelectionOption = false
                };
                Stage.Cosmetics.Add(cosmetic);
                OnPropertyChanged(nameof(Styles));
                OnPropertyChanged(nameof(Stage));
            }
        }

        private void RemoveStyle()
        {
            foreach(var cosmetic in Stage.Cosmetics.Items.Where(x => x.Style == SelectedStyle && x.CosmeticType == SelectedCosmeticOption).ToList())
            {
                Stage.Cosmetics.Remove(cosmetic);
            }
            OnPropertyChanged(nameof(Styles));
            OnPropertyChanged(nameof(Stage));
        }

        public void ReplaceCosmetic()
        {
            var image = _dialogService.OpenFileDialog("Select image", "PNG image (.png)|*.png");
            if (!string.IsNullOrEmpty(image))
            {
                var bitmap = new Bitmap(image);
                if (SelectedCosmetic == null)
                {
                    SelectedCosmetic = AddCosmetic();
                }
                SelectedCosmetic.Image = bitmap.ToBitmapImage();
                SelectedCosmetic.ImagePath = image;
                SelectedCosmetic.Texture = null;
                SelectedCosmetic.Palette = null;
                Stage.Cosmetics.ItemChanged(SelectedCosmetic);
                OnPropertyChanged(nameof(SelectedCosmetic));
                OnPropertyChanged(nameof(SelectedCosmetics));
            }
        }

        public void ReplaceHDCosmetic()
        {
            var image = _dialogService.OpenFileDialog("Select HD image", "PNG image (.png)|*.png");
            if (!string.IsNullOrEmpty(image))
            {
                var bitmap = new Bitmap(image);
                if (SelectedCosmetic == null)
                {
                    SelectedCosmetic = AddCosmetic();
                }
                SelectedCosmetic.HDImage = bitmap.ToBitmapImage();
                SelectedCosmetic.HDImagePath = image;
                Stage.Cosmetics.ItemChanged(SelectedCosmetic);
                OnPropertyChanged(nameof(SelectedCosmetic));
                OnPropertyChanged(nameof(SelectedCosmetics));
            }
        }

        public void ClearCosmetic()
        {
            Stage.Cosmetics.Remove(SelectedCosmetic);
            OnPropertyChanged(nameof(SelectedCosmetic));
            OnPropertyChanged(nameof(SelectedCosmetics));
        }

        public void ClearHDCosmetic()
        {
            if (SelectedCosmetic.Image == null)
            {
                Stage.Cosmetics.Remove(SelectedCosmetic);
            }
            else
            {
                SelectedCosmetic.HDImage = null;
                SelectedCosmetic.HDImagePath = "";
                Stage.Cosmetics.ItemChanged(SelectedCosmetic);
            }
            OnPropertyChanged(nameof(SelectedCosmetic));
            OnPropertyChanged(nameof(SelectedCosmetics));
        }
    }
}
