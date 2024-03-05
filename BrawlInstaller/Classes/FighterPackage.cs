using BrawlInstaller.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class FighterPackage
    {
        public List<MultiCosmeticGroup> CSPs { get; set; }
        public List<CosmeticGroup> BPs { get; set; }
    }

    // If there are multiple cosmetic groups with the same Style, user will be allowed to choose between the two
    public class CosmeticGroup
    {
        public CosmeticType CosmeticType { get; set; }
        public string Style { get; set; }
        public List<string> Images { get; set; }
    }

    public class MultiCosmeticGroup
    {
        public CosmeticType CosmeticType { get; set; }
        public string Style { get; set; }
        public List<ColorSmashGroup> ColorSmashGroups { get; set; }
    }

    public class ColorSmashGroup
    {
        public List<string> Images { get; set; }
    }
}
