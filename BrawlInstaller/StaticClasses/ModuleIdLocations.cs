using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.StaticClasses
{
    public static class ModuleIdLocations
    {
        public static List<(string ModuleName, List<int> Locations)> IdLocations = new List<(string ModuleName, List<int> Locations)>
        {
            ("ft_pit", new List<int> { 0xA0, 0x168, 0x1804, 0x8E4C, 0x8F3C, 0xDAE8, 0xDB50, 0xDBAC, 0x15A90, 0x16CC8 }),
            ("ft_marth", new List<int> { 0x98, 0x160, 0x17D8, 0x5724, 0xA430, 0xA498, 0xA4F4 }),
            ("ft_lucario", new List<int> { 0x98, 0x160, 0x1804, 0x83F8, 0x8510, 0x963C, 0x9794, 0xD2D0, 0xD338, 0xD394 }),
            ("ft_sonic", new List<int> { 0x98, 0x160, 0x1818, 0x8598, 0xEDB8, 0xEE20, 0xEE7C })
        };
    }
}
