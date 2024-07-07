using BrawlInstaller.Classes;
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
        private PatSettings _selectedPatSettings;

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
        public ObservableCollection<PatSettings> PatSettings { get => SelectedDefinition?.PatSettings != null ? new ObservableCollection<PatSettings>(SelectedDefinition?.PatSettings) : new ObservableCollection<PatSettings>(); }

        [DependsUpon(nameof(CopyPatSettings))]
        [DependsUpon(nameof(PatSettings))]
        public PatSettings SelectedPatSettings { get => _selectedPatSettings; set { _selectedPatSettings = value; OnPropertyChanged(nameof(SelectedPatSettings)); } }

        [DependsUpon(nameof(SelectedPatSettings))]
        public bool CopyPatSettings 
        { 
            get => SelectedPatSettings != null && SelectedPatSettings?.IdType == null && SelectedPatSettings?.Multiplier == null && SelectedPatSettings?.Offset == null; 
            set 
            {
                if (SelectedPatSettings != null)
                {
                    if (value)
                    {
                        SelectedPatSettings.IdType = null;
                        SelectedPatSettings.Multiplier = null;
                        SelectedPatSettings.Offset = null;
                    }
                    else
                    {
                        SelectedPatSettings.IdType = SelectedDefinition.IdType;
                        SelectedPatSettings.Multiplier = SelectedDefinition.Multiplier;
                        SelectedPatSettings.Offset = SelectedDefinition.Offset;
                    }
                }
                OnPropertyChanged(nameof(CopyPatSettings));
            }
        }

        [DependsUpon(nameof(CopyPatSettings))]
        [DependsUpon(nameof(SelectedPatSettings))]
        public bool PatControlsEnabled { get => SelectedPatSettings != null && !CopyPatSettings; }

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
