using BrawlInstaller.Classes;
using BrawlInstaller.Enums;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.StaticClasses
{
    public static class DefaultSettings
    {
        public static List<DefaultFilePath> DefaultFilePaths = new List<DefaultFilePath>
        {
            new DefaultFilePath(FileType.CreditsModule, "REL file (.rel)|*.rel"),
            new DefaultFilePath(FileType.SSEModule, "REL file (.rel)|*.rel"),
            new DefaultFilePath(FileType.GctRealMateExe, "EXE file (.exe)|*.exe"),
            new DefaultFilePath(FileType.CodeMenuConfig, "XML file (.xml)|*.xml"),
            new DefaultFilePath(FileType.CodeMenuBuilder, "EXE file (.exe)|*.exe"),
            new DefaultFilePath(FileType.CodeMenuSource, "ASM file (.asm)|*.asm"),
            new DefaultFilePath(FileType.CodeMenuData, "CMNU file (.cmnu)|*.cmnu", false),
            new DefaultFilePath(FileType.CodeMenuNetplayData, "CMNU file (.cmnu)|*.cmnu", false),
            new DefaultFilePath(FileType.TrophyNames, "MSBIN file (.msbin)|*.msbin"),
            new DefaultFilePath(FileType.FighterTrophyLocation, "ASM file (.asm)|*.asm"),
            new DefaultFilePath(FileType.SSETrophyModule, "REL file (.rel)|*.rel"),
            new DefaultFilePath(FileType.StageTablePath, "Stage table file (*.asm, *.rss)|*.asm;*.rss"),
            new DefaultFilePath(FileType.CreditsThemeAsmFile, "ASM file (.asm)|*.asm"),
            new DefaultFilePath(FileType.EndingAsmFile, "ASM file (.asm)|*.asm"),
            new DefaultFilePath(FileType.ThrowReleaseAsmFile, "ASM file (.asm)|*.asm"),
            new DefaultFilePath(FileType.FighterSpecificAsmFile, "ASM file (.asm)|*.asm"),
            new DefaultFilePath(FileType.LLoadAsmFile, "ASM file (.asm)|*.asm"),
            new DefaultFilePath(FileType.SlotExAsmFile, "ASM file (.asm)|*.asm"),
            new DefaultFilePath(FileType.TrophyLocation, "PAC file (.pac)|*.pac", true, new List<Type> { typeof(TyDataListNode) }),
            new DefaultFilePath(FileType.TrophyGameIconsLocation, "BRRES file (.brres)|*.brres", true, new List<Type> {typeof(PAT0TextureNode)}),
            new DefaultFilePath(FileType.CostumeSwapFile, "ASM file (.asm)|*.asm")
        };

        public static DefaultFilePath GetFilePath(FileType type)
        {
            return DefaultFilePaths.FirstOrDefault(x => x.FileType == type);
        }
    }
}
