using BrawlInstaller.Common;
using BrawlInstaller.ViewModels;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BrawlInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            CompositionBootstrapper.InitializeContainer(this);
            InitializeComponent();
            DataContext = this;
            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);
        }

        [Import]
        public IMainViewModel MainViewModel { get; set; }

        [Import]
        public IMainControlsViewModel MainControlsViewModel { get; set; }

        [Import]
        public ISettingsViewModel SettingsViewModel { get; set; }

        [Import]
        public IFighterViewModel FighterViewModel { get; set; }

        [Import]
        public IFighterInfoViewModel FighterInfoViewModel { get; set; }

        [Import]
        public IStageViewModel StageViewModel { get; set; }

        // Global error handler
        void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowUnhandledException(e);
        }

        void ShowUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            string errorMessage = e.Exception.Message;

            // TODO: Probably will need to make this a custom box, along with the dialog service box
            MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // TODO: Restore backups when an error occurs
        }
    }
}
