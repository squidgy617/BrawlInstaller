using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using BrawlLib.Internal;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections;
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
        ObservableCollection<string> PacFiles { get; }
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
        ICommand CosmeticUpCommand { get; }
        ICommand CosmeticDownCommand { get; }
        ICommand AddCostumeCommand { get; }
        ICommand AddPacFilesCommand { get; }
    }

    [Export(typeof(ICostumeViewModel))]
    internal class CostumeViewModel : ViewModelBase, ICostumeViewModel
    {
        // Private properties
        private FighterPackage _fighterPackage;
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
        public ICommand ReplaceCosmeticCommand => new RelayCommand(param => ReplaceCosmetic());
        public ICommand ReplaceHDCosmeticCommand => new RelayCommand(param => ReplaceHDCosmetic());
        public ICommand CostumeUpCommand => new RelayCommand(param => MoveCostumeUp());
        public ICommand CostumeDownCommand => new RelayCommand(param => MoveCostumeDown());
        public ICommand UpdateSharesDataCommand => new RelayCommand(param => UpdateSharesData(param));
        public ICommand CosmeticUpCommand => new RelayCommand(param => MoveCosmeticUp());
        public ICommand CosmeticDownCommand => new RelayCommand(param => MoveCosmeticDown());
        public ICommand AddCostumeCommand => new RelayCommand(param => AddCostume());
        public ICommand AddPacFilesCommand => new RelayCommand(param => AddPacFiles());

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
        public FighterPackage FighterPackage { get => _fighterPackage; set { _fighterPackage = value; OnPropertyChanged(); } }
        public ObservableCollection<Costume> Costumes { get => _costumes; set { _costumes = value; OnPropertyChanged(); OnPropertyChanged(nameof(CosmeticOptions)); OnPropertyChanged(nameof(Styles)); OnPropertyChanged(nameof(CosmeticList)); } }
        public Costume SelectedCostume { get => _selectedCostume; set { _selectedCostume = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCosmetic)); OnPropertyChanged(nameof(SelectedCosmeticOption)); OnPropertyChanged(nameof(Styles)); OnPropertyChanged(nameof(PacFiles)); } }
        public ObservableCollection<string> PacFiles { get => SelectedCostume != null ? new ObservableCollection<string>(SelectedCostume?.PacFiles) : new ObservableCollection<string>(); }
        public ObservableCollection<KeyValuePair<string, CosmeticType>> CosmeticOptions { get => _cosmeticOptions; set { _cosmeticOptions = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCosmeticOption)); } }
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCosmetic)); OnPropertyChanged(nameof(Styles)); OnPropertyChanged(nameof(CosmeticList)); } }
        public Cosmetic SelectedCosmetic { get => SelectedCostume?.Cosmetics?.FirstOrDefault(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle); }
        public List<string> Styles { get => Costumes?.SelectMany(x => x.Cosmetics)?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style).Distinct().ToList(); }
        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCosmetic)); OnPropertyChanged(nameof(CosmeticList)); } }
        public List<BrawlExColorID> Colors { get => _colors; set { _colors = value; OnPropertyChanged(); } }
        public List<Cosmetic> CosmeticList { get => Costumes?.SelectMany(x => x.Cosmetics).OrderBy(x => x.InternalIndex)
                .Where(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle).ToList(); }
        public Cosmetic SelectedCosmeticNode { get => _selectedCosmeticNode; set { _selectedCosmeticNode = value; OnPropertyChanged(); } }

        // Methods
        public void LoadCostumes(FighterLoadedMessage message)
        {
            FighterPackage = message.Value;
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

        public void AddCosmetic()
        {
            var cosmetic = new Cosmetic
            {
                CostumeIndex = Costumes.IndexOf(SelectedCostume) + 1,
                CosmeticType = SelectedCosmeticOption,
                Style = SelectedStyle,
                HasChanged = true
            };
            SelectedCostume.Cosmetics.Add(cosmetic);
            FighterPackage.Cosmetics.Add(cosmetic);
        }

        public void AddPacFiles()
        {
            var files = _dialogService.OpenMultiFileDialog("Select pac files", "PAC files (.pac)|*.pac");
            SelectedCostume.PacFiles.AddRange(files);
            OnPropertyChanged(nameof(PacFiles));
        }

        public void ReplaceCosmetic()
        {
            var image = _dialogService.OpenFileDialog("Select an image", "PNG images (.png)|*.png");
            // Update the image
            if (image != "")
            {
                var bitmap = new Bitmap(image);
                if (SelectedCosmetic == null)
                    AddCosmetic();
                SelectedCosmetic.Image = bitmap.ToBitmapImage();
                SelectedCosmetic.ImagePath = image;
                SelectedCosmetic.Texture = null;
                SelectedCosmetic.Palette = null;
                SelectedCosmetic.SharesData = false;
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

        public void ReplaceHDCosmetic()
        {
            var image = _dialogService.OpenFileDialog("Select an image", "PNG images (.png)|*.png");
            // Update the image
            if (!string.IsNullOrEmpty(image))
            {
                var bitmap = new Bitmap(image);
                if (SelectedCosmetic == null)
                    AddCosmetic();
                SelectedCosmetic.HDImage = bitmap.ToBitmapImage();
                SelectedCosmetic.HDImagePath = image;
                SelectedCosmetic.HasChanged = true;
                OnPropertyChanged(nameof(SelectedCosmetic));
                OnPropertyChanged(nameof(CosmeticList));
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

        public void MoveCosmeticUp()
        {
            // Swap internal index with cosmetic below
            if (SelectedCosmeticNode.InternalIndex > 0)
            {
                var cosmetic = CosmeticList.FirstOrDefault(x => x.InternalIndex == SelectedCosmeticNode.InternalIndex - 1);
                if (cosmetic != null)
                    cosmetic.InternalIndex++;
                SelectedCosmeticNode.InternalIndex = SelectedCosmeticNode.InternalIndex - 1;
            }
            OnPropertyChanged(nameof(CosmeticList));
            OnPropertyChanged(nameof(SelectedCosmeticNode));
        }

        public void MoveCosmeticDown()
        {
            // Swap internal index with cosmetic below
            if (SelectedCosmeticNode.InternalIndex < CosmeticList.Max(x => x.InternalIndex))
            {
                var cosmetic = CosmeticList.FirstOrDefault(x => x.InternalIndex == SelectedCosmeticNode.InternalIndex + 1);
                if (cosmetic != null)
                    cosmetic.InternalIndex--;
                SelectedCosmeticNode.InternalIndex = SelectedCosmeticNode.InternalIndex + 1;
            }
            OnPropertyChanged(nameof(CosmeticList));
            OnPropertyChanged(nameof(SelectedCosmeticNode));
        }

        public void UpdateSharesData(object selectedItems)
        {
            var nodes = ((IEnumerable)selectedItems).Cast<Cosmetic>().ToList();
            foreach (var item in nodes)
            {
                item.SharesData = !item.SharesData;
                item.ColorSmashChanged = true;
                item.HasChanged = true;
            }
            OnPropertyChanged(nameof(CosmeticList));
            OnPropertyChanged(nameof(SelectedCosmeticNode));
        }

        public void AddCostume()
        {
            var costumeId = 0;
            while (Costumes.Select(x => x.CostumeId).Contains(costumeId))
                costumeId++;
            var newCostume = new Costume
            {
                Color = 0x00,
                CostumeId = costumeId,
                PacFiles = new List<string>(),
                Cosmetics = new List<Cosmetic>()
            };
            Costumes.Add(newCostume);
            FighterPackage.Costumes.Add(newCostume);
            SelectedCostume = newCostume;
        }
    }
}
