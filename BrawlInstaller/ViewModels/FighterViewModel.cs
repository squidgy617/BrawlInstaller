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
using static BrawlInstaller.ViewModels.FighterViewModel;

namespace BrawlInstaller.ViewModels
{
    public interface IFighterViewModel
    {
        ICommand LoadCommand { get; }
        void LoadFighter();
        FighterPackage FighterPackage { get; }
        List<CostumeView> Costumes { get; }
        CostumeView SelectedCostume { get; set; }
    }

    [Export(typeof(IFighterViewModel))]
    internal class FighterViewModel : ViewModelBase, IFighterViewModel
    {
        // Private properties
        private List<CostumeView> _costumes;
        private CostumeView _selectedCostume;

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
        public List<CostumeView> Costumes { get => _costumes; set { _costumes = value; OnPropertyChanged(); } }
        public CostumeView SelectedCostume { get => _selectedCostume; set { _selectedCostume = value; OnPropertyChanged(); } }

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
            Costumes = new List<CostumeView>();
            foreach(var costume in FighterPackage.Costumes)
            {
                Costumes.Add(new CostumeView
                {
                    CSPs = new CosmeticView
                    {
                        AvailableStyles = costume.Cosmetics.Where(x => x.CosmeticType == Enums.CosmeticType.CSP).Select(x => x.Style).Distinct().ToList(),
                        SelectedStyle = costume.Cosmetics.Where(x => x.CosmeticType == Enums.CosmeticType.CSP).Select(x => x.Style).Distinct().ToList().First(),
                        Cosmetics = costume.Cosmetics.Where(x => x.CosmeticType == Enums.CosmeticType.CSP).ToList()
                    },
                    BPs = new CosmeticView
                    {
                        AvailableStyles = costume.Cosmetics.Where(x => x.CosmeticType == Enums.CosmeticType.BP).Select(x => x.Style).Distinct().ToList(),
                        SelectedStyle = costume.Cosmetics.Where(x => x.CosmeticType == Enums.CosmeticType.BP).Select(x => x.Style).Distinct().ToList().First(),
                        Cosmetics = costume.Cosmetics.Where(x => x.CosmeticType == Enums.CosmeticType.BP).ToList()
                    },
                    PacFiles = costume.PacFiles,
                    Color = costume.Color,
                    CostumeId = costume.CostumeId
                });
            }
        }
    }

    // Mappings
    public class CostumeView
    {
        public CosmeticView CSPs { get; set; }
        public CosmeticView BPs { get; set; }
        public List<string> PacFiles { get; set; }
        public byte Color { get; set; }
        public int CostumeId { get; set; }
    }

    public class CosmeticView
    {
        public List<string> AvailableStyles { get; set; }
        public string SelectedStyle { get; set; }
        public List<Cosmetic> Cosmetics { get; set; }
    }
}
