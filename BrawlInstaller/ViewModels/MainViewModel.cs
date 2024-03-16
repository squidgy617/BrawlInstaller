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
        IExtractService _extractService { get; }

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
        public MainViewModel(IFileService fileService, ISettingsService settingsService, IExtractService extractService)
        {
            _fileService = fileService;
            _settingsService = settingsService;
            _extractService = extractService;
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
            var buildSettings = _settingsService.GetDefaultSettings();
            _settingsService.SaveSettings(buildSettings, "F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild\\Builds\\P+Ex\\settings.json");
            _settingsService.LoadSettings("F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild\\Builds\\P+Ex\\settings.json");
        }

        public void TestExtract()
        {
            _settingsService.BuildPath = "F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild\\Builds\\P+Ex\\";
            _settingsService.BuildSettings = _settingsService.GetDefaultSettings();
            _extractService.ExtractFighter(new FighterIds
            {
                CosmeticId = 19,
                CosmeticConfigId = 19
            });
        }
    }
}
