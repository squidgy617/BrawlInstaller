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
using BrawlLib.SSBB.Types;
using BrawlInstaller.StaticClasses;
using System.IO;

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
        private FighterPackage _oldFighterPackage;
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
        public ICommand LoadCommand => new RelayCommand(param => LoadFighter(param));
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
            WeakReferenceMessenger.Default.Register<SettingsSavedMessage>(this, (recipient, message) =>
            {
                SettingsSaved();
            });
        }

        // ViewModels
        public IFranchiseIconViewModel FranchiseIconViewModel { get; }
        public ICostumeViewModel CostumeViewModel { get; }
        public ICosmeticViewModel CosmeticViewModel { get; }
        public IFighterFileViewModel FighterFileViewModel { get; }
        public IFighterSettingsViewModel FighterSettingsViewModel { get; }

        // Properties
        public FighterPackage OldFighterPackage { get => _oldFighterPackage; set { _oldFighterPackage = value; OnPropertyChanged(nameof(OldFighterPackage)); } }
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
        public bool ImportButtonEnabled { get => FighterPackage?.PackageType == PackageType.Update || (!string.IsNullOrEmpty(_settingsService.BuildSettings.FilePathSettings.BrawlEx) && _fileService.DirectoryExists(_settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.BrawlEx))); }

        // Methods
        public void LoadFighter(object param)
        {
            if (param != null)
            {
                using (new CursorWait())
                {
                    var info = (FighterInfo)param;
                    FighterPackage = new FighterPackage();
                    FighterPackage.FighterInfo = info.Copy();
                    var package = _packageService.ExtractFighter(FighterPackage.FighterInfo);
                    // TODO: Do we need to copy on load? Mostly helps with validating the copy method is actually good
                    FighterPackage = package.Copy();
                    OldFighterPackage = FighterPackage.Copy();
                    _oldVictoryThemePath = FighterPackage.VictoryTheme.SongPath;
                    _oldCreditsThemePath = FighterPackage.CreditsTheme.SongPath;
                    // Set package path to internal fighter
                    FighterPackagePath = string.Empty;
                    OnPropertyChanged(nameof(FighterPackage));
                    WeakReferenceMessenger.Default.Send(new FighterLoadedMessage(FighterPackage));
                }
            }
        }

        public void OpenFighter()
        {
            var file = _dialogService.OpenFileDialog("Select a fighter package to load", "FIGHTERPACKAGE file (.fighterpackage)|*.fighterpackage|ZIP file (.zip)|*.zip");
            if (!string.IsNullOrEmpty(file))
            {
                // Reset old fighter package
                OldFighterPackage = null;
                FighterPackage = new FighterPackage { PackageType = PackageType.New };
                if (file.EndsWith(".fighterpackage"))
                {
                    FighterPackage = _packageService.LoadFighterPackage(file);
                }
                else
                {
                    FighterPackage = _packageService.LoadLegacyPackage(file);
                }
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
            // Reset old fighter package
            OldFighterPackage = null;
            FighterPackage = new FighterPackage();
            FighterPackage.PackageType = PackageType.New;
            FighterPackage.FighterInfo.FighterAttributes = new FighterAttributes();
            FighterPackage.FighterInfo.CosmeticAttributes = new CosmeticAttributes();
            FighterPackage.FighterInfo.SlotAttributes = new SlotAttributes();
            FighterPackage.FighterInfo.CSSSlotAttributes = new CSSSlotAttributes();
            _oldVictoryThemePath = FighterPackage.VictoryTheme.SongPath;
            _oldCreditsThemePath = FighterPackage.CreditsTheme.SongPath;
            // Set IDs to first available
            FighterPackage.FighterInfo = _fighterService.UpdateIdsToFirstUnused(FighterPackage.FighterInfo);
            OnPropertyChanged(nameof(FighterPackage));
            WeakReferenceMessenger.Default.Send(new FighterLoadedMessage(FighterPackage));
        }

        public void DeleteFighter()
        {
            if (_dialogService.ShowMessage("WARNING! You are about to delete the currently loaded fighter. Are you sure?", "Delete Fighter", MessageBoxButton.YesNo, MessageBoxImage.Warning))
            {
                // Set up delete package
                var deletePackage = new FighterPackage
                {
                    PackageType = PackageType.Delete,
                    FighterInfo = FighterPackage.FighterInfo.CopyNoAttributes()
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
                _packageService.SaveFighter(deletePackage, OldFighterPackage);
                // Remove from fighter list
                var foundFighters = FighterList.Where(x => x.Ids.FighterConfigId == deletePackage.FighterInfo.Ids.FighterConfigId
                && x.Ids.CSSSlotConfigId == deletePackage.FighterInfo.Ids.CSSSlotConfigId
                && x.Ids.SlotConfigId == deletePackage.FighterInfo.Ids.SlotConfigId
                && x.Ids.CosmeticConfigId == deletePackage.FighterInfo.Ids.CosmeticConfigId);
                foreach (var foundFighter in foundFighters.ToList())
                {
                    _settingsService.FighterInfoList.Remove(foundFighter);
                }
                _settingsService.SaveFighterInfoSettings(_settingsService.FighterInfoList.ToList());
                // Set package path to internal fighter
                FighterPackagePath = string.Empty;
                // Update rosters
                UpdateRoster(PackageType.Delete, deletePackage.FighterInfo);
                // Reset old fighter package
                OldFighterPackage = null;
                OnPropertyChanged(nameof(FighterList));
                WeakReferenceMessenger.Default.Send(new UpdateFighterListMessage(_settingsService.FighterInfoList));
                _dialogService.ShowMessage("Changes saved.", "Saved");
            }
        }

        public void SaveFighter()
        {
            if (ErrorValidate() != true || Validate() != true)
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
            // Create copy of package before save
            var packageToSave = FighterPackage.Copy();
            // Update entry name if necessary
            if (packageToSave.FighterInfo.EntryName == null)
            {
                packageToSave.FighterInfo.EntryName = packageToSave.FighterInfo.DisplayName;
            }
            // Set franchise icon up
            foreach (var icon in FranchiseIconViewModel.FranchiseIconList.ChangedItems)
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
                    packageToSave.Cosmetics.Add(newIcon);
                // If it was removed, just add it to the change list
                else
                    packageToSave.Cosmetics.ItemChanged(newIcon);
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
                        packageToSave.Cosmetics.Add(newModel);
                    else
                        packageToSave.Cosmetics.ItemChanged(newModel);
                }
            }
            packageToSave.FighterInfo.Ids.FranchiseId = FranchiseIconViewModel.SelectedFranchiseIcon?.Id ?? packageToSave.FighterInfo.Ids.FranchiseId;
            // Prompt for items to delete if applicable
            if (packageToSave.VictoryTheme != null && packageToSave.VictoryTheme.SongPath != _oldVictoryThemePath)
            {
                var delete = _dialogService.ShowMessage($"Victory theme has changed. Would you like to delete the old file at {_oldVictoryThemePath}?\nWARNING: Only delete this theme if it is not used by other fighters.", "Delete Victory Theme?", MessageBoxButton.YesNo);
                if (!delete)
                {
                    packageToSave.FighterDeleteOptions.DeleteVictoryTheme = false;
                }
            }
            if (packageToSave.CreditsTheme != null && packageToSave.CreditsTheme.SongPath != _oldCreditsThemePath)
            {
                var delete = _dialogService.ShowMessage($"Credits theme has changed. Would you like to delete the old file at {_oldCreditsThemePath}?\nWARNING: Only delete this theme if it is not used by other fighters.", "Delete Credits Theme?", MessageBoxButton.YesNo);
                if (!delete)
                {
                    packageToSave.FighterDeleteOptions.DeleteCreditsTheme = false;
                }
            }
            // Save fighter
            _packageService.SaveFighter(packageToSave, OldFighterPackage);
            // Save was successful, so load changes
            FighterPackage = packageToSave;
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
            // Reset old fighter package
            OldFighterPackage = null;
            // Update UI
            OnPropertyChanged(nameof(FighterPackage));
            OnPropertyChanged(nameof(FighterList));
            WeakReferenceMessenger.Default.Send(new FighterLoadedMessage(FighterPackage));
            WeakReferenceMessenger.Default.Send(new UpdateFighterListMessage(_settingsService.FighterInfoList));
            _dialogService.ShowMessage("Changes saved.", "Saved");
        }

        public void ExportFighter()
        {
            var file = _dialogService.SaveFileDialog("Save fighter package", "FIGHTERPACKAGE file (.fighterpackage)|*.fighterpackage");
            ExportFighterAs(file);
        }

        public void ExportFighterAs(string file)
        {
            if (ErrorValidate() && !string.IsNullOrEmpty(file))
            {
                // Create copy of fighter package before save
                var packageToSave = FighterPackage.Copy();
                // Update entry name if necessary
                if (packageToSave.FighterInfo.EntryName == null)
                {
                    packageToSave.FighterInfo.EntryName = packageToSave.FighterInfo.DisplayName;
                }
                var franchiseIcon = FranchiseIconViewModel.SelectedFranchiseIcon;
                packageToSave.Cosmetics.Add(franchiseIcon);
                packageToSave.FighterInfo.Ids.FranchiseId = FranchiseIconViewModel.SelectedFranchiseIcon?.Id ?? FighterPackage.FighterInfo.Ids.FranchiseId;
                _packageService.ExportFighter(packageToSave, file);
                // Save successful, so load package
                FighterPackage = packageToSave;
                // Remove added franchise icons from package
                FighterPackage.Cosmetics.Items.RemoveAll(x => x.CosmeticType == CosmeticType.FranchiseIcon && FighterPackage.Cosmetics.HasChanged(x));
                FighterPackage.Cosmetics.ChangedItems.RemoveAll(x => x.CosmeticType == CosmeticType.FranchiseIcon);
                // Set package path to new file
                FighterPackagePath = file;
                // Set package to new
                FighterPackage.PackageType = PackageType.New;
                // Reset old fighter package
                OldFighterPackage = null;
                _dialogService.ShowMessage("Exported successfully.", "Success");
            }
            OnPropertyChanged(nameof(FighterPackage));
            WeakReferenceMessenger.Default.Send(new FighterLoadedMessage(FighterPackage));
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

        private void SettingsSaved()
        {
            FighterPackage = null;
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
            _dialogService.ShowMessage("Saved rosters.", "Saved");
        }

        public bool ErrorValidate()
        {
            var messages = new List<DialogMessage>();
            var result = true;
            if (FighterPackage.FighterInfo.FighterAttributes != null && (FighterPackage.FighterInfo.SoundbankId == null || FighterPackage.FighterInfo.KirbySoundbankId == null))
            {
                messages.Add(new DialogMessage("Soundbank IDs", "One or more soundbanks do not have IDs. You cannot save a fighter without specifying soundbank IDs."));
                result = false;
            }
            if (messages.Count > 0)
                _dialogService.ShowMessages("Errors have occurred that prevent your fighter from saving.", "Errors", messages, MessageBoxButton.OK, MessageBoxImage.Error);
            return result;
        }

        public bool Validate()
        {
            var messages = new List<DialogMessage>();
            var result = true;
            var missingPaths = GetMissingPaths();
            if (missingPaths.Count > 0)
            {
                var pathString = string.Join("\n", missingPaths);
                messages.Add(new DialogMessage("Paths", $"Some paths in settings are missing:\n\n{pathString}"));
            }
            var missingCosmetics = GetMissingCosmetics();
            if (missingCosmetics.Count > 0)
            {
                var cosmeticString = string.Join("\n", missingCosmetics.Select(x => $"Type: {x.CosmeticType.GetDescription()} Style: {x.Style}"));
                messages.Add(new DialogMessage("Cosmetics", $"Some cosmetics marked as required are missing:\n\n{cosmeticString}"));
            }
            if (FighterPackage.PackageType == PackageType.New)
            {
                var idConflicts = GetIdConflicts();
                if (idConflicts.Count > 0)
                {
                    var idString = string.Join("\n", idConflicts.Select(x => $"Type: {x.Type.GetDescription()} ID: {x.Id}"));
                    messages.Add(new DialogMessage("ID Conflicts", $"Some IDs conflict with existing fighters in your build or IDs reserved by bosses:\n\n{idString}"));
                }
                var usedNames = _fighterService.GetUsedInternalNames();
                if (usedNames.Contains(FighterPackage?.FighterInfo?.PartialPacName?.ToLower()))
                {
                    messages.Add(new DialogMessage("Internal Name", $"The internal name {FighterPackage.FighterInfo.PartialPacName} is already used in the build."));
                }
                var isExModule = _fighterService.IsExModule(FighterPackage.Module);
                if (!isExModule)
                {
                    messages.Add(new DialogMessage("Non-Ex Module", $"Fighter ID could not be found in {Path.GetFileName(FighterPackage.Module)}. Fighter ID in module will not be updated. Ensure your fighter ID matches the one this character was originally designed to use."));
                }
            }
            var soundbankIdConflict = GetSoundbankIdConflicts();
            if (soundbankIdConflict.Count > 0)
            {
                var soundbankString = string.Join("\n", soundbankIdConflict.Select(x => $"0x{x:X2}"));
                messages.Add(new DialogMessage("Soundbank IDs", $"Soundbank IDs conflict with existing soundbanks in your build:\n\n{soundbankString}"));
            }
            var effectPacIdConflicts = GetEffectPacIdConflicts();
            if (effectPacIdConflicts.Count > 0)
            {
                var effectPacString = string.Join("\n", effectPacIdConflicts.Select(x => EffectPacs.FighterEffectPacs.FirstOrDefault(y => y.Value == x).Key));
                messages.Add(new DialogMessage("Effect.pacs", $"Effect.pacs conflict with existing Effect.pacs in your build:\n\n{effectPacString}"));
            }
            if (messages.Count > 0)
            {
                result = _dialogService.ShowMessages("Validation errors have occurred. Installing fighters with these errors could have unexpected results. It is strongly recommended that you correct these errors before continuing. Continue anyway?", "Validation Errors", messages, MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
            // Check franchise icons
            var franchiseIconDefinitions = _settingsService.BuildSettings.CosmeticSettings.Where(x => x.CosmeticType == CosmeticType.FranchiseIcon && x.Required);
            if (franchiseIconDefinitions.Count() > 0)
            {
                // If there's franchise icon image, add it
                if (franchiseIconDefinitions.Any(x => x.Style == "Icon") && FranchiseIconViewModel.SelectedFranchiseIcon.Image == null && string.IsNullOrEmpty(FranchiseIconViewModel.SelectedFranchiseIcon.ImagePath))
                {
                    missingCosmetics.Add((CosmeticType.FranchiseIcon, "Icon"));
                }
                // If there's franchise icon model, add it
                if (franchiseIconDefinitions.Any(x => x.Style == "Model") && FranchiseIconViewModel.SelectedFranchiseIcon.Model == null && string.IsNullOrEmpty(FranchiseIconViewModel.SelectedFranchiseIcon.ModelPath))
                {
                    missingCosmetics.Add((CosmeticType.FranchiseIcon, "Model"));
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

        private List<uint> GetSoundbankIdConflicts()
        {
            var soundbankIdConflicts = new List<uint>();
            if (FighterPackage.FighterInfo.SoundbankId != FighterPackage.FighterInfo.OriginalSoundbankId 
                || FighterPackage.FighterInfo.KirbySoundbankId != FighterPackage.FighterInfo.OriginalKirbySoundbankId
                || FighterPackage.PackageType == PackageType.New)
            {
                var usedSoundbankIds = _fighterService.GetUsedSoundbankIds();
                if (FighterPackage.FighterInfo.SoundbankId != FighterPackage.FighterInfo.OriginalSoundbankId || FighterPackage.PackageType == PackageType.New)
                {
                    var soundbankIdConflict = usedSoundbankIds.AsParallel().FirstOrDefault(x => x == FighterPackage.FighterInfo.SoundbankId);
                    if (soundbankIdConflict != null)
                    {
                        soundbankIdConflicts.Add((uint)soundbankIdConflict);
                    }
                }
                if (FighterPackage.FighterInfo.KirbySoundbankId != FighterPackage.FighterInfo.OriginalKirbySoundbankId || FighterPackage.PackageType == PackageType.New)
                {
                    var soundbankIdConflict = usedSoundbankIds.AsParallel().FirstOrDefault(x => x == FighterPackage.FighterInfo.KirbySoundbankId);
                    if (soundbankIdConflict != null)
                    {
                        soundbankIdConflicts.Add((uint)soundbankIdConflict);
                    }
                }
            }
            return soundbankIdConflicts;
        }

        private List<int> GetEffectPacIdConflicts()
        {
            var effectPacIdConflicts = new List<int>();
            if (FighterPackage.FighterInfo.EffectPacId != FighterPackage.FighterInfo.OriginalEffectPacId 
                || FighterPackage.FighterInfo.KirbyEffectPacId != FighterPackage.FighterInfo.OriginalKirbyEffectPacId
                || FighterPackage.PackageType == PackageType.New)
            {
                var usedEffectPacIds = _fighterService.GetUsedEffectPacs();
                if (FighterPackage.FighterInfo.EffectPacId != FighterPackage.FighterInfo.OriginalEffectPacId || FighterPackage.PackageType == PackageType.New)
                {
                    var effectPacIdConflict = usedEffectPacIds.AsParallel().FirstOrDefault(x => x == FighterPackage.FighterInfo.EffectPacId);
                    if (effectPacIdConflict != null)
                    {
                        effectPacIdConflicts.Add((int)effectPacIdConflict);
                    }
                }
                if (FighterPackage.FighterInfo.KirbyEffectPacId != FighterPackage.FighterInfo.OriginalKirbyEffectPacId || FighterPackage.PackageType == PackageType.New)
                {
                    var effectPacIdConflict = usedEffectPacIds.AsParallel().FirstOrDefault(x => x == FighterPackage.FighterInfo.KirbyEffectPacId);
                    if (effectPacIdConflict != null)
                    {
                        effectPacIdConflicts.Add((int)effectPacIdConflict);
                    }
                }
            }
            return effectPacIdConflicts;
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
