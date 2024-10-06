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
        private string _oldVictoryThemePath;
        private string _oldCreditsThemePath;

        // Services
        IPackageService _packageService { get; }
        ISettingsService _settingsService { get; }
        IDialogService _dialogService { get; }

        // Commands
        public ICommand LoadCommand => new RelayCommand(param => LoadFighter());
        public ICommand SaveCommand => new RelayCommand(param => SaveFighter());
        public ICommand RefreshFightersCommand => new RelayCommand(param => GetFighters());

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
            using (new CursorWait())
            {
                FighterPackage = new FighterPackage();
                FighterPackage.FighterInfo = SelectedFighter;
                FighterPackage = _packageService.ExtractFighter(SelectedFighter);
                _oldVictoryThemePath = FighterPackage.VictoryTheme.SongPath;
                _oldCreditsThemePath = FighterPackage.CreditsTheme.SongPath;
                OnPropertyChanged(nameof(FighterPackage));
                WeakReferenceMessenger.Default.Send(new FighterLoadedMessage(FighterPackage));
            }
        }

        // TODO: When adding a new fighter, franchise icon will be added to the end of the list automatically, so we'll need to prompt the user whether they want to install or not
        public void SaveFighter()
        {
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
                if (delete)
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
        }

        private void GetFighters()
        {
            var list = _settingsService.LoadFighterInfoSettings();
            FighterList = new List<FighterInfo>(list);
            OnPropertyChanged(nameof(FighterList));
            OnPropertyChanged(nameof(SelectedFighter));
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
