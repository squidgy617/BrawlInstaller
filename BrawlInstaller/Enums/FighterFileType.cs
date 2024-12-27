using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Enums
{
    public enum FighterFileType
    {
        [Description("Fit")]
        FighterPacFile,
        [Description("FitKirby")]
        KirbyPacFile,
        [Description("Itm")]
        ItemPacFile
    }
}
