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
using System.Windows.Threading;
using System.Windows;

namespace BrawlInstaller.ViewModels
{
    public interface IMainViewModel
    {

    }

    [Export(typeof(IMainViewModel))]
    internal class MainViewModel : ViewModelBase, IMainViewModel
    {
        // Services
        IDialogService _dialogService;

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public MainViewModel(IDialogService dialogService, IMainControlsViewModel mainControlsViewModel, ISettingsViewModel settingsViewModel, IFighterViewModel fighterViewModel, 
            IFighterInfoViewModel fighterInfoViewModel, IStageViewModel stageViewModel)
        {
            _dialogService = dialogService;
            MainControlsViewModel = mainControlsViewModel;
            SettingsViewModel = settingsViewModel;
            FighterViewModel = fighterViewModel;
            FighterInfoViewModel = fighterInfoViewModel;
            StageViewModel = stageViewModel;

            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);
        }

        // Viewmodels
        public IMainControlsViewModel MainControlsViewModel { get; set; }
        public ISettingsViewModel SettingsViewModel { get; set; }
        public IFighterViewModel FighterViewModel { get; set; }
        public IFighterInfoViewModel FighterInfoViewModel { get; set; }
        public IStageViewModel StageViewModel { get; set; }

        // Global error handler
        void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowUnhandledException(e);
        }

        void ShowUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            string errorMessage = e.Exception.Message;

            _dialogService.ShowMessage(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // TODO: Restore backups when an error occurs
        }
    }
}
