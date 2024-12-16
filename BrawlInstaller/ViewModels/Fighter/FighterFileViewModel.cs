using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrawlInstaller.ViewModels
{
    public interface IFighterFileViewModel
    {
        FighterPackage FighterPackage { get; }
    }

    [Export(typeof(IFighterFileViewModel))]
    internal class FighterFileViewModel : ViewModelBase, IFighterFileViewModel
    {
        // Private properties
        private FighterPackage _fighterPackage;
        private FighterPacFile _selectedPacFile;
        private string _selectedExConfig;
        private string _selectedEndingPacFile;

        // Services
        IDialogService _dialogService { get; }
        IFighterService _fighterService { get; }

        // Commands
        public ICommand ChangedThemeCommand => new RelayCommand(param => ChangedThemeId(param));
        public ICommand AddPacFilesCommand => new RelayCommand(param => AddPacFiles());
        public ICommand RemovePacFileCommand => new RelayCommand(param => RemovePacFile());
        public ICommand AddEndingPacFilesCommand => new RelayCommand(param => AddEndingPacFiles());
        public ICommand RemoveEndingPacFileCommand => new RelayCommand(param => RemoveEndingPacFile());

        // Importing constructor
        [ImportingConstructor]
        public FighterFileViewModel(IDialogService dialogService, IFighterService fighterService)
        {
            _dialogService = dialogService;
            _fighterService = fighterService;
            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadFighterFiles(message);
            });

            WeakReferenceMessenger.Default.Register<AttributesUpdatedMessage>(this, (recipient, message) =>
            {
                OnPropertyChanged(nameof(SoundbankControlsEnabled));
                OnPropertyChanged(nameof(VictoryThemeControlsEnabled));
            });
        }

        // Properties
        public FighterPackage FighterPackage { get => _fighterPackage; set { _fighterPackage = value; OnPropertyChanged(nameof(FighterPackage)); } }

        [DependsUpon(nameof(FighterPackage))]
        public FighterPacFile SelectedPacFile { get => _selectedPacFile; set { _selectedPacFile = value; OnPropertyChanged(nameof(SelectedPacFile)); } }

        [DependsUpon(nameof(FighterPackage))]
        public string SelectedExConfig { get => _selectedExConfig; set { _selectedExConfig = value; OnPropertyChanged(nameof(SelectedExConfig)); } }

        [DependsUpon(nameof(FighterPackage))]
        public string SelectedEndingPacFile { get => _selectedEndingPacFile; set { _selectedEndingPacFile = value; OnPropertyChanged(nameof(SelectedEndingPacFile)); } }

        [DependsUpon(nameof(FighterPackage))]
        public ObservableCollection<FighterPacFile> PacFiles { get => FighterPackage != null ? new ObservableCollection<FighterPacFile>(FighterPackage?.PacFiles) : new ObservableCollection<FighterPacFile>(); }

        [DependsUpon(nameof(FighterPackage))]
        public ObservableCollection<string> EndingPacFiles { get => FighterPackage != null ? new ObservableCollection<string>(FighterPackage?.EndingPacFiles) : new ObservableCollection<string>(); }

        [DependsUpon(nameof(FighterPackage))]
        public bool SoundbankControlsEnabled { get => FighterPackage?.FighterInfo?.FighterAttributes != null; }

        [DependsUpon(nameof(FighterPackage))]
        public bool VictoryThemeControlsEnabled { get => FighterPackage?.FighterInfo?.SlotAttributes != null; }

        // Methods
        public void LoadFighterFiles(FighterLoadedMessage message)
        {
            FighterPackage = message.Value;
        }

        public void ChangedThemeId(object idObject)
        {
            var idString = idObject as string;
            var result = uint.TryParse(idString.Replace("0x", string.Empty), NumberStyles.HexNumber, null, out uint id);
            if (result)
            {
                if (id < 0x0000F000)
                {
                    _dialogService.ShowMessage("ID is less than minimum custom ID value of 0xF000. Tracklist entries will not be created for non-custom IDs. If you'd like to import a song, change the ID to 0xF000 or greater.", "Song Will Not Import");
                }
            }
        }

        public void AddPacFiles()
        {
            var files = _dialogService.OpenMultiFileDialog("Select pac files", "PAC files (.pac)|*.pac");
            foreach (var file in files)
            {
                var pacFile = new FighterPacFile { FilePath = file };
                pacFile = _fighterService.GetFighterPacName(pacFile, FighterPackage.FighterInfo, false);
                pacFile.Subdirectory = string.Empty;
                FighterPackage.PacFiles.Add(pacFile);
            }
            OnPropertyChanged(nameof(FighterPackage));
        }

        public void RemovePacFile()
        {
            FighterPackage.PacFiles.Remove(SelectedPacFile);
            OnPropertyChanged(nameof(FighterPackage));
        }

        public void AddEndingPacFiles()
        {
            var files = _dialogService.OpenMultiFileDialog("Select ending pac files", "PAC files (.pac)|*.pac");
            foreach (var file in files)
            {
                var pacFile = file;
                FighterPackage.EndingPacFiles.Add(pacFile);
            }
            OnPropertyChanged(nameof(FighterPackage));
        }

        public void RemoveEndingPacFile()
        {
            FighterPackage.EndingPacFiles.Remove(SelectedEndingPacFile);
            OnPropertyChanged(nameof(FighterPackage));
        }
    }
}
