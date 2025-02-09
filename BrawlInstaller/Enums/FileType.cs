using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Enums
{
    public enum FileType
    {
        [Description("Fighter Files")]
        FighterFiles,
        [Description("BrawlEx Folder")]
        BrawlEx,
        [Description("Masquerade Folder")]
        MasqueradePath,
        [Description("Module Folder")]
        Modules,
        [Description("Stage Slot Folder")]
        StageSlots,
        [Description("Stage Param Folder")]
        StageParamPath,
        [Description("Stage PAC Folder")]
        StagePacPath,
        [Description("Tracklist Folder")]
        TracklistPath,
        [Description("Classic Intro Folder")]
        ClassicIntroPath,
        [Description("Ending Folder")]
        EndingPath,
        [Description("Movie Folder")]
        MoviePath,
        [Description("BRSTM Folder")]
        BrstmPath,
        [Description("Soundbank Folder")]
        SoundbankPath,
        [Description("Stage Alt List Folder")]
        StageAltListPath,
        [Description("Credits Module")]
        CreditsModule,
        [Description("SSE Module")]
        SSEModule,
        [Description("Stage Table")]
        StageTablePath,
        [Description("Credits Theme Table")]
        CreditsThemeAsmFile,
        [Description("Ending Table")]
        EndingAsmFile,
        [Description("Throw Release Table")]
        ThrowReleaseAsmFile,
        [Description("Fighter-Specific Codes")]
        FighterSpecificAsmFile,
        [Description("L-Load Table")]
        LLoadAsmFile,
        [Description("SlotEx Table")]
        SlotExAsmFile,
        [Description("GCT Code File")]
        GCTCodeFile,
        [Description("Stage List File")]
        StageListFile,
        [Description("GCTRealMate EXE File")]
        GctRealMateExe,
        [Description("Fighter Target Break Folder")]
        TargetBreakFolder,
        [Description("Code Menu Config")]
        CodeMenuConfig,
        [Description("Code Menu Builder EXE File")]
        CodeMenuBuilder,
        [Description("Netplay Tracklist Folder")]
        NetplaylistPath,
        [Description("Trophy Location")]
        TrophyLocation,
        [Description("Trophy Name List")]
        TrophyNames,
        [Description("Trophy Description List")]
        TrophyDescriptions,
        [Description("Trophy Game Icons Location")]
        TrophyGameIconsLocation,
        [Description("Trophy BRRES Location")]
        TrophyBrresLocation,
        [Description("FighterTrophy Location")]
        FighterTrophyLocation
    }
}
