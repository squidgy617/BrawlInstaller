﻿using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BrawlInstaller.ViewModels
{
    public interface IMainControlsViewModel
    {
        AppSettings AppSettings { get; }
    }

    [Export(typeof(IMainControlsViewModel))]
    internal class MainControlsViewModel : ViewModelBase, IMainControlsViewModel
    {
        // Private properties
        private AppSettings _appSettings;

        // Services
        ISettingsService _settingsService { get; }
        IFileService _fileService { get; }
        IDialogService _dialogService { get; }

        // Commands
        public ICommand RefreshCommand => new RelayCommand(param => RefreshSettings());
        public ICommand RestoreBackupCommand => new RelayCommand(param => RestoreBackup());

        [ImportingConstructor]
        public MainControlsViewModel(ISettingsService settingsService, IFileService fileService, IDialogService dialogService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _dialogService = dialogService;

            _settingsService.AppSettings = _settingsService.LoadAppSettings();
            _settingsService.BuildSettings = _settingsService.LoadSettings(_settingsService.BuildSettingsPath);

            AppSettings = _settingsService.AppSettings.Copy();
        }

        // Properties
        public AppSettings AppSettings { get => _appSettings; set { _appSettings = value; OnPropertyChanged(nameof(AppSettings)); } }
        public bool BuildPathExists { get => !string.IsNullOrEmpty(_settingsService.AppSettings.BuildPath) && _fileService.DirectoryExists(_settingsService.AppSettings.BuildPath); }

        // Methods
        private void RefreshSettings()
        {
            if (_fileService.DirectoryExists(AppSettings.BuildPath) && _fileService.GetDirectories(AppSettings.BuildPath, "pf", SearchOption.TopDirectoryOnly).Count > 0)
            {
                _settingsService.AppSettings = AppSettings;
                _settingsService.SaveAppSettings(AppSettings);
                _settingsService.BuildSettings = _settingsService.LoadSettings(_settingsService.BuildSettingsPath);
                _settingsService.FighterInfoList = _settingsService.LoadFighterInfoSettings();
                WeakReferenceMessenger.Default.Send(new UpdateSettingsMessage(AppSettings));
            }
            else
            {
                _dialogService.ShowMessage("Build path does not appear to be valid. Ensure it is the root directory of your build. Should be the folder that contains a subfolder called 'pf'.", "Invalid Build Path", MessageBoxImage.Error);
            }
        }

        private void RestoreBackup()
        {
            var backups = _fileService.GetBackups().Where(x => x.BuildPath == AppSettings.BuildPath).OrderByDescending(x => x.TimeStamp);
            if (backups.Any())
            {
                var selectedBackup = _dialogService.OpenDropDownDialog(backups, "TimeStamp", "Restore Backup", "Select a backup to restore") as Backup;
                if (selectedBackup != null)
                {
                    _fileService.RestoreBackup(selectedBackup);
                    // Do this to re-load everything when a backup is restored
                    WeakReferenceMessenger.Default.Send(new UpdateSettingsMessage(AppSettings));
                    _dialogService.ShowMessage("Backup restored.", "Success");
                }
            }
        }

        // Messages
        public class UpdateSettingsMessage : ValueChangedMessage<AppSettings>
        {
            public UpdateSettingsMessage(AppSettings appSettings) : base(appSettings)
            {
            }
        }
    }
}
