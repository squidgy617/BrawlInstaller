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
        ICommand SaveCommand { get; }
        FighterPackage FighterPackage { get; }
    }

    [Export(typeof(IFighterViewModel))]
    internal class FighterViewModel : ViewModelBase, IFighterViewModel
    {
        // Private properties
        private List<FighterInfo> _fighterList;
        private FighterInfo _selectedFighter;

        // Services
        IPackageService _packageService { get; }
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

        public ICommand SaveCommand
        {
            get
            {
                return new RelayCommand(param => SaveFighter());
            }
        }

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public FighterViewModel(IPackageService packageService, ISettingsService settingsService, IDialogService dialogService, IFranchiseIconViewModel franchiseIconViewModel, ICostumeViewModel costumeViewModel, ICosmeticViewModel cosmeticViewmodel, IFighterFileViewModel fighterFileViewModel)
        {
            _packageService = packageService;
            _settingsService = settingsService;
            _dialogService = dialogService;
            FranchiseIconViewModel = franchiseIconViewModel;
            CostumeViewModel = costumeViewModel;
            CosmeticViewModel = cosmeticViewmodel;
            FighterFileViewModel = fighterFileViewModel;

            _settingsService.BuildPath = "F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild\\Builds\\P+Ex\\";
            _settingsService.BuildSettings = _settingsService.GetDefaultSettings();

            var list = _settingsService.LoadFighterInfoSettings();
            FighterList = list;
        }

        // ViewModels
        public IFranchiseIconViewModel FranchiseIconViewModel { get; }
        public ICostumeViewModel CostumeViewModel { get; }
        public ICosmeticViewModel CosmeticViewModel { get; }
        public IFighterFileViewModel FighterFileViewModel { get; }

        // Properties
        public FighterPackage FighterPackage { get; set; }
        public List<FighterInfo> FighterList { get => _fighterList; set { _fighterList = value; OnPropertyChanged(nameof(FighterList)); } }
        public FighterInfo SelectedFighter { get => _selectedFighter; set { _selectedFighter = value; OnPropertyChanged(nameof(SelectedFighter)); } }

        // Methods
        public void LoadFighter()
        {
            FighterPackage = new FighterPackage();
            FighterPackage.FighterInfo = SelectedFighter;
            FighterPackage = _packageService.ExtractFighter(SelectedFighter.Ids);
            WeakReferenceMessenger.Default.Send(new FighterLoadedMessage(FighterPackage));
        }

        public void SaveFighter()
        {
            // TODO: Need a way to track if costumes/cosmetics were modified if one is removed, HasChanged on whole list?
            // Set costume indexes for cosmetics
            foreach(var costume in FighterPackage.Costumes)
            {
                foreach(var cosmetic in costume.Cosmetics)
                {
                    cosmetic.CostumeIndex = CostumeViewModel.Costumes.IndexOf(costume) + 1;
                }
            }
            // TODO: Possibly a way to improve this?
            // Set franchise icon up
            if (FranchiseIconViewModel.FranchiseIcons.HasChanged(FranchiseIconViewModel.SelectedFranchiseIcon))
            {
                // Add model for import
                if (FranchiseIconViewModel.SelectedFranchiseIcon.ModelPath != "")
                {
                    var franchiseModels = FighterPackage.Cosmetics.Items.Where(x => x.CosmeticType == CosmeticType.FranchiseIcon && x.Style == "Model").ToList();
                    if (franchiseModels.Count >= 1)
                        franchiseModels.ForEach(x =>
                        {
                            x.ModelPath = FranchiseIconViewModel.SelectedFranchiseIcon.ModelPath;
                            x.Id = FranchiseIconViewModel.SelectedFranchiseIcon.Id;
                            FranchiseIconViewModel.FranchiseIcons.ItemChanged(x);
                        });
                    else
                    {
                        var newCosmetic = new Cosmetic
                        {
                            CosmeticType = CosmeticType.FranchiseIcon,
                            Style = "Model",
                            ModelPath = FranchiseIconViewModel.SelectedFranchiseIcon.ModelPath,
                            Id = FranchiseIconViewModel.SelectedFranchiseIcon.Id
                        };
                        FighterPackage.Cosmetics.Add(newCosmetic);
                    }
                }
                FighterPackage.FighterInfo.Ids.FranchiseId = FranchiseIconViewModel.SelectedFranchiseIcon.Id ?? -1;
            }
            _packageService.SaveFighter(FighterPackage);
            FighterPackage.Cosmetics.Items.ForEach(x => { FighterPackage.Cosmetics.ClearChanges(); x.ImagePath = ""; x.HDImagePath = ""; x.ColorSmashChanged = false; } );
        }
    }

    // Messages
    public class FighterLoadedMessage : ValueChangedMessage<FighterPackage>
    {
        public FighterLoadedMessage(FighterPackage fighterPackage) : base(fighterPackage)
        {
        }
    }
}
