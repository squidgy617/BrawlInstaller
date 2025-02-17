using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BrawlInstaller.ViewModels
{
    public interface IProgressBarViewModel
    {
        string Caption { get; set; }
        int Maximum { get; set; }
        int Progress { get; set; }
        event EventHandler OnRequestClose;
    }

    [Export(typeof(IProgressBarViewModel))]
    internal class ProgressBarViewModel : ViewModelBase, IProgressBarViewModel
    {
        // Private properties
        private string _caption;
        private int _progress;
        private int _maximum;

        // Events
        public event EventHandler OnRequestClose;

        // Importing constructor
        [ImportingConstructor]
        public ProgressBarViewModel()
        {
            WeakReferenceMessenger.Default.Register<UpdateProgressMessage>(this, (recipient, message) =>
            {
                UpdateProgress(message.Value);
            });
        }

        // Properties
        public string Caption { get => _caption; set { _caption = value.Replace("\n", "\n\n"); OnPropertyChanged(nameof(Caption)); } }
        public int Progress { get => _progress; set { _progress = value; OnPropertyChanged(nameof(Progress)); } }
        public int Maximum { get =>  _maximum; set { _maximum = value; OnPropertyChanged(nameof(Maximum)); } }

        // Methods
        private void UpdateProgress(int progress)
        {
            Progress += progress;
            AllowUIToUpdate();
            if (Progress >= Maximum)
            {
                OnRequestClose?.Invoke(this, EventArgs.Empty);
            }
        }

        private void AllowUIToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                          new Action(delegate { }));
        }
    }

    public class UpdateProgressMessage : ValueChangedMessage<int>
    {
        public UpdateProgressMessage(int increment) : base(increment)
        {

        }
    }
}
