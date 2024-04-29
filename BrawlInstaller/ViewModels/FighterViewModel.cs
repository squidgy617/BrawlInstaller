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

namespace BrawlInstaller.ViewModels
{
    public interface IFighterViewModel
    {
        ICommand LoadCommand { get; }
        FighterPackage FighterPackage { get; }
        FighterIdsViewModel FighterIds { get; set; }
    }

    [Export(typeof(IFighterViewModel))]
    internal class FighterViewModel : ViewModelBase, IFighterViewModel
    {
        // Private properties
        private FighterIdsViewModel _fighterIds;

        // Services
        IExtractService _extractService { get; }
        ISettingsService _settingsService { get; }
        IDialogService _dialogService { get; }

        // Commands
        public ICommand LoadCommand
        {
            get
            {
                return new RelayCommand(param => LoadFighter());
            }
        }

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public FighterViewModel(IExtractService extractService, ISettingsService settingsService, IDialogService dialogService, IFranchiseIconViewModel franchiseIconViewModel, ICostumeViewModel costumeViewModel, ICosmeticViewModel cosmeticViewmodel, IFighterFileViewModel fighterFileViewModel)
        {
            _extractService = extractService;
            _settingsService = settingsService;
            _dialogService = dialogService;
            FranchiseIconViewModel = franchiseIconViewModel;
            CostumeViewModel = costumeViewModel;
            CosmeticViewModel = cosmeticViewmodel;
            FighterFileViewModel = fighterFileViewModel;

            _settingsService.BuildPath = "F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild\\Builds\\P+Ex\\";
            _settingsService.BuildSettings = _settingsService.GetDefaultSettings();

            FighterIds = new FighterIdsViewModel ();
        }

        // ViewModels
        public IFranchiseIconViewModel FranchiseIconViewModel { get; }
        public ICostumeViewModel CostumeViewModel { get; }
        public ICosmeticViewModel CosmeticViewModel { get; }
        public IFighterFileViewModel FighterFileViewModel { get; }

        // Properties
        public FighterPackage FighterPackage { get; set; }
        public FighterIdsViewModel FighterIds { get => _fighterIds; set { _fighterIds = value; OnPropertyChanged(); } }

        // Methods
        public void LoadFighter()
        {
            FighterPackage = _extractService.ExtractFighter(new FighterIds
            {
                FighterConfigId = FighterIds.FighterConfigId ?? -1,
                CosmeticConfigId = FighterIds.CosmeticConfigId ?? -1,
                CSSSlotConfigId = FighterIds.CSSSlotConfigId ?? -1,
                SlotConfigId = FighterIds.SlotConfigId ?? -1,
                CosmeticId = FighterIds.CosmeticId ?? -1
                //FighterConfigId = 37,
                //SlotConfigId = 39,
                //CosmeticConfigId = 35,
                //CSSSlotConfigId = 35
            });
            WeakReferenceMessenger.Default.Send(new FighterLoadedMessage(FighterPackage));
        }
    }

    // Mappings
    public class FighterIdsViewModel
    {
        public int? FighterConfigId { get; set; }
        public int? SlotConfigId { get; set; }
        public int? CSSSlotConfigId { get; set; }
        public int? CosmeticConfigId { get; set; }
        public int? CosmeticId { get; set; }
    }

    // Messages
    public class FighterLoadedMessage : ValueChangedMessage<FighterPackage>
    {
        public FighterLoadedMessage(FighterPackage fighterPackage) : base(fighterPackage)
        {
        }
    }
}
