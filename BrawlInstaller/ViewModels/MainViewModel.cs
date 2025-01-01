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
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using static BrawlInstaller.ViewModels.MainControlsViewModel;

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
        ISettingsService _settingsService;
        IFileService _fileService;

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        // Order of viewmodels in this constructor determines the order they load in!
        public MainViewModel(IDialogService dialogService, ISettingsService settingsService, IFileService fileService, IMainControlsViewModel mainControlsViewModel, ISettingsViewModel settingsViewModel,
            IFighterViewModel fighterViewModel, IStageViewModel stageViewModel)
        {
            _dialogService = dialogService;
            _settingsService = settingsService;
            _fileService = fileService;
            MainControlsViewModel = mainControlsViewModel;
            SettingsViewModel = settingsViewModel;
            FighterViewModel = fighterViewModel;
            StageViewModel = stageViewModel;

            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);

            WeakReferenceMessenger.Default.Register<UpdateSettingsMessage>(this, (recipient, message) =>
            {
                UpdateSettings();
            });

            WeakReferenceMessenger.Default.Register<SettingsSavedMessage>(this, (recipient, message) =>
            {
                UpdateSettings();
            });
        }

        // Properties
        public bool BuildSettingsExist { get => _fileService.FileExists(_settingsService.BuildSettingsPath); }
        public bool BuildPathExists { get => !string.IsNullOrEmpty(_settingsService.AppSettings.BuildPath) && _fileService.DirectoryExists(_settingsService.AppSettings.BuildPath); }

        // Viewmodels
        public IMainControlsViewModel MainControlsViewModel { get; set; }
        public ISettingsViewModel SettingsViewModel { get; set; }
        public IFighterViewModel FighterViewModel { get; set; }
        public IStageViewModel StageViewModel { get; set; }

        //Methods
        private void UpdateSettings()
        {
            OnPropertyChanged(nameof(BuildSettingsExist));
            OnPropertyChanged(nameof(BuildPathExists));
        }

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
