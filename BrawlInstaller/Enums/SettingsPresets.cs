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
        [Description("P+Ex")]
        ProjectPlusEx,
        [Description("PMEX REMIX")]
        REMIX
    }
}
