using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
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

        // Services
        IDialogService _dialogService { get; }

        // Commands
        public ICommand ChangedThemeCommand => new RelayCommand(param => ChangedThemeId(param));

        // Importing constructor
        [ImportingConstructor]
        public FighterFileViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadFighterFiles(message);
            });
        }

        // Properties
        public FighterPackage FighterPackage { get => _fighterPackage; set { _fighterPackage = value; OnPropertyChanged(nameof(FighterPackage)); } }

        [DependsUpon(nameof(FighterPackage))]
        public FighterPacFile SelectedPacFile { get => _selectedPacFile; set { _selectedPacFile = value; OnPropertyChanged(nameof(SelectedPacFile)); } }

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
    }
}
