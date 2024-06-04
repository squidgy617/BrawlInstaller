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
        int? CostumeId { get; set; }
        ObservableCollection<string> PacFiles { get; }
        string SelectedPacFile { get; set; }
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
        ICommand RemovePacFileCommand { get; }
    }

    [Export(typeof(ICostumeViewModel))]
    internal class CostumeViewModel : ViewModelBase, ICostumeViewModel
    {
        // Private properties
        private FighterPackage _fighterPackage;
        private ObservableCollection<Costume> _costumes;
        private Costume _selectedCostume;
        private string _selectedPacFile;
        private ObservableCollection<KeyValuePair<string, CosmeticType>> _cosmeticOptions;
        private CosmeticType _selectedCosmeticOption;
        private string _selectedStyle;
        private List<BrawlExColorID> _colors;
        private Cosmetic _selectedCosmeticNode;

        // Services
        ISettingsService _settingsService { get; }
        IDialogService _dialogService { get; }
        ICosmeticService _cosmeticService { get; }

        // Commands
        public ICommand ReplaceCosmeticCommand => new RelayCommand(param => ReplaceCosmetic(param));
        public ICommand ReplaceHDCosmeticCommand => new RelayCommand(param => ReplaceHDCosmetic(param));
        public ICommand CostumeUpCommand => new RelayCommand(param => MoveCostumeUp());
        public ICommand CostumeDownCommand => new RelayCommand(param => MoveCostumeDown());
        public ICommand UpdateSharesDataCommand => new RelayCommand(param => UpdateSharesData(param));
        public ICommand CosmeticUpCommand => new RelayCommand(param => MoveCosmeticUp());
        public ICommand CosmeticDownCommand => new RelayCommand(param => MoveCosmeticDown());
        public ICommand AddCostumeCommand => new RelayCommand(param => AddCostume());
        public ICommand AddPacFilesCommand => new RelayCommand(param => AddPacFiles());
        public ICommand RemovePacFileCommand => new RelayCommand(param => RemovePacFile());

        // Importing constructor
        [ImportingConstructor]
        public CostumeViewModel(ISettingsService settingsService, IDialogService dialogService, ICosmeticService cosmeticService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _cosmeticService = cosmeticService;

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
        public FighterPackage FighterPackage { get => _fighterPackage; set { _fighterPackage = value; OnPropertyChanged(nameof(FighterPackage)); } }
        
        public ObservableCollection<Costume> Costumes { get => _costumes; set { _costumes = value; OnPropertyChanged(nameof(Costumes)); } }
        
        public Costume SelectedCostume { get => _selectedCostume; set { _selectedCostume = value; OnPropertyChanged(nameof(SelectedCostume)); } }
        
        [DependsUpon(nameof(SelectedCostume))]
        public int? CostumeId { get => SelectedCostume?.CostumeId; set { UpdateCostumeId(value); OnPropertyChanged(nameof(CostumeId)); } }

        [DependsUpon(nameof(SelectedCostume))]
        public ObservableCollection<string> PacFiles { get => SelectedCostume != null ? new ObservableCollection<string>(SelectedCostume?.PacFiles) : new ObservableCollection<string>(); }
        
        public string SelectedPacFile { get => _selectedPacFile; set { _selectedPacFile = value; OnPropertyChanged(nameof(SelectedPacFile)); } }

        [DependsUpon(nameof(Costumes))]
        public ObservableCollection<KeyValuePair<string, CosmeticType>> CosmeticOptions { get => _cosmeticOptions; set { _cosmeticOptions = value; OnPropertyChanged(nameof(CosmeticOptions)); } }

        [DependsUpon(nameof(SelectedCostume))]
        [DependsUpon(nameof(CosmeticOptions))]
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(nameof(SelectedCosmeticOption)); } }

        [DependsUpon(nameof(SelectedCostume))]
        [DependsUpon(nameof(SelectedCosmeticOption))]
        [DependsUpon(nameof(SelectedStyle))]
        public Cosmetic SelectedCosmetic { get => SelectedCostume?.Cosmetics?.FirstOrDefault(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle); }

        [DependsUpon(nameof(Costumes))]
        [DependsUpon(nameof(SelectedCostume))]
        [DependsUpon(nameof(SelectedCosmeticOption))]
        public List<string> Styles { get => Costumes?.SelectMany(x => x.Cosmetics)?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style).Distinct().ToList(); }
        
        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(nameof(SelectedStyle)); } }
        
        public List<BrawlExColorID> Colors { get => _colors; set { _colors = value; OnPropertyChanged(nameof(Colors)); } }

        [DependsUpon(nameof(Costumes))]
        [DependsUpon(nameof(SelectedCosmeticOption))]
        [DependsUpon(nameof(SelectedStyle))]
        public List<Cosmetic> CosmeticList { get => Costumes?.SelectMany(x => x.Cosmetics).OrderBy(x => x.InternalIndex)
                .Where(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle).ToList(); }
        
        public Cosmetic SelectedCosmeticNode { get => _selectedCosmeticNode; set { _selectedCosmeticNode = value; OnPropertyChanged(nameof(SelectedCosmeticNode)); } }

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

        public Cosmetic AddCosmetic(Costume costume)
        {
            var cosmetic = new Cosmetic
            {
                CostumeIndex = Costumes.IndexOf(costume) + 1,
                CosmeticType = SelectedCosmeticOption,
                InternalIndex = Costumes.SelectMany(x => x.Cosmetics).Where(x => x.CosmeticType == SelectedCosmeticOption && x.Style == SelectedStyle)
                    .Max(x => x.InternalIndex) + 1,
                Style = SelectedStyle,
                HasChanged = true
            };
            costume.Cosmetics.Add(cosmetic);
            FighterPackage.Cosmetics.Add(cosmetic);
            return cosmetic;
        }

        public void AddPacFiles()
        {
            var files = _dialogService.OpenMultiFileDialog("Select pac files", "PAC files (.pac)|*.pac");
            SelectedCostume.PacFiles.AddRange(files);
            OnPropertyChanged(nameof(PacFiles));
        }

        public void RemovePacFile()
        {
            SelectedCostume.PacFiles.Remove(SelectedPacFile);
            OnPropertyChanged(nameof(PacFiles));
        }

        public void ReplaceCosmetic(object selectedItems)
        {
            var costumes = ((IEnumerable)selectedItems).Cast<Costume>().OrderBy(x => Costumes.IndexOf(x)).ToList();
            var images = _dialogService.OpenMultiFileDialog("Select images", "PNG images (.png)|*.png");
            if (costumes.Count != images.Count)
            {
                if (images.Count > 0)
                    _dialogService.ShowMessage("Number of images and number of costumes selected must be equal!", "Import Error", System.Windows.MessageBoxImage.Stop);
                return;
            }
            // Don't allow replacing root or last color smashed cosmetic in a group
            foreach(var costume in costumes)
            {
                var cosmetic = costume.Cosmetics.FirstOrDefault(x => x.Style == SelectedStyle && x.CosmeticType == SelectedCosmeticOption);
                var group = _cosmeticService.GetSharesDataGroups(CosmeticList).FirstOrDefault(x => x.Contains(cosmetic));
                if (group != null && ((group.Count > 1 && cosmetic?.SharesData == false) || (group.Count == 2 && cosmetic?.SharesData == true)))
                {
                    _dialogService.ShowMessage("Selected cosmetics contain either image data for a color smash group or the last color smashed texture in a group. " +
                        "Undo color smashing on the cosmetics to replace them.", "Color Smash Error", System.Windows.MessageBoxImage.Stop);
                    return;
                }
            }
            // Update the image
            foreach(var image in images)
            {
                var currentCostume = costumes[images.IndexOf(image)];
                var currentCosmetic = currentCostume.Cosmetics.FirstOrDefault(x => x.Style == SelectedStyle && x.CosmeticType == SelectedCosmeticOption);
                if (!string.IsNullOrEmpty(image))
                {
                    var bitmap = new Bitmap(image);
                    if (currentCosmetic == null)
                        currentCosmetic = AddCosmetic(currentCostume);
                    currentCosmetic.Image = bitmap.ToBitmapImage();
                    currentCosmetic.ImagePath = image;
                    currentCosmetic.Texture = null;
                    currentCosmetic.Palette = null;
                    currentCosmetic.SharesData = false;
                    MoveCosmeticToEnd(currentCosmetic);
                    currentCosmetic.HasChanged = true;
                    OnPropertyChanged(nameof(SelectedCosmetic));
                    OnPropertyChanged(nameof(CosmeticList));
                    OnPropertyChanged(nameof(SelectedCosmeticNode));
                }
            }
        }

        public void ReplaceHDCosmetic(object selectedItems)
        {
            var costumes = ((IEnumerable)selectedItems).Cast<Costume>().OrderBy(x => Costumes.IndexOf(x)).ToList();
            var images = _dialogService.OpenMultiFileDialog("Select an image", "PNG images (.png)|*.png");
            if (costumes.Count != images.Count)
            {
                if (images.Count > 0)
                    _dialogService.ShowMessage("Number of images and number of costumes selected must be equal!", "Import Error", System.Windows.MessageBoxImage.Stop);
                return;
            }
            // Update the image
            foreach (var image in images)
            {
                var currentCostume = costumes[images.IndexOf(image)];
                var currentCosmetic = currentCostume.Cosmetics.FirstOrDefault(x => x.Style == SelectedStyle && x.CosmeticType == SelectedCosmeticOption);
                if (!string.IsNullOrEmpty(image))
                {
                    var bitmap = new Bitmap(image);
                    if (currentCosmetic == null)
                        AddCosmetic(currentCostume);
                    currentCosmetic.HDImage = bitmap.ToBitmapImage();
                    currentCosmetic.HDImagePath = image;
                    currentCosmetic.HasChanged = true;
                    OnPropertyChanged(nameof(SelectedCosmetic));
                    OnPropertyChanged(nameof(CosmeticList));
                }
            }
        }

        private void MoveCostume()
        {
            if (SelectedCostume != null)
            {
                var movedCostume = SelectedCostume;
                movedCostume.Cosmetics.ForEach(x => x.HasChanged = true);
            }
        }

        public void MoveCostumeUp()
        {
            MoveCostume();
            var selectedCostume = SelectedCostume;
            Costumes.MoveUp(selectedCostume);
            FighterPackage.Costumes.MoveUp(selectedCostume);
            SelectedCostume = selectedCostume;
        }

        public void MoveCostumeDown()
        {
            MoveCostume();
            var selectedCostume = SelectedCostume;
            Costumes.MoveDown(selectedCostume);
            FighterPackage.Costumes.MoveDown(selectedCostume);
            SelectedCostume = selectedCostume;
        }

        public void MoveCosmeticUp()
        {
            // Get node groups
            var nodeGroups = _cosmeticService.GetSharesDataGroups(CosmeticList);
            var selectedNodes = new List<Cosmetic>();
            var nodesToMove = new List<Cosmetic>();
            // Select related nodes if this is the root of a color smash group
            if (SelectedCosmeticNode.SharesData == false)
                selectedNodes = nodeGroups.FirstOrDefault(x => x.Contains(SelectedCosmeticNode)) ?? selectedNodes;
            else
                selectedNodes.Add(SelectedCosmeticNode);
            // Get nodes to move past
            var prevNode = CosmeticList.FirstOrDefault(x => x.InternalIndex == selectedNodes.FirstOrDefault()?.InternalIndex - 1);
            if (SelectedCosmeticNode.SharesData == false && prevNode != null)
                nodesToMove = nodeGroups.FirstOrDefault(x => x.Contains(prevNode)) ?? nodesToMove;
            else if (prevNode != null)
                nodesToMove.Add(prevNode);
            if (selectedNodes.FirstOrDefault()?.InternalIndex > 0 
                && !(selectedNodes.LastOrDefault()?.SharesData == true && nodesToMove.LastOrDefault()?.SharesData == false))
            {
                // Move selected nodes down
                foreach (var item in selectedNodes)
                {
                    item.InternalIndex -= nodesToMove.Count;
                }
                // Move node after selected to before them
                foreach (var node in nodesToMove)
                    node.InternalIndex += selectedNodes.Count;
            }
            OnPropertyChanged(nameof(CosmeticList));
            OnPropertyChanged(nameof(SelectedCosmeticNode));
        }

        public void MoveCosmeticDown()
        {
            // Get node groups
            var nodeGroups = _cosmeticService.GetSharesDataGroups(CosmeticList);
            var selectedNodes = new List<Cosmetic>();
            var nodesToMove = new List<Cosmetic>();
            // Select related nodes if this is the root of a color smash group
            if (SelectedCosmeticNode.SharesData == false)
                selectedNodes = nodeGroups.FirstOrDefault(x => x.Contains(SelectedCosmeticNode)) ?? selectedNodes;
            else
                selectedNodes.Add(SelectedCosmeticNode);
            // Get nodes to move past
            var nextNode = CosmeticList.FirstOrDefault(x => x.InternalIndex == selectedNodes.LastOrDefault()?.InternalIndex + 1);
            if (SelectedCosmeticNode.SharesData == false && nextNode != null)
                nodesToMove = nodeGroups.FirstOrDefault(x => x.Contains(nextNode)) ?? nodesToMove;
            else if (nextNode != null)
                nodesToMove.Add(nextNode);
            if (selectedNodes.LastOrDefault()?.InternalIndex < CosmeticList.Max(x => x.InternalIndex)
                && !(selectedNodes.LastOrDefault()?.SharesData == true && nodesToMove.FirstOrDefault()?.SharesData == false))
            {
                // Move selected nodes down
                foreach (var item in selectedNodes)
                {
                    item.InternalIndex += nodesToMove.Count;
                }
                // Move node after selected to before them
                foreach(var node in nodesToMove)
                    node.InternalIndex -= selectedNodes.Count;
            }
            OnPropertyChanged(nameof(CosmeticList));
            OnPropertyChanged(nameof(SelectedCosmeticNode));
        }

        private void MoveCosmeticToEnd(Cosmetic selectedCosmetic)
        {
            var cosmeticsToMove = CosmeticList.Where(x => x.InternalIndex > selectedCosmetic.InternalIndex);
            if (cosmeticsToMove.Any())
            {
                // Decrement internal indexes of all cosmetics after this one
                foreach (var cosmetic in CosmeticList.Where(x => x.InternalIndex > selectedCosmetic.InternalIndex))
                {
                    cosmetic.InternalIndex -= 1;
                }
                // Put this image at the end
                selectedCosmetic.InternalIndex = CosmeticList.Max(x => x.InternalIndex) + 1;
                selectedCosmetic.HasChanged = true;
            }
        }

        private bool ValidateSharesData(List<Cosmetic> nodes)
        {
            var startingNode = nodes.FirstOrDefault();
            var index = startingNode?.InternalIndex;
            var sharesData = startingNode?.SharesData;
            var groupCount = 0;
            // Trying to color smash a single node, not valid
            if (nodes.Count == 1 && startingNode.SharesData == false)
                return false;
            foreach(var node in nodes)
            {
                // If nodes are not sequential, not valid
                if (index != node.InternalIndex)
                    return false;
                index++;
                // If nodes from more than one shared data group are selected, not valid
                if (node.SharesData != sharesData)
                    groupCount++;
                sharesData = node.SharesData;
                if (groupCount > 1)
                    return false;
            }
            // If first node is from a different group, not valid
            if (startingNode.SharesData == false && sharesData == true)
                return false;
            return true;
        }


        public void UpdateSharesData(object selectedItems)
        {
            var nodes = ((IEnumerable)selectedItems).Cast<Cosmetic>().OrderBy(x => x.InternalIndex).ToList();
            // Check if operation can be performed
            var valid = ValidateSharesData(nodes);
            if (!valid)
            {
                _dialogService.ShowMessage("Color smashing could not be changed. If color smashing, ensure more than one cosmetic is selected. " +
                    "If undoing color smashing, all cosmetics must be from the same color smash group. All selected cosmetics must be sequential.", "Color Smash Error",
                    System.Windows.MessageBoxImage.Stop);
                return;
            }
            // Get groups
            var nodeGroups = _cosmeticService.GetSharesDataGroups(CosmeticList);
            var nodeGroup = nodeGroups.FirstOrDefault(x => x.Contains(nodes.LastOrDefault())) ?? nodes;
            // If a group's root is selected, the entire group will be affected
            var mixedGroup = nodes.Any(x => x.SharesData == false) && nodes.Any(x => x.SharesData == true);
            if (mixedGroup)
                nodes = nodeGroup;
            // If we are changing all but the root, root will be affected too
            if (!mixedGroup && nodeGroup.Count == nodes.Count + 1)
                nodes.Add(CosmeticList.FirstOrDefault(x => x.InternalIndex == nodes.LastOrDefault()?.InternalIndex + 1));
            var moveToEnd = nodes.Any(x => x.SharesData == true);
            // Update color smashing for all nodes
            foreach (var item in nodes)
            {
                // The root will not be flipped
                if (item.SharesData == true || item != nodes.LastOrDefault())
                    item.SharesData = !item.SharesData;
                // Move all to end if unsmashing
                if (moveToEnd)
                    MoveCosmeticToEnd(item);
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
                Color = 0x0B,
                CostumeId = costumeId,
                PacFiles = new List<string>(),
                Cosmetics = new List<Cosmetic>()
            };
            Costumes.Add(newCostume);
            FighterPackage.Costumes.Add(newCostume);
            SelectedCostume = newCostume;
        }

        public void UpdateCostumeId(int? costumeId)
        {
            if (Costumes.Any(x => x.CostumeId == costumeId))
            {
                _dialogService.ShowMessage("You cannot set a costume ID to an ID that is already in use.", "Costume ID Already in Use", System.Windows.MessageBoxImage.Stop);
                return;
            }
            if (costumeId != null)
                SelectedCostume.CostumeId = (int)costumeId;
        }
    }
}
