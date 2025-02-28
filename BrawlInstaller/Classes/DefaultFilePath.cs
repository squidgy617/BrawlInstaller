using BrawlInstaller.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class DefaultFilePath
    {
        public FileType FileType { get; set; }
        public string Filter { get; set; } = string.Empty;
        public bool Required { get; set; } = true;
        public List<Type> AllowedNodes = null;

        public DefaultFilePath(FileType fileType, string filter, bool required = true, List<Type> allowedNodes = null) 
        { 
            FileType = fileType;
            Filter = filter;
            Required = required;
            AllowedNodes = allowedNodes;
        }
    }
}
