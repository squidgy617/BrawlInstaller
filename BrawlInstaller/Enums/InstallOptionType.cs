using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Enums
{
    public enum InstallOptionType
    {
        [Description("Moveset File")]
        MovesetFile,
        [Description("Module")]
        Module,
        [Description("Soundbank")]
        Sounbank,
        [Description("Kirby Soundbank")]
        KirbySoundbank
    }
}
