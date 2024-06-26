﻿using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
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
        private ObservableCollection<KeyValuePair<string, CosmeticType>> _cosmeticOptions = new ObservableCollection<KeyValuePair<string, CosmeticType>>();
        private CosmeticType _selectedCosmeticOption;
        private string _selectedStyle;

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
        public ObservableCollection<KeyValuePair<string, CosmeticType>> CosmeticOptions { get => _cosmeticOptions; set { _cosmeticOptions = value; OnPropertyChanged(nameof(CosmeticOptions)); } }

        [DependsUpon(nameof(CosmeticOptions))]
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(nameof(SelectedCosmeticOption)); } }

        [DependsUpon(nameof(SelectedCosmeticOption))]
        public List<string> Styles { get => CosmeticSettings?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style).Distinct().ToList(); }

        [DependsUpon(nameof(Styles))]
        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(nameof(SelectedStyle)); } }


        // Methods
        public void LoadSettings(SettingsLoadedMessage message)
        {
            CosmeticSettings = message.Value.CosmeticSettings;
            foreach (CosmeticType option in CosmeticSettings.Select(x => x.CosmeticType).Distinct())
            {
                CosmeticOptions.Add(option.GetKeyValuePair());
            }
        }
    }
}
