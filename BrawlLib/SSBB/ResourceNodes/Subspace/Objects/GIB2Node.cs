using BrawlLib.Internal;
using BrawlLib.SSBB.Types.Subspace.Objects;
using System;
using System.ComponentModel;

namespace BrawlLib.SSBB.ResourceNodes
{
    public class GIB2Node : BLOCEntryNode
    {
        public override Type SubEntryType => typeof(GIB2EntryNode);
        protected override string baseName => "Item Boxes";

        internal static ResourceNode TryParse(DataSource source, ResourceNode parent)
        {
            return source.Tag == "GIB2" ? new GIB2Node() : null;
        }
    }

    public unsafe class GIB2EntryNode : ResourceNode
    {
        protected internal GIB2Entry Data;

        [DisplayName("Unknown0x00 (float)")]
        [Category("Unknown")]
        public float Unknown0x00
        {
            get => Data._unknown0x00;
            set
            {
                Data._unknown0x00 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x04 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x04
        {
            get => Data._unknown0x04;
            set
            {
                Data._unknown0x04 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x05 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x05
        {
            get => Data._unknown0x05;
            set
            {
                Data._unknown0x05 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x06 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x06
        {
            get => Data._unknown0x06;
            set
            {
                Data._unknown0x06 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x07 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x07
        {
            get => Data._unknown0x07;
            set
            {
                Data._unknown0x07 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x08 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x08
        {
            get => Data._unknown0x08;
            set
            {
                Data._unknown0x08 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x0C (uint)")]
        [Category("Unknown")]
        public uint Unknown0x0C
        {
            get => Data._unknown0x0C;
            set
            {
                Data._unknown0x0C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x10 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x10
        {
            get => Data._unknown0x10;
            set
            {
                Data._unknown0x10 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x14 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x14
        {
            get => Data._unknown0x14;
            set
            {
                Data._unknown0x14 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x18 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x18
        {
            get => Data._unknown0x18;
            set
            {
                Data._unknown0x18 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x1C (uint)")]
        [Category("Unknown")]
        public uint Unknown0x1C
        {
            get => Data._unknown0x1C;
            set
            {
                Data._unknown0x1C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x20 (float)")]
        [Category("Unknown")]
        public float Unknown0x20
        {
            get => Data._unknown0x20;
            set
            {
                Data._unknown0x20 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x24 (short)")]
        [Category("Unknown")]
        public short Unknown0x24
        {
            get => Data._unknown0x24;
            set
            {
                Data._unknown0x24 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x26 (short)")]
        [Category("Unknown")]
        public short Unknown0x26
        {
            get => Data._unknown0x26;
            set
            {
                Data._unknown0x26 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x28 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x28
        {
            get => Data._unknown0x28;
            set
            {
                Data._unknown0x28 = value;
                SignalPropertyChange();
            }
        }

        [Category("Item Box")]
        public byte ModelDataIndex
        {
            get => Data._modelDataIndex;
            set
            {
                Data._modelDataIndex = value;
                SignalPropertyChange();
            }
        }

        [Category("Item Box")]
        public byte CollisionDataIndex
        {
            get => Data._collisionDataIndex;
            set
            {
                Data._collisionDataIndex = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x2E (byte)")]
        [Category("Unknown")]
        public byte Unknown0x2E
        {
            get => Data._unknown0x2E;
            set
            {
                Data._unknown0x2E = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x2F (byte)")]
        [Category("Unknown")]
        public byte Unknown0x2F
        {
            get => Data._unknown0x2F;
            set
            {
                Data._unknown0x2F = value;
                SignalPropertyChange();
            }
        }

        [Category("Item Box")]
        [TypeConverter(typeof(Vector2StringConverter))]
        public Vector2 Position
        {
            get => new Vector2(Data._positionX, Data._positionY);
            set
            {
                Data._positionX = value.X;
                Data._positionY = value.Y;
                SignalPropertyChange();
            }
        }

        [Category("Item Box")]
        public uint ItemSpawnGroup
        {
            get => Data._itemSpawnGroup;
            set
            {
                Data._itemSpawnGroup = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x3C (byte)")]
        [Category("Unknown")]
        public byte Unknown0x3C
        {
            get => Data._unknown0x3C;
            set
            {
                Data._unknown0x3C = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x3D (byte)")]
        [Category("Unknown")]
        public byte Unknown0x3D
        {
            get => Data._unknown0x3D;
            set
            {
                Data._unknown0x3D = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x3E (byte)")]
        [Category("Unknown")]
        public byte Unknown0x3E
        {
            get => Data._unknown0x3E;
            set
            {
                Data._unknown0x3E = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x3F (byte)")]
        [Category("Unknown")]
        public byte Unknown0x3F
        {
            get => Data._unknown0x3F;
            set
            {
                Data._unknown0x3F = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x40 (uint)")]
        [Category("Unknown")]
        public uint Unknown0x40
        {
            get => Data._unknown0x40;
            set
            {
                Data._unknown0x40 = value;
                SignalPropertyChange();
            }
        }

        public TriggerDataClass _unknown0x44;
        [TypeConverter(typeof(ExpandableObjectCustomConverter))]
        [DisplayName("Unknown0x44 (TriggerData)")]
        [Category("Unknown")]
        public TriggerDataClass Unknown0x44
        {
            get => _unknown0x44 ?? new TriggerDataClass(this);
            set
            {
                _unknown0x44 = value;
                Data._unknown0x44 = value;
                SignalPropertyChange();
            }
        }

        public TriggerDataClass _unknown0x48;
        [TypeConverter(typeof(ExpandableObjectCustomConverter))]
        [DisplayName("Unknown0x48 (TriggerData)")]
        [Category("Unknown")]
        public TriggerDataClass Unknown0x48
        {
            get => _unknown0x48 ?? new TriggerDataClass(this);
            set
            {
                _unknown0x48 = value;
                Data._unknown0x48 = value;
                SignalPropertyChange();
            }
        }

        public TriggerDataClass _unknown0x4C;
        [TypeConverter(typeof(ExpandableObjectCustomConverter))]
        [DisplayName("Unknown0x4C (TriggerData)")]
        [Category("Unknown")]
        public TriggerDataClass Unknown0x4C
        {
            get => _unknown0x4C ?? new TriggerDataClass(this);
            set
            {
                _unknown0x4C = value;
                Data._unknown0x4C = value;
                SignalPropertyChange();
            }
        }

        public TriggerDataClass _unknown0x50;
        [TypeConverter(typeof(ExpandableObjectCustomConverter))]
        [DisplayName("Unknown0x50 (TriggerData)")]
        [Category("Unknown")]
        public TriggerDataClass Unknown0x50
        {
            get => _unknown0x50 ?? new TriggerDataClass(this);
            set
            {
                _unknown0x50 = value;
                Data._unknown0x50 = value;
                SignalPropertyChange();
            }
        }

        public override int OnCalculateSize(bool force)
        {
            return GIB2Entry.Size;
        }

        public override bool OnInitialize()
        {
            Data = *(GIB2Entry*) WorkingUncompressed.Address;
            _unknown0x44 = new TriggerDataClass(this, Data._unknown0x44);
            _unknown0x48 = new TriggerDataClass(this, Data._unknown0x48);
            _unknown0x4C = new TriggerDataClass(this, Data._unknown0x4C);
            _unknown0x50 = new TriggerDataClass(this, Data._unknown0x50);

            if (_name == null)
            {
                _name = $"Item Box [{Index}]";
            }

            return base.OnInitialize();
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            GIB2Entry* hdr = (GIB2Entry*)address;
            Data._unknown0x44 = _unknown0x44;
            Data._unknown0x48 = _unknown0x48;
            Data._unknown0x4C = _unknown0x4C;
            Data._unknown0x50 = _unknown0x50;
            *hdr = Data;
        }
    }
}