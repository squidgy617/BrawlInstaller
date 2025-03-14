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
        [Description("CSP Name")]
        PortraitName,
        [Description("BP Name")]
        BPName,
        [Description("CSS Icon Name")]
        CSSIconName,
        [Description("Replay Icon")]
        ReplayIcon,
        [Description("Franchise Icon")]
        FranchiseIcon,
        [Description("Stock Icon")]
        StockIcon,
        [Description("Credits Icon")]
        CreditsIcon,
        [Description("Trophy Thumbnail")]
        TrophyThumbnail,
        [Description("Records Icon")]
        RecordsIcon,
        [Description("Stage Preview")]
        StagePreview,
        [Description("Stage Franchise Icon")]
        StageFranchiseIcon,
        [Description("Stage Icon")]
        StageIcon,
        [Description("Stage Name")]
        StageName,
        [Description("Stage Game Logo")]
        StageGameLogo,
        [Description("Stage Alt Name")]
        StageAltName,
        [Description("Stage Random Banner")]
        StageRandomBanner,
        [Description("Stage Replay Banner")]
        StageReplayBanner,
        [Description("Stage Stats")]
        StageStats,
        [Description("SSE Unlock Icon")]
        SSEUnlockIcon,
        [Description("Trophy Figure")]
        TrophyStandee
    }
}
