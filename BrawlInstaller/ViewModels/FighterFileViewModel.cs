using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.ViewModels
{
    public interface IFighterFileViewModel
    {
        List<FighterFiles> FighterFiles { get; }
        FighterFiles SelectedFighterFiles { get; }
    }

    [Export(typeof(IFighterFileViewModel))]
    internal class FighterFileViewModel : ViewModelBase, IFighterFileViewModel
    {
        // Private properties
        private List<FighterFiles> _fighterFiles;
        private FighterFiles _selectedFighterFiles;

        // Importing constructor
        [ImportingConstructor]
        public FighterFileViewModel()
        {
            WeakReferenceMessenger.Default.Register<FighterLoadedMessage>(this, (recipient, message) =>
            {
                LoadFighterFiles(message);
            });
        }

        // Properties
        public List<FighterFiles> FighterFiles { get => _fighterFiles; set {  _fighterFiles = value; OnPropertyChanged(nameof(FighterFiles)); } }

        [DependsUpon(nameof(FighterFiles))]
        public FighterFiles SelectedFighterFiles { get => _selectedFighterFiles; set { _selectedFighterFiles = value; OnPropertyChanged(); } }

        // Methods
        public void LoadFighterFiles(FighterLoadedMessage message)
        {
            FighterFiles = message.Value.FighterFiles;
            SelectedFighterFiles = FighterFiles.FirstOrDefault();
        }
    }
}
