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
        IFileService _fileService { get; }

        // Commands
        public ICommand ReplaceCosmeticCommand => new RelayCommand(param => ReplaceCosmetic());
        public ICommand ReplaceHDCosmeticCommand => new RelayCommand(param => ReplaceHDCosmetic());
        public ICommand ClearCosmeticCommand => new RelayCommand(param =>  ClearCosmetic());
        public ICommand ClearHDCosmeticCommand => new RelayCommand(param => ClearHDCosmetic());
        public ICommand AddCosmeticOptionCommand => new RelayCommand(param => AddCosmeticOption());
        public ICommand AddStyleCommand => new RelayCommand(param => AddStyle());
        public ICommand RemoveStyleCommand => new RelayCommand(param => RemoveStyle());

        [ImportingConstructor]
        public StageCosmeticViewModel(IDialogService dialogService, ISettingsService settingsService, IFileService fileService)
        {
            _dialogService = dialogService;
            _settingsService = settingsService;
            _fileService = fileService;

            WeakReferenceMessenger.Default.Register<StageLoadedMessage>(this, (recipient, message) =>
            {
                LoadStage(message);
            });
        }

        // Properties
        public StageInfo Stage { get => _stage; set { _stage = value; OnPropertyChanged(nameof(Stage)); } }

        [DependsUpon(nameof(Stage))]
        public List<KeyValuePair<string, CosmeticType>> CosmeticOptions { get => DefaultCosmetics.DefaultStageCosmetics.Select(x => x.CosmeticType.GetKeyValuePair()).Distinct().ToList(); }

        [DependsUpon(nameof(CosmeticOptions))]
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(nameof(SelectedCosmeticOption)); } }

        // Combines defaults, build settings, and styles found in loaded cosmetics to get all available styles
        [DependsUpon(nameof(SelectedCosmeticOption))]
        public List<string> Styles { get => DefaultCosmetics.DefaultStageCosmetics?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style)
                .Concat(BuildSettings?.CosmeticSettings?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style) ?? new List<string>())
                .Concat(Stage?.Cosmetics?.Items?.Where(y => y.CosmeticType == SelectedCosmeticOption)?.Select(y => y.Style) ?? new List<string>()).Distinct().ToList(); }

        [DependsUpon(nameof(Styles))]
        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(nameof(SelectedStyle)); } }

        [DependsUpon(nameof(SelectedStyle))]
        [DependsUpon(nameof(Stage))]
        [DependsUpon(nameof(SelectedCosmeticOption))]
        public List<Cosmetic> SelectedCosmetics { get => Stage?.Cosmetics?.Items.Where(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle).ToList(); }

        [DependsUpon(nameof(SelectedCosmetics))]
        public Cosmetic SelectedCosmetic { 
            get => SelectedCosmetics?.FirstOrDefault(x => !x.SelectionOption) ?? SelectedCosmetics?.FirstOrDefault();
            set 
            {
                var selectedCosmetic = SelectedCosmetic;
                // Mark other cosmetics as unselected
                SelectedCosmetics.Where(x => x != selectedCosmetic).ToList().ForEach(x => x.SelectionOption = true);
                // If this is a new cosmetic, add it to list
                if (value != null && Stage?.Cosmetics?.Items?.Contains(value) != true)
                {
                    Stage.Cosmetics.Items.Add(value);
                }
                if (value != null)
                {
                    if (value != selectedCosmetic)
                    {
                        // If we changed cosmetics, deselect the current cosmetic and undo changes to it
                        selectedCosmetic.SelectionOption = true;
                        Stage.Cosmetics.ChangedItems.Remove(selectedCosmetic);
                        // Select the new cosmetic and apply changes
                        value.SelectionOption = false;
                        Stage.Cosmetics.ItemChanged(value);
                    }
                }
                OnPropertyChanged(nameof(SelectedCosmetic));
            }
        }

        [DependsUpon(nameof(SelectedCosmetics))]
        public bool DisplayCosmeticSelect { get => SelectedCosmetics?.Count > 1; }

        [DependsUpon(nameof(Stage))]
        public BuildSettings BuildSettings { get => _buildSettings; set { _buildSettings = value; OnPropertyChanged(nameof(BuildSettings)); } }

        // Methods
        // TODO: New stages still need to load the selectable cosmetic lists. If we're loading a stage package, it would be like a new stage with other stuff
        // pre-loaded, so it would also load the lists.
        public void LoadStage(StageLoadedMessage message)
        {
            BuildSettings = _settingsService.BuildSettings;
            OnPropertyChanged(nameof(BuildSettings));
            OnPropertyChanged(nameof(CosmeticOptions));
            Stage = message.Value.Stage;
            OnPropertyChanged(nameof(Stage));
            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;
            OnPropertyChanged(nameof(SelectedCosmeticOption));
            SelectedStyle = Styles.FirstOrDefault(x => Stage.Cosmetics.Items.Select(y => y.Style).Contains(x)) ?? Styles.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedStyle));
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
                SelectedCosmetic = newCosmetic;
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
                    // Added styles are never selectable because there's no point, why would you add multiple selection options when you 
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
                var bitmap = _fileService.LoadImage(image);
                if (SelectedCosmetic == null)
                {
                    SelectedCosmetic = AddCosmetic();
                }
                SelectedCosmetic.Image = bitmap;
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
                var bitmap = _fileService.LoadImage(image);
                if (SelectedCosmetic == null)
                {
                    SelectedCosmetic = AddCosmetic();
                }
                SelectedCosmetic.HDImage = bitmap;
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
