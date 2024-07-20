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
    public interface IMainViewModel
    {
        ICommand ButtonCommand { get; }
        void Button();
        string Title { get; }
    }

    [Export(typeof(IMainViewModel))]
    internal class MainViewModel : ViewModelBase, IMainViewModel
    {
        // Services
        IFileService _fileService { get; }
        ISettingsService _settingsService { get; }
        IPackageService _packageService { get; }

        // Commands
        public ICommand ButtonCommand
        {
            get
            {
                var buttonCommand = new RelayCommand(param => this.Button());
                return buttonCommand;
            }
        }

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public MainViewModel(IFileService fileService, ISettingsService settingsService, IPackageService packageService)
        {
            _fileService = fileService;
            _settingsService = settingsService;
            _packageService = packageService;
            Title = "Test title";
        }

        // Properties
        public string Title { get; }

        // Methods
        public void Button()
        {
            //TestFiles();
            //TestJson();
            TestExtract();
        }

        public void TestFiles()
        {
            var rootNode = _fileService.OpenFile("F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild\\Builds\\P+Ex\\pf\\menu2\\sc_selcharacter.pac");
            var testName = rootNode.Children.Last().Name;
            rootNode.Children.Last().MoveUp();
            _fileService.SaveFile(rootNode);
            Debug.Print(testName);
        }

        public void TestJson()
        {
            var buildSettings = _settingsService.LoadSettings($"{_settingsService.AppSettings.BuildPath}\\BuildSettings.json");
            _settingsService.SaveSettings(buildSettings, "F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild\\Builds\\P+Ex\\settings.json");
            _settingsService.LoadSettings("F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild\\Builds\\P+Ex\\settings.json");
        }

        public void TestExtract()
        {
            _settingsService.AppSettings.BuildPath = "F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild\\Builds\\P+Ex\\";
            _settingsService.BuildSettings = _settingsService.LoadSettings($"{_settingsService.AppSettings.BuildPath}\\BuildSettings.json");
            _packageService.ExtractFighter(new FighterInfo
            {
                Ids = new BrawlIds
                {
                    //CosmeticId = 19,
                    //CosmeticConfigId = 19,
                    //FranchiseId = 38,
                    //TrophyThumbnailId = 39
                    FighterConfigId = 37,
                    SlotConfigId = 39,
                    CosmeticConfigId = 35,
                    CSSSlotConfigId = 35
                }
            });
        }
    }
}
