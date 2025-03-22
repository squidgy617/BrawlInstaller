using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.StaticClasses
{
    public static class PacFiles
    {
        public static List<string> PacFileSuffixes = new List<string>
        {
            "MotionEtc",
            "Motion",
            "Etc",
            "Final",
            "Spy",
            "Dark",
            "Result",
            "Entry",
            "AltR",
            "AltZ",
            "Alt",
            "##Param",
            "Param",
            "##Brres",
            "Brres",
            "Fake",
            "$TAG"
        };

        public static List<string> PacFileRegexes = new List<string>(PacFileSuffixes).Append("\\$.+").ToList();
    }
}
