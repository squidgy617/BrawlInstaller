using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using BrawlInstaller.StaticClasses;
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
    // TODO: Moving costumes or changing their colors should mark them as changed
    public interface ICostumeViewModel
    {
        ObservableCollection<Costume> Costumes { get; }
        Costume SelectedCostume { get; set; }
        List<int> AvailableCostumeIds { get; }
        ObservableCollection<FighterPacFile> PacFiles { get; }
        FighterPacFile SelectedPacFile { get; set; }
        Dictionary<string, CosmeticType> CosmeticOptions { get; }
        CosmeticType SelectedCosmeticOption { get; set; }
        Cosmetic SelectedCosmetic { get; }
        List<string> Styles { get; }
        string SelectedStyle { get; }
        List<BrawlExColorID> Colors { get; }
        ObservableCollection<Cosmetic> CosmeticList { get; }
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
        private BuildSettings _buildSettings;
        private ObservableCollection<Costume> _costumes;
        private Costume _selectedCostume;
        private FighterPacFile _selectedPacFile;
        private CosmeticType _selectedCosmeticOption;
        private string _selectedStyle;
        private List<BrawlExColorID> _colors;
        private Cosmetic _selectedCosmeticNode;

        // Services
        ISettingsService _settingsService { get; }
        IDialogService _dialogService { get; }
        ICosmeticService _cosmeticService { get; }
        IFileService _fileService { get; }
        IFighterService _fighterService { get; }

        // Commands
        public ICommand ReplaceCosmeticCommand => new RelayCommand(param => ReplaceCosmetic(param));
        public ICommand ReplaceHDCosmeticCommand => new RelayCommand(param => ReplaceHDCosmetic(param));
        public ICommand RemoveCostumesCommand => new RelayCommand(param => RemoveCostumes(param));
        public ICommand ClearCosmeticCommand => new RelayCommand(param => ClearCosmetic());
        public ICommand ClearHDCosmeticCommand => new RelayCommand(param => ClearHDCosmetic());
        public ICommand CostumeUpCommand => new RelayCommand(param => MoveCostumeUp());
        public ICommand CostumeDownCommand => new RelayCommand(param => MoveCostumeDown());
        public ICommand UpdateSharesDataCommand => new RelayCommand(param => UpdateSharesData(param));
        public ICommand CosmeticUpCommand => new RelayCommand(param => MoveCosmeticUp());
        public ICommand CosmeticDownCommand => new RelayCommand(param => MoveCosmeticDown());
        public ICommand AddCostumeCommand => new RelayCommand(param => AddCostume());
        public ICommand AddPacFilesCommand => new RelayCommand(param => AddPacFiles());
        public ICommand RemovePacFileCommand => new RelayCommand(param => RemovePacFile());
        public ICommand AddStyleCommand => new RelayCommand(param => AddStyle());
        public ICommand RemoveStyleCommand => new RelayCommand(param => RemoveStyle());
        public ICommand ViewTexturesCommand => new RelayCommand(param => ViewTextures());

        // Importing constructor
        [ImportingConstructor]
        public CostumeViewModel(ISettingsService settingsService, IDialogService dialogService, ICosmeticService cosmeticService, IFileService fileService, IFighterService fighterService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _cosmeticService = cosmeticService;
            _fileService = fileService;
            _fighterService = fighterService;
            _fileService = fileService;

            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;

            Colors = BrawlExColorID.Colors.ToList();

            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadCostumes(message);
            });

            WeakReferenceMessenger.Default.Register<AttributesUpdatedMessage>(this, (recipient, message) =>
            {
                OnPropertyChanged(nameof(CostumeEditorEnabled));
            });
        }

        // Properties
        public FighterPackage FighterPackage { get => _fighterPackage; set { _fighterPackage = value; OnPropertyChanged(nameof(FighterPackage)); } }

        [DependsUpon(nameof(FighterPackage))]
        public BuildSettings BuildSettings { get => _buildSettings; set { _buildSettings = value; OnPropertyChanged(nameof(BuildSettings)); } }

        [DependsUpon(nameof(FighterPackage))]
        public ObservableCollection<Costume> Costumes { get => FighterPackage != null ? new ObservableCollection<Costume>(FighterPackage.Costumes) : new ObservableCollection<Costume>(); }

        [DependsUpon(nameof(Costumes))]
        public Costume SelectedCostume { get => _selectedCostume; set { _selectedCostume = value; OnPropertyChanged(nameof(SelectedCostume)); } }

        [DependsUpon(nameof(SelectedCostume))]
        [DependsUpon(nameof(Costumes))]
        public List<int> AvailableCostumeIds { get => GetCostumeIds(); }

        [DependsUpon(nameof(SelectedCostume))]
        public ObservableCollection<FighterPacFile> PacFiles { get => SelectedCostume != null ? new ObservableCollection<FighterPacFile>(SelectedCostume?.PacFiles) : new ObservableCollection<FighterPacFile>(); }
        
        public FighterPacFile SelectedPacFile { get => _selectedPacFile; set { _selectedPacFile = value; OnPropertyChanged(nameof(SelectedPacFile)); } }

        [DependsUpon(nameof(Costumes))]
        public Dictionary<string, CosmeticType> CosmeticOptions { get => DefaultCosmetics.DefaultCostumeCosmetics.Select(x => x.CosmeticType.GetKeyValuePair()).Distinct().ToList().ToDictionary(x => x.Key, x => x.Value); }

        [DependsUpon(nameof(SelectedCostume))]
        [DependsUpon(nameof(CosmeticOptions))]
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(nameof(SelectedCosmeticOption)); } }

        [DependsUpon(nameof(SelectedCostume))]
        [DependsUpon(nameof(SelectedCosmeticOption))]
        [DependsUpon(nameof(SelectedStyle))]
        [DependsUpon(nameof(InheritedStyle))]
        public Cosmetic SelectedCosmetic { get => SelectedCostume?.Cosmetics?.FirstOrDefault(x => x.CosmeticType == SelectedCosmeticOption && x.Style == InheritedStyle); }

        [DependsUpon(nameof(Costumes))]
        [DependsUpon(nameof(SelectedCostume))]
        [DependsUpon(nameof(SelectedCosmeticOption))]
        public List<string> Styles
        {
            get => DefaultCosmetics.DefaultCostumeCosmetics?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style)
                .Concat(BuildSettings?.CosmeticSettings?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style) ?? new List<string>())
                .Concat(FighterPackage?.Cosmetics?.Items?.Where(y => y.CosmeticType == SelectedCosmeticOption)?.Select(y => y.Style) ?? new List<string>()).Distinct().ToList();
        }

        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(nameof(SelectedStyle)); } }
        
        public List<BrawlExColorID> Colors { get => _colors; set { _colors = value; OnPropertyChanged(nameof(Colors)); } }

        [DependsUpon(nameof(Costumes))]
        [DependsUpon(nameof(SelectedCosmeticOption))]
        [DependsUpon(nameof(SelectedStyle))]
        [DependsUpon(nameof(InheritedStyle))]
        public ObservableCollection<Cosmetic> CosmeticList { get => new ObservableCollection<Cosmetic>(Costumes?.SelectMany(x => x.Cosmetics).OrderBy(x => x.InternalIndex)
                .Where(x => x.CosmeticType == SelectedCosmeticOption && x.Style == InheritedStyle)); }
        
        public Cosmetic SelectedCosmeticNode { get => _selectedCosmeticNode; set { _selectedCosmeticNode = value; OnPropertyChanged(nameof(SelectedCosmeticNode)); } }

        [DependsUpon(nameof(SelectedCosmeticNode))]
        public string ColorSmashText { get => 
                SelectedCosmeticNode != null &&
                (
                    SelectedCosmeticNode.SharesData == true || 
                    (
                        CosmeticList.IndexOf(SelectedCosmeticNode) > 0 && CosmeticList[CosmeticList.IndexOf(SelectedCosmeticNode) - 1].SharesData == true
                    )
                )
                ? "Undo Color Smash" : "Color Smash";
        }

        [DependsUpon(nameof(SelectedStyle))]
        [DependsUpon(nameof(Styles))]
        [DependsUpon(nameof(FighterPackage))]
        public string InheritedStyle
        {
            get => FighterPackage?.Cosmetics?.InheritedStyles?.ContainsKey((SelectedCosmeticOption, SelectedStyle)) == true
                ? FighterPackage.Cosmetics.InheritedStyles.FirstOrDefault(x => x.Key == (SelectedCosmeticOption, SelectedStyle)).Value : SelectedStyle;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var matchKey = (SelectedCosmeticOption, SelectedStyle);
                    if (FighterPackage?.Cosmetics?.InheritedStyles?.Any(x => x.Key == matchKey) == true)
                    {
                        var match = FighterPackage.Cosmetics.InheritedStyles.FirstOrDefault(x => x.Key == (SelectedCosmeticOption, SelectedStyle));
                        FighterPackage.Cosmetics.InheritedStyles.Remove(matchKey);
                    }
                    FighterPackage?.Cosmetics?.InheritedStyles.Add(matchKey, value);
                    OnPropertyChanged(nameof(InheritedStyle));
                }
            }
        }

        [DependsUpon(nameof(FighterPackage))]
        public bool CostumeEditorEnabled { get => !string.IsNullOrEmpty(FighterPackage?.FighterInfo?.Masquerade) || FighterPackage?.FighterInfo?.CSSSlotAttributes != null; }

        // Methods
        public void LoadCostumes(FighterLoadedMessage message)
        {
            FighterPackage = message.Value;
            BuildSettings = _settingsService.BuildSettings;
            SelectedCostume = Costumes.FirstOrDefault();

            SelectedCosmeticOption = CosmeticOptions.FirstOrDefault().Value;
            OnPropertyChanged(nameof(FighterPackage));
            OnPropertyChanged(nameof(BuildSettings));
            OnPropertyChanged(nameof(Costumes));
            OnPropertyChanged(nameof(SelectedCostume));
            OnPropertyChanged(nameof(SelectedCosmeticOption));
        }

        public Cosmetic AddCosmetic(Costume costume)
        {
            var cosmetic = new Cosmetic
            {
                CostumeIndex = Costumes.IndexOf(costume) + 1,
                CosmeticType = SelectedCosmeticOption,
                InternalIndex = Costumes.SelectMany(x => x.Cosmetics).Where(x => x.CosmeticType == SelectedCosmeticOption && x.Style == InheritedStyle)
                    .Max(x => x.InternalIndex) + 1,
                Style = InheritedStyle
            };
            costume.Cosmetics.Add(cosmetic);
            FighterPackage.Cosmetics.Add(cosmetic);
            return cosmetic;
        }

        public void AddPacFiles()
        {
            var files = _dialogService.OpenMultiFileDialog("Select pac files", "PAC files (.pac)|*.pac");
            foreach(var file in files)
            {
                var pacFile = new FighterPacFile { FilePath = file };
                pacFile = _fighterService.GetFighterPacName(pacFile, FighterPackage.FighterInfo);
                pacFile.Subdirectory = string.Empty;
                SelectedCostume.PacFiles.Add(pacFile);
            }
            OnPropertyChanged(nameof(PacFiles));
        }

        public void RemovePacFile()
        {
            SelectedCostume.PacFiles.Remove(SelectedPacFile);
            OnPropertyChanged(nameof(PacFiles));
        }

        private void FlipColorSmashedCosmetics(Cosmetic cosmetic)
        {
            var group = _cosmeticService.GetSharesDataGroups(CosmeticList.ToList()).FirstOrDefault(x => x.Contains(cosmetic));
            if (group != null && ((group.Count > 1 && cosmetic?.SharesData == false) || (group.Count == 2 && cosmetic?.SharesData == true)))
            {
                group.ForEach(x => { x.SharesData = false; x.ColorSmashChanged = true; MoveCosmeticToEnd(x); });
            }
            else
            {
                if (cosmetic.SharesData == true)
                {
                    MoveCosmeticToEnd(cosmetic);
                }
                cosmetic.SharesData = false;
                cosmetic.ColorSmashChanged = true;
            }
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
            // Update the image
            foreach(var image in images)
            {
                var currentCostume = costumes[images.IndexOf(image)];
                var currentCosmetic = currentCostume.Cosmetics.FirstOrDefault(x => x.Style == InheritedStyle && x.CosmeticType == SelectedCosmeticOption);
                if (!string.IsNullOrEmpty(image))
                {
                    var bitmap = _fileService.LoadImage(image);
                    if (currentCosmetic == null)
                        currentCosmetic = AddCosmetic(currentCostume);
                    currentCosmetic.Image = bitmap;
                    currentCosmetic.ImagePath = image;
                    currentCosmetic.Texture = null;
                    currentCosmetic.Palette = null;
                    FighterPackage.Cosmetics.ItemChanged(currentCosmetic);
                }
            }
            // Adjust color smashing based on cosmetics replaced
            foreach (var costume in costumes)
            {
                var cosmetic = costume.Cosmetics.FirstOrDefault(x => x.Style == InheritedStyle && x.CosmeticType == SelectedCosmeticOption);
                FlipColorSmashedCosmetics(cosmetic);
            }
            OnPropertyChanged(nameof(SelectedCosmetic));
            OnPropertyChanged(nameof(CosmeticList));
            OnPropertyChanged(nameof(SelectedCosmeticNode));
        }

        // TODO: Don't allow adding HD cosmetics if there is no corresponding regular texture?
        // Or some validation when trying to save with HD textures with no match
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
                var currentCosmetic = currentCostume.Cosmetics.FirstOrDefault(x => x.Style == InheritedStyle && x.CosmeticType == SelectedCosmeticOption);
                if (!string.IsNullOrEmpty(image))
                {
                    var bitmap = _fileService.LoadImage(image);
                    if (currentCosmetic == null)
                        currentCosmetic = AddCosmetic(currentCostume);
                    currentCosmetic.HDImage = bitmap;
                    currentCosmetic.HDImagePath = image;
                    FighterPackage.Cosmetics.ItemChanged(currentCosmetic);
                }
            }
            OnPropertyChanged(nameof(SelectedCosmetic));
            OnPropertyChanged(nameof(CosmeticList));
        }

        public void ClearCosmetic()
        {
            FlipColorSmashedCosmetics(SelectedCosmetic);
            FighterPackage.Cosmetics.Remove(SelectedCosmetic);
            SelectedCostume.Cosmetics.Remove(SelectedCosmetic);
            OnPropertyChanged(nameof(SelectedCosmetic));
            OnPropertyChanged(nameof(CosmeticList));
            OnPropertyChanged(nameof(SelectedCosmeticNode));
        }

        public void ClearHDCosmetic()
        {
            if (SelectedCosmetic.Image == null)
            {
                FighterPackage.Cosmetics.Remove(SelectedCosmetic);
                SelectedCostume.Cosmetics.Remove(SelectedCosmetic);
            }
            else
            {
                SelectedCosmetic.HDImage = null;
                SelectedCosmetic.HDImagePath = "";
                FighterPackage.Cosmetics.ItemChanged(SelectedCosmetic);
            }
            OnPropertyChanged(nameof(SelectedCosmetic));
            OnPropertyChanged(nameof(CosmeticList));
            OnPropertyChanged(nameof(SelectedCosmeticNode));
        }

        private void MoveCostume()
        {
            if (SelectedCostume != null)
            {
                var movedCostume = SelectedCostume;
                movedCostume.Cosmetics.ForEach(x => FighterPackage.Cosmetics.ItemChanged(x));
            }
        }

        public void MoveCostumeUp()
        {
            MoveCostume();
            var selectedCostume = SelectedCostume;
            Costumes.MoveUp(selectedCostume);
            FighterPackage.Costumes.MoveUp(selectedCostume);
            SelectedCostume = selectedCostume;
            OnPropertyChanged(nameof(Costumes));
        }

        public void MoveCostumeDown()
        {
            MoveCostume();
            var selectedCostume = SelectedCostume;
            Costumes.MoveDown(selectedCostume);
            FighterPackage.Costumes.MoveDown(selectedCostume);
            SelectedCostume = selectedCostume;
            OnPropertyChanged(nameof(Costumes));
        }

        public void MoveCosmeticUp()
        {
            // Update internal indexes
            foreach(var cosmetic in CosmeticList)
            {
                cosmetic.InternalIndex = CosmeticList.IndexOf(cosmetic);
            }
            // Get node groups
            var nodeGroups = _cosmeticService.GetSharesDataGroups(CosmeticList.ToList());
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
                    FighterPackage.Cosmetics.ItemChanged(item);
                }
                // Move node after selected to before them
                foreach (var node in nodesToMove)
                {
                    node.InternalIndex += selectedNodes.Count;
                    FighterPackage.Cosmetics.ItemChanged(node);
                }
            }
            OnPropertyChanged(nameof(CosmeticList));
            OnPropertyChanged(nameof(SelectedCosmeticNode));
        }

        public void MoveCosmeticDown()
        {
            // Update internal indexes
            foreach (var cosmetic in CosmeticList)
            {
                cosmetic.InternalIndex = CosmeticList.IndexOf(cosmetic);
            }
            // Get node groups
            var nodeGroups = _cosmeticService.GetSharesDataGroups(CosmeticList.ToList());
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
                    FighterPackage.Cosmetics.ItemChanged(item);
                }
                // Move node after selected to before them
                foreach(var node in nodesToMove)
                {
                    node.InternalIndex -= selectedNodes.Count;
                    FighterPackage.Cosmetics.ItemChanged(node);
                }
            }
            OnPropertyChanged(nameof(CosmeticList));
            OnPropertyChanged(nameof(SelectedCosmeticNode));
        }

        private void MoveCosmeticToEnd(Cosmetic selectedCosmetic)
        {
            // Update internal indexes
            foreach (var cosmetic in CosmeticList)
            {
                cosmetic.InternalIndex = CosmeticList.IndexOf(cosmetic);
            }
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
                FighterPackage.Cosmetics.ItemChanged(selectedCosmetic);
            }
        }

        private bool ValidateSharesData(List<Cosmetic> nodes)
        {
            var startingNode = nodes.FirstOrDefault();
            var index = startingNode?.InternalIndex;
            var sharesData = startingNode?.SharesData;
            var groupCount = 0;
            var prevNode = CosmeticList.IndexOf(startingNode) > 0 ? CosmeticList[CosmeticList.IndexOf(startingNode) - 1] : null;
            // Trying to color smash a single node, not valid
            if (nodes.Count == 1 && startingNode.SharesData == false && (prevNode == null || prevNode.SharesData == false))
                return false;
            // If any nodes are color smashed and there are multiple un-color smashed nodes, not valid
            if (nodes.Any(x => x.SharesData == true) && nodes.Count(x => x.SharesData == false) > 1)
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
            if ((startingNode.SharesData == false && sharesData == true) 
                || (nodes.Count > 1 && startingNode.SharesData == false && prevNode != null && prevNode.SharesData == true))
                return false;
            return true;
        }


        public void UpdateSharesData(object selectedItems)
        {
            var nodes = ((IEnumerable)selectedItems).Cast<Cosmetic>().OrderBy(x => x.InternalIndex).ToList();
            var moveToEnd = nodes.Any(x => x.SharesData == true);
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
            var nodeGroups = _cosmeticService.GetSharesDataGroups(CosmeticList.ToList());
            var nodeGroup = nodeGroups.FirstOrDefault(x => x.Contains(nodes.LastOrDefault())) ?? nodes;
            // If a group's root is selected, the entire group will be affected
            var mixedGroup = nodes.Any(x => x.SharesData == false) && nodeGroup.Any(x => x.SharesData == true);
            if (mixedGroup)
                nodes = nodeGroup;
            // If we are changing all but the root, root will be affected too
            if (!mixedGroup && nodeGroup.Count == nodes.Count + 1)
                nodes.Add(CosmeticList.FirstOrDefault(x => x.InternalIndex == nodes.LastOrDefault()?.InternalIndex + 1));
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
                FighterPackage.Cosmetics.ItemChanged(item);
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
                PacFiles = new List<FighterPacFile>(),
                Cosmetics = new List<Cosmetic>()
            };
            Costumes.Add(newCostume);
            FighterPackage.Costumes.Add(newCostume);
            SelectedCostume = newCostume;
            OnPropertyChanged(nameof(Costumes));
        }

        public void RemoveCostumes(object selectedItems)
        {
            var costumes = ((IEnumerable)selectedItems).Cast<Costume>().OrderBy(x => Costumes.IndexOf(x)).ToList();
            // Don't allow removing costumes with cosmetics that are the root or last color smashed cosmetic in a group
            foreach (var costume in costumes)
            {
                foreach (var cosmetic in costume.Cosmetics)
                {
                    FlipColorSmashedCosmetics(cosmetic);
                }
            }
            // Select previous costume, if available
            var index = FighterPackage.Costumes.IndexOf(costumes.FirstOrDefault());
            if (index > 0)
            {
                SelectedCostume = FighterPackage.Costumes[index - 1];
            }
            // Remove costumes
            foreach (var costume in costumes)
            {
                foreach (var cosmetic in costume.Cosmetics)
                {
                    FighterPackage.Cosmetics.Remove(cosmetic);
                }
                FighterPackage.Costumes.Remove(costume);
                Costumes.Remove(costume);
            }
            OnPropertyChanged(nameof(SelectedCosmetic));
            OnPropertyChanged(nameof(CosmeticList));
            OnPropertyChanged(nameof(SelectedCosmeticNode));
            OnPropertyChanged(nameof(Costumes));
        }

        private List<int> GetCostumeIds()
        {
            var ids = new List<int>();
            if (Costumes != null && SelectedCostume != null)
            {
                for (int i = 0; i < 50; i++)
                {
                    if (!Costumes.Select(x => x.CostumeId).Contains(i) || SelectedCostume?.CostumeId == i)
                    {
                        ids.Add(i);
                    }
                }
            }
            return ids;
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

        private void ViewTextures()
        {
            if (SelectedPacFile != null)
            {
                _dialogService.OpenTextureViewer(SelectedPacFile.FilePath, "Texture Viewer", "Select a texture to view");
            }
        }
    }
}
