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
        private FighterPackage _fighterPackage;
        private FighterPackage _oldFighterPackage;
        private FighterInfo _selectedFighter;
        private string _oldVictoryThemePath;
        private string _oldCreditsThemePath;
        private string _fighterPackagePath;
        private List<Roster> _rosters;
        private Roster _selectedRoster;
        private RosterEntry _selectedRosterEntry;
        private TrophyType _selectedTrophyType;

        // Services
        IPackageService _packageService { get; }
        ISettingsService _settingsService { get; }
        IDialogService _dialogService { get; }
        IFighterService _fighterService { get; }
        IFileService _fileService { get; }
        ICodeService _codeService { get; }
        ITrophyService _trophyService { get; }
        ICosmeticService _cosmeticService { get; }

        // Commands
        public ICommand LoadRosterFighterCommand => new RelayCommand(param => LoadFighterFromRoster(param));
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
        public ICommand ChangeTrophyCommand => new RelayCommand(param => ChangeTrophy());
        public ICommand NewTrophyCommand => new RelayCommand(param => NewTrophy());
        public ICommand ClearTrophyCommand => new RelayCommand(param => ClearTrophy());
        public ICommand ExportCosmeticsCommand => new RelayCommand(param => ExportCosmetics());

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public FighterViewModel(IPackageService packageService, ISettingsService settingsService, IDialogService dialogService, IFighterService fighterService, IFileService fileService,
            ICodeService codeService, ITrophyService trophyService, ICosmeticService cosmeticService, IFranchiseIconViewModel franchiseIconViewModel, ICostumeViewModel costumeViewModel, 
            ICosmeticViewModel cosmeticViewmodel, IFighterFileViewModel fighterFileViewModel, IFighterSettingsViewModel fighterSettingsViewModel, 
            IFighterTrophyViewModel fighterTrophyViewModel)
        {
            _packageService = packageService;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fighterService = fighterService;
            _fileService = fileService;
            _codeService = codeService;
            _trophyService = trophyService;
            _cosmeticService = cosmeticService;
            FranchiseIconViewModel = franchiseIconViewModel;
            CostumeViewModel = costumeViewModel;
            CosmeticViewModel = cosmeticViewmodel;
            FighterFileViewModel = fighterFileViewModel;
            FighterSettingsViewModel = fighterSettingsViewModel;
            FighterTrophyViewModel = fighterTrophyViewModel;

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
        public IFighterTrophyViewModel FighterTrophyViewModel { get; }

        // Properties
        public FighterPackage OldFighterPackage { get => _oldFighterPackage; set { _oldFighterPackage = value; OnPropertyChanged(nameof(OldFighterPackage)); } }
        public FighterPackage FighterPackage { get => _fighterPackage; set { _fighterPackage = value; OnPropertyChanged(nameof(FighterPackage)); } }
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

        public Dictionary<string, TrophyType> TrophyTypes { get => typeof(TrophyType).GetDictionary<TrophyType>(); }

        [DependsUpon(nameof(FighterPackage))]
        public TrophyType SelectedTrophyType { get => _selectedTrophyType; set { _selectedTrophyType = value; ChangeSelectedTrophyType(value); OnPropertyChanged(nameof(SelectedTrophyType)); } }

        [DependsUpon(nameof(FighterPackage))]
        [DependsUpon(nameof(SelectedTrophyType))]
        public FighterTrophy SelectedFighterTrophy { get => FighterPackage?.Trophies?.FirstOrDefault(x => x.Type == SelectedTrophyType); }

        [DependsUpon(nameof(SelectedRosterEntry))]
        [DependsUpon(nameof(SelectedRoster))]
        public bool ShowCssCheckBoxes { get => SelectedRosterEntry != null && SelectedRoster?.RosterType == RosterType.CSS; }

        [DependsUpon(nameof(SelectedRosterEntry))]
        [DependsUpon(nameof(SelectedRoster))]
        public bool ShowRosterEntryNameField { get => SelectedRosterEntry != null && SelectedRoster?.RosterType == RosterType.CodeMenu; }

        // Methods
        public void LoadFighterFromRoster(object param)
        {
            if (param != null)
            {
                var rosterEntry = (RosterEntry)param;
                var info = FighterList.FirstOrDefault(x => x.Ids.GetIdOfType(SelectedRoster.IdType) == rosterEntry.Id);
                if (info != null)
                {
                    LoadFighter(info);
                }
                else
                {
                    _dialogService.ShowMessage("Could not find fighter in fighter list.", "Fighter Not Found");
                }
            }
        }

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
                    SendFighterLoadedMessage(FighterPackage);
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
                    else
                    {
                        // Update all icons to have a null ID so they'll be seen as new
                        foreach (var icon in FighterPackage.Cosmetics.Items.Where(x => x.CosmeticType == CosmeticType.FranchiseIcon).ToList())
                        {
                            icon.Id = null;
                        }
                    }
                }
                _oldVictoryThemePath = FighterPackage.VictoryTheme.SongPath;
                _oldCreditsThemePath = FighterPackage.CreditsTheme.SongPath;
                // Set package path to newly opened fighter
                FighterPackagePath = file;
                OnPropertyChanged(nameof(FighterPackage));
                SendFighterLoadedMessage(FighterPackage);
            }
        }

        public void NewFighter()
        {
            // Reset old fighter package
            OldFighterPackage = null;
            FighterPackage = new FighterPackage();
            FighterPackage.PackageType = PackageType.New;
            FighterPackage.FighterInfo.FighterAttributes = new FighterAttributes();
            FighterPackage.FighterInfo.FighterAttributes.Version = _settingsService.BuildSettings.MiscSettings.DefaultExConfigVersion;
            FighterPackage.FighterInfo.CosmeticAttributes = new CosmeticAttributes();
            FighterPackage.FighterInfo.CosmeticAttributes.Version = _settingsService.BuildSettings.MiscSettings.DefaultExConfigVersion;
            FighterPackage.FighterInfo.SlotAttributes = new SlotAttributes();
            FighterPackage.FighterInfo.SlotAttributes.Version = _settingsService.BuildSettings.MiscSettings.DefaultExConfigVersion;
            FighterPackage.FighterInfo.CSSSlotAttributes = new CSSSlotAttributes();
            FighterPackage.FighterInfo.CSSSlotAttributes.Version = _settingsService.BuildSettings.MiscSettings.DefaultExConfigVersion;
            _oldVictoryThemePath = FighterPackage.VictoryTheme.SongPath;
            _oldCreditsThemePath = FighterPackage.CreditsTheme.SongPath;
            // Set IDs to first available
            FighterPackage.FighterInfo = _fighterService.UpdateIdsToFirstUnused(FighterPackage.FighterInfo);
            FighterPackage.FighterSettings.LLoadCharacterId = FighterPackage.FighterInfo.Ids.CSSSlotConfigId;
            FighterPackage.FighterSettings.SSESubCharacterId = FighterPackage.FighterInfo.Ids.CSSSlotConfigId;
            OnPropertyChanged(nameof(FighterPackage));
            SendFighterLoadedMessage(FighterPackage);
        }

        public void DeleteFighter()
        {
            if (_dialogService.ShowMessage("WARNING! You are about to delete the currently loaded fighter. Are you sure?", "Delete Fighter", MessageBoxButton.YesNo, MessageBoxImage.Warning))
            {
                _fileService.StartBackup();
                // Set up delete package
                var deletePackage = new FighterPackage
                {
                    PackageType = PackageType.Delete,
                    FighterInfo = FighterPackage.FighterInfo.CopyNoAttributes()
                };
                deletePackage.FighterInfo.DisplayName = "Unknown";
                deletePackage.FighterSettings.LLoadCharacterId = FighterPackage.FighterInfo.Ids.CSSSlotConfigId;
                // Prompt for items to delete if applicable
                deletePackage = SelectDeleteOptions(deletePackage);
                // Update UI
                FighterPackage = null;
                OnPropertyChanged(nameof(FighterPackage));
                // Save
                _dialogService.ShowProgressBar("Deleting Fighter", "Deleting fighter...");
                using (new CursorWait())
                {
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
                    WeakReferenceMessenger.Default.Send(new FighterSavedMessage(FighterPackage));
                    // Compile GCT
                    _codeService.CompileCodes();
                    _fileService.EndBackup();
                }
                _dialogService.CloseProgressBar();
                _dialogService.ShowMessage("Changes saved.", "Saved");
            }
        }

        public void SaveFighter()
        {
            if (ErrorValidate() != true || Validate() != true)
            {
                return;
            }
            // Get install options
            var selectedInstallOptions = new List<FighterInstallOption>();
            var installOptionsGroups = new List<RadioButtonGroup>();
            foreach(var installGroup in FighterPackage.InstallOptions.GroupBy(x => x.Type).Where(x => x.Count() > 1))
            {
                var group = new RadioButtonGroup { DisplayName = installGroup.Key.GetDescription(), GroupName = installGroup.Key.ToString() };
                foreach(var installOption in installGroup)
                {
                    group.Items.Add(new RadioButtonItem(installOption, installOption.Name, installOption.Description, installOption.Type.ToString(), installOption == FighterPackage.InstallOptions.FirstOrDefault(x => x.Type == installOption.Type)));
                }
                installOptionsGroups.Add(group);
            }
            if (installOptionsGroups.Count > 0)
            {
                selectedInstallOptions = _dialogService.OpenRadioButtonDialog(installOptionsGroups, "Install Options", "Select options to install").Where(x => x.IsChecked).Select(x => x.Item as FighterInstallOption).ToList();
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
            packageToSave = ApplyInstallOptions(selectedInstallOptions, packageToSave);
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
            packageToSave = SelectDeleteOptions(packageToSave);
            using (new CursorWait())
            {
                _dialogService.ShowProgressBar("Installing Fighter", "Installing fighter...");
                _fileService.StartBackup();
                // Save fighter
                _packageService.SaveFighter(packageToSave, OldFighterPackage);
                // Save was successful, so load changes
                FighterPackage = packageToSave;
                // Remove added franchise icons from package
                FighterPackage.Cosmetics.Items.RemoveAll(x => x.CosmeticType == CosmeticType.FranchiseIcon && FighterPackage.Cosmetics.HasChanged(x));
                // Clear changes on all cosmetics
                FighterPackage.Cosmetics.Items.ForEach(x => { x.ImagePath = ""; x.ModelPath = ""; x.ColorSmashChanged = false; });
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
                OldFighterPackage = packageToSave.Copy();
                // Update UI
                OnPropertyChanged(nameof(FighterPackage));
                OnPropertyChanged(nameof(FighterList));
                SendFighterLoadedMessage(FighterPackage);
                WeakReferenceMessenger.Default.Send(new UpdateFighterListMessage(_settingsService.FighterInfoList));
                WeakReferenceMessenger.Default.Send(new FighterSavedMessage(FighterPackage));
                // Compile GCT
                _codeService.CompileCodes();
                _fileService.EndBackup();
            }
            _dialogService.CloseProgressBar();
            _dialogService.ShowMessage("Changes saved.", "Saved");
        }

        private FighterPackage ApplyInstallOptions(List<FighterInstallOption> installOptions, FighterPackage fighterPackage)
        {
            foreach(var installOption in installOptions.Where(x => !string.IsNullOrEmpty(x.File)))
            {
                if (installOption.Type == InstallOptionType.Module)
                {
                    fighterPackage.Module = installOption.File;
                }
                else if (installOption.Type == InstallOptionType.Sounbank)
                {
                    fighterPackage.Soundbank = installOption.File;
                }
                else if (installOption.Type == InstallOptionType.KirbySoundbank)
                {
                    fighterPackage.KirbySoundbank = installOption.File;
                }
                else if (installOption.Type == InstallOptionType.MovesetFile)
                {
                    var movesetPac = fighterPackage.PacFiles.FirstOrDefault(x => x.FileType == FighterFileType.FighterPacFile && string.IsNullOrEmpty(x.Suffix));
                    if (movesetPac != null)
                    {
                        movesetPac.FilePath = installOption.File;
                    }
                }
            }
            return fighterPackage;
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
                // Set package path to new file
                FighterPackagePath = file;
                // Set package to new
                FighterPackage.PackageType = PackageType.New;
                // Reset old fighter package
                OldFighterPackage = null;
                _dialogService.ShowMessage("Exported successfully.", "Success");
                OnPropertyChanged(nameof(FighterPackage));
                SendFighterLoadedMessage(FighterPackage);
            }
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
                var foundMatches = new List<RosterEntry>();
                foreach(var roster in Rosters.Where(x => x.RosterType != RosterType.CodeMenu || _settingsService.BuildSettings.MiscSettings.UpdateCodeMenuNames))
                {
                    foundMatches.AddRange(roster.Entries.Where(x => x.Id == fighterInfo.Ids.GetIdOfType(roster.IdType) && !updatedMatches.Contains(x)));
                }
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
                    var id = fighterInfo.Ids.GetIdOfType(roster.IdType);
                    var newEntry = new RosterEntry
                    {
                        Id = id != null ? id.Value : 0,
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
                    var id = fighterInfo.Ids.GetIdOfType(roster.IdType);
                    var foundEntry = roster.Entries.FirstOrDefault(x => x.Id == id);
                    if (foundEntry != null)
                    {
                        if (packageType == PackageType.Delete)
                        {
                            roster.Entries.Remove(foundEntry);
                        }
                        else if (packageType == PackageType.Update && (roster.RosterType != RosterType.CodeMenu || _settingsService.BuildSettings.MiscSettings.UpdateCodeMenuNames))
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
                var id = SelectedFighter.Ids.GetIdOfType(SelectedRoster.IdType);
                var newEntry = new RosterEntry
                {
                    Id = id != null ? id.Value : 0,
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
                    var id = fighter.Ids.GetIdOfType(SelectedRoster.IdType);
                    var newEntry = new RosterEntry
                    {
                        Id = id != null ? id.Value : 0,
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
            _fileService.StartBackup();
            using (new CursorWait())
            {
                _dialogService.ShowProgressBar("Saving Rosters", "Saving rosters...");
                _fighterService.SaveRosters(Rosters);
                _codeService.CompileCodes();
                _dialogService.CloseProgressBar();
            }
            _fileService.EndBackup();
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
            var changedTrophies = FighterPackage.Trophies.Where(x => x.Trophy.Ids.TrophyId != x.OldTrophy?.Ids.TrophyId || x.Trophy.Name != x.OldTrophy?.Name);
            if (changedTrophies.Any())
            {
                var trophyConflict = _trophyService.GetTrophyList().FirstOrDefault(x => changedTrophies.Any(y => y.Trophy.Ids.TrophyId == x.Ids.TrophyId
                && y.OldTrophy?.Ids.TrophyId != x.Ids.TrophyId && y.Trophy.Name == x.Name));
                if (trophyConflict != null)
                {
                    messages.Add(new DialogMessage("Trophies", "One or more trophies has the same name and ID as another trophy in the build. Change either the name or ID to continue."));
                    result = false;
                }
            }
            if ((!string.IsNullOrEmpty(FighterPackage?.VictoryTheme?.SongFile) && string.IsNullOrEmpty(FighterPackage?.VictoryTheme?.SongPath))
                || !string.IsNullOrEmpty(FighterPackage?.CreditsTheme?.SongFile) && string.IsNullOrEmpty(FighterPackage?.CreditsTheme?.SongPath))
            {
                messages.Add(new DialogMessage("Missing Song Paths", "One or more songs have a file, but a blank name/path. Add a path to these files to continue."));
                result = false;
            }
            List<(FighterPacFile PacFile, string Name)> allPacNames = FighterPackage.PacFiles.Select(x => (x, $"{x.GetPrefix(FighterPackage.FighterInfo)}{x.Suffix}")).ToList();
            allPacNames.AddRange(FighterPackage.Costumes.SelectMany(c => c.PacFiles.Select(x => (x, $"{x.GetPrefix(FighterPackage.FighterInfo)}{x.Suffix}{c.CostumeId:D2}"))));
            var duplicatePacs = allPacNames.GroupBy(x => x.Name).Where(g => g.Select(item => item.PacFile.FilePath).Distinct().Count() > 1).Where(g => g.Count() > 1);
            if (duplicatePacs.Any())
            {
                var duplicatePacStringList = duplicatePacs.Select(group => $"Calculated Name: {group.Key}\nFiles:\n{string.Join("\n",group.Select(x => x.PacFile.FilePath))}");
                var duplicatePacString = string.Join("\n\n", duplicatePacStringList);
                messages.Add(new DialogMessage("Duplicate PAC files", $"One or more PAC files have the exact same name configured. Make all PAC file names unique to continue:\n\n{duplicatePacString}"));
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
                messages.Add(new DialogMessage("Cosmetics", $"Some cosmetics marked as required are missing. Review if they are needed for your fighter:\n\n{cosmeticString}"));
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
            var changedTrophies = FighterPackage.Trophies.Where(x => x.Trophy.Ids.TrophyId != x.OldTrophy?.Ids.TrophyId || x.Trophy.Ids.TrophyThumbnailId != x.OldTrophy?.Ids.TrophyThumbnailId ||
                x.Trophy.Brres != x.OldTrophy?.Brres);
            if (changedTrophies.Any())
            {
                var trophyConflict = _trophyService.GetTrophyList().FirstOrDefault(x => changedTrophies.Any(y => y.Trophy.Ids.TrophyId == x.Ids.TrophyId 
                && y.OldTrophy?.Ids.TrophyId != x.Ids.TrophyId || (y.OldTrophy?.Ids.TrophyId != x.Ids.TrophyId && 
                (y.Trophy.Ids.TrophyThumbnailId == x.Ids.TrophyThumbnailId || y.Trophy.Brres == x.Brres))));
                if (trophyConflict != null)
                {
                    messages.Add(new DialogMessage("Trophy Conflicts", "One of the fighter's trophies shares an ID, thumbnail, or BRRES with another trophy in your build.\n\nIf two trophies have the same ID, the fighter will load the FIRST trophy in the build. Change your trophy's ID or change the order of trophies after saving to ensure they are ordered correctly.\n\nIf trophies have the same thumbnail ID or BRRES, the existing thumbnail/BRRES will be overwritten. If this is undesired, change the thumbnail ID or BRRES name."));
                }
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
            foreach (var path in paths.Where(x => !string.IsNullOrEmpty(x)))
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
                if (franchiseIconDefinitions.Any(x => x.Style == "Icon") && FranchiseIconViewModel.SelectedFranchiseIcon?.Image == null && string.IsNullOrEmpty(FranchiseIconViewModel.SelectedFranchiseIcon?.ImagePath))
                {
                    missingCosmetics.Add((CosmeticType.FranchiseIcon, "Icon"));
                }
                // If there's franchise icon model, add it
                if (franchiseIconDefinitions.Any(x => x.Style == "Model") && FranchiseIconViewModel.SelectedFranchiseIcon?.Model == null && string.IsNullOrEmpty(FranchiseIconViewModel.SelectedFranchiseIcon?.ModelPath))
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

        private FighterPackage SelectDeleteOptions(FighterPackage fighterPackage)
        {
            var deleteOptions = new List<CheckListItem>();
            // Victory Theme
            if ((fighterPackage.PackageType == PackageType.Update && fighterPackage.VictoryTheme != null && fighterPackage.VictoryTheme.SongPath != _oldVictoryThemePath)
                || fighterPackage.PackageType == PackageType.Delete && FighterPackage.VictoryTheme != null && !string.IsNullOrEmpty(FighterPackage.VictoryTheme.SongFile))
            {
                deleteOptions.Add(new CheckListItem("VictoryTheme", "Victory Theme BRSTM", "The victory theme BRSTM file", true));
            }
            if ((fighterPackage.PackageType == PackageType.Update && fighterPackage.VictoryTheme != null && fighterPackage.VictoryTheme.SongId != OldFighterPackage?.VictoryTheme.SongId)
                || fighterPackage.PackageType == PackageType.Delete)
            {
                deleteOptions.Add(new CheckListItem("VictoryEntry", "Victory Tracklist Entry", "The tracklist entry for the victory theme", true));
            }
            // Credits Theme
            if ((fighterPackage.PackageType == PackageType.Update && fighterPackage.CreditsTheme != null && fighterPackage.CreditsTheme.SongPath != _oldCreditsThemePath)
                || fighterPackage.PackageType == PackageType.Delete && FighterPackage.CreditsTheme != null && !string.IsNullOrEmpty(FighterPackage.CreditsTheme.SongFile))
            {
                deleteOptions.Add(new CheckListItem("CreditsTheme", "Credits Theme BRSTM", "The credits theme BRSTM file", true));
            }
            if ((fighterPackage.PackageType == PackageType.Update && fighterPackage.CreditsTheme != null && fighterPackage.CreditsTheme.SongId != OldFighterPackage?.CreditsTheme.SongId)
                || fighterPackage.PackageType == PackageType.Delete)
            {
                deleteOptions.Add(new CheckListItem("CreditsEntry", "Credits Tracklist Entry", "The tracklist entry for the credits theme", true));
            }
            // Franchise icon
            if (FranchiseIconViewModel.FranchiseIconList.Items.Any(x => x.Id == FighterPackage.FighterInfo.Ids.FranchiseId) && fighterPackage.PackageType == PackageType.Delete)
            {
                deleteOptions.Add(new CheckListItem("FranchiseIcon", "Franchise Icon", "The selected franchise icon", false, FranchiseIconViewModel.SelectedFranchiseIcon.Image));
            }
            // Cosmetics
            if (fighterPackage.PackageType == PackageType.Delete)
            {
                deleteOptions.Add(new CheckListItem("Cosmetics", "Cosmetics", "All fighter cosmetics", true));
            }
            // Trophies
            if (fighterPackage.PackageType == PackageType.Delete)
            {
                deleteOptions.Add(new CheckListItem("Trophies", "Trophies", "Trophies associated with fighter",
                    true, FighterPackage?.Trophies?.FirstOrDefault()?.Trophy?.Thumbnails?.Items?.FirstOrDefault()?.Image));
            }
            // Open dialog
            if (deleteOptions.Count > 0)
            {
                var items = _dialogService.OpenCheckListDialog(deleteOptions, "Select items to delete", "The following items were changed, but may be shared by other fighters. Ensure they are not used by other fighters and then select the items you would like to delete.");
                var selectedItems = items.Where(x => x.IsChecked);
                if (items.Any(x => (string)x.Item == "VictoryTheme"))
                {
                    fighterPackage.FighterDeleteOptions.DeleteVictoryTheme = selectedItems.Any(x => (string)x.Item == "VictoryTheme");
                }
                if (items.Any(x => (string)x.Item == "CreditsTheme"))
                {
                    fighterPackage.FighterDeleteOptions.DeleteCreditsTheme = selectedItems.Any(x => (string)x.Item == "CreditsTheme");
                }
                if (items.Any(x => (string)x.Item == "VictoryEntry"))
                {
                    fighterPackage.FighterDeleteOptions.DeleteVictoryEntry = selectedItems.Any(x => (string)x.Item == "VictoryEntry");
                }
                if (items.Any(x => (string)x.Item == "CreditsEntry"))
                {
                    fighterPackage.FighterDeleteOptions.DeleteCreditsEntry = selectedItems.Any(x => (string)x.Item == "CreditsEntry");
                }
                if(selectedItems.Any(x => (string)x.Item == "Cosmetics"))
                {
                    // Mark all cosmetics as changed
                    foreach (var cosmetic in FighterPackage.Cosmetics.Items)
                    {
                        fighterPackage.Cosmetics.ItemChanged(cosmetic);
                    }
                }
                if (selectedItems.Any(x => (string)x.Item == "FranchiseIcon"))
                {
                    fighterPackage.Cosmetics.ItemChanged(FranchiseIconViewModel.FranchiseIconList.Items.FirstOrDefault(x => x.Id == FighterPackage.FighterInfo.Ids.FranchiseId));
                }
                if (selectedItems.Any(x => (string)x.Item == "Trophies"))
                {
                    foreach(var fighterTrophy in FighterPackage.Trophies.GroupBy(x => x.Trophy).Select(group => group.FirstOrDefault()))
                    {
                        var deleteTrophy = new FighterTrophy { Trophy = null, OldTrophy = fighterTrophy?.Trophy?.Copy(), Type = fighterTrophy.Type };
                        deleteTrophy?.Trophy?.Thumbnails?.MarkAllChanged();
                        fighterPackage.Trophies.Add(deleteTrophy);
                    }
                }
            }
            return fighterPackage;
        }

        private void ChangeSelectedTrophyType(TrophyType trophyType)
        {
            var trophy = FighterPackage?.Trophies?.FirstOrDefault(x => x.Type == trophyType);
            WeakReferenceMessenger.Default.Send(new TrophyChangedMessage(trophy?.Trophy));
            OnPropertyChanged(nameof(SelectedTrophyType));
        }

        private void SendFighterLoadedMessage(FighterPackage fighterPackage)
        {
            WeakReferenceMessenger.Default.Send(new FighterLoadedMessage(fighterPackage));
            SelectedTrophyType = TrophyTypes.FirstOrDefault().Value;
        }

        private void ChangeTrophy()
        {
            var trophyList = _trophyService.GetTrophyList();
            var trophySelect = _dialogService.OpenDropDownDialog(trophyList, "Name", "Select a trophy", "Select a trophy to replace current trophy") as Trophy;
            if (trophySelect != null)
            {
                var trophyMatch = FighterPackage.Trophies.FirstOrDefault(x => x.OldTrophy.Ids.TrophyId == trophySelect.Ids.TrophyId && x.OldTrophy.Name == trophySelect.Name
                    && x.Type != SelectedTrophyType);
                var newTrophy = new FighterTrophy();
                var selectedTrophy = FighterPackage.Trophies.FirstOrDefault(x => x.Type == SelectedTrophyType);
                // If this trophy is already selected for a different trophy type, use the same reference
                if (trophyMatch != null)
                {
                    newTrophy.Trophy = trophyMatch.Trophy;
                    newTrophy.OldTrophy = trophyMatch.OldTrophy;
                    newTrophy.Type = SelectedTrophyType;
                }
                // Otherwise, load it
                else
                {
                    trophySelect = _trophyService.LoadTrophyData(trophySelect);
                    newTrophy = new FighterTrophy { Trophy = trophySelect, Type = SelectedTrophyType, OldTrophy = trophySelect.Copy() };
                }
                ChangeTrophy(newTrophy, selectedTrophy);
            }
        }

        private void NewTrophy()
        {
            var selectedTrophy = FighterPackage.Trophies.FirstOrDefault(x => x.Type == SelectedTrophyType);
            var newTrophy = new FighterTrophy { Trophy = new Trophy(), Type = SelectedTrophyType, OldTrophy = null };
            ChangeTrophy(newTrophy, selectedTrophy);
        }

        private void ClearTrophy()
        {
            var selectedTrophy = FighterPackage.Trophies.FirstOrDefault(x => x.Type == SelectedTrophyType);
            ChangeTrophy(null, selectedTrophy);
        }

        private void ChangeTrophy(FighterTrophy newTrophy, FighterTrophy oldTrophy)
        {
            // Remove selected trophy if it exists
            if (oldTrophy != null)
            {
                FighterPackage.Trophies.Remove(oldTrophy);
            }
            // Add new trophy
            if (newTrophy != null)
            {
                FighterPackage.Trophies.Add(newTrophy);
            }
            WeakReferenceMessenger.Default.Send(new TrophyChangedMessage(newTrophy?.Trophy));
            OnPropertyChanged(nameof(SelectedFighterTrophy));
        }

        private void ExportCosmetics()
        {
            var path = _dialogService.OpenFolderDialog("Select directory to save cosmetics to");
            if (!string.IsNullOrEmpty(path))
            {
                var cosmeticsToExport = new CosmeticList();
                cosmeticsToExport.Items.AddRange(FighterPackage.Cosmetics.Items);
                cosmeticsToExport.Items.Add(FranchiseIconViewModel.SelectedFranchiseIcon);
                _cosmeticService.ExportCosmetics(path, cosmeticsToExport);
                _dialogService.ShowMessage("Success", "Cosmetics exported.");
            }
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

    public class TrophyChangedMessage : ValueChangedMessage<Trophy>
    {
        public TrophyChangedMessage(Trophy trophy) : base(trophy)
        {
        }
    }

    public class FighterSavedMessage : ValueChangedMessage<FighterPackage>
    {
        public FighterSavedMessage(FighterPackage fighterPackage) : base(fighterPackage)
        {

        }
    }
}
