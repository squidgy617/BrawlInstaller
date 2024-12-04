using BrawlLib.Internal;
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
        // TODO: Should credits theme ID go in here too?
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
        public FighterAttributes FighterAttributes { get; set; } = new FighterAttributes();
        public SlotAttributes SlotAttributes { get; set; } = new SlotAttributes();
        public CosmeticAttributes CosmeticAttributes { get; set; } = new CosmeticAttributes();
        public CSSSlotAttributes CSSSlotAttributes { get; set; } = new CSSSlotAttributes();

        public FighterInfo Copy()
        {
            var newFighterInfo = new FighterInfo
            {
                EntryName = EntryName,
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
                    FranchiseId = Ids.FranchiseId
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
                FighterAttributes = FighterAttributes.Copy(),
                SlotAttributes = SlotAttributes.Copy(),
                CosmeticAttributes = CosmeticAttributes.Copy(),
                CSSSlotAttributes = CSSSlotAttributes.Copy()
            };
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
        [Category("Characteristics")] public AirJumpFlags AirJumpCount { get; set; } = AirJumpFlags.NormalAirJump;
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
        [Category("Resources")] public MotionEtcTypes MotionEtcType { get; set; } = MotionEtcTypes.SingleMerged;
        [Category("Resources")] public bool UnknownLoadFlagA { get; set; } = false;
        [Category("Resources")] public bool UnknownLoadFlagB { get; set; } = false;
        [Category("Resources")] public EntryResultLoadFlags EntryLoadType { get; set; } = EntryResultLoadFlags.Single;
        [Category("Resources")] public EntryResultLoadFlags ResultLoadType { get; set; } = EntryResultLoadFlags.Single;
        [Category("Resources")] public FinalLoadFlags FinalLoadType { get; set; } = FinalLoadFlags.Single;
        [Category("Sound")] public bool FinalSmashMusic { get; set; } = false;
        [Category("Misc")] public uint AIController { get; set; } = 0;
        [Category("Misc")] public uint EntryFlag { get; set; } = 4;
        [Category("Misc")] public bool IkPhysics { get; set; } = false;
        [Category("Misc")] public uint TextureLoader { get; set; } = 0;
        [Category("Misc")] public ThrownTypes ThrownType { get; set; } = ThrownTypes.Default;
        [Category("Misc")] public GrabSizes GrabSize { get; set; } = GrabSizes.Short;
        [Category("Misc")] public bool WorkManage { get; set; } = false;

        public FighterAttributes Copy()
        {
            var newAttributes = JsonConvert.DeserializeObject<FighterAttributes>(JsonConvert.SerializeObject(this));
            return newAttributes;
        }
    }

    public class SlotAttributes
    {
        [Category("Characters")] public byte Records { get; set; } = 0;
        [Category("Victory")] public uint AnnouncerID { get; set; } = 0x0000FF00;
        [Category("Victory")] public float CameraDistance1 { get; set; } = 0;
        [Category("Victory")] public float CameraDistance2 { get; set; } = 0;
        [Category("Victory")] public float CameraDistance3 { get; set; } = 0;
        [Category("Victory")] public float CameraDistance4 { get; set; } = 0;

        public SlotAttributes Copy()
        {
            var newAttributes = JsonConvert.DeserializeObject<SlotAttributes>(JsonConvert.SerializeObject(this));
            return newAttributes;
        }
    }

    public class CosmeticAttributes
    {
        [Category("Cosmetics")] public uint AnnouncerID { get; set; } = 0x0000FF00;

        public CosmeticAttributes Copy()
        {
            var newAttributes = JsonConvert.DeserializeObject<CosmeticAttributes>(JsonConvert.SerializeObject(this));
            return newAttributes;
        }
    }

    public class CSSSlotAttributes
    {
        [Category("Character")] public byte Records { get; set; } = 0;
        [Category("Cosmetics")] public uint WiimoteSFX { get; set; } = 0x000019BD;
        [Category("Misc")] public uint Status { get; set; } = 0;

        public CSSSlotAttributes Copy()
        {
            var newAttributes = JsonConvert.DeserializeObject<CSSSlotAttributes>(JsonConvert.SerializeObject(this));
            return newAttributes;
        }
    }
}
