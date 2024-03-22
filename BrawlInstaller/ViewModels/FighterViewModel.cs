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

namespace BrawlInstaller.ViewModels
{
    public interface IFighterViewModel
    {
        ICommand LoadCommand { get; }
        void LoadFighter();
        FighterPackage FighterPackage { get; }
        List<Costume> Costumes { get; }
    }

    [Export(typeof(IFighterViewModel))]
    internal class FighterViewModel : ViewModelBase, IFighterViewModel
    {
        // Private properties
        private List<Costume> _costumes;

        // Services
        IExtractService _extractService { get; }
        ISettingsService _settingsService { get; }

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
        public FighterViewModel(IExtractService extractService, ISettingsService settingsService)
        {
            _extractService = extractService;
            _settingsService = settingsService;
        }

        // Properties
        public FighterPackage FighterPackage { get; set; }
        public List<Costume> Costumes { get { return _costumes; } set { _costumes = value; OnPropertyChanged(); } }

        // Methods
        public void LoadFighter()
        {
            _settingsService.BuildPath = "F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild\\Builds\\P+Ex\\";
            _settingsService.BuildSettings = _settingsService.GetDefaultSettings();
            FighterPackage = _extractService.ExtractFighter(new FighterIds
            {
                FighterConfigId = 37,
                SlotConfigId = 39,
                CosmeticConfigId = 35,
                CSSSlotConfigId = 35
            });
            Costumes = FighterPackage.Costumes;
        }
    }
}
