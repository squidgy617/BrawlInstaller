﻿using BrawlInstaller.Classes;
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
        public static string Caption { get; set; } = string.Empty;
        public static int Value { get; set; } = 0;
        public static int Minimum { get; set; } = 0;
        public static int Maximum { get; set; } = 100;

        public static void Start(string caption, int minimum, int maximum)
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

        public static void End()
        {
            Value = Maximum;
            WeakReferenceMessenger.Default.Send(new UpdateProgressMessage(Value));
        }
    }
}
