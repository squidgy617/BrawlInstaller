using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using BrawlInstaller.StaticClasses;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
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
        // Private properties
        private FighterPackage _fighterPackage;

        // Services
        IDialogService _dialogService { get; }
        IFighterService _fighterService { get; }

        // Commands
        public ICommand LoadKirbyHatCommand => new RelayCommand(param => LoadKirbyHat());
        public ICommand ClearKirbyHatCommand => new RelayCommand(param => ClearKirbyHat());

        // Importing constructor
        [ImportingConstructor]
        public FighterSettingsViewModel(IDialogService dialogService, IFighterService fighterService)
        {
            _dialogService = dialogService;
            _fighterService = fighterService;

            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadFighterSettings(message);
            });
        }

        // Properties
        public FighterPackage FighterPackage { get => _fighterPackage; set { _fighterPackage = value; OnPropertyChanged(nameof(FighterPackage)); } }

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
    }
}
