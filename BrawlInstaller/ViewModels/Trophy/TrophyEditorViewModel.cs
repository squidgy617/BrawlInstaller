using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Helpers;
using BrawlInstaller.Services;
using BrawlInstaller.StaticClasses;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static BrawlInstaller.ViewModels.MainControlsViewModel;

namespace BrawlInstaller.ViewModels
{
    public interface ITrophyEditorViewModel
    {
        Trophy Trophy { get; set; }
        ICommand SaveTrophyCommand { get; }
        ICommand DeleteTrophyCommand { get; }
    }

    [Export(typeof(ITrophyEditorViewModel))]
    internal class TrophyEditorViewModel : TrophyEditorViewModelBase, ITrophyEditorViewModel
    {
        // Private properties
        private Trophy _trophy;
        private Trophy _oldTrophy;
        private List<TrophyGameIcon> _gameIconList;

        // Services
        ISettingsService _settingsService;
        IFileService _fileService;
        ITrophyService _trophyService;
        IDialogService _dialogService;

        // Commands
        public ICommand SaveTrophyCommand => new RelayCommand(() => SaveTrophy());
        public ICommand DeleteTrophyCommand => new RelayCommand(() => DeleteTrophy());

        [ImportingConstructor]
        public TrophyEditorViewModel(ISettingsService settingsService, IFileService fileService, ITrophyService trophyService, IDialogService dialogService) 
            : base(settingsService, fileService, trophyService, dialogService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _trophyService = trophyService;
            _dialogService = dialogService;

            GameIconList = new List<TrophyGameIcon>();

            WeakReferenceMessenger.Default.Register<LoadTrophyMessage>(this, (recipient, message) =>
            {
                using (new CursorWait())
                {
                    LoadTrophy(message.Value);
                }
            });
            WeakReferenceMessenger.Default.Register<SettingsSavedMessage>(this, (recipient, message) =>
            {
                ResetTrophy();
            });
            WeakReferenceMessenger.Default.Register<UpdateSettingsMessage>(this, (recipient, message) =>
            {
                ResetTrophy();
            });
        }

        // Properties

        // Methods
        public void SaveTrophy()
        {
            _fileService.StartBackup();
            using (new CursorWait())
            {
                // Create copies of trophies before save
                var trophyToSave = Trophy.Copy();
                var oldTrophy = OldTrophy.Copy();
                // Save trophy
                Trophy = _trophyService.SaveTrophy(trophyToSave, oldTrophy);
                OldTrophy = Trophy.Copy();
                // Clear cosmetic changes
                Trophy.Thumbnails.ClearChanges();
                // Update UI
                OnPropertyChanged(nameof(Trophy));
                OnPropertyChanged(nameof(OldTrophy));
                WeakReferenceMessenger.Default.Send(new UpdateTrophyListMessage(Trophy));
            }
            _fileService.EndBackup();
            _dialogService.ShowMessage("Changes saved.", "Saved");
        }

        public void DeleteTrophy()
        {
            var result = _dialogService.ShowMessage("WARNING! You are about to delete the currently loaded trophy. Are you sure?", "Delete Trophy", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result)
            {
                _fileService.StartBackup();
                using (new CursorWait())
                {
                    var trophyToDelete = new Trophy { Ids = OldTrophy.Ids.Copy() };
                    var oldTrophy = OldTrophy.Copy();
                    // Add all cosmetics as changes
                    foreach (var thumbnail in oldTrophy.Thumbnails.Items)
                    {
                        trophyToDelete.Thumbnails.ItemChanged(thumbnail);
                    }
                    // Delete trophy
                    _trophyService.SaveTrophy(trophyToDelete, oldTrophy, false);
                    Trophy = null;
                    OldTrophy = null;
                    OnPropertyChanged(nameof(Trophy));
                    OnPropertyChanged(nameof(OldTrophy));
                    WeakReferenceMessenger.Default.Send(new UpdateTrophyListMessage(Trophy));
                }
                _fileService.EndBackup();
                _dialogService.ShowMessage("Changes saved.", "Saved");
            }
        }

        private void ResetTrophy()
        {
            Trophy = null;
            OldTrophy = null;
            OnPropertyChanged(nameof(Trophy));
            OnPropertyChanged(nameof(OldTrophy));
        }
    }

    // Messages
    public class UpdateTrophyListMessage : ValueChangedMessage<Trophy>
    {
        public UpdateTrophyListMessage(Trophy trophy) : base(trophy)
        {
        }
    }
}
