using BrawlInstaller.StaticClasses;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BrawlInstaller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (Directory.Exists(Paths.TempPath))
            {
                Directory.Delete(Paths.TempPath, true);
            }
        }
    }
}
