using BrawlLib.SSBB.ResourceNodes;
using System.Collections.Generic;
using System.Linq;

namespace BrawlCrate.UI
{
    public static class Icons
    {
        private static readonly Dictionary<ResourceType, string> IconDictionary = new Dictionary<ResourceType, string>
        {
            //Base Types
            {ResourceType.Unknown, "Unknown"},
            {ResourceType.Container, "Folder"},

            //Archives
            {ResourceType.ARC, "ARC"},
            {ResourceType.U8, "U8"},
            {ResourceType.U8Folder, "Folder"},
            {ResourceType.BRES, "BRES"},
            {ResourceType.BFRESGroup, "Folder"},
            {ResourceType.MRG, "Folder"},
            {ResourceType.BLOC, "BLOC"},
            {ResourceType.Redirect, "Redirect"},
            {ResourceType.RARCFolder, "Folder"},

            //Effects
            {ResourceType.EFLS, "EFLS"},
            {ResourceType.REFF, "REFF"},
            {ResourceType.REFFEntry, "REFFEntry"},
            {ResourceType.REFT, "REFT"},
            {ResourceType.REFTImage, "IMG"},

            //Modules
            {ResourceType.REL, "REL"},

            //Misc
            {ResourceType.CollisionDef, "Coll"},
            {ResourceType.MSBin, "MSG"},
            {ResourceType.STPM, "STPM"},
            {ResourceType.STDT, "STDT"},
            {ResourceType.SCLA, "SCLA"},
            {ResourceType.SndBgmTitleDataFolder, "Folder"},
            {ResourceType.ClassicStageTbl, "Folder"},

            //AI
            {ResourceType.AI, "AI"},
            {ResourceType.CE, "CE"},
            {ResourceType.AIPD, "AIPD"},
            {ResourceType.ATKD, "ATKD"},

            //Textures
            {ResourceType.TPL, "TPL"},
            {ResourceType.TPLTexture, "IMG"},
            {ResourceType.TPLPalette, "Palette"},

            //NW4R
            {ResourceType.TEX0, "TEX0"},
            {ResourceType.SharedTEX0, "SharedTEX0"},
            {ResourceType.PLT0, "PLT0"},

            {ResourceType.MDL0, "MDL0"},
            {ResourceType.MDL0Group, "Folder"},

            {ResourceType.CHR0, "CHR"},

            {ResourceType.CLR0, "CLR"},

            {ResourceType.VIS0, "VIS"},
            {ResourceType.SCN0, "SCN0"},

            {ResourceType.SHP0, "SHP"},

            {ResourceType.SRT0, "SRT"},

            {ResourceType.PAT0, "PAT"},

            //Audio
            {ResourceType.RSAR, "RSAR"},
            {ResourceType.RSTM, "RSTM"},
            {ResourceType.RSARFile, "S"},
            {ResourceType.RSARGroup, "G"},
            {ResourceType.RSARType, "T"},
            {ResourceType.RSARBank, "B"},

            //Groups
            {ResourceType.BRESGroup, "Folder"},
            {ResourceType.RSARFolder, "Folder"},
            {ResourceType.RSARFileSoundGroup, "Folder"},
            {ResourceType.RWSDDataGroup, "Folder"},
            {ResourceType.RSEQGroup, "Folder"},
            {ResourceType.RBNKGroup, "Folder"},

            //Moveset
            {ResourceType.MDef, "MDef"},
            {ResourceType.NoEditFolder, "Folder"},
            {ResourceType.NoEditEntry, "Folder"},
            {ResourceType.NoEditItem, "Unknown"},
            {ResourceType.MDefAction, "MDefAction"},
            {ResourceType.MDefActionGroup, "Folder"},
            {ResourceType.MDefSubActionGroup, "Folder"},
            {ResourceType.MDefMdlVisGroup, "Folder"},
            {ResourceType.MDefMdlVisRef, "Folder"},
            {ResourceType.MDefMdlVisSwitch, "Folder"},
            {ResourceType.MDefActionList, "Folder"},
            {ResourceType.MDefSubroutineList, "Folder"},
            {ResourceType.MDefActionOverrideList, "Folder"},
            {ResourceType.MDefHurtboxList, "Folder"},
            {ResourceType.MDefRefList, "Folder"},
            {ResourceType.Event, "Event"},

            //Nintendo Disc Image
            {ResourceType.DiscImagePartition, "Folder"},

            //J3D
            {ResourceType.BMDGroup, "Folder"},

            //Subspace Emmisary
            {ResourceType.GDOR, "GDOR"},
            {ResourceType.GEG1, "GEG1"},
            {ResourceType.ENEMY, "ENEMY"},
            {ResourceType.GMOV, "GMOV"},
            {ResourceType.GSND, "GSND"},
            {ResourceType.GMOT, "GMOT"},
            {ResourceType.ADSJ, "ADSJ"},
            {ResourceType.GBLK, "GBLK"},
            {ResourceType.GMPS, "GMPS"},
            {ResourceType.BGMG, "BGMG"},
            {ResourceType.GDBF, "GDBF"},
            {ResourceType.GWAT, "GWAT"},
            {ResourceType.GCAM, "GCAM"},
            {ResourceType.GITM, "GITM"},
            {ResourceType.GIB2, "GIB2"},

            {ResourceType.HavokGroup, "Folder"},

            //BrawlEx
            {ResourceType.RSTCGroup, "Folder"},

            // Items
            {ResourceType.ItemFreqNode, "Folder"},
            {ResourceType.ItemFreqTableNode, "Folder"},
            {ResourceType.ItemFreqTableGroupNode, "Folder"},
            {ResourceType.ItemFreqEntryNode, "itembox"},

            //P+
            {ResourceType.ASLS, "Folder"},

            {ResourceType.Folder, "Folder"},
            {ResourceType.SELCTeam, "Folder"}
        };

        public static string GetIconString(ResourceType resourceType)
        {
            IconDictionary.TryGetValue(resourceType, out string value);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
            else
            {
                return IconDictionary.FirstOrDefault().Value;
            }
        }
    }
}