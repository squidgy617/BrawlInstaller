using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using BrawlInstaller.StaticClasses;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrawlInstaller.ViewModels
{
    public interface ICosmeticViewModel
    {
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
        private CosmeticType _selectedCosmeticOption;
        private string _selectedStyle;

        // Services
        ISettingsService _settingsService { get; }
        IDialogService _dialogService { get; }
        IFileService _fileService { get; }

        // Commands
        public ICommand ReplaceCosmeticCommand => new RelayCommand(param => ReplaceCosmetic());
        public ICommand ClearCosmeticCommand => new RelayCommand(param => ClearCosmetic());
        public ICommand ReplaceHDCosmeticCommand => new RelayCommand(param => ReplaceHDCosmetic());
        public ICommand ClearHDCosmeticCommand => new RelayCommand(param => ClearHDCosmetic());
        public ICommand AddStyleCommand => new RelayCommand(param => AddStyle());
        public ICommand RemoveStyleCommand => new RelayCommand(param => RemoveStyle());

        // Importing constructor
        [ImportingConstructor]
        public CosmeticViewModel(ISettingsService settingsService, IDialogService dialogService, IFileService fileService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileService = fileService;

            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;

            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadCosmetics(message);
            });
        }

        //Properties
        public FighterPackage FighterPackage { get => _fighterPackage; set { _fighterPackage = value; OnPropertyChanged(nameof(FighterPackage)); } }

        [DependsUpon(nameof(FighterPackage))]
        public Dictionary<string, CosmeticType> CosmeticOptions { get => DefaultCosmetics.DefaultFighterCosmetics.Select(x => x.CosmeticType.GetKeyValuePair()).Distinct().ToList().ToDictionary(x => x.Key, x => x.Value); }
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(nameof(SelectedCosmeticOption)); } }

        [DependsUpon(nameof(FighterPackage))]
        [DependsUpon(nameof(SelectedStyle))]
        public Cosmetic SelectedCosmetic { get => FighterPackage?.Cosmetics?.Items.FirstOrDefault(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle); }

        [DependsUpon(nameof(FighterPackage))]
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
            FighterPackage = message.Value;
            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;
            OnPropertyChanged(nameof(FighterPackage));
        }

        public Cosmetic AddCosmetic()
        {
            var cosmetic = new Cosmetic
            {
                CosmeticType = SelectedCosmeticOption,
                Style = SelectedStyle
            };
            FighterPackage.Cosmetics.Add(cosmetic);
            return cosmetic;
        }

        public void ReplaceCosmetic()
        {
            var image = _dialogService.OpenFileDialog("Select image", "PNG images (.png)|*.png");
            // Update the image
            if (!string.IsNullOrEmpty(image))
            {
                var bitmap = _fileService.LoadImage(image);
                if (SelectedCosmetic == null)
                    AddCosmetic();
                SelectedCosmetic.Image = bitmap;
                SelectedCosmetic.ImagePath = image;
                SelectedCosmetic.Texture = null;
                SelectedCosmetic.Palette = null;
                FighterPackage.Cosmetics.ItemChanged(SelectedCosmetic);
            }
            OnPropertyChanged(nameof(SelectedCosmetic));
        }

        public void ReplaceHDCosmetic()
        {
            var image = _dialogService.OpenFileDialog("Select image", "PNG images (.png)|*.png");
            // Update the image
            if (!string.IsNullOrEmpty(image))
            {
                var bitmap = _fileService.LoadImage(image);
                if (SelectedCosmetic == null)
                    AddCosmetic();
                SelectedCosmetic.HDImage = bitmap;
                SelectedCosmetic.HDImagePath = image;
                FighterPackage.Cosmetics.ItemChanged(SelectedCosmetic);
            }
            OnPropertyChanged(nameof(SelectedCosmetic));
        }

        public void ClearCosmetic()
        {
            FighterPackage?.Cosmetics?.Remove(SelectedCosmetic);
            OnPropertyChanged(nameof(SelectedCosmetic));
        }

        public void ClearHDCosmetic()
        {
            if (SelectedCosmetic.Image == null)
            {
                FighterPackage.Cosmetics.Remove(SelectedCosmetic);
            }
            else
            {
                SelectedCosmetic.HDImage = null;
                SelectedCosmetic.HDImagePath = string.Empty;
                FighterPackage.Cosmetics.ItemChanged(SelectedCosmetic);
            }
            OnPropertyChanged(nameof(SelectedCosmetic));
        }

        private void AddStyle()
        {
            var styleName = _dialogService.OpenStringInputDialog("Style Name Input", "Enter the name for your new style");
            if (styleName != null && !FighterPackage.Cosmetics.Items.Any(x => x.Style == styleName && x.CosmeticType == SelectedCosmeticOption))
            {
                var cosmetic = new Cosmetic
                {
                    CosmeticType = SelectedCosmeticOption,
                    Style = styleName
                };
                FighterPackage.Cosmetics.Add(cosmetic);
                OnPropertyChanged(nameof(Styles));
                OnPropertyChanged(nameof(FighterPackage));
            }
        }

        private void RemoveStyle()
        {
            foreach (var cosmetic in FighterPackage.Cosmetics.Items.Where(x => x.Style == SelectedStyle && x.CosmeticType == SelectedCosmeticOption).ToList())
            {
                FighterPackage.Cosmetics.Remove(cosmetic);
            }
            OnPropertyChanged(nameof(Styles));
            OnPropertyChanged(nameof(FighterPackage));
        }
    }
}
