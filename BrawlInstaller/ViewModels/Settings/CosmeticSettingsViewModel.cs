using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Enums;
using BrawlInstaller.Services;
using BrawlInstaller.StaticClasses;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.Types;
using BrawlLib.Wii.Compression;
using BrawlLib.Wii.Textures;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
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
        ISettingsService _settingsService { get; }

        // Commands
        public ICommand AddStyleCommand => new RelayCommand(param => AddStyle());
        public ICommand RemoveStyleCommand => new RelayCommand(param => RemoveStyle());
        public ICommand AddPatSettingsCommand => new RelayCommand(param => AddPatSettings());
        public ICommand RemovePatSettingsCommand => new RelayCommand(param => RemovePatSettings());
        public ICommand ResetPatSettingsCommand => new RelayCommand(param => ResetPatSettings());
        public ICommand AddDefinitionCommand => new RelayCommand(param => AddDefinition());
        public ICommand RemoveDefinitionCommand => new RelayCommand(param => RemoveDefinition());
        public ICommand CopyDefinitionCommand => new RelayCommand(param => CopyDefinition());
        public ICommand SelectCosmeticNodePathCommand => new RelayCommand(param => SelectCosmeticNodePath());
        public ICommand ClearCosmeticNodePathCommand => new RelayCommand(param => ClearCosmeticNodePath());
        public ICommand SelectModelPathCommand => new RelayCommand(param => SelectModelPath());
        public ICommand ClearModelPathCommand => new RelayCommand(param => ClearModelPath());
        public ICommand NullModelPathCommand => new RelayCommand(param => NullModelPath());
        public ICommand SelectPatNodePathCommand => new RelayCommand(param => SelectPatNodePath());
        public ICommand ClearPatNodePathCommand => new RelayCommand(param => ClearPatNodePath());

        [ImportingConstructor]
        public CosmeticSettingsViewModel(IDialogService dialogService, ISettingsService settingsService)
        {
            _dialogService = dialogService;
            _settingsService = settingsService;

            WeakReferenceMessenger.Default.Register<SettingsLoadedMessage>(this, (recipient, message) =>
            {
                LoadSettings(message);
            });
        }

        // Properties
        public AppSettings AppSettings { get => _settingsService.AppSettings; }
        public List<CosmeticDefinition> CosmeticSettings { get => _cosmeticSettings; set { _cosmeticSettings = value; OnPropertyChanged(nameof(CosmeticSettings)); } }

        [DependsUpon(nameof(CosmeticSettings))]
        public Dictionary<string, CosmeticType> CosmeticOptions { get => _cosmeticOptions; set { _cosmeticOptions = value; OnPropertyChanged(nameof(CosmeticOptions)); } }

        [DependsUpon(nameof(CosmeticOptions))]
        public CosmeticType SelectedCosmeticOption { get => _selectedCosmeticOption; set { _selectedCosmeticOption = value; OnPropertyChanged(nameof(SelectedCosmeticOption)); } }

        [DependsUpon(nameof(SelectedCosmeticOption))]
        public ObservableCollection<string> Styles { get => new ObservableCollection<string>(DefaultCosmetics.AllDefaultCosmetics?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style)
                .Concat(CosmeticSettings != null ? new List<string>(CosmeticSettings?.Where(x => x.CosmeticType == SelectedCosmeticOption).Select(x => x.Style).Distinct().ToList()) : new List<string>()).Distinct()); }

        [DependsUpon(nameof(Styles))]
        public string SelectedStyle { get => _selectedStyle; set { _selectedStyle = value; OnPropertyChanged(nameof(SelectedStyle)); } }

        [DependsUpon(nameof(SelectedCosmeticOption))]
        [DependsUpon(nameof(SelectedStyle))]
        [DependsUpon(nameof(CosmeticSettings))]
        public ObservableCollection<CosmeticDefinition> DefinitionList { get => CosmeticSettings != null ? new ObservableCollection<CosmeticDefinition>(CosmeticSettings.Where(x => x.Style == SelectedStyle && x.CosmeticType == SelectedCosmeticOption)) : new ObservableCollection<CosmeticDefinition>(); }

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

        public void RemovePatSettings()
        {
            if (SelectedPatSettings != null)
            {
                SelectedDefinition.PatSettings.Remove(SelectedPatSettings);
                OnPropertyChanged(nameof(SelectedDefinition));
            }
        }

        public void ResetPatSettings()
        {
            if (SelectedDefinition != null)
            {
                SelectedDefinition.PatSettings.Clear();
                OnPropertyChanged(nameof(SelectedDefinition));
            }
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

        public void SelectCosmeticNodePath()
        {
            if (SelectedDefinition != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var path = Path.Combine(buildPath, SelectedDefinition.InstallLocation.FilePath);
                var allowedNodes = new List<Type> { typeof(ARCNode), typeof(BRRESNode) };
                var result = _dialogService.OpenNodeSelectorDialog(path, "Select Node", "Select node containing cosmetics", allowedNodes);
                if (result.Result)
                {
                    SelectedDefinition.InstallLocation.NodePath = result.NodePath;
                    OnPropertyChanged(nameof(SelectedDefinition));
                }
            }
        }

        public void ClearCosmeticNodePath()
        {
            if (SelectedDefinition != null)
            {
                SelectedDefinition.InstallLocation.NodePath = string.Empty;
                OnPropertyChanged(nameof(SelectedDefinition));
            }
        }

        public void SelectModelPath()
        {
            if (SelectedDefinition != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var path = Path.Combine(buildPath, SelectedDefinition.InstallLocation.FilePath);
                var allowedNodes = new List<Type> { typeof(ARCNode), typeof(BRRESNode) };
                var result = _dialogService.OpenNodeSelectorDialog(path, "Select Node", "Select node containing models", allowedNodes);
                if (result.Result)
                {
                    SelectedDefinition.ModelPath = result.NodePath;
                    OnPropertyChanged(nameof(SelectedDefinition));
                }
            }
        }

        public void ClearModelPath()
        {
            if (SelectedDefinition != null)
            {
                SelectedDefinition.ModelPath = string.Empty;
                OnPropertyChanged(nameof(SelectedDefinition));
            }
        }

        public void NullModelPath()
        {
            if (SelectedDefinition != null)
            {
                SelectedDefinition.ModelPath = null;
                OnPropertyChanged(nameof(SelectedDefinition));
            }
        }

        public void SelectPatNodePath()
        {
            if (SelectedPatSettings != null)
            {
                var buildPath = _settingsService.AppSettings.BuildPath;
                var path = Path.Combine(buildPath, SelectedDefinition.InstallLocation.FilePath);
                var allowedNodes = new List<Type> { typeof(PAT0TextureNode) };
                var result = _dialogService.OpenNodeSelectorDialog(path, "Select Node", "Select node containing PAT0 texture entries", allowedNodes);
                if (result.Result)
                {
                    SelectedPatSettings.Path = result.NodePath;
                    OnPropertyChanged(nameof(SelectedDefinition));
                }
            }
        }

        public void ClearPatNodePath()
        {
            if (SelectedPatSettings != null)
            {
                SelectedPatSettings.Path = string.Empty;
                OnPropertyChanged(nameof(SelectedDefinition));
            }
        }
    }
}
