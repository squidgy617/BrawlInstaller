using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using BrawlLib.SSBB.Types;
using BrawlLib.Wii.Compression;
using BrawlLib.Wii.Textures;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
        IDialogService _dialogService { get; }

        // Commands
        public ICommand AddStyleCommand => new RelayCommand(param => AddStyle());
        public ICommand RemoveStyleCommand => new RelayCommand(param => RemoveStyle());
        public ICommand AddPatSettingsCommand => new RelayCommand(param => AddPatSettings());
        public ICommand AddDefinitionCommand => new RelayCommand(param => AddDefinition());
        public ICommand RemoveDefinitionCommand => new RelayCommand(param => RemoveDefinition());
        public ICommand CopyDefinitionCommand => new RelayCommand(param => CopyDefinition());

        [ImportingConstructor]
        public CosmeticSettingsViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            WeakReferenceMessenger.Default.Register<SettingsLoadedMessage>(this, (recipient, message) =>
            {
                LoadSettings(message);
            });
        }

        // Properties
        public List<CosmeticDefinition> CosmeticSettings { get => _cosmeticSettings; set { _cosmeticSettings = value; OnPropertyChanged(nameof(CosmeticSettings)); } }

        [DependsUpon(nameof(CosmeticSettings))]
        public Dictionary<string, CosmeticType> CosmeticOptions { get => _cosmeticOptions; set { _cosmeticOptions = value; OnPropertyChanged(nameof(CosmeticOptions)); } }

        [DependsUpon(nameof(CosmeticOptions))]
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(nameof(SelectedCosmeticOption)); } }

        [DependsUpon(nameof(SelectedCosmeticOption))]
        public ObservableCollection<string> Styles { get => new ObservableCollection<string>(CosmeticSettings?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style).Distinct().ToList()); }

        [DependsUpon(nameof(Styles))]
        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(nameof(SelectedStyle)); } }

        [DependsUpon(nameof(SelectedCosmeticOption))]
        [DependsUpon(nameof(SelectedStyle))]
        [DependsUpon(nameof(CosmeticSettings))]
        public ObservableCollection<CosmeticDefinition> DefinitionList { get => new ObservableCollection<CosmeticDefinition>(CosmeticSettings.Where(x => x.Style == SelectedStyle && x.CosmeticType == SelectedCosmeticOption)); }

        [DependsUpon(nameof(DefinitionList))]
        public CosmeticDefinition SelectedDefinition { get => _selectedDefinition; set { _selectedDefinition = value; OnPropertyChanged(nameof(SelectedDefinition)); } }

        public Dictionary<string, CompressionType> CompressionTypes { get => typeof(CompressionType).GetDictionary<CompressionType>(); }

        public Dictionary<string, ARCFileType> FileTypes { get => typeof(ARCFileType).GetDictionary<ARCFileType>(); }

        public List<string> ExtensionOptions { get => new List<string> { "brres", "pac" }; }

        public Dictionary<string, IdType> IdTypes { get => _idTypes; set { _idTypes = value; OnPropertyChanged(nameof(IdTypes)); } }

        public Dictionary<string, WiiPixelFormat> Formats { get => _formats; set { _formats = value; OnPropertyChanged(nameof(Formats)); } }

        [DependsUpon(nameof(SelectedDefinition))]
        public ObservableCollection<PatSettings> PatSettings { get => SelectedDefinition?.PatSettings.Count > 0 ? new ObservableCollection<PatSettings>(SelectedDefinition?.PatSettings) : new ObservableCollection<PatSettings>(); }

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

        public void AddStyle()
        {
            var styleName = _dialogService.OpenStringInputDialog("Style Name Input", "Enter the name for your new style");
            if (styleName != null && !CosmeticSettings.Any(x => x.Style == styleName && x.CosmeticType == SelectedCosmeticOption))
            {
                var newDef = new CosmeticDefinition
                {
                    CosmeticType = SelectedCosmeticOption,
                    Style = styleName,
                    InstallLocation = new InstallLocation { FilePath = "pf" }
                };
                DefinitionList.Add(newDef);
                CosmeticSettings.Add(newDef);
                OnPropertyChanged(nameof(DefinitionList));
                OnPropertyChanged(nameof(CosmeticSettings));
            }
        }

        public void RemoveStyle()
        {
            foreach(var definition in DefinitionList.ToList())
            {
                if (definition.CosmeticType == SelectedCosmeticOption && definition.Style == SelectedStyle)
                {
                    DefinitionList.Remove(definition);
                    CosmeticSettings.Remove(definition);
                }
            }
            OnPropertyChanged(nameof(DefinitionList));
            OnPropertyChanged(nameof(CosmeticSettings));
        }

        public void AddPatSettings()
        {
            SelectedDefinition.PatSettings.Add(new PatSettings());
            OnPropertyChanged(nameof(SelectedDefinition));
        }

        public void AddDefinition()
        {
            var newDefinition = new CosmeticDefinition
            {
                CosmeticType = SelectedCosmeticOption,
                Style = SelectedStyle
            };
            CosmeticSettings.Add(newDefinition);
            OnPropertyChanged(nameof(CosmeticSettings));
            OnPropertyChanged(nameof(DefinitionList));
        }

        public void RemoveDefinition()
        {
            CosmeticSettings.Remove(SelectedDefinition);
            OnPropertyChanged(nameof(CosmeticSettings));
            OnPropertyChanged(nameof(DefinitionList));
            OnPropertyChanged(nameof(SelectedDefinition));
        }

        public void CopyDefinition()
        {
            if (SelectedDefinition != null)
            {
                var newDefinition = SelectedDefinition.Copy();
                CosmeticSettings.Add(newDefinition);
                SelectedDefinition = newDefinition;
                OnPropertyChanged(nameof(CosmeticSettings));
                OnPropertyChanged(nameof(DefinitionList));
                OnPropertyChanged(nameof(SelectedDefinition));
            }
        }
    }
}
