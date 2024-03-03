using BrawlLib.Internal;
using BrawlLib.SSBB.Types.Subspace.Animation;
using System;
using System.ComponentModel;

namespace BrawlLib.SSBB.ResourceNodes
{
    public class GMOVNode : BLOCEntryNode
    {
        public override Type SubEntryType => typeof(GMOVEntryNode);
        public override ResourceType ResourceFileType => ResourceType.GMOV;
        protected override string baseName => "Moving Platforms";

        internal static ResourceNode TryParse(DataSource source, ResourceNode parent)
        {
            return source.Tag == "GMOV" ? new GMOVNode() : null;
        }
    }

    public unsafe class GMOVEntryNode : ResourceNode
    {
        protected internal GMOVEntry Data;

        public MotionPathDataClass _motionPath;
        [TypeConverter(typeof(ExpandableObjectCustomConverter))]
        [Category("Platform")]
        public MotionPathDataClass MotionPath
        {
            get => _motionPath ?? new MotionPathDataClass(this);
            set
            {
                _motionPath = value;
                Data._motionPath = value;
                SignalPropertyChange();
            }
        }

        public DifficultyRatiosClass _difficultyRatio;
        [TypeConverter(typeof(ExpandableObjectCustomConverter))]
        [Category("Platform")]
        public DifficultyRatiosClass DifficultyRatio
        {
            get => _difficultyRatio ?? new DifficultyRatiosClass(this);
            set
            {
                _difficultyRatio = value;
                Data._difficultyRatio = value;
                SignalPropertyChange();
            }
        }

        [Category("Platform")]
        public byte ModelIndex
        {
            get => Data._modelIndex;
            set
            {
                Data._modelIndex = value;
                SignalPropertyChange();
            }
        }

        [Category("Platform")]
        public byte CollisionIndex
        {
            get => Data._collisionIndex;
            set
            {
                Data._collisionIndex = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x046 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x046
        {
            get => Data._unknown0x046;
            set
            {
                Data._unknown0x046 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x047 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x047
        {
            get => Data._unknown0x047;
            set
            {
                Data._unknown0x047 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x048 (int)")]
        [Category("Unknown")]
        public int Unknown0x048
        {
            get => Data._unknown0x048;
            set
            {
                Data._unknown0x048 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x04C (byte)")]
        [Category("Unknown")]
        public byte Unknown0x04C
        {
            get => Data._unknown0x04C;
            set
            {
                Data._unknown0x04C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x04D (bool)")]
        [Category("Unknown")]
        public bool Unknown0x04D
        {
            get => Data._unknown0x04D;
            set
            {
                Data._unknown0x04D = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x04E (byte)")]
        [Category("Unknown")]
        public byte Unknown0x04E
        {
            get => Data._unknown0x04E;
            set
            {
                Data._unknown0x04E = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x04F (byte)")]
        [Category("Unknown")]
        public byte Unknown0x04F
        {
            get => Data._unknown0x04F;
            set
            {
                Data._unknown0x04F = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x050 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x050
        {
            get => Data._unknown0x050;
            set
            {
                Data._unknown0x050 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x054 (int)")]
        [Category("Unknown")]
        public int Unknown0x054
        {
            get => Data._unknown0x054;
            set
            {
                Data._unknown0x054 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x058 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x058
        {
            get => Data._unknown0x058;
            set
            {
                Data._unknown0x058 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x059 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x059
        {
            get => Data._unknown0x059;
            set
            {
                Data._unknown0x059 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x05A (byte)")]
        [Category("Unknown")]
        public byte Unknown0x05A
        {
            get => Data._unknown0x05A;
            set
            {
                Data._unknown0x05A = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x05B (byte)")]
        [Category("Unknown")]
        public byte Unknown0x05B
        {
            get => Data._unknown0x05B;
            set
            {
                Data._unknown0x05B = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x05C (uint)")]
        [Category("Unknown")]
        public uint Unknown0x05C
        {
            get => Data._unknown0x05C;
            set
            {
                Data._unknown0x05C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x060 (int)")]
        [Category("Unknown")]
        public int Unknown0x060
        {
            get => Data._unknown0x060;
            set
            {
                Data._unknown0x060 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x064 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x064
        {
            get => Data._unknown0x064;
            set
            {
                Data._unknown0x064 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x065 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x065
        {
            get => Data._unknown0x065;
            set
            {
                Data._unknown0x065 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x066 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x066
        {
            get => Data._unknown0x066;
            set
            {
                Data._unknown0x066 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x067 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x067
        {
            get => Data._unknown0x067;
            set
            {
                Data._unknown0x067 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x068 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x068
        {
            get => Data._unknown0x068;
            set
            {
                Data._unknown0x068 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x06C (uint)")]
        [Category("Unknown")]
        public uint Unknown0x06C
        {
            get => Data._unknown0x06C;
            set
            {
                Data._unknown0x06C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x070 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x070
        {
            get => Data._unknown0x070;
            set
            {
                Data._unknown0x070 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x071 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x071
        {
            get => Data._unknown0x071;
            set
            {
                Data._unknown0x071 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x072 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x072
        {
            get => Data._unknown0x072;
            set
            {
                Data._unknown0x072 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x073 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x073
        {
            get => Data._unknown0x073;
            set
            {
                Data._unknown0x073 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x074 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x074
        {
            get => Data._unknown0x074;
            set
            {
                Data._unknown0x074 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x078 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x078
        {
            get => Data._unknown0x078;
            set
            {
                Data._unknown0x078 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x07C (byte)")]
        [Category("Unknown")]
        public byte Unknown0x07C
        {
            get => Data._unknown0x07C;
            set
            {
                Data._unknown0x07C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x07D (bool)")]
        [Category("Unknown")]
        public bool Unknown0x07D
        {
            get => Data._unknown0x07D;
            set
            {
                Data._unknown0x07D = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x07E (byte)")]
        [Category("Unknown")]
        public byte Unknown0x07E
        {
            get => Data._unknown0x07E;
            set
            {
                Data._unknown0x07E = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x07F (byte)")]
        [Category("Unknown")]
        public byte Unknown0x07F
        {
            get => Data._unknown0x07F;
            set
            {
                Data._unknown0x07F = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x080 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x080
        {
            get => Data._unknown0x080;
            set
            {
                Data._unknown0x080 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x084 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x084
        {
            get => Data._unknown0x084;
            set
            {
                Data._unknown0x084 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x088 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x088
        {
            get => Data._unknown0x088;
            set
            {
                Data._unknown0x088 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x089 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x089
        {
            get => Data._unknown0x089;
            set
            {
                Data._unknown0x089 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x08A (byte)")]
        [Category("Unknown")]
        public byte Unknown0x08A
        {
            get => Data._unknown0x08A;
            set
            {
                Data._unknown0x08A = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x08B (byte)")]
        [Category("Unknown")]
        public byte Unknown0x08B
        {
            get => Data._unknown0x08B;
            set
            {
                Data._unknown0x08B = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x08C (uint)")]
        [Category("Unknown")]
        public uint Unknown0x08C
        {
            get => Data._unknown0x08C;
            set
            {
                Data._unknown0x08C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x090 (int)")]
        [Category("Unknown")]
        public int Unknown0x090
        {
            get => Data._unknown0x090;
            set
            {
                Data._unknown0x090 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x094 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x094
        {
            get => Data._unknown0x094;
            set
            {
                Data._unknown0x094 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x095 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x095
        {
            get => Data._unknown0x095;
            set
            {
                Data._unknown0x095 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x096 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x096
        {
            get => Data._unknown0x096;
            set
            {
                Data._unknown0x096 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x097 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x097
        {
            get => Data._unknown0x097;
            set
            {
                Data._unknown0x097 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x098 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x098
        {
            get => Data._unknown0x098;
            set
            {
                Data._unknown0x098 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x09C (int)")]
        [Category("Unknown")]
        public int Unknown0x09C
        {
            get => Data._unknown0x09C;
            set
            {
                Data._unknown0x09C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0A0 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0A0
        {
            get => Data._unknown0x0A0;
            set
            {
                Data._unknown0x0A0 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0A1 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x0A1
        {
            get => Data._unknown0x0A1;
            set
            {
                Data._unknown0x0A1 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0A2 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0A2
        {
            get => Data._unknown0x0A2;
            set
            {
                Data._unknown0x0A2 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0A3 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0A3
        {
            get => Data._unknown0x0A3;
            set
            {
                Data._unknown0x0A3 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0A4 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x0A4
        {
            get => Data._unknown0x0A4;
            set
            {
                Data._unknown0x0A4 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0A8 (int)")]
        [Category("Unknown")]
        public int Unknown0x0A8
        {
            get => Data._unknown0x0A8;
            set
            {
                Data._unknown0x0A8 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0AC (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0AC
        {
            get => Data._unknown0x0AC;
            set
            {
                Data._unknown0x0AC = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0AD (bool)")]
        [Category("Unknown")]
        public bool Unknown0x0AD
        {
            get => Data._unknown0x0AD;
            set
            {
                Data._unknown0x0AD = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0AE (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0AE
        {
            get => Data._unknown0x0AE;
            set
            {
                Data._unknown0x0AE = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0AF (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0AF
        {
            get => Data._unknown0x0AF;
            set
            {
                Data._unknown0x0AF = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0B0 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x0B0
        {
            get => Data._unknown0x0B0;
            set
            {
                Data._unknown0x0B0 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0B4 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x0B4
        {
            get => Data._unknown0x0B4;
            set
            {
                Data._unknown0x0B4 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0B8 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0B8
        {
            get => Data._unknown0x0B8;
            set
            {
                Data._unknown0x0B8 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0B9 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x0B9
        {
            get => Data._unknown0x0B9;
            set
            {
                Data._unknown0x0B9 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0BA (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0BA
        {
            get => Data._unknown0x0BA;
            set
            {
                Data._unknown0x0BA = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0BB (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0BB
        {
            get => Data._unknown0x0BB;
            set
            {
                Data._unknown0x0BB = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0BC (uint)")]
        [Category("Unknown")]
        public uint Unknown0x0BC
        {
            get => Data._unknown0x0BC;
            set
            {
                Data._unknown0x0BC = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0C0 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x0C0
        {
            get => Data._unknown0x0C0;
            set
            {
                Data._unknown0x0C0 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0C4 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0C4
        {
            get => Data._unknown0x0C4;
            set
            {
                Data._unknown0x0C4 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0C5 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x0C5
        {
            get => Data._unknown0x0C5;
            set
            {
                Data._unknown0x0C5 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0C6 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0C6
        {
            get => Data._unknown0x0C6;
            set
            {
                Data._unknown0x0C6 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0C7 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0C7
        {
            get => Data._unknown0x0C7;
            set
            {
                Data._unknown0x0C7 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0C8 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x0C8
        {
            get => Data._unknown0x0C8;
            set
            {
                Data._unknown0x0C8 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0CC (uint)")]
        [Category("Unknown")]
        public uint Unknown0x0CC
        {
            get => Data._unknown0x0CC;
            set
            {
                Data._unknown0x0CC = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0D0 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0D0
        {
            get => Data._unknown0x0D0;
            set
            {
                Data._unknown0x0D0 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0D1 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x0D1
        {
            get => Data._unknown0x0D1;
            set
            {
                Data._unknown0x0D1 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0D2 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0D2
        {
            get => Data._unknown0x0D2;
            set
            {
                Data._unknown0x0D2 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0D3 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0D3
        {
            get => Data._unknown0x0D3;
            set
            {
                Data._unknown0x0D3 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0D4 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x0D4
        {
            get => Data._unknown0x0D4;
            set
            {
                Data._unknown0x0D4 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0D8 (int)")]
        [Category("Unknown")]
        public int Unknown0x0D8
        {
            get => Data._unknown0x0D8;
            set
            {
                Data._unknown0x0D8 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0DC (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0DC
        {
            get => Data._unknown0x0DC;
            set
            {
                Data._unknown0x0DC = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0DD (bool)")]
        [Category("Unknown")]
        public bool Unknown0x0DD
        {
            get => Data._unknown0x0DD;
            set
            {
                Data._unknown0x0DD = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0DE (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0DE
        {
            get => Data._unknown0x0DE;
            set
            {
                Data._unknown0x0DE = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0DF (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0DF
        {
            get => Data._unknown0x0DF;
            set
            {
                Data._unknown0x0DF = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0E0 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x0E0
        {
            get => Data._unknown0x0E0;
            set
            {
                Data._unknown0x0E0 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0E4 (int)")]
        [Category("Unknown")]
        public int Unknown0x0E4
        {
            get => Data._unknown0x0E4;
            set
            {
                Data._unknown0x0E4 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0E8 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0E8
        {
            get => Data._unknown0x0E8;
            set
            {
                Data._unknown0x0E8 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0E9 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x0E9
        {
            get => Data._unknown0x0E9;
            set
            {
                Data._unknown0x0E9 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0EA (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0EA
        {
            get => Data._unknown0x0EA;
            set
            {
                Data._unknown0x0EA = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0EB (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0EB
        {
            get => Data._unknown0x0EB;
            set
            {
                Data._unknown0x0EB = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0EC (uint)")]
        [Category("Unknown")]
        public uint Unknown0x0EC
        {
            get => Data._unknown0x0EC;
            set
            {
                Data._unknown0x0EC = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0F0 (int)")]
        [Category("Unknown")]
        public int Unknown0x0F0
        {
            get => Data._unknown0x0F0;
            set
            {
                Data._unknown0x0F0 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0F4 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0F4
        {
            get => Data._unknown0x0F4;
            set
            {
                Data._unknown0x0F4 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0F5 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x0F5
        {
            get => Data._unknown0x0F5;
            set
            {
                Data._unknown0x0F5 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0F6 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0F6
        {
            get => Data._unknown0x0F6;
            set
            {
                Data._unknown0x0F6 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0F7 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x0F7
        {
            get => Data._unknown0x0F7;
            set
            {
                Data._unknown0x0F7 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0F8 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x0F8
        {
            get => Data._unknown0x0F8;
            set
            {
                Data._unknown0x0F8 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0FC (uint)")]
        [Category("Unknown")]
        public uint Unknown0x0FC
        {
            get => Data._unknown0x0FC;
            set
            {
                Data._unknown0x0FC = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x100 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x100
        {
            get => Data._unknown0x100;
            set
            {
                Data._unknown0x100 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x101 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x101
        {
            get => Data._unknown0x101;
            set
            {
                Data._unknown0x101 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x102 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x102
        {
            get => Data._unknown0x102;
            set
            {
                Data._unknown0x102 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x103 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x103
        {
            get => Data._unknown0x103;
            set
            {
                Data._unknown0x103 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x104 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x104
        {
            get => Data._unknown0x104;
            set
            {
                Data._unknown0x104 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x108 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x108
        {
            get => Data._unknown0x108;
            set
            {
                Data._unknown0x108 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x10C (byte)")]
        [Category("Unknown")]
        public byte Unknown0x10C
        {
            get => Data._unknown0x10C;
            set
            {
                Data._unknown0x10C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x10D (bool)")]
        [Category("Unknown")]
        public bool Unknown0x10D
        {
            get => Data._unknown0x10D;
            set
            {
                Data._unknown0x10D = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x10E (byte)")]
        [Category("Unknown")]
        public byte Unknown0x10E
        {
            get => Data._unknown0x10E;
            set
            {
                Data._unknown0x10E = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x10F (byte)")]
        [Category("Unknown")]
        public byte Unknown0x10F
        {
            get => Data._unknown0x10F;
            set
            {
                Data._unknown0x10F = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x110 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x110
        {
            get => Data._unknown0x110;
            set
            {
                Data._unknown0x110 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x114 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x114
        {
            get => Data._unknown0x114;
            set
            {
                Data._unknown0x114 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x118 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x118
        {
            get => Data._unknown0x118;
            set
            {
                Data._unknown0x118 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x119 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x119
        {
            get => Data._unknown0x119;
            set
            {
                Data._unknown0x119 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x11A (byte)")]
        [Category("Unknown")]
        public byte Unknown0x11A
        {
            get => Data._unknown0x11A;
            set
            {
                Data._unknown0x11A = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x11B (byte)")]
        [Category("Unknown")]
        public byte Unknown0x11B
        {
            get => Data._unknown0x11B;
            set
            {
                Data._unknown0x11B = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x11C (uint)")]
        [Category("Unknown")]
        public uint Unknown0x11C
        {
            get => Data._unknown0x11C;
            set
            {
                Data._unknown0x11C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x120 (int)")]
        [Category("Unknown")]
        public int Unknown0x120
        {
            get => Data._unknown0x120;
            set
            {
                Data._unknown0x120 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x124 (short)")]
        [Category("Unknown")]
        public short Unknown0x124
        {
            get => Data._unknown0x124;
            set
            {
                Data._unknown0x124 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x126 (short)")]
        [Category("Unknown")]
        public short Unknown0x126
        {
            get => Data._unknown0x126;
            set
            {
                Data._unknown0x126 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x128 (short)")]
        [Category("Unknown")]
        public short Unknown0x128
        {
            get => Data._unknown0x128;
            set
            {
                Data._unknown0x128 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x12A (short)")]
        [Category("Unknown")]
        public short Unknown0x12A
        {
            get => Data._unknown0x12A;
            set
            {
                Data._unknown0x12A = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x12C (int)")]
        [Category("Unknown")]
        public int Unknown0x12C
        {
            get => Data._unknown0x12C;
            set
            {
                Data._unknown0x12C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x130 (short)")]
        [Category("Unknown")]
        public short Unknown0x130
        {
            get => Data._unknown0x130;
            set
            {
                Data._unknown0x130 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x132 (short)")]
        [Category("Unknown")]
        public short Unknown0x132
        {
            get => Data._unknown0x132;
            set
            {
                Data._unknown0x132 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x134 (short)")]
        [Category("Unknown")]
        public short Unknown0x134
        {
            get => Data._unknown0x134;
            set
            {
                Data._unknown0x134 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x136 (short)")]
        [Category("Unknown")]
        public short Unknown0x136
        {
            get => Data._unknown0x136;
            set
            {
                Data._unknown0x136 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x138 (int)")]
        [Category("Unknown")]
        public int Unknown0x138
        {
            get => Data._unknown0x138;
            set
            {
                Data._unknown0x138 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x13C (short)")]
        [Category("Unknown")]
        public short Unknown0x13C
        {
            get => Data._unknown0x13C;
            set
            {
                Data._unknown0x13C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x13E (short)")]
        [Category("Unknown")]
        public short Unknown0x13E
        {
            get => Data._unknown0x13E;
            set
            {
                Data._unknown0x13E = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x140 (short)")]
        [Category("Unknown")]
        public short Unknown0x140
        {
            get => Data._unknown0x140;
            set
            {
                Data._unknown0x140 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x142 (short)")]
        [Category("Unknown")]
        public short Unknown0x142
        {
            get => Data._unknown0x142;
            set
            {
                Data._unknown0x142 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x144 (int)")]
        [Category("Unknown")]
        public int Unknown0x144
        {
            get => Data._unknown0x144;
            set
            {
                Data._unknown0x144 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x148 (short)")]
        [Category("Unknown")]
        public short Unknown0x148
        {
            get => Data._unknown0x148;
            set
            {
                Data._unknown0x148 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x14A (short)")]
        [Category("Unknown")]
        public short Unknown0x14A
        {
            get => Data._unknown0x14A;
            set
            {
                Data._unknown0x14A = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x14C (uint)")]
        [Category("Unknown")]
        public uint Unknown0x14C
        {
            get => Data._unknown0x14C;
            set
            {
                Data._unknown0x14C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x150 (int)")]
        [Category("Unknown")]
        public int Unknown0x150
        {
            get => Data._unknown0x150;
            set
            {
                Data._unknown0x150 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x154 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x154
        {
            get => Data._unknown0x154;
            set
            {
                Data._unknown0x154 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x158 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x158
        {
            get => Data._unknown0x158;
            set
            {
                Data._unknown0x158 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x15C (bool)")]
        [Category("Unknown")]
        public bool Unknown0x15C
        {
            get => Data._unknown0x15C;
            set
            {
                Data._unknown0x15C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x15D (byte)")]
        [Category("Unknown")]
        public byte Unknown0x15D
        {
            get => Data._unknown0x15D;
            set
            {
                Data._unknown0x15D = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x15E (byte)")]
        [Category("Unknown")]
        public byte Unknown0x15E
        {
            get => Data._unknown0x15E;
            set
            {
                Data._unknown0x15E = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x15F (byte)")]
        [Category("Unknown")]
        public byte Unknown0x15F
        {
            get => Data._unknown0x15F;
            set
            {
                Data._unknown0x15F = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x160 (short)")]
        [Category("Unknown")]
        public short Unknown0x160
        {
            get => Data._unknown0x160;
            set
            {
                Data._unknown0x160 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x162 (short)")]
        [Category("Unknown")]
        public short Unknown0x162
        {
            get => Data._unknown0x162;
            set
            {
                Data._unknown0x162 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x164 (short)")]
        [Category("Unknown")]
        public short Unknown0x164
        {
            get => Data._unknown0x164;
            set
            {
                Data._unknown0x164 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x166 (short)")]
        [Category("Unknown")]
        public short Unknown0x166
        {
            get => Data._unknown0x166;
            set
            {
                Data._unknown0x166 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x168 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x168
        {
            get => Data._unknown0x168;
            set
            {
                Data._unknown0x168 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x169 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x169
        {
            get => Data._unknown0x169;
            set
            {
                Data._unknown0x169 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x16A (byte)")]
        [Category("Unknown")]
        public byte Unknown0x16A
        {
            get => Data._unknown0x16A;
            set
            {
                Data._unknown0x16A = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x16B (byte)")]
        [Category("Unknown")]
        public byte Unknown0x16B
        {
            get => Data._unknown0x16B;
            set
            {
                Data._unknown0x16B = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x16C (short)")]
        [Category("Unknown")]
        public short Unknown0x16C
        {
            get => Data._unknown0x16C;
            set
            {
                Data._unknown0x16C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x16E (short)")]
        [Category("Unknown")]
        public short Unknown0x16E
        {
            get => Data._unknown0x16E;
            set
            {
                Data._unknown0x16E = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x170 (short)")]
        [Category("Unknown")]
        public short Unknown0x170
        {
            get => Data._unknown0x170;
            set
            {
                Data._unknown0x170 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x172 (short)")]
        [Category("Unknown")]
        public short Unknown0x172
        {
            get => Data._unknown0x172;
            set
            {
                Data._unknown0x172 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x174 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x174
        {
            get => Data._unknown0x174;
            set
            {
                Data._unknown0x174 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x178 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x178
        {
            get => Data._unknown0x178;
            set
            {
                Data._unknown0x178 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x179 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x179
        {
            get => Data._unknown0x179;
            set
            {
                Data._unknown0x179 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x17A (byte)")]
        [Category("Unknown")]
        public byte Unknown0x17A
        {
            get => Data._unknown0x17A;
            set
            {
                Data._unknown0x17A = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x17B (byte)")]
        [Category("Unknown")]
        public byte Unknown0x17B
        {
            get => Data._unknown0x17B;
            set
            {
                Data._unknown0x17B = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x17C (uint)")]
        [Category("Unknown")]
        public uint Unknown0x17C
        {
            get => Data._unknown0x17C;
            set
            {
                Data._unknown0x17C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x180 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x180
        {
            get => Data._unknown0x180;
            set
            {
                Data._unknown0x180 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x184 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x184
        {
            get => Data._unknown0x184;
            set
            {
                Data._unknown0x184 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x188 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x188
        {
            get => Data._unknown0x188;
            set
            {
                Data._unknown0x188 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x18C (uint)")]
        [Category("Unknown")]
        public uint Unknown0x18C
        {
            get => Data._unknown0x18C;
            set
            {
                Data._unknown0x18C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x190 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x190
        {
            get => Data._unknown0x190;
            set
            {
                Data._unknown0x190 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x194 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x194
        {
            get => Data._unknown0x194;
            set
            {
                Data._unknown0x194 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x198 (int)")]
        [Category("Unknown")]
        public int Unknown0x198
        {
            get => Data._unknown0x198;
            set
            {
                Data._unknown0x198 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x19C (byte)")]
        [Category("Unknown")]
        public byte Unknown0x19C
        {
            get => Data._unknown0x19C;
            set
            {
                Data._unknown0x19C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x19D (bool)")]
        [Category("Unknown")]
        public bool Unknown0x19D
        {
            get => Data._unknown0x19D;
            set
            {
                Data._unknown0x19D = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x19E (byte)")]
        [Category("Unknown")]
        public byte Unknown0x19E
        {
            get => Data._unknown0x19E;
            set
            {
                Data._unknown0x19E = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x19F (byte)")]
        [Category("Unknown")]
        public byte Unknown0x19F
        {
            get => Data._unknown0x19F;
            set
            {
                Data._unknown0x19F = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1A0 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x1A0
        {
            get => Data._unknown0x1A0;
            set
            {
                Data._unknown0x1A0 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1A4 (int)")]
        [Category("Unknown")]
        public int Unknown0x1A4
        {
            get => Data._unknown0x1A4;
            set
            {
                Data._unknown0x1A4 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1A8 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1A8
        {
            get => Data._unknown0x1A8;
            set
            {
                Data._unknown0x1A8 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1A9 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x1A9
        {
            get => Data._unknown0x1A9;
            set
            {
                Data._unknown0x1A9 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1AA (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1AA
        {
            get => Data._unknown0x1AA;
            set
            {
                Data._unknown0x1AA = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1AB (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1AB
        {
            get => Data._unknown0x1AB;
            set
            {
                Data._unknown0x1AB = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1AC (uint)")]
        [Category("Unknown")]
        public uint Unknown0x1AC
        {
            get => Data._unknown0x1AC;
            set
            {
                Data._unknown0x1AC = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1B0 (int)")]
        [Category("Unknown")]
        public int Unknown0x1B0
        {
            get => Data._unknown0x1B0;
            set
            {
                Data._unknown0x1B0 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1B4 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1B4
        {
            get => Data._unknown0x1B4;
            set
            {
                Data._unknown0x1B4 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1B5 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x1B5
        {
            get => Data._unknown0x1B5;
            set
            {
                Data._unknown0x1B5 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1B6 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1B6
        {
            get => Data._unknown0x1B6;
            set
            {
                Data._unknown0x1B6 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1B7 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1B7
        {
            get => Data._unknown0x1B7;
            set
            {
                Data._unknown0x1B7 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1B8 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x1B8
        {
            get => Data._unknown0x1B8;
            set
            {
                Data._unknown0x1B8 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1BC (uint)")]
        [Category("Unknown")]
        public uint Unknown0x1BC
        {
            get => Data._unknown0x1BC;
            set
            {
                Data._unknown0x1BC = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1C0 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1C0
        {
            get => Data._unknown0x1C0;
            set
            {
                Data._unknown0x1C0 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1C1 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x1C1
        {
            get => Data._unknown0x1C1;
            set
            {
                Data._unknown0x1C1 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1C2 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1C2
        {
            get => Data._unknown0x1C2;
            set
            {
                Data._unknown0x1C2 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1C3 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1C3
        {
            get => Data._unknown0x1C3;
            set
            {
                Data._unknown0x1C3 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1C4 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x1C4
        {
            get => Data._unknown0x1C4;
            set
            {
                Data._unknown0x1C4 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1C8 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x1C8
        {
            get => Data._unknown0x1C8;
            set
            {
                Data._unknown0x1C8 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1CC (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1CC
        {
            get => Data._unknown0x1CC;
            set
            {
                Data._unknown0x1CC = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1CD (bool)")]
        [Category("Unknown")]
        public bool Unknown0x1CD
        {
            get => Data._unknown0x1CD;
            set
            {
                Data._unknown0x1CD = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1CE (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1CE
        {
            get => Data._unknown0x1CE;
            set
            {
                Data._unknown0x1CE = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1CF (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1CF
        {
            get => Data._unknown0x1CF;
            set
            {
                Data._unknown0x1CF = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1D0 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x1D0
        {
            get => Data._unknown0x1D0;
            set
            {
                Data._unknown0x1D0 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1D4 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x1D4
        {
            get => Data._unknown0x1D4;
            set
            {
                Data._unknown0x1D4 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1D8 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1D8
        {
            get => Data._unknown0x1D8;
            set
            {
                Data._unknown0x1D8 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1D9 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x1D9
        {
            get => Data._unknown0x1D9;
            set
            {
                Data._unknown0x1D9 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1DA (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1DA
        {
            get => Data._unknown0x1DA;
            set
            {
                Data._unknown0x1DA = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1DB (byte)")]
        [Category("Unknown")]
        public byte Unknown0x1DB
        {
            get => Data._unknown0x1DB;
            set
            {
                Data._unknown0x1DB = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1DC (uint)")]
        [Category("Unknown")]
        public uint Unknown0x1DC
        {
            get => Data._unknown0x1DC;
            set
            {
                Data._unknown0x1DC = value;
                SignalPropertyChange();
            }
        }

        public TriggerDataClass _unknown0x1E0;
        [TypeConverter(typeof(ExpandableObjectCustomConverter))]
        [DisplayName("Unknown0x1E0 (TriggerData)")]
        [Category("Unknown")]
        public TriggerDataClass Unknown0x1E0
        {
            get => _unknown0x1E0 ?? new TriggerDataClass(this);
            set
            {
                _unknown0x1E0 = value;
                Data._unknown0x1E0 = value;
                SignalPropertyChange();
            }
        }

        public TriggerDataClass _unknown0x1E4;
        [TypeConverter(typeof(ExpandableObjectCustomConverter))]
        [DisplayName("Unknown0x1E4 (TriggerData)")]
        [Category("Unknown")]
        public TriggerDataClass Unknown0x1E4
        {
            get => _unknown0x1E4 ?? new TriggerDataClass(this);
            set
            {
                _unknown0x1E4 = value;
                Data._unknown0x1E4 = value;
                SignalPropertyChange();
            }
        }

        public TriggerDataClass _unknown0x1E8;
        [TypeConverter(typeof(ExpandableObjectCustomConverter))]
        [DisplayName("Unknown0x1E8 (TriggerData)")]
        [Category("Unknown")]
        public TriggerDataClass Unknown0x1E8
        {
            get => _unknown0x1E8 ?? new TriggerDataClass(this);
            set
            {
                _unknown0x1E8 = value;
                Data._unknown0x1E8 = value;
                SignalPropertyChange();
            }
        }

        public override int OnCalculateSize(bool force)
        {
            return GMOVEntry.Size;
        }

        public override bool OnInitialize()
        {
            Data = *(GMOVEntry*) WorkingUncompressed.Address;
            _motionPath = new MotionPathDataClass(this, Data._motionPath);
            _difficultyRatio = new DifficultyRatiosClass(this, Data._difficultyRatio);
            _unknown0x1E0 = new TriggerDataClass(this, Data._unknown0x1E0);
            _unknown0x1E4 = new TriggerDataClass(this, Data._unknown0x1E4);
            _unknown0x1E8 = new TriggerDataClass(this, Data._unknown0x1E8);

            if (_name == null)
            {
                _name = $"Platform [{Index}]";
            }

            return base.OnInitialize();
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            GMOVEntry* hdr = (GMOVEntry*)address;
            Data._motionPath = _motionPath;
            Data._difficultyRatio = _difficultyRatio;
            Data._unknown0x1E0 = _unknown0x1E0;
            Data._unknown0x1E4 = _unknown0x1E4;
            Data._unknown0x1E8 = _unknown0x1E8;
            *hdr = Data;
        }
    }
}