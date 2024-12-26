using BrawlLib.Internal;
using BrawlLib.SSBB.ResourceNodes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrawlLib.SSBB.ResourceNodes.FCFGNode;

namespace BrawlInstaller.Classes
{
    public class FighterInfo
    {
        public string EntryName { get; set; }
        public string FighterFileName { get; set; }
        public string FullPacFileName { get; set; }
        [JsonIgnore] public string PacFileName { get => FullPacFileName?.Substring(FullPacFileName.LastIndexOf('/') + 1, FullPacFileName.LastIndexOf('.') - FullPacFileName.LastIndexOf('/') - 1); }
        [JsonIgnore] public string PacExtension { get => FullPacFileName?.Substring(FullPacFileName.LastIndexOf('.'), FullPacFileName.Length - FullPacFileName.LastIndexOf('.')); }
        [JsonIgnore] public string PartialPacName { get => PacFileName?.Substring(3, PacFileName.Length - 3); } 
        [JsonIgnore] public string PacFolder { get => FullPacFileName?.Substring(0, FullPacFileName.LastIndexOf("/")); }
        public string FullKirbyPacFileName { get; set; }
        [JsonIgnore] public string KirbyPacFileName { get => FullKirbyPacFileName?.Substring(FullKirbyPacFileName.LastIndexOf('/') + 1, FullKirbyPacFileName.LastIndexOf('.') - FullKirbyPacFileName.LastIndexOf('/') - 1); }
        [JsonIgnore] public string KirbyPacExtension { get => FullKirbyPacFileName?.Substring(FullKirbyPacFileName.LastIndexOf('.'), FullKirbyPacFileName.Length - FullKirbyPacFileName.LastIndexOf('.')); }
        [JsonIgnore] public string PartialKirbyPacName { get => KirbyPacFileName?.Substring(3, KirbyPacFileName.Length - 3); }
        [JsonIgnore] public string KirbyPacFolder { get => FullKirbyPacFileName?.Substring(0, FullKirbyPacFileName.LastIndexOf("/")); }
        public string ModuleFileName { get; set; }
        public string InternalName { get; set; }
        public string DisplayName { get; set; }
        public BrawlIds Ids { get; set; } = new BrawlIds();
        [JsonIgnore]
        public string FighterConfig { get; set; }
        [JsonIgnore]
        public string CosmeticConfig { get; set; }
        [JsonIgnore]
        public string CSSSlotConfig { get; set; }
        [JsonIgnore]
        public string SlotConfig { get; set; }
        [JsonIgnore]
        public string Masquerade { get; set; }
        public uint? VictoryThemeId { get; set; } = null;
        public uint? CreditsThemeId { get; set; } = null;
        public uint? SoundbankId { get; set; } = null;
        [JsonIgnore] public  uint? OriginalSoundbankId { get; set; } = null;
        public uint? KirbySoundbankId { get; set; } = null;
        [JsonIgnore] public uint? OriginalKirbySoundbankId { get; set; } = null;
        [JsonIgnore] public int EndingId { get; set; } = -1;
        public KirbyLoadFlags KirbyLoadType { get; set; } = KirbyLoadFlags.None;
        public int? EffectPacId { get; set; } = null;
        [JsonIgnore] public int? OriginalEffectPacId { get; set; } = null;
        public int? KirbyEffectPacId { get; set; } = null;
        [JsonIgnore] public int? OriginalKirbyEffectPacId { get; set; } = null;
        public FighterAttributes FighterAttributes { get; set; } = null;
        public SlotAttributes SlotAttributes { get; set; } = null;
        public CosmeticAttributes CosmeticAttributes { get; set; } = null;
        public CSSSlotAttributes CSSSlotAttributes { get; set; } = null;

        public FighterInfo Copy()
        {
            var newFighterInfo = new FighterInfo
            {
                EntryName = EntryName,
                FighterFileName = FighterFileName,
                FullPacFileName = FullPacFileName,
                FullKirbyPacFileName = FullKirbyPacFileName,
                ModuleFileName = ModuleFileName,
                InternalName = InternalName,
                DisplayName = DisplayName,
                Ids = new BrawlIds
                {
                    FighterConfigId = Ids.FighterConfigId,
                    CosmeticConfigId = Ids.CosmeticConfigId,
                    CSSSlotConfigId = Ids.CSSSlotConfigId,
                    SlotConfigId = Ids.SlotConfigId,
                    CosmeticId = Ids.CosmeticId,
                    RecordsIconId = Ids.RecordsIconId,
                    FranchiseId = Ids.FranchiseId,
                    MasqueradeId = Ids.MasqueradeId
                },
                VictoryThemeId = VictoryThemeId,
                CreditsThemeId = CreditsThemeId,
                SoundbankId = SoundbankId,
                OriginalSoundbankId = OriginalSoundbankId,
                KirbySoundbankId = KirbySoundbankId,
                OriginalKirbySoundbankId = OriginalKirbySoundbankId,
                EndingId = EndingId,
                KirbyLoadType = KirbyLoadType,
                EffectPacId = EffectPacId,
                OriginalEffectPacId = OriginalEffectPacId,
                KirbyEffectPacId = KirbyEffectPacId,
                OriginalKirbyEffectPacId = OriginalKirbyEffectPacId,
                FighterAttributes = FighterAttributes?.Copy(),
                SlotAttributes = SlotAttributes?.Copy(),
                CosmeticAttributes = CosmeticAttributes?.Copy(),
                CSSSlotAttributes = CSSSlotAttributes?.Copy()
            };
            return newFighterInfo;
        }

        public FighterInfo CopyNoAttributes()
        {
            var newFighterInfo = Copy();
            newFighterInfo.FighterAttributes = null;
            newFighterInfo.CosmeticAttributes = null;
            newFighterInfo.CSSSlotAttributes = null;
            newFighterInfo.SlotAttributes = null;
            return newFighterInfo;
        }
    }

    public class FighterAttributes
    {
        [Category("Abilities")] public bool CanCrawl { get; set; } = false;
        [Category("Abilities")] public byte CanFTilt { get; set; } = 1;
        [Category("Abilities")] public bool CanGlide { get; set; } = false;
        [Category("Abilities")] public bool CanWallCling { get; set; } = false;
        [Category("Abilities")] public bool CanWallJump { get; set; } = false;
        [Category("Abilities")] public bool CanZAir { get; set; } = false;
        [Category("Characteristics")] [Browsable(true)] public AirJumpFlags AirJumpCount { get; set; } = AirJumpFlags.NormalAirJump;
        [Category("Characteristics")] public bool DAIntoCrouch { get; set; } = false;
        [Category("Characteristics")] public uint FSmashCount { get; set; } = 1;
        [Category("Characteristics")] public bool HasRapidJab { get; set; } = false;
        [Category("Characteristics")] public uint JabCount { get; set; } = 1;
        [Category("Characteristics")] public uint JabFlag { get; set; } = 1;
        [Category("Costumes")] public bool HasCostume00 { get; set; } = true;
        [Category("Costumes")] public bool HasCostume01 { get; set; } = true;
        [Category("Costumes")] public bool HasCostume02 { get; set; } = true;
        [Category("Costumes")] public bool HasCostume03 { get; set; } = true;
        [Category("Costumes")] public bool HasCostume04 { get; set; } = true;
        [Category("Costumes")] public bool HasCostume05 { get; set; } = true;
        [Category("Costumes")] public bool HasCostume06 { get; set; } = true;
        [Category("Costumes")] public bool HasCostume07 { get; set; } = true;
        [Category("Costumes")] public bool HasCostume08 { get; set; } = true;
        [Category("Costumes")] public bool HasCostume09 { get; set; } = true;
        [Category("Costumes")] public bool HasCostume10 { get; set; } = true;
        [Category("Costumes")] public bool HasCostume11 { get; set; } = true;
        [Category("Costumes")] public bool UnknownFlagA { get; set; } = true;
        [Category("Costumes")] public bool UnknownFlagB { get; set; } = true;
        [Category("Costumes")] public bool UnknownFlagC { get; set; } = true;
        [Category("Costumes")] public bool UnknownFlagD { get; set; } = true;
        [Category("Resources")] public bool HasPac { get; set; } = true;
        [Category("Resources")] public bool HasModule { get; set; } = true;
        [Category("Resources")] [Browsable(true)] public MotionEtcTypes MotionEtcType { get; set; } = MotionEtcTypes.SingleMerged;
        [Category("Resources")] public bool UnknownLoadFlagA { get; set; } = false;
        [Category("Resources")] public bool UnknownLoadFlagB { get; set; } = false;
        [Category("Resources")] [Browsable(true)] public EntryResultLoadFlags EntryLoadType { get; set; } = EntryResultLoadFlags.Single;
        [Category("Resources")] [Browsable(true)] public EntryResultLoadFlags ResultLoadType { get; set; } = EntryResultLoadFlags.Single;
        [Category("Resources")] [Browsable(true)] public FinalLoadFlags FinalLoadType { get; set; } = FinalLoadFlags.Single;
        [Category("Sound")] public bool FinalSmashMusic { get; set; } = false;
        [Category("Fighter Config")] public uint Version { get; set; } = 2;
        [Category("Misc")] public uint AIController { get; set; } = 0;
        [Category("Misc")] public uint EntryFlag { get; set; } = 4;
        [Category("Misc")] public bool IkPhysics { get; set; } = false;
        [Category("Misc")] public uint TextureLoader { get; set; } = 0;
        [Category("Misc")] [Browsable(true)] public ThrownTypes ThrownType { get; set; } = ThrownTypes.Default;
        [Category("Misc")] [Browsable(true)] public GrabSizes GrabSize { get; set; } = GrabSizes.Short;
        [Category("Misc")] public bool WorkManage { get; set; } = false;

        public FighterAttributes Copy()
        {
            var newAttributes = JsonConvert.DeserializeObject<FighterAttributes>(JsonConvert.SerializeObject(this));
            return newAttributes;
        }

        public FCFGNode ToFCFGNode(FighterInfo fighterInfo)
        {
            var node = ToFCFGNode();
            node.FighterName = fighterInfo.FighterFileName;
            node.PacName = fighterInfo.FullPacFileName;
            node.KirbyPacName = fighterInfo.FullKirbyPacFileName;
            node.ModuleName = fighterInfo.ModuleFileName;
            node.InternalFighterName = fighterInfo.InternalName;
            node.KirbyLoadType = fighterInfo.KirbyLoadType;
            node.SoundBank = (uint)fighterInfo.SoundbankId;
            node.KirbySoundBank = (uint)fighterInfo.KirbySoundbankId;
            return node;
        }

        private FCFGNode ToFCFGNode()
        {
            var node = new FCFGNode
            {
                _tag = 0x47464346,
                _size = 0x100,
                CanCrawl = CanCrawl,
                CanFTilt = CanFTilt,
                CanGlide = CanGlide,
                CanWallCling = CanWallCling,
                CanWallJump = CanWallJump,
                CanZAir = CanZAir,
                AirJumpCount = AirJumpCount,
                DAIntoCrouch = DAIntoCrouch,
                FSmashCount = FSmashCount,
                HasRapidJab = HasRapidJab,
                JabCount = JabCount,
                JabFlag = JabFlag,
                HasCostume00 = HasCostume00,
                HasCostume01 = HasCostume01,
                HasCostume02 = HasCostume02,
                HasCostume03 = HasCostume03,
                HasCostume04 = HasCostume04,
                HasCostume05 = HasCostume05,
                HasCostume06 = HasCostume06,
                HasCostume07 = HasCostume07,
                HasCostume08 = HasCostume08,
                HasCostume09 = HasCostume09,
                HasCostume10 = HasCostume10,
                HasCostume11 = HasCostume11,
                UnknownFlagA = UnknownFlagA,
                UnknownFlagB = UnknownFlagB,
                UnknownFlagC = UnknownFlagC,
                UnknownFlagD = UnknownFlagD,
                HasPac = HasPac,
                HasModule = HasModule,
                MotionEtcType = MotionEtcType,
                UnknownLoadFlagA = UnknownLoadFlagA,
                UnknownLoadFlagB = UnknownLoadFlagB,
                EntryLoadType = EntryLoadType,
                ResultLoadType = ResultLoadType,
                FinalLoadType = FinalLoadType,
                FinalSmashMusic = FinalSmashMusic,
                AIController = AIController,
                EntryFlag = EntryFlag,
                IkPhysics = IkPhysics,
                TextureLoader = TextureLoader,
                ThrownType = ThrownType,
                GrabSize = GrabSize,
                WorkManage = WorkManage,
                Version = Version
            };
            return node;
        }
    }

    public class SlotAttributes
    {
        [Category("Characters")]
        [Browsable(false)]
        public bool SetSlot { get; set; } = false;

        [Category("Characters")]
        [Browsable(false)]
        public uint CharSlot1 { get; set; } = 0xFF;

        [Category("Characters")]
        [Browsable(false)]
        public uint CharSlot2 { get; set; } = 0xFF;

        [Category("Characters")]
        [Browsable(false)]
        public uint CharSlot3 { get; set; } = 0xFF;

        [Category("Characters")]
        [Browsable(false)]
        public uint CharSlot4 { get; set; } = 0xFF;

        [Category("Characters")] 
        public byte Records { get; set; } = 0;

        [Category("Victory")] 
        public uint AnnouncerID { get; set; } = 0x0000FF00;

        [Category("Victory")] 
        public float CameraDistance1 { get; set; } = 0;

        [Category("Victory")] 
        public float CameraDistance2 { get; set; } = 0;

        [Category("Victory")] 
        public float CameraDistance3 { get; set; } = 0;

        [Category("Victory")] 
        public float CameraDistance4 { get; set; } = 0;

        [Browsable(false)]
        public uint Size { get; set; } = 0x40;

        [Browsable(false)]
        public uint Version { get; set; } = 2;

        [Browsable(false)]
        public byte Unknown0x25 { get; set; } = 0xCC;

        [Browsable(false)]
        public byte Unknown0x26 { get; set; } = 0xCC;

        [Browsable(false)]
        public byte Unknown0x27 { get; set; } = 0xCC;

        [Browsable(false)]
        public uint Unknown0x2C { get; set; } = 0xCCCCCCCC;

        public SlotAttributes Copy()
        {
            var newAttributes = JsonConvert.DeserializeObject<SlotAttributes>(JsonConvert.SerializeObject(this));
            return newAttributes;
        }

        public SLTCNode ToSLTCNode(FighterInfo fighterInfo, bool updateLinks = false)
        {
            var node = ToSLTCNode();
            node.VictoryTheme = (uint)fighterInfo.VictoryThemeId;
            if (updateLinks)
            {
                if (fighterInfo.Ids.FighterConfigId != fighterInfo.Ids.SlotConfigId)
                {
                    node.CharSlot1 = (uint)fighterInfo.Ids.FighterConfigId;
                    node.SetSlot = true;
                }
                else
                {
                    node.SetSlot = false;
                }
            }
            return node;
        }

        private SLTCNode ToSLTCNode()
        {
            var node = new SLTCNode
            {
                _tag = 0x43544C53,
                _size = Size,
                _version = Version,
                SetSlot = SetSlot,
                CharSlot1 = CharSlot1,
                CharSlot2 = CharSlot2,
                CharSlot3 = CharSlot3,
                CharSlot4 = CharSlot4,
                Records = Records,
                AnnouncerID = AnnouncerID,
                CameraDistance1 = CameraDistance1,
                CameraDistance2 = CameraDistance2,
                CameraDistance3 = CameraDistance3,
                CameraDistance4 = CameraDistance4,
                _unknown0x25 = Unknown0x25,
                _unknown0x26 = Unknown0x26,
                _unknown0x27 = Unknown0x27,
                _unknown0x2C = Unknown0x2C
            };
            return node;
        }
    }

    public class CosmeticAttributes
    {
        [Category("Character")]
        [Browsable(false)]
        public bool HasSecondary { get; set; } = false;

        [Category("Character")]
        [Browsable(false)]
        public byte CharSlot1 { get; set; } = 0xFF;

        [Category("Character")]
        [Browsable(false)]
        public byte CharSlot2 { get; set; } = 0xFF;

        [Category("Cosmetics")] 
        public uint AnnouncerID { get; set; } = 0x0000FF00;

        [Browsable(false)]
        public uint Size { get; set; } = 0x40;

        [Browsable(false)]
        public uint Version { get; set; } = 2;

        [Browsable(false)]
        public byte Unknown0x11 { get; set; } = 1;

        [Browsable(false)]
        public byte Unknown0x15 { get; set; } = 0;

        [Browsable(false)]
        public byte Unknown0x16 { get; set; } = 0;

        [Browsable(false)]
        public byte Unknown0x17 { get; set; } = 0;

        [Browsable(false)]
        public uint Unknown0x1C { get; set; } = 0x2633;

        public CosmeticAttributes Copy()
        {
            var newAttributes = JsonConvert.DeserializeObject<CosmeticAttributes>(JsonConvert.SerializeObject(this));
            return newAttributes;
        }

        public COSCNode ToCOSCNode(FighterInfo fighterInfo, bool updateLinks = false)
        {
            var node = ToCOSCNode();
            node.CosmeticID = (byte)fighterInfo.Ids.CosmeticId;
            node.CharacterName = fighterInfo.DisplayName;
            node.FranchiseIconID = (byte)(fighterInfo.Ids.FranchiseId - 1);
            if (updateLinks)
            {
                if (fighterInfo.Ids.SlotConfigId != fighterInfo.Ids.CosmeticConfigId)
                {
                    node.CharSlot1 = (byte)fighterInfo.Ids.SlotConfigId;
                    node.HasSecondary = true;
                }
                else
                {
                    node.HasSecondary = false;
                }
            }
            return node;
        }

        private COSCNode ToCOSCNode()
        {
            var node = new COSCNode
            {
                _tag = 0x43534F43,
                _size = Size,
                _version = Version,
                HasSecondary = HasSecondary,
                CharSlot1 = CharSlot1,
                CharSlot2 = CharSlot2,
                AnnouncerID = AnnouncerID,
                _unknown0x11 = Unknown0x11,
                _unknown0x15 = Unknown0x15,
                _unknown0x16 = Unknown0x16,
                _unknown0x17 = Unknown0x17,
                _unknown0x1C = Unknown0x1C,
            };
            return node;
        }
    }

    public class CSSSlotAttributes
    {
        [Category("Character")]
        [Browsable(false)]
        public bool SetPrimarySecondary { get; set; } = false;

        [Category("Character")]
        [Browsable(false)]
        public byte CharSlot1 { get; set; } = 0xFF;

        [Category("Character")]
        [Browsable(false)]
        public byte CharSlot2 { get; set; } = 0xFF;

        [Category("Character")] 
        public byte Records { get; set; } = 0;

        [Category("Cosmetics")]
        [Browsable(false)]
        public bool SetCosmeticSlot { get; set; } = false;

        [Category("Cosmetics")]
        [Browsable(false)]
        public byte CosmeticSlot { get; set; } = 0xFF;

        [Category("Cosmetics")] 
        public uint WiimoteSFX { get; set; } = 0x000019BD;

        [Category("Misc")] 
        public uint Status { get; set; } = 0;

        [Browsable(false)]
        public uint Unknown0x18 { get; set; } = 0xCCCCCCCC; // Appears to be padding

        [Browsable(false)]
        public uint Version { get; set; } = 2;

        [Browsable(false)]
        public uint Size { get; set; } = 0x40;

        public CSSSlotAttributes Copy()
        {
            var newAttributes = JsonConvert.DeserializeObject<CSSSlotAttributes>(JsonConvert.SerializeObject(this));
            return newAttributes;
        }

        public CSSCNode ToCSSCNode(FighterInfo fighterInfo, bool updateLinks = false)
        {
            var node = ToCSSCNode();
            if (updateLinks)
            {
                if (fighterInfo.Ids.SlotConfigId != fighterInfo.Ids.CSSSlotConfigId)
                {
                    node.CharSlot1 = (byte)fighterInfo.Ids.SlotConfigId;
                    node.SetPrimarySecondary = true;
                }
                else
                {
                    node.SetPrimarySecondary = false;
                }
                if (fighterInfo.Ids.CosmeticConfigId != fighterInfo.Ids.CSSSlotConfigId)
                {
                    node.CosmeticSlot = (byte)fighterInfo.Ids.CosmeticId;
                    node.SetCosmeticSlot = true;
                }
                else
                {
                    node.SetCosmeticSlot = false;
                }
            }
            return node;
        }

        private CSSCNode ToCSSCNode()
        {
            var node = new CSSCNode
            {
                _tag = 0x43535343,
                _unknown0x18 = Unknown0x18,
                _version = Version,
                _size = Size,
                SetPrimarySecondary = SetPrimarySecondary,
                CharSlot1 = CharSlot1,
                CharSlot2 = CharSlot2,
                SetCosmeticSlot = SetCosmeticSlot,
                CosmeticSlot = CosmeticSlot,
                Records = Records,
                WiimoteSFX = WiimoteSFX,
                Status = Status
            };
            return node;
        }
    }
}
