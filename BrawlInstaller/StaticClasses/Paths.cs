using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.StaticClasses
{
    public static class Paths
    {
        public static string AppPath = AppDomain.CurrentDomain.BaseDirectory;
        public static string TempPath = Path.Combine(AppPath, "temp");
        public static string AppSettingsPath = Path.Combine(AppPath, "AppSettings.json");
        public static string ErrorPath = Path.Combine(AppPath, "Error.txt");
        public static string BackupPath = Path.Combine(AppPath, "Backups");
    }
}
