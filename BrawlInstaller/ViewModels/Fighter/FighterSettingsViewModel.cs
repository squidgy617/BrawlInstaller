using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using BrawlInstaller.StaticClasses;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static BrawlLib.SSBB.ResourceNodes.FCFGNode;

namespace BrawlInstaller.ViewModels
{
    public interface IFighterSettingsViewModel
    {
        FighterPackage FighterPackage { get; }
    }

    [Export(typeof(IFighterSettingsViewModel))]
    internal class FighterSettingsViewModel : ViewModelBase, IFighterSettingsViewModel
    {
        // Private properties
        private FighterPackage _fighterPackage;

        // Services
        IDialogService _dialogService { get; }
        IFighterService _fighterService { get; }
        ISettingsService _settingsService { get; }

        // Commands
        public ICommand LoadKirbyHatCommand => new RelayCommand(param => LoadKirbyHat());
        public ICommand ClearKirbyHatCommand => new RelayCommand(param => ClearKirbyHat());
        public ICommand ImportExConfigsCommand => new RelayCommand(param => ImportExConfigs());
        public ICommand GenerateFighterAttributesCommand => new RelayCommand(param => GenerateFighterAttributes());
        public ICommand GenerateSlotAttributesCommand => new RelayCommand(param => GenerateSlotAttributes());
        public ICommand GenerateCosmeticAttributesCommand => new RelayCommand(param => GenerateCosmeticAttributes());
        public ICommand GenerateCSSSlotAttributesCommand => new RelayCommand(param => GenerateCSSSlotAttributes());
        public ICommand RefreshEffectPacCommand => new RelayCommand(param => RefreshEffectPac());
        public ICommand RefreshKirbyEffectPacCommand => new RelayCommand(param => RefreshKirbyEffectPac());

        // Importing constructor
        [ImportingConstructor]
        public FighterSettingsViewModel(IDialogService dialogService, IFighterService fighterService, ISettingsService settingsService)
        {
            _dialogService = dialogService;
            _fighterService = fighterService;
            _settingsService = settingsService;

            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadFighterSettings(message);
            });
        }

        // Properties
        public FighterPackage FighterPackage { get => _fighterPackage; set { _fighterPackage = value; OnPropertyChanged(nameof(FighterPackage)); } }
        public List<FighterInfo> FighterInfoList { get => _settingsService.FighterInfoList; }
        public Dictionary<string, int> FighterEffectPacs { get => EffectPacs.FighterEffectPacs; }

        [DependsUpon(nameof(FighterPackage))]
        public int? OldFighterEffectPac { get => FighterPackage?.FighterInfo?.OriginalEffectPacId; set { FighterPackage.FighterInfo.OriginalEffectPacId = value; } }

        [DependsUpon(nameof(FighterPackage))]
        public int? SelectedFighterEffectPac { get => FighterPackage?.FighterInfo?.EffectPacId; set { ChangedFighterEffectPac(FighterPackage.FighterInfo.OriginalEffectPacId, value); OnPropertyChanged(nameof(SelectedFighterEffectPac)); } }

        [DependsUpon(nameof(FighterPackage))]
        public int? OldKirbyEffectPac { get => FighterPackage?.FighterInfo?.OriginalKirbyEffectPacId; set { FighterPackage.FighterInfo.OriginalKirbyEffectPacId = value; } }

        [DependsUpon(nameof(FighterPackage))]
        public int? SelectedKirbyEffectPac { get => FighterPackage?.FighterInfo?.KirbyEffectPacId; set { ChangedKirbyEffectPac(FighterPackage.FighterInfo.OriginalKirbyEffectPacId, value); OnPropertyChanged(nameof(SelectedKirbyEffectPac)); } }

        [DependsUpon(nameof(FighterPackage))]
        public Dictionary<string, KirbyLoadFlags> KirbyLoadFlagOptions { get => typeof(KirbyLoadFlags).GetDictionary<KirbyLoadFlags>(); }

        [DependsUpon(nameof(FighterPackage))]
        public bool KirbyHatTypeEnabled { get => FighterPackage?.FighterInfo?.FighterAttributes != null; }

        [DependsUpon(nameof(FighterPackage))]
        public bool InternalNameEnabled { get => FighterPackage?.FighterInfo?.FighterAttributes != null; }

        [DependsUpon(nameof(FighterPackage))]
        public bool DisplayNameEnabled { get => FighterPackage?.FighterInfo?.CosmeticAttributes != null; }

        [DependsUpon(nameof(FighterPackage))]
        public string FighterFileName { get => FighterPackage?.FighterInfo?.FighterFileName; set { FighterPackage.FighterInfo.FighterFileName = value; UpdateFighterName(); OnPropertyChanged(nameof(FighterFileName)); } }

        // Methods
        public void ChangedFighterEffectPac(int? oldEffectPacId, int? newEffectPacId)
        {
            var newId = newEffectPacId;
            // If we are going from a custom/vanilla ID to a vanilla/custom one, display a message
            if (oldEffectPacId != null && oldEffectPacId != newEffectPacId && (oldEffectPacId < 311 || newEffectPacId < 311))
            {
                var result = _dialogService.ShowMessage("You are either changing to a different vanilla Effect.pac or switching between a custom and vanilla Effect.pac. GFX IDs will be updated correctly, however if the fighter uses traces/sword trails, these will NOT be updated. Are you sure you want to proceed?",
                    "Trace IDs cannot be changed", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != true)
                {
                    newId = oldEffectPacId;
                }
            }
            FighterPackage.FighterInfo.EffectPacId = newId;
        }

        public void ChangedKirbyEffectPac(int? oldEffectPacId, int? newEffectPacId)
        {
            var newId = newEffectPacId;
            // If we are going from a custom/vanilla ID to a vanilla/custom one, display a message
            if (oldEffectPacId != null && oldEffectPacId != newEffectPacId && (oldEffectPacId < 311 || newEffectPacId < 311))
            {
                var result = _dialogService.ShowMessage("You are either changing to a different vanilla Effect.pac or switching between a custom and vanilla Effect.pac. GFX IDs will be updated correctly, however if the fighter uses traces/sword trails, these will NOT be updated. Are you sure you want to proceed?",
                    "Trace IDs cannot be changed", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != true)
                {
                    newId = oldEffectPacId;
                }
            }
            FighterPackage.FighterInfo.KirbyEffectPacId = newId;
        }

        public void LoadFighterSettings(FighterLoadedMessage message)
        {
            FighterPackage = message.Value;
        }

        public void LoadKirbyHat()
        {
            var file = _dialogService.OpenFileDialog("Select Kirby Hat XML file", "XML file (.xml)|*.xml");
            if (!string.IsNullOrEmpty(file))
            {
                FighterPackage.FighterSettings.KirbyHatData = _fighterService.ConvertXMLToKirbyHatData(file);
                OnPropertyChanged(nameof(FighterPackage));
            }
        }

        public void ClearKirbyHat()
        {
            FighterPackage.FighterSettings.KirbyHatData = null;
            OnPropertyChanged(nameof(FighterPackage));
        }

        public void ImportExConfigs()
        {
            var files = _dialogService.OpenMultiFileDialog("Select ex configs to import", "DAT file (.dat)|*.dat");
            FighterPackage.FighterInfo = _fighterService.GetFighterAttributes(FighterPackage.FighterInfo, files);
            OnPropertyChanged(nameof(FighterPackage));
            WeakReferenceMessenger.Default.Send(new AttributesUpdatedMessage(FighterPackage.FighterInfo));
        }

        public void GenerateFighterAttributes()
        {
            FighterPackage.FighterInfo.FighterAttributes = new FighterAttributes { Version = _settingsService.BuildSettings.MiscSettings.DefaultExConfigVersion };
            OnPropertyChanged(nameof(FighterPackage));
            WeakReferenceMessenger.Default.Send(new AttributesUpdatedMessage(FighterPackage.FighterInfo));
        }
        public void GenerateSlotAttributes()
        {
            FighterPackage.FighterInfo.SlotAttributes = new SlotAttributes { Version = _settingsService.BuildSettings.MiscSettings.DefaultExConfigVersion };
            OnPropertyChanged(nameof(FighterPackage));
            WeakReferenceMessenger.Default.Send(new AttributesUpdatedMessage(FighterPackage.FighterInfo));
        }
        public void GenerateCosmeticAttributes()
        {
            FighterPackage.FighterInfo.CosmeticAttributes = new CosmeticAttributes { Version = _settingsService.BuildSettings.MiscSettings.DefaultExConfigVersion };
            OnPropertyChanged(nameof(FighterPackage));
            WeakReferenceMessenger.Default.Send(new AttributesUpdatedMessage(FighterPackage.FighterInfo));
        }
        public void GenerateCSSSlotAttributes()
        {
            FighterPackage.FighterInfo.CSSSlotAttributes = new CSSSlotAttributes { Version = _settingsService.BuildSettings.MiscSettings.DefaultExConfigVersion };
            OnPropertyChanged(nameof(FighterPackage));
            WeakReferenceMessenger.Default.Send(new AttributesUpdatedMessage(FighterPackage.FighterInfo));
        }
        public void UpdateFighterName()
        {
            if (FighterPackage?.FighterInfo?.FighterFileName != null)
            {
                var fileName = FighterPackage?.FighterInfo?.FighterFileName;
                FighterPackage.FighterInfo.FullPacFileName = $"{fileName.ToLower()}/Fit{fileName}.pac";
                FighterPackage.FighterInfo.FullKirbyPacFileName = $"kirby/FitKirby{fileName}.pac";
                FighterPackage.FighterInfo.ModuleFileName = $"ft_{fileName.ToLower()}.rel";
                FighterPackage.FighterInfo.InternalName = fileName.ToUpper();
                if (DisplayNameEnabled)
                    FighterPackage.FighterInfo.DisplayName = fileName;
                OnPropertyChanged(nameof(FighterPackage));
            }
        }

        private void RefreshEffectPac()
        {
            SelectedFighterEffectPac = GetUnusedEffectPacId(SelectedFighterEffectPac, SelectedKirbyEffectPac);
        }

        private void RefreshKirbyEffectPac()
        {
            SelectedKirbyEffectPac = GetUnusedEffectPacId(SelectedKirbyEffectPac, SelectedFighterEffectPac);
        }

        private int? GetUnusedEffectPacId(int? currentId, int? otherUsedId)
        {
            if (_dialogService.ShowMessage("This will update your fighter's Effect.pac to the first available custom Effect.pac in the build. Continue?", "Update Effect.pac", MessageBoxButton.YesNo))
            {
                int newEffectPacId = 311; // 311 is first custom Effect.pac ID
                var usedEffectPacs = _fighterService.GetUsedEffectPacs();
                while (usedEffectPacs.Contains(newEffectPacId) || newEffectPacId == otherUsedId)
                {
                    newEffectPacId++;
                }
                return newEffectPacId;
            }
            return currentId;
        }
    }

    // Messages
    public class AttributesUpdatedMessage : ValueChangedMessage<FighterInfo>
    {
        public AttributesUpdatedMessage(FighterInfo fighterInfo) : base(fighterInfo)
        {
        }
    }
}
