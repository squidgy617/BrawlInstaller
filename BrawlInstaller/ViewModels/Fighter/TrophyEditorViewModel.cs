﻿using BrawlInstaller.Classes;
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
    public interface IFighterTrophyViewModel
    {
        Trophy Trophy { get; set; }
    }

    [Export(typeof(IFighterTrophyViewModel))]
    internal class FighterTrophyViewModel : TrophyEditorViewModelBase, IFighterTrophyViewModel
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

        [ImportingConstructor]
        public FighterTrophyViewModel(ISettingsService settingsService, IFileService fileService, ITrophyService trophyService, IDialogService dialogService) 
            : base(settingsService, fileService, trophyService, dialogService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _trophyService = trophyService;
            _dialogService = dialogService;

            GameIconList = new List<TrophyGameIcon>();
        }

        // Properties

        // Methods
    }

    // Messages
}
