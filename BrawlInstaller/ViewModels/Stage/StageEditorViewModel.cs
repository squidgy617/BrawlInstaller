﻿using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
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
        private StageEntry _selectedStageEntry;
        private Substage _selectedSubstage;
        private List<string> _tracklists;
        private string _originalRandomName;

        // Services
        IStageService _stageService { get; }
        IDialogService _dialogService { get; }
        ITracklistService _tracklistService { get; }
        IFileService _fileService { get; }

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

        [ImportingConstructor]
        public StageEditorViewModel(IStageService stageService, IDialogService dialogService, ITracklistService tracklistService, IFileService fileService, IStageCosmeticViewModel stageCosmeticViewModel)
        {
            _stageService = stageService;
            _dialogService = dialogService;
            _tracklistService = tracklistService;
            _fileService = fileService;
            StageCosmeticViewModel = stageCosmeticViewModel;

            WeakReferenceMessenger.Default.Register<StageLoadedMessage>(this, (recipient, message) =>
            {
                LoadStage(message);
            });
        }

        // Properties
        public StageInfo Stage { get => _stage; set { _stage = value; OnPropertyChanged(nameof(Stage)); } }

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
        public bool LAltsEnabled { get => ListAltsEnabled && (SelectedButtonFlags < 0x4000 || SelectedButtonFlags >= 0x8000); }

        [DependsUpon(nameof(ListAltsEnabled))]
        public bool RAltsEnabled { get => ListAltsEnabled && ((SelectedButtonFlags >= 0x4000 && SelectedButtonFlags < 0x8000) || SelectedButtonFlags <= 0x4000); }

        public Dictionary<string, VariantType> VariantTypes { get => typeof(VariantType).GetDictionary<VariantType>(); }

        [DependsUpon(nameof(SelectedStageEntry))]
        public ObservableCollection<Substage> Substages { get => SelectedStageEntry?.Params?.Substages != null ? new ObservableCollection<Substage>(SelectedStageEntry.Params.Substages) : new ObservableCollection<Substage>(); }

        [DependsUpon(nameof(SelectedStageEntry))]
        [DependsUpon(nameof(Substages))]
        public Substage SelectedSubstage { get => _selectedSubstage; set { _selectedSubstage = value; OnPropertyChanged(nameof(SelectedSubstage)); } }

        [DependsUpon(nameof(SelectedButtonFlags))]
        public bool RListAlt { get => SelectedButtonFlags >= 0x4000 && SelectedButtonFlags < 0x8000; }

        [DependsUpon(nameof(SelectedButtonFlags))]
        public bool LListAlt { get => SelectedButtonFlags >= 0x8000; }

        [DependsUpon(nameof(LListAlt))]
        [DependsUpon(nameof(RListAlt))]
        public bool ListAlt { get => LListAlt || RListAlt; }

        [DependsUpon(nameof(SelectedStageEntry))]
        [DependsUpon(nameof(SelectedButtonFlags))]
        public string BinIndexString { get => RListAlt ? StageEntries.Where(x => x.IsRAlt).ToList().IndexOf(SelectedStageEntry).ToString("D2")
                : (LListAlt ? StageEntries.Where(x => x.IsLAlt).ToList().IndexOf(SelectedStageEntry).ToString("D2") : "");
        }

        public List<string> Tracklists { get => _tracklists; set { _tracklists = value; OnPropertyChanged(nameof(Tracklists)); } }

        // ViewModels
        public IStageCosmeticViewModel StageCosmeticViewModel { get; }

        // Methods
        public void LoadStage(StageLoadedMessage message)
        {
            Stage = message.Value;
            _originalRandomName = Stage.RandomName;
            Tracklists = _tracklistService.GetTracklists();
            OnPropertyChanged(nameof(Stage));
            OnPropertyChanged(nameof(Tracklists));
        }

        public void SaveStage(StageInfo stage)
        {
            // Create copy of stage before save
            var stageToSave = stage.Copy();
            var deleteOptions = new List<string>();
            deleteOptions = _stageService.SaveStage(stageToSave, _originalRandomName != stageToSave.RandomName);

            // Prompt user for delete options
            foreach (var item in deleteOptions)
            {
                var delete = _dialogService.ShowMessage($"Delete the file {Path.GetFileName(item)}?", "Delete", MessageBoxButton.YesNo);
                if (delete)
                {
                    _fileService.DeleteFile(item);
                }
            }

            // Stage saving was successful, so load changes
            Stage = stageToSave;
            stage = Stage;

            stage.Cosmetics.Items.ForEach(x => { x.ImagePath = ""; x.ModelPath = ""; x.ColorSmashChanged = false; });
            stage.Cosmetics.ClearChanges();

            OnPropertyChanged(nameof(Stage));
            // Update stage list
            WeakReferenceMessenger.Default.Send(new StageSavedMessage(stage));
            // Update loaded stage
            WeakReferenceMessenger.Default.Send(new StageLoadedMessage(stage));
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
            var result = _dialogService.ShowMessage("Are you sure you'd like to delete this stage?", "Delete stage?", MessageBoxButton.YesNo);
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
                // TODO: instead of prompting for each one separately, create one prompt with checkbox options for each. Maybe images too?
                // Prompt for selectable cosmetics
                foreach (var cosmeticGroup in Stage.Cosmetics.Items.Where(x => x.SelectionOption).GroupBy(x => new { x.CosmeticType, x.Style }))
                {
                    var delete = _dialogService.ShowMessage($"Would you like to delete cosmetics of type {cosmeticGroup.Key.CosmeticType.GetDescription()} with style {cosmeticGroup.Key.Style}?",
                        "Delete cosmetics?", MessageBoxButton.YesNo);
                    if (delete)
                    {
                        selectableCosmeticsToDelete.Add((cosmeticGroup.Key.CosmeticType, cosmeticGroup.Key.Style));
                    }
                }
                // Mark selectable cosmetics as changed
                foreach (var cosmetic in Stage.Cosmetics.Items.Where(x => selectableCosmeticsToDelete.Any(y => y.CosmeticType == x.CosmeticType && y.Style == x.Style)))
                {
                    deleteStage.Cosmetics.ItemChanged(cosmetic);
                }
                Stage = null;
                OnPropertyChanged(nameof(Stage));
                SaveStage(deleteStage);
                Stage = null;
                OnPropertyChanged(nameof(Stage));
                // Update stage lists
                WeakReferenceMessenger.Default.Send(new StageDeletedMessage(oldSlot));
            }
        }

        private void UpdateListAltImage()
        {
            if (SelectedStageEntry?.ListAlt != null)
            {
                var image = _dialogService.OpenFileDialog("Select an image", "PNG file (.png)|*.png");
                if (!string.IsNullOrEmpty(image))
                {
                    SelectedStageEntry.ListAlt.ImageData = GetBinThumbnailData(image);
                    OnPropertyChanged(nameof(SelectedStageEntry));
                }
            }
        }

        private byte[] GetBinThumbnailData(string imagePath)
        {
            var imageData = _fileService.LoadImage(imagePath);
            var bitmap = imageData.ToBitmap();
            var pixelData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var pixelFormat = PixelFormats.Bgra32;
            var bitmapSource = BitmapSource.Create(bitmap.Width, bitmap.Height, 1, 1, pixelFormat, null, pixelData.Scan0, pixelData.Stride * bitmap.Height, pixelData.Stride);
            var encoder = new JpegBitmapEncoder();
            using (MemoryStream outStream = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(outStream);
                return outStream.ToArray();
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
