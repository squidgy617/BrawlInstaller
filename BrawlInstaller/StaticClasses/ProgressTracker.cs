using BrawlInstaller.Classes;
using BrawlInstaller.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.StaticClasses
{
    public static class ProgressTracker
    {
        public static string Caption { get; private set; } = string.Empty;
        public static int Value { get; private set; } = 0;
        public static int? Minimum { get; private set; } = 0;
        public static int? Maximum { get; private set; } = null;

        public static void Start(string caption, int? minimum = null, int? maximum = null)
        {
            Caption = caption;
            Value = 0;
            Minimum = minimum;
            Maximum = maximum;
            WeakReferenceMessenger.Default.Send(new StartProgressMessage(null));
        }

        public static void Update(int increment)
        {
            Value += increment;
            WeakReferenceMessenger.Default.Send(new UpdateProgressMessage(Value));
        }

        public static void Update(int increment, string caption)
        {
            Update(increment);
            UpdateCaption(caption);
        }

        public static void UpdateCaption(string caption)
        {
            Caption = caption;
            WeakReferenceMessenger.Default.Send(new UpdateProgressCaptionMessage(Caption));
        }

        public static void End()
        {
            Value = Maximum ?? 0;
            WeakReferenceMessenger.Default.Send(new UpdateProgressMessage(Value));
        }

        public static void End(string caption)
        {
            Value = Maximum ?? 0;
            Caption = caption;
            WeakReferenceMessenger.Default.Send(new UpdateProgressMessage(Value));
            WeakReferenceMessenger.Default.Send(new UpdateProgressCaptionMessage(Caption));
        }
    }
}
