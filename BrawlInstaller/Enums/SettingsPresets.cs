using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Enums
{
    public enum SettingsPresets
    {
        [Description("")]
        None,
        [Description("Project+ 3.1.0")]
        ProjectPlus31,
        [Description("Project+ 3.0.2")]
        ProjectPlus30,
        [Description("P+Ex 1.6")]
        ProjectPlusEx16,
        [Description("P+Ex 1.5.5")]
        ProjectPlusEx155,
        [Description("P+Ex 1.5")]
        ProjectPlusEx15,
        [Description("REX")]
        REX,
        [Description("PMEX REMIX")]
        REMIX,
        [Description("P+Ex 1.6 (PM CSS)")]
        ProjectPlusEx16PM,
        [Description("KJP's vBrawl Build")]
        KJPBuild
    }
}
