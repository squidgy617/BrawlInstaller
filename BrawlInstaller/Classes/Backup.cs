using BrawlInstaller.StaticClasses;
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
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string BuildBackupPath { get => Path.Combine(Paths.BackupPath, Guid.ToString(), "Build"); }
        public string TextureBackupPath { get => Path.Combine(Paths.BackupPath, Guid.ToString(), "HD Textures"); }

    }
}
