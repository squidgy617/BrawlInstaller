﻿using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlLib.Wii.Textures;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.ViewModels
{
    public interface ICosmeticSettingsViewModel
    {
        List<CosmeticDefinition> CosmeticSettings { get; }
    }

    [Export(typeof(ICosmeticSettingsViewModel))]
    internal class CosmeticSettingsViewModel : ViewModelBase, ICosmeticSettingsViewModel
    {
        // Private properties
        private List<CosmeticDefinition> _cosmeticSettings;
        private Dictionary<string, CosmeticType> _cosmeticOptions = new Dictionary<string, CosmeticType>();
        private CosmeticType _selectedCosmeticOption;
        private string _selectedStyle;
        private CosmeticDefinition _selectedDefinition;
        private List<string> _extensionOptions;
        private Dictionary<string, IdType> _idTypes = new Dictionary<string, IdType>();
        private Dictionary<string, WiiPixelFormat> _formats = new Dictionary<string, WiiPixelFormat>();
        private string _selectedPatPath;

        // Services

        [ImportingConstructor]
        public CosmeticSettingsViewModel()
        {
            WeakReferenceMessenger.Default.Register<SettingsLoadedMessage>(this, (recipient, message) =>
            {
                LoadSettings(message);
            });
        }

        // Properties
        public List<CosmeticDefinition> CosmeticSettings { get => _cosmeticSettings; set { _cosmeticSettings = value; OnPropertyChanged(nameof(BuildSettings)); } }

        [DependsUpon(nameof(BuildSettings))]
        public Dictionary<string, CosmeticType> CosmeticOptions { get => _cosmeticOptions; set { _cosmeticOptions = value; OnPropertyChanged(nameof(CosmeticOptions)); } }

        [DependsUpon(nameof(CosmeticOptions))]
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(nameof(SelectedCosmeticOption)); } }

        [DependsUpon(nameof(SelectedCosmeticOption))]
        public List<string> Styles { get => CosmeticSettings?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style).Distinct().ToList(); }

        [DependsUpon(nameof(Styles))]
        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(nameof(SelectedStyle)); } }

        [DependsUpon(nameof(SelectedCosmeticOption))]
        [DependsUpon(nameof(SelectedStyle))]
        public ObservableCollection<CosmeticDefinition> DefinitionList { get => new ObservableCollection<CosmeticDefinition>(CosmeticSettings.Where(x => x.Style == SelectedStyle && x.CosmeticType == SelectedCosmeticOption)); }

        [DependsUpon(nameof(DefinitionList))]
        public CosmeticDefinition SelectedDefinition { get => _selectedDefinition; set { _selectedDefinition = value; OnPropertyChanged(nameof(SelectedDefinition)); } }

        public List<string> ExtensionOptions { get => new List<string> { "brres", "pac" }; }

        public Dictionary<string, IdType> IdTypes { get => _idTypes; set { _idTypes = value; OnPropertyChanged(nameof(IdTypes)); } }

        public Dictionary<string, WiiPixelFormat> Formats { get => _formats; set { _formats = value; OnPropertyChanged(nameof(Formats)); } }

        [DependsUpon(nameof(SelectedDefinition))]
        public ObservableCollection<string> SelectedPatPaths { get => SelectedDefinition?.PatSettings?.Paths != null ? new ObservableCollection<string>(SelectedDefinition?.PatSettings?.Paths) : new ObservableCollection<string>(); }

        [DependsUpon(nameof(SelectedPatPaths))]
        public string SelectedPatPath { get => _selectedPatPath; set { _selectedPatPath = value; OnPropertyChanged(nameof(SelectedPatPath)); } }

        // Methods
        public void LoadSettings(SettingsLoadedMessage message)
        {
            CosmeticSettings = message.Value.CosmeticSettings;
            CosmeticOptions = typeof(CosmeticType).GetDictionary<CosmeticType>();
            IdTypes = typeof(IdType).GetDictionary<IdType>();
            Formats = typeof(WiiPixelFormat).GetDictionary<WiiPixelFormat>();
        }
    }
}
