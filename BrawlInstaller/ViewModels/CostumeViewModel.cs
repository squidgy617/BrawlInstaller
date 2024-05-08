﻿using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using BrawlLib.Internal;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BrawlInstaller.ViewModels
{
    public interface ICostumeViewModel
    {
        ObservableCollection<Costume> Costumes { get; }
        Costume SelectedCostume { get; set; }
        ObservableCollection<KeyValuePair<string, CosmeticType>> CosmeticOptions { get; }
        CosmeticType SelectedCosmeticOption { get; set; }
        Cosmetic SelectedCosmetic { get; }
        List<string> Styles { get; }
        string SelectedStyle { get; }
        List<BrawlExColorID> Colors { get; }
        List<Cosmetic> CosmeticList { get; }
        Cosmetic SelectedCosmeticNode { get; set; }
        ICommand ReplaceCosmeticCommand { get; }
        ICommand CostumeUpCommand { get; }
        ICommand CostumeDownCommand { get; }
        ICommand UpdateSharesDataCommand { get; }
    }

    [Export(typeof(ICostumeViewModel))]
    internal class CostumeViewModel : ViewModelBase, ICostumeViewModel
    {
        // Private properties
        private ObservableCollection<Costume> _costumes;
        private Costume _selectedCostume;
        private ObservableCollection<KeyValuePair<string, CosmeticType>> _cosmeticOptions;
        private CosmeticType _selectedCosmeticOption;
        private string _selectedStyle;
        private List<BrawlExColorID> _colors;
        private Cosmetic _selectedCosmeticNode;

        // Services
        ISettingsService _settingsService { get; }
        IDialogService _dialogService { get; }

        // Commands
        public ICommand ReplaceCosmeticCommand
        {
            get
            {
                return new RelayCommand(param => ReplaceCosmetic());
            }
        }

        public ICommand CostumeUpCommand
        {
            get
            {
                return new RelayCommand(param => MoveCostumeUp());
            }
        }

        public ICommand CostumeDownCommand
        {
            get
            {
                return new RelayCommand(param => MoveCostumeDown());
            }
        }

        public ICommand UpdateSharesDataCommand
        {
            get
            {
                return new RelayCommand(param => UpdateSharesData());
            }
        }

        // Importing constructor
        [ImportingConstructor]
        public CostumeViewModel(ISettingsService settingsService, IDialogService dialogService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;

            CosmeticOptions = new ObservableCollection<KeyValuePair<string, CosmeticType>>
            {
                CosmeticType.CSP.GetKeyValuePair(),
                CosmeticType.PortraitName.GetKeyValuePair(),
                CosmeticType.BP.GetKeyValuePair(),
                CosmeticType.StockIcon.GetKeyValuePair(),
                CosmeticType.CSSIcon.GetKeyValuePair(),
                CosmeticType.ReplayIcon.GetKeyValuePair()
            };

            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;

            Colors = BrawlExColorID.Colors.ToList();

            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadCostumes(message);
            });
        }

        // Properties
        public ObservableCollection<Costume> Costumes { get => _costumes; set { _costumes = value; OnPropertyChanged(); OnPropertyChanged(nameof(CosmeticOptions)); OnPropertyChanged(nameof(Styles)); OnPropertyChanged(nameof(CosmeticList)); } }
        public Costume SelectedCostume { get => _selectedCostume; set { _selectedCostume = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCosmetic)); OnPropertyChanged(nameof(SelectedCosmeticOption)); OnPropertyChanged(nameof(Styles)); } }
        public ObservableCollection<KeyValuePair<string, CosmeticType>> CosmeticOptions { get => _cosmeticOptions; set { _cosmeticOptions = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCosmeticOption)); } }
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCosmetic)); OnPropertyChanged(nameof(Styles)); OnPropertyChanged(nameof(CosmeticList)); } }
        public Cosmetic SelectedCosmetic { get => SelectedCostume?.Cosmetics?.FirstOrDefault(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle); }
        public List<string> Styles { get => Costumes?.SelectMany(x => x.Cosmetics)?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style).Distinct().ToList(); }
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
            Costumes = new ObservableCollection<Costume>(message.Value.Costumes);
            SelectedCostume = Costumes.FirstOrDefault();

            //foreach (CosmeticType option in Enum.GetValues(typeof(CosmeticType)))
            // Get build setting cosmetics that aren't already in list
            foreach (CosmeticType option in _settingsService.BuildSettings.CosmeticSettings.Where(x => x.IdType == IdType.Cosmetic 
            && !CosmeticOptions.Select(y => y.Value).Contains(x.CosmeticType)).Select(x => x.CosmeticType).Distinct())
            {
                CosmeticOptions.Add(option.GetKeyValuePair());
            }
            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;
        }

        public void ReplaceCosmetic()
        {
            var image = _dialogService.OpenFileDialog("Select an image", "PNG images (.png)|*.png");
            // Update the image
            if (image != "")
            {
                var bitmap = new Bitmap(image);
                SelectedCosmetic.Image = bitmap.ToBitmapImage();
                SelectedCosmetic.ImagePath = image;
                SelectedCosmetic.Texture = null;
                SelectedCosmetic.Palette = null;
                SelectedCosmetic.SharesData = false;
                SelectedCosmetic.ColorSmashChanged = true;
                // Decrement internal indexes of all cosmetics after this one
                foreach(var cosmetic in CosmeticList.Where(x => x.InternalIndex > SelectedCosmetic.InternalIndex))
                {
                    cosmetic.InternalIndex -= 1;
                }
                // Put this image at the end
                SelectedCosmetic.InternalIndex = CosmeticList.Max(x => x.InternalIndex) + 1;
                SelectedCosmetic.HasChanged = true;
                OnPropertyChanged(nameof(SelectedCosmetic));
                OnPropertyChanged(nameof(CosmeticList));
                OnPropertyChanged(nameof(SelectedCosmeticNode));
            }
        }

        private void MoveCostume()
        {
            var movedCostume = SelectedCostume;
            movedCostume.Cosmetics.ForEach(x => x.HasChanged = true);
        }

        public void MoveCostumeUp()
        {
            MoveCostume();
            Costumes.MoveUp(SelectedCostume);
        }

        public void MoveCostumeDown()
        {
            MoveCostume();
            Costumes.MoveDown(SelectedCostume);
        }

        public void UpdateSharesData()
        {
            SelectedCosmeticNode.SharesData = !SelectedCosmeticNode.SharesData;
            SelectedCosmeticNode.ColorSmashChanged = true;
            SelectedCosmeticNode.HasChanged = true;
            OnPropertyChanged(nameof(CosmeticList));
        }
    }
}
