using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Enums
{
    public enum IdType
    {
        [Description("Fighter Config")]
        FighterConfig,
        [Description("Cosmetic Config")]
        CosmeticConfig,
        [Description("CSS Slot Config")]
        CSSSlotConfig,
        [Description("Slot Config")]
        SlotConfig,
        [Description("Cosmetic")]
        Cosmetic,
        [Description("Franchise")]
        Franchise,
        [Description("Trophy")]
        Trophy,
        [Description("Trophy Thumbnail")]
        Thumbnail,
        [Description("Records Icon")]
        RecordsIcon,
        [Description("Stage")]
        Stage,
        [Description("Stage Cosmetic")]
        StageCosmetic,
        [Description("Masquerade")]
        Masquerade
    }
}
