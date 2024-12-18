using BrawlInstaller.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.Windows.Input;
using BrawlLib.SSBB.ResourceNodes;
using System.Diagnostics;
using BrawlInstaller.Services;
using BrawlInstaller.Classes;
using BrawlInstaller.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BrawlLib.Internal;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Collections.ObjectModel;
using BrawlInstaller.Helpers;
using System.Windows;
using System.Windows.Forms;
using static BrawlInstaller.ViewModels.MainControlsViewModel;

namespace BrawlInstaller.ViewModels
{
    public interface IFighterViewModel
    {
        ICommand LoadCommand { get; }
        ICommand SaveCommand { get; }
        FighterPackage FighterPackage { get; }
    }

    [Export(typeof(IFighterViewModel))]
    internal class FighterViewModel : ViewModelBase, IFighterViewModel
    {
        // Private properties
        private FighterInfo _selectedFighter;
        private string _oldVictoryThemePath;
        private string _oldCreditsThemePath;
        private string _fighterPackagePath;
        private List<Roster> _rosters;
        private Roster _selectedRoster;
        private RosterEntry _selectedRosterEntry;

        // Services
        IPackageService _packageService { get; }
        ISettingsService _settingsService { get; }
        IDialogService _dialogService { get; }
        IFighterService _fighterService { get; }
        IFileService _fileService { get; }

        // Commands
        public ICommand LoadCommand => new RelayCommand(param => LoadFighter());
        public ICommand SaveCommand => new RelayCommand(param => SaveFighter());
        public ICommand DeleteCommand => new RelayCommand(param => DeleteFighter());
        public ICommand RefreshFightersCommand => new RelayCommand(param => GetFighters());
        public ICommand ExportFighterCommand => new RelayCommand(param => ExportFighter());
        public ICommand SavePackageCommand => new RelayCommand(param => ExportFighterAs(FighterPackagePath));
        public ICommand OpenFighterCommand => new RelayCommand(param => OpenFighter());
        public ICommand NewFighterCommand => new RelayCommand(param => NewFighter());
        public ICommand MoveFighterUpCommand => new RelayCommand(param => MoveFighterUp());
        public ICommand MoveFighterDownCommand => new RelayCommand(param => MoveFighterDown());
        public ICommand RemoveFighterCommand => new RelayCommand(param =>  RemoveFighter());
        public ICommand SaveRostersCommand => new RelayCommand(param => SaveRosters());
        public ICommand AddFighterCommand => new RelayCommand(param => AddFighter());
        public ICommand CopyFighterCommand => new RelayCommand(param => CopyFighter());

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public FighterViewModel(IPackageService packageService, ISettingsService settingsService, IDialogService dialogService, IFighterService fighterService, IFileService fileService,
            IFranchiseIconViewModel franchiseIconViewModel, ICostumeViewModel costumeViewModel, ICosmeticViewModel cosmeticViewmodel, IFighterFileViewModel fighterFileViewModel, 
            IFighterSettingsViewModel fighterSettingsViewModel)
        {
            _packageService = packageService;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fighterService = fighterService;
            _fileService = fileService;
            FranchiseIconViewModel = franchiseIconViewModel;
            CostumeViewModel = costumeViewModel;
            CosmeticViewModel = cosmeticViewmodel;
            FighterFileViewModel = fighterFileViewModel;
            FighterSettingsViewModel = fighterSettingsViewModel;

            Rosters = _fighterService.GetRosters();
            SelectedRoster = Rosters.FirstOrDefault();

            WeakReferenceMessenger.Default.Register<UpdateFighterListMessage>(this, (recipient, message) =>
            {
                UpdateFighterList(message);
            });
            WeakReferenceMessenger.Default.Register<UpdateSettingsMessage>(this, (recipient, message) =>
            {
                UpdateSettings();
            });
        }

        // ViewModels
        public IFranchiseIconViewModel FranchiseIconViewModel { get; }
        public ICostumeViewModel CostumeViewModel { get; }
        public ICosmeticViewModel CosmeticViewModel { get; }
        public IFighterFileViewModel FighterFileViewModel { get; }
        public IFighterSettingsViewModel FighterSettingsViewModel { get; }

        // Properties
        public FighterPackage FighterPackage { get; set; }
        public ObservableCollection<FighterInfo> FighterList { get => new ObservableCollection<FighterInfo>(_settingsService.FighterInfoList); }
        public FighterInfo SelectedFighter { get => _selectedFighter; set { _selectedFighter = value; OnPropertyChanged(nameof(SelectedFighter)); } }

        [DependsUpon(nameof(FighterPackage))]
        public string ImportButtonText { get => FighterPackage?.PackageType == PackageType.Update ? "Save" : "Import"; }

        [DependsUpon(nameof(FighterPackage))]
        public string FighterPackagePath { get => _fighterPackagePath; set { _fighterPackagePath = value; OnPropertyChanged(nameof(FighterPackagePath)); } }

        [DependsUpon(nameof(FighterPackage))]
        [DependsUpon(nameof(FighterPackagePath))]
        public bool InternalPackage { get => string.IsNullOrEmpty(FighterPackagePath) && !(FighterPackage?.PackageType == PackageType.New); }

        public List<Roster> Rosters { get => _rosters; set { _rosters = value; OnPropertyChanged(nameof(Rosters)); } }

        [DependsUpon(nameof(Rosters))]
        public Roster SelectedRoster { get => _selectedRoster; set { _selectedRoster = value; OnPropertyChanged(nameof(SelectedRoster)); } }

        [DependsUpon(nameof(SelectedRoster))]
        public ObservableCollection<RosterEntry> RosterEntries { get => SelectedRoster != null ? new ObservableCollection<RosterEntry>(SelectedRoster?.Entries) : new ObservableCollection<RosterEntry>(); }

        [DependsUpon(nameof(SelectedRoster))]
        public RosterEntry SelectedRosterEntry { get => _selectedRosterEntry; set { _selectedRosterEntry = value; OnPropertyChanged(nameof(SelectedRosterEntry)); } }

        [DependsUpon(nameof(FighterPackage))]
        public bool ImportButtonEnabled { get => FighterPackage?.PackageType == PackageType.Update || _fileService.DirectoryExists(_settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.BrawlEx)); }

        // Methods
        public void LoadFighter()
        {
            using (new CursorWait())
            {
                FighterPackage = new FighterPackage();
                FighterPackage.FighterInfo = SelectedFighter.Copy();
                FighterPackage = _packageService.ExtractFighter(FighterPackage.FighterInfo);
                _oldVictoryThemePath = FighterPackage.VictoryTheme.SongPath;
                _oldCreditsThemePath = FighterPackage.CreditsTheme.SongPath;
                // Set package path to internal fighter
                FighterPackagePath = string.Empty;
                OnPropertyChanged(nameof(FighterPackage));
                WeakReferenceMessenger.Default.Send(new FighterLoadedMessage(FighterPackage));
            }
        }

        public void OpenFighter()
        {
            var file = _dialogService.OpenFileDialog("Select a fighter package to load", "FIGHTERPACKAGE file (.fighterpackage)|*.fighterpackage");
            if (!string.IsNullOrEmpty(file))
            {
                FighterPackage = new FighterPackage { PackageType = PackageType.New };
                FighterPackage = _packageService.LoadFighterPackage(file);
                // Prompt if user wants to load franchise icon
                var franchiseIcon = FighterPackage.Cosmetics.ChangedItems.FirstOrDefault(x => x.CosmeticType == CosmeticType.FranchiseIcon);
                if (franchiseIcon != null)
                {
                    var importIcons = _dialogService.ShowMessage("This package includes a franchise icon. Would you like to import it?\nNOTE: Only import NEW franchise icons, not ones already in your build.", "Import franchise icon?", MessageBoxButton.YesNo, bitmapImage:franchiseIcon.Image);
                    if (!importIcons)
                    {
                        // Remove franchise icons from package if user selects No
                        foreach(var icon in FighterPackage.Cosmetics.Items.Where(x => x.CosmeticType == CosmeticType.FranchiseIcon).ToList())
                        {
                            FighterPackage.Cosmetics.Remove(icon, false);
                            FighterPackage.Cosmetics.RemoveChange(icon);
                        }
                    }
                }
                _oldVictoryThemePath = FighterPackage.VictoryTheme.SongPath;
                _oldCreditsThemePath = FighterPackage.CreditsTheme.SongPath;
                // Set package path to newly opened fighter
                FighterPackagePath = file;
                OnPropertyChanged(nameof(FighterPackage));
                WeakReferenceMessenger.Default.Send(new FighterLoadedMessage(FighterPackage));
            }
        }

        public void NewFighter()
        {
            FighterPackage = new FighterPackage();
            FighterPackage.PackageType = PackageType.New;
            _oldVictoryThemePath = FighterPackage.VictoryTheme.SongPath;
            _oldCreditsThemePath = FighterPackage.CreditsTheme.SongPath;
            // Set IDs to first available
            FighterPackage.FighterInfo = _fighterService.UpdateIdsToFirstUnused(FighterPackage.FighterInfo);
            OnPropertyChanged(nameof(FighterPackage));
            WeakReferenceMessenger.Default.Send(new FighterLoadedMessage(FighterPackage));
        }

        public void DeleteFighter()
        {
            // Set up delete package
            var deletePackage = new FighterPackage
            {
                PackageType = PackageType.Delete,
                FighterInfo = FighterPackage.FighterInfo.Copy()
            };
            deletePackage.FighterInfo.DisplayName = "Unknown";
            // Mark all cosmetics as changed
            foreach (var cosmetic in FighterPackage.Cosmetics.Items)
            {
                deletePackage.Cosmetics.ItemChanged(cosmetic);
            }
            // Prompt for items to delete if applicable
            if (FighterPackage.VictoryTheme != null && !string.IsNullOrEmpty(FighterPackage.VictoryTheme.SongFile))
            {
                var delete = _dialogService.ShowMessage($"Would you like to delete the victory theme at {_oldVictoryThemePath}?\nWARNING: Only delete this theme if it is not used by other fighters.", "Delete Victory Theme?", MessageBoxButton.YesNo);
                if (!delete)
                {
                    deletePackage.FighterDeleteOptions.DeleteVictoryTheme = false;
                }
            }
            if (FighterPackage.CreditsTheme != null && !string.IsNullOrEmpty(FighterPackage.CreditsTheme.SongFile))
            {
                var delete = _dialogService.ShowMessage($"Would you like to delete the credits theme at {_oldCreditsThemePath}?\nWARNING: Only delete this theme if it is not used by other fighters.", "Delete Credits Theme?", MessageBoxButton.YesNo);
                if (!delete)
                {
                    deletePackage.FighterDeleteOptions.DeleteCreditsTheme = false;
                }
            }
            if (FranchiseIconViewModel.FranchiseIconList.Items.Any(x => x.Id == FighterPackage.FighterInfo.Ids.FranchiseId))
            {
                var delete = _dialogService.ShowMessage("Would you like to delete the fighter's franchise icon?\nWARNING: Only delete this theme if it is not used by other fighters.", "Delete Franchise Icon?", MessageBoxButton.YesNo);
                if (delete)
                {
                    deletePackage.Cosmetics.ItemChanged(FranchiseIconViewModel.FranchiseIconList.Items.FirstOrDefault(x => x.Id == FighterPackage.FighterInfo.Ids.FranchiseId));
                }
            }
            // Update UI
            FighterPackage = null;
            OnPropertyChanged(nameof(FighterPackage));
            // Save
            _packageService.SaveFighter(deletePackage);
            // Remove from fighter list
            var foundFighters = FighterList.Where(x => x.Ids.FighterConfigId == deletePackage.FighterInfo.Ids.FighterConfigId 
            && x.Ids.CSSSlotConfigId == deletePackage.FighterInfo.Ids.CSSSlotConfigId
            && x.Ids.SlotConfigId == deletePackage.FighterInfo.Ids.SlotConfigId
            && x.Ids.CosmeticConfigId == deletePackage.FighterInfo.Ids.CosmeticConfigId);
            foreach(var foundFighter in foundFighters.ToList())
            {
                _settingsService.FighterInfoList.Remove(foundFighter);
            }
            _settingsService.SaveFighterInfoSettings(_settingsService.FighterInfoList.ToList());
            // Set package path to internal fighter
            FighterPackagePath = string.Empty;
            // Update rosters
            UpdateRoster(PackageType.Delete, deletePackage.FighterInfo);
            OnPropertyChanged(nameof(FighterList));
            WeakReferenceMessenger.Default.Send(new UpdateFighterListMessage(_settingsService.FighterInfoList));
        }

        // TODO: Copy fighter package before save, only update after save completes successfully
        // TODO: When adding a new fighter, franchise icon will be added to the end of the list automatically, so we'll need to prompt the user whether they want to install or not
        public void SaveFighter()
        {
            if (Validate() != true)
            {
                return;
            }
            var packageType = FighterPackage.PackageType;
            // Set costume indexes for cosmetics
            foreach(var costume in FighterPackage.Costumes)
            {
                foreach(var cosmetic in costume.Cosmetics)
                {
                    cosmetic.CostumeIndex = CostumeViewModel.Costumes.IndexOf(costume) + 1;
                }
            }
            // Set franchise icon up
            foreach(var icon in FranchiseIconViewModel.FranchiseIconList.ChangedItems)
            {
                var newIcon = new Cosmetic
                {
                    CosmeticType = CosmeticType.FranchiseIcon,
                    Style = "Icon",
                    Image = icon.Image,
                    ImagePath = icon.ImagePath,
                    HDImage = icon.HDImage,
                    HDImagePath = icon.HDImagePath,
                    Texture = icon.Texture,
                    Palette = icon.Palette,
                    Id = icon.Id
                };
                // Only add it to cosmetic list if it is actually in the list
                if (FranchiseIconViewModel.FranchiseIconList.Items.Contains(icon))
                    FighterPackage.Cosmetics.Add(newIcon);
                // If it was removed, just add it to the change list
                else
                    FighterPackage.Cosmetics.ItemChanged(newIcon);
                if (!string.IsNullOrEmpty(icon.ModelPath))
                {
                    var newModel = new Cosmetic
                    {
                        CosmeticType = CosmeticType.FranchiseIcon,
                        Style = "Model",
                        Model = icon.Model,
                        ModelPath = icon.ModelPath,
                        ColorSequence = icon.ColorSequence,
                        Id = icon.Id
                    };
                    if (FranchiseIconViewModel.FranchiseIconList.Items.Contains(icon))
                        FighterPackage.Cosmetics.Add(newModel);
                    else
                        FighterPackage.Cosmetics.ItemChanged(newModel);
                }
            }
            FighterPackage.FighterInfo.Ids.FranchiseId = FranchiseIconViewModel.SelectedFranchiseIcon?.Id ?? FighterPackage.FighterInfo.Ids.FranchiseId;
            // Prompt for items to delete if applicable
            if (FighterPackage.VictoryTheme != null && FighterPackage.VictoryTheme.SongPath != _oldVictoryThemePath)
            {
                var delete = _dialogService.ShowMessage($"Victory theme has changed. Would you like to delete the old theme at {_oldVictoryThemePath}?\nWARNING: Only delete this theme if it is not used by other fighters.", "Delete Victory Theme?", MessageBoxButton.YesNo);
                if (!delete)
                {
                    FighterPackage.FighterDeleteOptions.DeleteVictoryTheme = false;
                }
            }
            if (FighterPackage.CreditsTheme != null && FighterPackage.CreditsTheme.SongPath != _oldCreditsThemePath)
            {
                var delete = _dialogService.ShowMessage($"Credits theme has changed. Would you like to delete the old theme at {_oldCreditsThemePath}?\nWARNING: Only delete this theme if it is not used by other fighters.", "Delete Credits Theme?", MessageBoxButton.YesNo);
                if (!delete)
                {
                    FighterPackage.FighterDeleteOptions.DeleteCreditsTheme = false;
                }
            }
            // Save fighter
            _packageService.SaveFighter(FighterPackage);
            // Remove added franchise icons from package
            FighterPackage.Cosmetics.Items.RemoveAll(x => x.CosmeticType == CosmeticType.FranchiseIcon && FighterPackage.Cosmetics.HasChanged(x));
            // Clear changes on all cosmetics
            FighterPackage.Cosmetics.Items.ForEach(x => { x.ImagePath = ""; x.ModelPath = ""; x.ColorSmashChanged = false; } );
            FighterPackage.Cosmetics.ClearChanges();
            // Update delete options
            FighterPackage.FighterDeleteOptions.DeleteVictoryTheme = true;
            FighterPackage.FighterDeleteOptions.DeleteCreditsTheme = true;
            _oldVictoryThemePath = FighterPackage.VictoryTheme?.SongPath;
            _oldCreditsThemePath = FighterPackage.CreditsTheme?.SongPath;
            // Set package path to internal fighter
            FighterPackagePath = string.Empty;
            // Update fighter list
            var newFighterInfo = FighterPackage.FighterInfo.CopyNoAttributes();
            if (packageType == PackageType.New)
            {
                _settingsService.FighterInfoList.Add(newFighterInfo);
            }
            else
            {
                var match = _settingsService.FighterInfoList.FirstOrDefault(x => x.Ids.FighterConfigId == newFighterInfo.Ids.FighterConfigId
                && x.Ids.CSSSlotConfigId == newFighterInfo.Ids.CSSSlotConfigId && x.Ids.SlotConfigId == newFighterInfo.Ids.SlotConfigId
                && x.Ids.CosmeticConfigId == newFighterInfo.Ids.CosmeticConfigId);
                if (match != null)
                {
                    match = newFighterInfo;
                }
            }
            _settingsService.SaveFighterInfoSettings(_settingsService.FighterInfoList.ToList());
            // Update rosters
            UpdateRoster(packageType, FighterPackage.FighterInfo);
            // Update UI
            OnPropertyChanged(nameof(FighterPackage));
            OnPropertyChanged(nameof(FighterList));
            WeakReferenceMessenger.Default.Send(new FighterLoadedMessage(FighterPackage));
            WeakReferenceMessenger.Default.Send(new UpdateFighterListMessage(_settingsService.FighterInfoList));
        }

        public void ExportFighter()
        {
            var file = _dialogService.SaveFileDialog("Save fighter package", "FIGHTERPACKAGE file (.fighterpackage)|*.fighterpackage");
            ExportFighterAs(file);
        }

        public void ExportFighterAs(string file)
        {
            if (!string.IsNullOrEmpty(file))
            {
                var franchiseIcon = FranchiseIconViewModel.SelectedFranchiseIcon;
                FighterPackage.Cosmetics.Add(franchiseIcon);
                FighterPackage.FighterInfo.Ids.FranchiseId = FranchiseIconViewModel.SelectedFranchiseIcon?.Id ?? FighterPackage.FighterInfo.Ids.FranchiseId;
                _packageService.ExportFighter(FighterPackage, file);
                // Remove added franchise icons from package
                FighterPackage.Cosmetics.Items.RemoveAll(x => x.CosmeticType == CosmeticType.FranchiseIcon && FighterPackage.Cosmetics.HasChanged(x));
                // Set package path to new file
                FighterPackagePath = file;
                // Set package to new
                FighterPackage.PackageType = PackageType.New;
            }
            OnPropertyChanged(nameof(FighterPackage));
        }

        private void GetFighters()
        {
            var list = _settingsService.LoadFighterInfoSettings();
            _settingsService.FighterInfoList = list;
            OnPropertyChanged(nameof(FighterList));
            OnPropertyChanged(nameof(SelectedFighter));
            WeakReferenceMessenger.Default.Send(new UpdateFighterListMessage(_settingsService.FighterInfoList));
        }

        private void UpdateFighterList(UpdateFighterListMessage message)
        {
            var fighterInfoList = message.Value;
            // Update roster entries
            var updatedMatches = new List<RosterEntry>();
            foreach(var fighterInfo in fighterInfoList)
            {
                var foundMatches = Rosters.SelectMany(x => x.Entries).Where(x => x.Id == fighterInfo.Ids.CSSSlotConfigId && !updatedMatches.Contains(x));
                foreach(var match in foundMatches)
                {
                    match.Name = fighterInfo.DisplayName;
                    updatedMatches.Add(match);
                }
            }
            OnPropertyChanged(nameof(Rosters));
            OnPropertyChanged(nameof(FighterList));
        }

        private void UpdateRoster(PackageType packageType, FighterInfo fighterInfo)
        {
            if (packageType == PackageType.New)
            {
                foreach(var roster in Rosters.Where(x => x.AddNewCharacters))
                {
                    var newEntry = new RosterEntry
                    {
                        Id = fighterInfo.Ids.CSSSlotConfigId != null ? fighterInfo.Ids.CSSSlotConfigId.Value : 0,
                        Name = fighterInfo.DisplayName,
                        InCss = true,
                        InRandom = true
                    };
                    roster.Entries.Add(newEntry);
                }
            }
            else
            {
                foreach(var roster in Rosters)
                {
                    var foundEntry = roster.Entries.FirstOrDefault(x => x.Id == fighterInfo.Ids.CSSSlotConfigId);
                    if (foundEntry != null)
                    {
                        if (packageType == PackageType.Delete)
                        {
                            roster.Entries.Remove(foundEntry);
                        }
                        else if (packageType == PackageType.Update)
                        {
                            foundEntry.Name = fighterInfo.DisplayName;
                        }
                    }
                }
            }
            _fighterService.SaveRosters(Rosters);
            OnPropertyChanged(nameof(Rosters));
        }

        private void UpdateSettings()
        {
            FighterPackage = null;
            GetFighters();
            Rosters = _fighterService.GetRosters();
            SelectedRoster = Rosters.FirstOrDefault();
            OnPropertyChanged(nameof(FighterPackage));
            OnPropertyChanged(nameof(Rosters));
            OnPropertyChanged(nameof(SelectedRoster));
        }

        private void MoveFighterUp()
        {
            if (SelectedRosterEntry != null)
            {
                SelectedRoster.Entries.MoveUp(SelectedRosterEntry);
            }
            OnPropertyChanged(nameof(RosterEntries));
        }
        private void MoveFighterDown()
        {
            if (SelectedRosterEntry != null)
            {
                SelectedRoster.Entries.MoveDown(SelectedRosterEntry);
            }
            OnPropertyChanged(nameof(RosterEntries));
        }

        private void RemoveFighter()
        {
            if (SelectedRosterEntry != null)
            {
                SelectedRoster.Entries.Remove(SelectedRosterEntry);
            }
            OnPropertyChanged(nameof(RosterEntries));
        }

        private void CopyFighter()
        {
            if (SelectedRoster != null && SelectedFighter != null)
            {
                var newEntry = new RosterEntry
                {
                    Id = SelectedFighter.Ids.CSSSlotConfigId != null ? SelectedFighter.Ids.CSSSlotConfigId.Value : 0,
                    InCss = true,
                    InRandom = true,
                    Name = SelectedFighter.DisplayName
                };
                SelectedRoster.Entries.Add(newEntry);
                SelectedRosterEntry = newEntry;
                OnPropertyChanged(nameof(RosterEntries));
                OnPropertyChanged(nameof(SelectedRosterEntry));
            }
        }

        private void AddFighter()
        {
            if (SelectedRoster != null)
            {
                var rosterOptions = new List<RosterEntry>();
                var randomEntry = new RosterEntry
                {
                    Id = 0x29,
                    Name = "Random",
                    InCss = true,
                    InRandom = false
                };
                rosterOptions.Add(randomEntry);
                foreach (var fighter in _settingsService.FighterInfoList)
                {
                    var newEntry = new RosterEntry
                    {
                        Id = fighter.Ids.CSSSlotConfigId != null ? fighter.Ids.CSSSlotConfigId.Value : 0,
                        Name = fighter.DisplayName,
                        InCss = true,
                        InRandom = true
                    };
                    rosterOptions.Add(newEntry);
                }
                var selectedOption = _dialogService.OpenDropDownDialog(rosterOptions, "Name", "Add Fighter", "Select a fighter");
                if (selectedOption != null)
                {
                    SelectedRoster.Entries.Add((RosterEntry)selectedOption);
                    SelectedRosterEntry = (RosterEntry)selectedOption;
                    OnPropertyChanged(nameof(RosterEntries));
                    OnPropertyChanged(nameof(SelectedRosterEntry));
                }
            }
        }

        private void SaveRosters()
        {
            _fighterService.SaveRosters(Rosters);
        }

        public bool Validate()
        {
            var result = true;
            var missingPaths = GetMissingPaths();
            if (missingPaths.Count > 0)
            {
                var pathString = string.Join("\n", missingPaths);
                result = _dialogService.ShowMessage($"Some paths in settings are missing. Installing a fighter without these paths may have unexpected results. Continue anyway?\nMissing Paths:\n{pathString}", 
                    "Missing Paths", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == false) return false;
            }
            var missingCosmetics = GetMissingCosmetics();
            if (missingCosmetics.Count > 0)
            {
                var cosmeticString = string.Join("\n", missingCosmetics.Select(x => $"Type: {x.CosmeticType.GetDescription()} Style: {x.Style}"));
                result = _dialogService.ShowMessage($"Some cosmetics marked as required are missing. Installing a fighter without these cosmetics may have unexpected results. Continue anyway?\nMissing Cosmetics:\n{cosmeticString}",
                    "Missing Cosmetics", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == false) return false;
            }
            if (FighterPackage.PackageType == PackageType.New)
            {
                var idConflicts = GetIdConflicts();
                if (idConflicts.Count > 0)
                {
                    var idString = string.Join("\n", idConflicts.Select(x => $"Type: {x.Type.GetDescription()} ID: {x.Id}"));
                    result = _dialogService.ShowMessage($"Some IDs conflict with existing fighters in your build or IDs reserved by bosses. Installing a fighter with ID conflicts could cause unexpected results. Continue anyway?\nID Conflicts:\n{idString}",
                        "ID Conflicts", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                }
            }
            return result;
        }

        private List<string> GetMissingPaths()
        {
            var missingPaths = new List<string>();
            var paths = _settingsService.GetAllPaths();
            foreach (var path in paths)
            {
                var buildFilePath = _settingsService.GetBuildFilePath(path);
                if (!_fileService.FileOrDirectoryExists(buildFilePath))
                {
                    missingPaths.Add(buildFilePath);
                }
            }
            return missingPaths;
        }

        private List<(CosmeticType CosmeticType, string Style)> GetMissingCosmetics()
        {
            var missingCosmetics = new List<(CosmeticType CosmeticType, string Style)>();
            var definitionGroups = _settingsService.BuildSettings.CosmeticSettings.Where(x => x.FighterCosmetic && x.Required).GroupBy(x => new { x.CosmeticType, x.Style });
            foreach (var group in definitionGroups)
            {
                // If no cosmetics are found for a group (including inherited styles), add it to list
                if (!FighterPackage.Cosmetics.Items.Any(x => x.Style == group.Key.Style && x.CosmeticType == group.Key.CosmeticType)
                    && !FighterPackage.Cosmetics.InheritedStyles.Any(x => x.Key.Item1 == group.Key.CosmeticType && x.Key.Item2 == group.Key.Style))
                {
                    missingCosmetics.Add((group.Key.CosmeticType, group.Key.Style));
                }
            }
            return missingCosmetics;
        }

        private List<BrawlId> GetIdConflicts()
        {
            var idConflicts = new List<BrawlId>();
            var usedIds = _fighterService.GetUsedFighterIds();
            foreach(var id in FighterPackage.FighterInfo.Ids.Ids)
            {
                if (usedIds.Any(x => x.Type == id.Type && x.Id == id.Id))
                {
                    idConflicts.Add(id);
                }
            }
            return idConflicts;
        }
    }

    // Messages
    public class FighterLoadedMessage : ValueChangedMessage<FighterPackage>
    {
        public FighterLoadedMessage(FighterPackage fighterPackage) : base(fighterPackage)
        {
        }
    }

    public class UpdateFighterListMessage : ValueChangedMessage<List<FighterInfo>>
    {
        public UpdateFighterListMessage(List<FighterInfo> fighterInfoList) : base(fighterInfoList)
        {
        }
    }
}
