using BrawlLib.Internal;
using BrawlLib.SSBB.Types.Subspace.Objects;
using System;
using System.ComponentModel;

namespace BrawlLib.SSBB.ResourceNodes
{
    public class GITMNode : BLOCEntryNode
    {
        public override Type SubEntryType => typeof(GITMEntryNode);
        protected override string baseName => "Item Spawns";

        internal static ResourceNode TryParse(DataSource source, ResourceNode parent)
        {
            return source.Tag == "GITM" ? new GITMNode() : null;
        }
    }

    public unsafe class GITMEntryNode : ResourceNode
    {
        protected internal GITMEntry Data;

        [Category("Item")]
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

        public TriggerDataClass _unknown0x0C;
        [TypeConverter(typeof(ExpandableObjectCustomConverter))]
        [DisplayName("Unknown0x0C (TriggerData)")]
        [Category("Unknown")]
        public TriggerDataClass Unknown0x0C
        {
            get => _unknown0x0C ?? new TriggerDataClass(this);
            set
            {
                _unknown0x0C = value;
                Data._unknown0x0C = value;
                SignalPropertyChange();
            }
        }

        public TriggerDataClass _unknown0x10;
        [TypeConverter(typeof(ExpandableObjectCustomConverter))]
        [DisplayName("Unknown0x10 (TriggerData)")]
        [Category("Unknown")]
        public TriggerDataClass Unknown0x10
        {
            get => _unknown0x10 ?? new TriggerDataClass(this);
            set
            {
                _unknown0x10 = value;
                Data._unknown0x10 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x14 (bool)")]
        [Category("Unknown")]
        public bool Unknown0x14
        {
            get => Data._unknown0x14;
            set
            {
                Data._unknown0x14 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x15 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x15
        {
            get => Data._unknown0x15;
            set
            {
                Data._unknown0x15 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x16 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x16
        {
            get => Data._unknown0x16;
            set
            {
                Data._unknown0x16 = value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Unknown0x17 (byte)")]
        [Category("Unknown")]
        public byte Unknown0x17
        {
            get => Data._unknown0x17;
            set
            {
                Data._unknown0x17 = value;
                SignalPropertyChange();
            }
        }

        public override int OnCalculateSize(bool force)
        {
            return GITMEntry.Size;
        }

        public override bool OnInitialize()
        {
            Data = *(GITMEntry*) WorkingUncompressed.Address;
            _unknown0x0C = new TriggerDataClass(this, Data._unknown0x0C);
            _unknown0x10 = new TriggerDataClass(this, Data._unknown0x10);

            if (_name == null)
            {
                _name = $"Item [{Index}]";
            }

            return base.OnInitialize();
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            GITMEntry* hdr = (GITMEntry*)address;
            Data._unknown0x0C = _unknown0x0C;
            Data._unknown0x10 = _unknown0x10;
            *hdr = Data;
        }
    }
}