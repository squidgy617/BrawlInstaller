using BrawlInstaller.StaticClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class Backup
    {
        public string BuildPath { get; set; }
        public List<string> AddedFiles { get; set; } = new List<string>();
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public string Guid { get; set; }
        [JsonIgnore] public string BuildBackupPath { get => Path.Combine(Paths.BackupPath, Guid.ToString(), "Build"); }
        [JsonIgnore] public string TextureBackupPath { get => Path.Combine(Paths.BackupPath, Guid.ToString(), "HD Textures"); }

    }
}
