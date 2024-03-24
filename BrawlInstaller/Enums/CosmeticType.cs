using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Enums
{
    public enum CosmeticType
    {
        [Description("CSS Icon")]
        CSSIcon,
        [Description("CSP")]
        CSP,
        [Description("BP")]
        BP,
        PortraitName,
        BPName,
        CSSIconName,
        ReplayIcon,
        FranchiseIcon,
        StockIcon,
        CreditsIcon,
        TrophyThumbnail
    }
}
