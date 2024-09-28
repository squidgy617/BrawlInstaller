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
        FighterPackage FighterPackage { get; }
    }

    [Export(typeof(IFighterFileViewModel))]
    internal class FighterFileViewModel : ViewModelBase, IFighterFileViewModel
    {
        // Private properties
        private FighterPackage _fighterPackage;

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
        public FighterPackage FighterPackage { get => _fighterPackage; set { _fighterPackage = value; OnPropertyChanged(nameof(FighterPackage)); } }

        // Methods
        public void LoadFighterFiles(FighterLoadedMessage message)
        {
            FighterPackage = message.Value;
        }
    }
}
