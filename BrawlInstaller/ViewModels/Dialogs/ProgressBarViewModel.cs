using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
using BrawlInstaller.StaticClasses;
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
        event EventHandler OnRequestClose;
        int Maximum { get; set; }
        int Minimum { get; set; }
        int Value { get; set; }
        string Caption { get; set; }
    }

    [Export(typeof(IProgressBarViewModel))]
    internal class ProgressBarViewModel : ViewModelBase, IProgressBarViewModel
    {
        // Private properties
        private int _value;
        private int _maximum;
        private int _minimum;
        private string _caption;

        // Events
        public event EventHandler OnRequestClose;

        // Importing constructor
        [ImportingConstructor]
        public ProgressBarViewModel()
        {
            WeakReferenceMessenger.Default.Register<StartProgressMessage>(this, (recipient, message) =>
            {
                StartProgress();
            });
            WeakReferenceMessenger.Default.Register<UpdateProgressMessage>(this, (recipient, message) =>
            {
                UpdateProgress(message.Value);
            });
            WeakReferenceMessenger.Default.Register<UpdateProgressCaptionMessage>(this, (recipient, message) =>
            {
                UpdateCaption(message.Value);
            });
            WeakReferenceMessenger.Default.Register<EndProgressMessage>(this, (recipient, message) =>
            {
                EndProgressTracker();
            });
        }

        // Properties
        public int Value { get => _value; set { _value = value; OnPropertyChanged(nameof(Value)); } }
        public int Maximum { get => _maximum; set { _maximum = value; OnPropertyChanged(nameof(Maximum)); } }
        public int Minimum { get => _minimum; set { _minimum = value; OnPropertyChanged(nameof(Minimum)); } }
        public string Caption { get => _caption; set { _caption = value; OnPropertyChanged(nameof(Caption)); } }

        // Methods
        private void StartProgress()
        {
            Value = ProgressTracker.Value;
            Maximum = ProgressTracker.Maximum;
            Minimum = ProgressTracker.Minimum;
            Caption = ProgressTracker.Caption;
            AllowUIToUpdate();
        }

        private void UpdateCaption(string value)
        {
            Caption = value;
            AllowUIToUpdate();
        }

        private void UpdateProgress(int value)
        {
            Value = value;
            AllowUIToUpdate();
        }

        private void EndProgressTracker()
        {
            OnRequestClose?.Invoke(this, EventArgs.Empty);
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

    public class StartProgressMessage : ValueChangedMessage<object>
    {
        public StartProgressMessage(object param) : base(param)
        {

        }
    }

    public class UpdateProgressMessage : ValueChangedMessage<int>
    {
        public UpdateProgressMessage(int value) : base(value)
        {

        }
    }

    public class UpdateProgressCaptionMessage : ValueChangedMessage<string>
    {
        public UpdateProgressCaptionMessage(string value) : base(value)
        {

        }
    }

    public class EndProgressMessage : ValueChangedMessage<int>
    {
        public EndProgressMessage(int increment) : base(increment)
        {

        }
    }
}
