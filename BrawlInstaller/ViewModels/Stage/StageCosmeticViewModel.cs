using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BrawlInstaller.ViewModels
{
    public interface IStageCosmeticViewModel
    {
        BitmapImage Image { get; }
    }

    [Export(typeof(IStageCosmeticViewModel))]
    internal class StageCosmeticViewModel : ViewModelBase, IStageCosmeticViewModel
    {
        // Private properties
        private StageInfo _stage;

        // Services

        [ImportingConstructor]
        public StageCosmeticViewModel()
        {
            WeakReferenceMessenger.Default.Register<StageLoadedMessage>(this, (recipient, message) =>
            {
                LoadStage(message);
            });
        }

        // Properties
        public StageInfo Stage { get => _stage; set { _stage = value; OnPropertyChanged(nameof(Stage)); } }

        [DependsUpon(nameof(Stage))]
        public BitmapImage Image { get => Stage?.Cosmetics?.Items?.FirstOrDefault()?.Image; }

        // Methods
        public void LoadStage(StageLoadedMessage message)
        {
            Stage = message.Value;
            OnPropertyChanged(nameof(Stage));
        }
    }
}
