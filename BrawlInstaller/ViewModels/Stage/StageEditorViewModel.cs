using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Helpers;
using BrawlInstaller.Services;
using BrawlLib.Internal;
using BrawlLib.SSBB.ResourceNodes.ProjectPlus;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static BrawlInstaller.ViewModels.MainControlsViewModel;
using static BrawlLib.SSBB.ResourceNodes.ProjectPlus.STEXNode;

namespace BrawlInstaller.ViewModels
{
    public interface IStageEditorViewModel
    {

    }

    [Export(typeof(IStageEditorViewModel))]
    internal class StageEditorViewModel : ViewModelBase, IStageEditorViewModel
    {
        // Private properties
        private StageInfo _stage;
        private StageInfo _oldStage;
        private StageEntry _selectedStageEntry;
        private Substage _selectedSubstage;
        private List<string> _tracklists;
        private string _originalRandomName;

        // Services
        IStageService _stageService { get; }
        IDialogService _dialogService { get; }
        ITracklistService _tracklistService { get; }
        IFileService _fileService { get; }
        ISettingsService _settingsService { get; }

        // Commands
        public ICommand SaveStageCommand => new RelayCommand(param => SaveStage(Stage));
        public ICommand MoveEntryUpCommand => new RelayCommand(param => MoveEntryUp());
        public ICommand MoveEntryDownCommand => new RelayCommand(param => MoveEntryDown());
        public ICommand MoveSubstageUpCommand => new RelayCommand(param => MoveSubstageUp());
        public ICommand MoveSubstageDownCommand => new RelayCommand(param => MoveSubstageDown());
        public ICommand AddStageEntryCommand => new RelayCommand(param =>  AddStageEntry());
        public ICommand RemoveStageEntryCommand => new RelayCommand(param => RemoveStageEntry());
        public ICommand AddStageParamCommand => new RelayCommand(param => AddStageParam());
        public ICommand RemoveStageParamCommand => new RelayCommand(param => RemoveStageParam());
        public ICommand AddSubstageCommand => new RelayCommand(param => AddSubstage());
        public ICommand RemoveSubstageCommand => new RelayCommand(param => RemoveSubstage());
        public ICommand DeleteStageCommand => new RelayCommand(param => DeleteStage());
        public ICommand ImportParamsCommand => new RelayCommand(param => ImportParams());
        public ICommand UpdateListAltImageCommand => new RelayCommand(param => UpdateListAltImage());
        public ICommand UpdateListAltHDImageCommand => new RelayCommand(param => UpdateListAltHDImage());

        [ImportingConstructor]
        public StageEditorViewModel(IStageService stageService, IDialogService dialogService, ITracklistService tracklistService, IFileService fileService, ISettingsService settingsService, IStageCosmeticViewModel stageCosmeticViewModel)
        {
            _stageService = stageService;
            _dialogService = dialogService;
            _tracklistService = tracklistService;
            _fileService = fileService;
            _settingsService = settingsService;
            StageCosmeticViewModel = stageCosmeticViewModel;

            WeakReferenceMessenger.Default.Register<StageLoadedMessage>(this, (recipient, message) =>
            {
                LoadStage(message);
            });
            WeakReferenceMessenger.Default.Register<UpdateSettingsMessage>(this, (recipient, message) =>
            {
                UpdateSettings();
            });
            WeakReferenceMessenger.Default.Register<SettingsSavedMessage>(this, (recipient, message) =>
            {
                UpdateSettings();
            });
        }

        // Properties
        public StageInfo Stage { get => _stage; set { _stage = value; OnPropertyChanged(nameof(Stage)); } }
        public StageInfo OldStage { get => _oldStage; set { _oldStage = value; OnPropertyChanged(nameof(OldStage)); } }

        [DependsUpon(nameof(Stage))]
        public ObservableCollection<StageEntry> StageEntries { get => Stage?.StageEntries != null ? new ObservableCollection<StageEntry>(Stage.StageEntries) : new ObservableCollection<StageEntry>(); }

        [DependsUpon(nameof(Stage))]
        [DependsUpon(nameof(StageEntries))]
        public StageEntry SelectedStageEntry { get => _selectedStageEntry; set { _selectedStageEntry = value; OnPropertyChanged(nameof(SelectedStageEntry)); } }

        [DependsUpon(nameof(Stage))]
        public ObservableCollection<StageParams> ParamList { get => Stage?.AllParams != null ? new ObservableCollection<StageParams>(Stage.AllParams) :  new ObservableCollection<StageParams>(); }

        [DependsUpon(nameof(SelectedStageEntry))]
        public ushort SelectedButtonFlags { get => SelectedStageEntry?.ButtonFlags ?? 0x0; set { SelectedStageEntry.ButtonFlags = value; OnPropertyChanged(nameof(SelectedButtonFlags)); } }

        [DependsUpon(nameof(SelectedButtonFlags))]
        public bool ButtonsEnabled { get => SelectedButtonFlags < 0x4000; }

        [DependsUpon(nameof(ButtonsEnabled))]
        [DependsUpon(nameof(SelectedButtonFlags))]
        public bool ListAltsEnabled { get => SelectedButtonFlags <= 0x0 || !ButtonsEnabled; }

        [DependsUpon(nameof(ListAltsEnabled))]
        public bool LAltsEnabled { get => ListAltsEnabled && (SelectedButtonFlags < 0x4000 || (SelectedButtonFlags >= 0x8000 && SelectedButtonFlags < 0xC000)); }

        [DependsUpon(nameof(ListAltsEnabled))]
        public bool RAltsEnabled { get => ListAltsEnabled && ((SelectedButtonFlags >= 0x4000 && SelectedButtonFlags < 0x8000) || SelectedButtonFlags <= 0x4000); }

        [DependsUpon(nameof(ListAltsEnabled))]
        [DependsUpon(nameof(ButtonsEnabled))]
        [DependsUpon(nameof(SelectedButtonFlags))]
        public bool EventAltsEnabled { get => SelectedButtonFlags <= 0x0 || SelectedButtonFlags >= 0xC000; }

        public Dictionary<string, VariantType> VariantTypes { get => typeof(VariantType).GetDictionary<VariantType>(); }

        [DependsUpon(nameof(SelectedStageEntry))]
        public ObservableCollection<Substage> Substages { get => SelectedStageEntry?.Params?.Substages != null ? new ObservableCollection<Substage>(SelectedStageEntry.Params.Substages) : new ObservableCollection<Substage>(); }

        [DependsUpon(nameof(SelectedStageEntry))]
        [DependsUpon(nameof(Substages))]
        public Substage SelectedSubstage { get => _selectedSubstage; set { _selectedSubstage = value; OnPropertyChanged(nameof(SelectedSubstage)); } }

        [DependsUpon(nameof(SelectedButtonFlags))]
        public bool RListAlt { get => SelectedButtonFlags >= 0x4000 && SelectedButtonFlags < 0x8000; }

        [DependsUpon(nameof(SelectedButtonFlags))]
        public bool LListAlt { get => SelectedButtonFlags >= 0x8000 && SelectedButtonFlags < 0xC000; }

        [DependsUpon(nameof(LListAlt))]
        [DependsUpon(nameof(RListAlt))]
        public bool ListAlt { get => LListAlt || RListAlt; }

        [DependsUpon(nameof(SelectedButtonFlags))]
        public bool EventAlt { get => SelectedButtonFlags >= 0xC000; }

        [DependsUpon(nameof(SelectedStageEntry))]
        [DependsUpon(nameof(SelectedButtonFlags))]
        public string BinIndexString { get => RListAlt ? StageEntries.Where(x => x.IsRAlt).ToList().IndexOf(SelectedStageEntry).ToString("D2")
                : (LListAlt ? StageEntries.Where(x => x.IsLAlt).ToList().IndexOf(SelectedStageEntry).ToString("D2") : "");
        }

        public List<string> Tracklists { get => _tracklists; set { _tracklists = value; OnPropertyChanged(nameof(Tracklists)); } }

        [DependsUpon(nameof(SelectedStageEntry))]
        public string SelectedBinFilePath { get => SelectedStageEntry?.ListAlt?.BinFilePath; set { SelectedStageEntry.ListAlt.BinFilePath = value; UpdateListAlt(value); OnPropertyChanged(nameof(SelectedBinFilePath)); } }

        // ViewModels
        public IStageCosmeticViewModel StageCosmeticViewModel { get; }

        // Methods
        private void UpdateSettings()
        {
            Stage = null;
            OnPropertyChanged(nameof(Stage));
        }
        public void LoadStage(StageLoadedMessage message)
        {
            Stage = message.Value.Stage;
            OldStage = message.Value.NewStage ? null : message.Value.Stage.Copy();
            _originalRandomName = Stage.RandomName;
            Tracklists = _tracklistService.GetTracklists();
            OnPropertyChanged(nameof(Stage));
            OnPropertyChanged(nameof(Tracklists));
        }

        public void SaveStage(StageInfo stage, bool deleteStage=false)
        {
            if (!ErrorValidate())
            {
                return;
            }
            var oldStage = Stage;
            // Create copy of stage before save
            var stageToSave = stage.Copy();

            // Get delete options
            var deleteOptions = OldStage?.StageEntries?.Select(x => x.Params.ModuleFile).Where(x => Stage == null || !Stage.StageEntries.Select(y => y.Params.ModuleFile).Contains(x)).Where(x => x != null).Distinct().ToList() ?? new List<string>();
            deleteOptions.AddRange(OldStage?.StageEntries?.Select(x => x.Params.TrackListFile).Where(x => Stage == null || !Stage.StageEntries.Select(y => y.Params.TrackListFile).Contains(x)).Where(x => x != null).Distinct().ToList() ?? new List<string>());
            // Add netplay tracklists if syncing is on
            if (_settingsService.BuildSettings.MiscSettings.SyncTracklists && !string.IsNullOrEmpty(_settingsService.BuildSettings.FilePathSettings.NetplaylistPath))
            {
                var netplayListPath = _settingsService.GetBuildFilePath(_settingsService.BuildSettings.FilePathSettings.NetplaylistPath);
                deleteOptions.AddRange(OldStage?.StageEntries?.Select(x => x.Params.TrackListFile).Where(x => Stage == null || !Stage.StageEntries.Select(y => y.Params.TrackListFile).Contains(x)).Select(x => Path.Combine(netplayListPath, Path.GetFileName(x))).Distinct().Where(x => _fileService.FileExists(x)).ToList() ?? new List<string>());
            }

            // Prompt user for delete options
            var deleteItems = new List<CheckListItem>();
            var stageDeleteOptions = new List<string>();
            foreach (var item in deleteOptions)
            {
                deleteItems.Add(new CheckListItem(item, Path.GetFileName(item), item));
            }
            if (deleteItems.Count > 0)
            {
                var selectedItems = _dialogService.OpenCheckListDialog(deleteItems, "Select items to delete", "The following items are no longer found in this stage slot, but may be used by other stages. Verify they are not used by other stages and then select the options you wish to delete.").Where(x => x.IsChecked);
                foreach (var selectedItem in selectedItems)
                {
                    stageDeleteOptions.Add((string)selectedItem.Item);
                }
            }

            _fileService.StartBackup();
            _dialogService.ShowProgressBar("Saving Stage Changes", "Saving stage changes...");
            // Save stage
            using (new CursorWait())
            {
                _stageService.SaveStage(stageToSave, OldStage, stageDeleteOptions, _originalRandomName != stageToSave.RandomName);

                // Stage saving was successful, so load changes
                Stage = stageToSave;
                stage = Stage;

                stage.Cosmetics.Items.ForEach(x => { x.ImagePath = ""; x.ModelPath = ""; x.ColorSmashChanged = false; });
                stage.Cosmetics.ClearChanges();

                // Update loaded stage
                WeakReferenceMessenger.Default.Send(new StageLoadedMessage(new StageLoadObject(stage)));

                if (deleteStage)
                {
                    // Update stage list
                    WeakReferenceMessenger.Default.Send(new StageDeletedMessage(oldStage.Slot));
                    Stage = null;
                }
                else
                {
                    // Update stage list
                    WeakReferenceMessenger.Default.Send(new StageSavedMessage(stage));
                }
            }
            _dialogService.CloseProgressBar();
            _fileService.EndBackup();
            OnPropertyChanged(nameof(Stage));
            _dialogService.ShowMessage("Changes saved.", "Saved");
        }

        private bool ErrorValidate()
        {
            var messages = new List<DialogMessage>();
            var result = true;
            if (Stage.StageEntries.Any(x => string.IsNullOrEmpty(x.Params.PacName) || string.IsNullOrEmpty(x.Params.Module) || string.IsNullOrEmpty(x.Params.Name)))
            {
                messages.Add(new DialogMessage("Missing File Names", "One or more PAC files, modules, or entries are missing a name. Fill in all missing names to continue."));
                result = false;
            }
            if (Stage.StageEntries.Any(x => !string.IsNullOrEmpty(x.Params.Module) && (!x.Params.Module.ToLower().StartsWith("st_") || !x.Params.Module.ToLower().EndsWith(".rel"))))
            {
                messages.Add(new DialogMessage("Invalid Module Names", "One or more modules are named incorrectly. Ensure all modules are named in the format 'st_XX.rel', where 'XX' can be anything."));
                result = false;
            }
            if (messages.Count > 0)
                _dialogService.ShowMessages("Errors have occurred that prevent your stage from saving.", "Errors", messages, MessageBoxButton.OK, MessageBoxImage.Error);
            return result;
        }

        private void MoveEntryUp()
        {
            Stage.StageEntries.MoveUp(SelectedStageEntry);
            StageEntries.MoveUp(SelectedStageEntry);
            OnPropertyChanged(nameof(SelectedStageEntry));
            OnPropertyChanged(nameof(StageEntries));
        }

        private void MoveEntryDown()
        {
            Stage.StageEntries.MoveDown(SelectedStageEntry);
            StageEntries.MoveDown(SelectedStageEntry);
            OnPropertyChanged(nameof(SelectedStageEntry));
            OnPropertyChanged(nameof(StageEntries));
        }

        private void MoveSubstageUp()
        {
            SelectedStageEntry.Params.Substages.MoveUp(SelectedSubstage);
            Substages.MoveUp(SelectedSubstage);
            OnPropertyChanged(nameof(SelectedSubstage));
            OnPropertyChanged(nameof(Substages));
        }

        private void MoveSubstageDown()
        {
            SelectedStageEntry.Params.Substages.MoveDown(SelectedSubstage);
            Substages.MoveDown(SelectedSubstage);
            OnPropertyChanged(nameof(SelectedSubstage));
            OnPropertyChanged(nameof(Substages));
        }

        private void AddStageEntry()
        {
            var newEntry = new StageEntry();
            if (Stage.AllParams.Count > 0)
            {
                newEntry.Params = Stage.AllParams.FirstOrDefault();
            }
            else
            {
                Stage.AllParams.Add(newEntry.Params);
            }
            Stage.StageEntries.Add(newEntry);
            SelectedStageEntry = newEntry;
            OnPropertyChanged(nameof(Stage));
            OnPropertyChanged(nameof(StageEntries));
            OnPropertyChanged(nameof(SelectedStageEntry));
        }

        private void RemoveStageEntry()
        {
            Stage.StageEntries.Remove(SelectedStageEntry);
            OnPropertyChanged(nameof(Stage));
            OnPropertyChanged(nameof(StageEntries));
            OnPropertyChanged(nameof(SelectedStageEntry));
        }

        private void AddStageParam()
        {
            var newParams = new StageParams();
            Stage.AllParams.Add(newParams);
            SelectedStageEntry.Params = newParams;
            OnPropertyChanged(nameof(Stage));
            OnPropertyChanged(nameof(SelectedStageEntry));
        }

        private void RemoveStageParam()
        {
            if (Stage.AllParams.Count > 1)
            {
                var removeParams = SelectedStageEntry.Params;
                Stage.AllParams.Remove(removeParams);
                foreach (var stageEntry in Stage.StageEntries.Where(x => x.Params == removeParams))
                {
                    stageEntry.Params = Stage.AllParams.FirstOrDefault();
                }
                OnPropertyChanged(nameof(Stage));
                OnPropertyChanged(nameof(SelectedStageEntry));
                OnPropertyChanged(nameof(StageEntries));
            }
        }

        private void AddSubstage()
        {
            var newSubstage = new Substage();
            SelectedStageEntry.Params.Substages.Add(newSubstage);
            SelectedSubstage = newSubstage;
            OnPropertyChanged(nameof(Stage));
            OnPropertyChanged(nameof(SelectedStageEntry));
            OnPropertyChanged(nameof(Substages));
        }

        private void RemoveSubstage()
        {
            SelectedStageEntry.Params.Substages.Remove(SelectedSubstage);
            OnPropertyChanged(nameof(Stage));
            OnPropertyChanged(nameof(SelectedStageEntry));
            OnPropertyChanged(nameof(Substages));
        }

        private void ImportParams()
        {
            if (Stage?.StageEntries != null)
            {
                var stageParams = _dialogService.OpenMultiFileDialog("Select param files", "PARAM file (.param)|*.param");
                foreach (var param in stageParams)
                {
                    var rootNode = _fileService.OpenFile(param);
                    if (rootNode != null)
                    {
                        var newParams = ((STEXNode)rootNode).ToStageParams();
                        Stage.AllParams.Add(newParams);
                        Stage.StageEntries.Add(new StageEntry { Params = newParams });
                    }
                }
                OnPropertyChanged(nameof(ParamList));
                OnPropertyChanged(nameof(StageEntries));
            }
        }

        private void DeleteStage()
        {
            var result = _dialogService.ShowMessage("WARNING! You are about to delete the currently loaded stage slot. Are you sure?", "Delete Stage", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result)
            {
                var oldSlot = Stage.Slot;
                var selectableCosmeticsToDelete = new List<(CosmeticType CosmeticType, string Style)>();
                // Set to new stage that is missing everything
                var deleteStage = new StageInfo { Slot = new StageSlot { StageIds = new BrawlIds { StageId = Stage.Slot.StageIds.StageId, StageCosmeticId = Stage.Slot.StageIds.StageCosmeticId } } };
                // Mark all non-selectable cosmetics as changed
                foreach (var cosmetic in Stage.Cosmetics.Items)
                {
                    if (Stage.Cosmetics.Items.Count(x => x.CosmeticType == cosmetic.CosmeticType && x.Style == cosmetic.Style) == 1)
                    {
                        deleteStage.Cosmetics.ItemChanged(cosmetic);
                    }
                }
                // Prompt for selectable cosmetics
                var deleteCosmetics = new List<CheckListItem>();
                foreach (var cosmeticGroup in Stage.Cosmetics.Items.Where(x => x.SelectionOption).GroupBy(x => new { x.CosmeticType, x.Style }))
                {
                    var image = Stage.Cosmetics.Items.FirstOrDefault(x => x.CosmeticType == cosmeticGroup.Key.CosmeticType && x.Style == cosmeticGroup.Key.Style 
                    && x.Image != null && !x.SelectionOption)?.Image;
                    deleteCosmetics.Add(new CheckListItem(cosmeticGroup.Key, $"{cosmeticGroup.Key.Style} style {cosmeticGroup.Key.CosmeticType.GetDescription()}",
                        $"Cosmetics of type {cosmeticGroup.Key.CosmeticType.GetDescription()} with style {cosmeticGroup.Key.Style}", false, image));
                }
                if (deleteCosmetics.Count > 0)
                {
                    var selectedItems = _dialogService.OpenCheckListDialog(deleteCosmetics, "Select items to delete", "The following cosmetics may be used by other stages. Verify they are not used by other stages and select the ones you'd like to delete.").Where(x => x.IsChecked);
                    selectableCosmeticsToDelete = selectedItems.Select(x => ((CosmeticType, string))x.Item).ToList();
                }
                // Mark selectable cosmetics as changed
                foreach (var cosmetic in Stage.Cosmetics.Items.Where(x => selectableCosmeticsToDelete.Any(y => y.CosmeticType == x.CosmeticType && y.Style == x.Style)))
                {
                    deleteStage.Cosmetics.ItemChanged(cosmetic);
                }
                SaveStage(deleteStage, true);
                OldStage = null;
            }
        }

        private void UpdateListAltImage()
        {
            if (SelectedStageEntry?.ListAlt != null)
            {
                var image = _dialogService.OpenFileDialog("Select an image", "PNG file (.png)|*.png");
                if (!string.IsNullOrEmpty(image))
                {
                    SelectedStageEntry.ListAlt.Image = _fileService.LoadImage(image);
                    OnPropertyChanged(nameof(SelectedStageEntry));
                }
            }
        }

        private void UpdateListAltHDImage()
        {
            if (SelectedStageEntry?.ListAlt != null)
            {
                var image = _dialogService.OpenFileDialog("Select an image", "PNG file (.png)|*.png");
                if (!string.IsNullOrEmpty(image))
                {
                    SelectedStageEntry.ListAlt.HDImage = _fileService.LoadImage(image);
                    OnPropertyChanged(nameof(SelectedStageEntry));
                }
            }
        }

        private void UpdateListAlt(string filePath)
        {
            if (SelectedStageEntry?.ListAlt != null)
            {
                SelectedStageEntry.ListAlt = _stageService.GetListAlt(filePath);
                OnPropertyChanged(nameof(SelectedStageEntry));
            }
        }
    }

    // Messages
    public class StageSavedMessage : ValueChangedMessage<StageInfo>
    {
        public StageSavedMessage(StageInfo stage) : base(stage)
        {
        }
    }

    public class StageDeletedMessage : ValueChangedMessage<StageSlot>
    {
        public StageDeletedMessage(StageSlot stageSlot) : base(stageSlot)
        {
        }
    }
}
