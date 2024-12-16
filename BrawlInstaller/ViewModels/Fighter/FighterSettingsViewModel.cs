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
        //TODO: warn user that sword trails will not be changed when switching between custom and vanilla Effect.pacs

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
        public Dictionary<string, KirbyLoadFlags> KirbyLoadFlagOptions { get => typeof(KirbyLoadFlags).GetDictionary<KirbyLoadFlags>(); }

        // Methods
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
        }

        public void GenerateFighterAttributes()
        {
            FighterPackage.FighterInfo.FighterAttributes = new FighterAttributes();
            OnPropertyChanged(nameof(FighterPackage));
            WeakReferenceMessenger.Default.Send(new AttributesUpdatedMessage(FighterPackage.FighterInfo));
        }
        public void GenerateSlotAttributes()
        {
            FighterPackage.FighterInfo.SlotAttributes = new SlotAttributes();
            OnPropertyChanged(nameof(FighterPackage));
            WeakReferenceMessenger.Default.Send(new AttributesUpdatedMessage(FighterPackage.FighterInfo));
        }
        public void GenerateCosmeticAttributes()
        {
            FighterPackage.FighterInfo.CosmeticAttributes = new CosmeticAttributes();
            OnPropertyChanged(nameof(FighterPackage));
            WeakReferenceMessenger.Default.Send(new AttributesUpdatedMessage(FighterPackage.FighterInfo));
        }
        public void GenerateCSSSlotAttributes()
        {
            FighterPackage.FighterInfo.CSSSlotAttributes = new CSSSlotAttributes();
            OnPropertyChanged(nameof(FighterPackage));
            WeakReferenceMessenger.Default.Send(new AttributesUpdatedMessage(FighterPackage.FighterInfo));
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
