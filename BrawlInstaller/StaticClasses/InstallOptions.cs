using BrawlInstaller.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.StaticClasses
{
    public static class InstallOptions
    {
        public static Dictionary<InstallOptionType, string> InstallOptionFilters = new Dictionary<InstallOptionType, string>
        {
            { InstallOptionType.MovesetFile, "PAC file (.pac)|*.pac" },
            { InstallOptionType.Module, "REL file (.rel)|*.rel" },
            { InstallOptionType.Sounbank, "SAWND file (.sawnd)|*.sawnd" },
            { InstallOptionType.KirbySoundbank, "SAWND file (.sawnd)|*.sawnd" },
            { InstallOptionType.VictoryTheme, "BRSTM file (.brstm)|*.brstm" },
            { InstallOptionType.CreditsTheme, "BRSTM file (.brstm)|*.brstm" }
        };

        public static Dictionary<InstallOptionType, string> InstallOptionExtensions = new Dictionary<InstallOptionType, string>
        {
            { InstallOptionType.MovesetFile, "pac" },
            { InstallOptionType.Module, "rel" },
            { InstallOptionType.Sounbank, "sawnd" },
            { InstallOptionType.KirbySoundbank, "sawnd" },
            { InstallOptionType.VictoryTheme, "brstm" },
            { InstallOptionType.CreditsTheme, "brstm" }
        };
    }
}
