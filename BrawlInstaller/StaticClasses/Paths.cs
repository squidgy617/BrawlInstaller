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
        public static string LocalStore = Directory.GetParent(AppPath.TrimEnd('\\')).FullName;
        public static string TempPath = Path.Combine(AppPath, "temp");
        public static string AppSettingsPath = Path.Combine(LocalStore, "AppSettings.json");
        public static string ErrorPath = Path.Combine(LocalStore, "Error.txt");
        public static string BackupPath = Path.Combine(LocalStore, "Backups");
    }
}
