using BrawlInstaller.Enums;
using BrawlLib.Imaging;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.ResourceNodes.ProjectPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrawlLib.SSBB.ResourceNodes.ProjectPlus.STEXNode;

namespace BrawlInstaller.Classes
{
    public class StageInfo
    {
        public string RandomName { get; set; } = string.Empty;
        public StageSlot Slot { get; set; }
        public CosmeticList Cosmetics { get; set; } = new CosmeticList();
        public List<StageEntry> StageEntries { get; set; } = new List<StageEntry>();
        public List<StageParams> AllParams { get; set; } = new List<StageParams>();
    }

    public class StageList
    {
        public string Name { get; set; }
        public List<StagePage> Pages { get; set; } = new List<StagePage>();
        public List<StageSlot> UnusedSlots { get; set; } = new List<StageSlot>();
    }

    public class StagePage
    {
        public int PageNumber { get; set; }
        public List<StageSlot> StageSlots { get; set; } = new List<StageSlot>();
    }

    public class StageSlot
    {
        public BrawlIds StageIds { get; set; } = new BrawlIds();
        public string Name { get; set; } = "Unknown";
        public int Index { get; set; }

        public StageSlot Copy()
        {
            var newStageSlot = new StageSlot
            {
                Index = Index,
                Name = Name,
                StageIds = new BrawlIds
                {
                    StageId = StageIds.StageId,
                    StageCosmeticId = StageIds.StageCosmeticId
                }
            };
            return newStageSlot;
        }
    }

    public class StageEntry
    {
        public ushort ButtonFlags { get; set; } = 0x0000;
        public string BinFileName { get; set; } = "Unknown";
        public string BinFilePath { get; set; } = null;
        public bool IsRAlt { get => ((GameCubeButtons)ButtonFlags & GameCubeButtons.Unused0x4000) != 0; }
        public bool IsLAlt { get => ((GameCubeButtons)ButtonFlags & GameCubeButtons.Unused0x8000) != 0; }
        public StageParams Params { get; set; } = new StageParams();
    }

    public class StageParams
    {
        public string Name { get; set; } = "Unknown";
        public string PacName { get; set; } = "Unknown";
        public string PacFile { get; set; } = null;
        public string TrackList { get; set; } = string.Empty;
        public string TrackListFile { get; set; } = null;
        public string Module { get; set; } = string.Empty;
        public string ModuleFile { get; set; } = null;
        public RGBAPixel CharacterOverlay { get; set; } = new RGBAPixel { R = 0, G = 0, B = 0, A = 0 };
        public ushort SoundBank { get; set; } = 0xFFFF;
        public string SoundBankFile { get; set; } = null;
        public ushort EffectBank { get; set; } = 0x0032;
        public uint MemoryAllocation { get; set; } = 0x00000000;
        public float WildSpeed { get; set; } = 0;
        public bool IsFlat { get; set; } = false;
        public bool IsFixedCamera { get; set; } = false;
        public bool IsSlowStart { get; set; } = false;
        public bool IsDualLoad { get; set; } = false;
        public bool IsDualShuffle { get; set; } = false;
        public bool IsOldSubStage { get; set; } = false;
        public VariantType VariantType { get; set; } = VariantType.None;
        public byte SubstageRange { get; set; } = 0;
        public List<Substage> Substages { get; set; } = new List<Substage>();

        public STEXNode ConvertToNode()
        {
            // Generate param file
            var newParam = new STEXNode
            {
                Name = Name,
                IsFlat = IsFlat,
                IsFixedCamera = IsFixedCamera,
                IsSlowStart = IsSlowStart,
                StageName = PacName,
                TrackList = TrackList,
                Module = Module,
                CharacterOverlay = CharacterOverlay,
                SoundBank = SoundBank,
                EffectBank = EffectBank,
                MemoryAllocation = MemoryAllocation,
                WildSpeed = WildSpeed,
                IsDualLoad = IsDualLoad,
                IsDualShuffle = IsDualShuffle,
                IsOldSubstage = IsOldSubStage,
                SubstageVarianceType =  VariantType,
                SubstageRange = SubstageRange
            };
            // Add substages if applicable
            foreach (var substage in Substages)
            {
                var substageNode = new RawDataNode
                {
                    Name = substage.Name
                };
                newParam.AddChild(substageNode);
            }
            return newParam;
        }
    }

    public class Substage
    {
        public string Name { get; set; } = "00";
        public string PacFile { get; set; } = null;
    }
}
