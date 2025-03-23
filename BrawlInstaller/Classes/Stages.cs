using BrawlInstaller.Enums;
using BrawlLib.Imaging;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.ResourceNodes.ProjectPlus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static BrawlLib.SSBB.ResourceNodes.ProjectPlus.STEXNode;
using System.Windows.Interop;
using BrawlInstaller.Common;
using System.Windows.Media;

namespace BrawlInstaller.Classes
{
    public class StageInfo
    {
        public string RandomName { get; set; } = string.Empty;
        public StageSlot Slot { get; set; } = new StageSlot();
        public CosmeticList Cosmetics { get; set; } = new CosmeticList();
        public List<StageEntry> StageEntries { get; set; } = new List<StageEntry>();
        public List<StageParams> AllParams { get; set; } = new List<StageParams>();

        public StageInfo Copy()
        {
            var copy = new StageInfo
            {
                RandomName = RandomName,
                Slot = Slot.Copy(),
                Cosmetics = Cosmetics.Copy(),
                StageEntries = StageEntries.Copy()
            };
            copy.AllParams = copy.StageEntries.Select(x => x.Params).Distinct().ToList();
            return copy;
        }
    }

    public class StageList
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public List<StagePage> Pages { get; set; } = new List<StagePage>();
        [JsonIgnore] public StageListType Type { get => FilePath?.EndsWith(".rss") == true ? StageListType.RSS : StageListType.ASM; }
    }

    public class StagePage
    {
        public int PageNumber { get; set; }
        public List<StageSlot> StageSlots { get; set; } = new List<StageSlot>();
        public ulong RandomFlags { get; set; } = 0;
        public ulong HazardFlags { get; set; } = 0;

        public List<AsmTableEntry> ConvertToAsmTable()
        {
            var newAsmTable = new List<AsmTableEntry>();
            foreach(var slot in StageSlots)
            {
                var newAsmEntry = new AsmTableEntry
                {
                    Item = $"0x{slot.Index:X2}",
                    Comment = slot.Name
                };
                newAsmTable.Add(newAsmEntry);
            }
            return newAsmTable;
        }
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

        public AsmTableEntry ConvertToAsmTableEntry()
        {
            var tableEntry = new AsmTableEntry
            {
                Item = $"0x{StageIds.StageId:X2}{(StageIds.StageCosmeticId != null ? StageIds.StageCosmeticId?.ToString("X2") : string.Empty)}",
                Comment = Name
            };
            return tableEntry;
        }
    }

    public static class StageSlotListExtensions
    {
        public static List<AsmTableEntry> ConvertToAsmTable(this List<StageSlot> stageSlotList)
        {
            var list = new List<AsmTableEntry>();
            foreach (var slot in stageSlotList)
            {
                list.Add(slot.ConvertToAsmTableEntry());
            }
            return list;
        }

        public static byte[] ToByteArray(this List<StageSlot> list)
        {
            var slotIds = new List<byte>();
            foreach (var item in list)
            {
                slotIds.Add((byte)item.StageIds.StageId);
                slotIds.Add((byte)item.StageIds.StageCosmeticId);
            }
            var bytes = slotIds.ToArray();
            return bytes;
        }
    }

    public class StageEntry
    {
        public ushort ButtonFlags { get; set; } = 0x0000;
        public ListAlt ListAlt { get; set; } = new ListAlt();
        [JsonIgnore] public bool IsRAlt { get => ((GameCubeButtons)ButtonFlags & GameCubeButtons.Unused0x4000) != 0 && ((GameCubeButtons)ButtonFlags & GameCubeButtons.Unused0x8000) == 0; }
        [JsonIgnore] public bool IsLAlt { get => ((GameCubeButtons)ButtonFlags & GameCubeButtons.Unused0x8000) != 0 && ((GameCubeButtons)ButtonFlags & GameCubeButtons.Unused0x4000) == 0; }
        [JsonIgnore] public bool IsEventStage { get => ((GameCubeButtons)ButtonFlags & GameCubeButtons.Unused0x4000) != 0 && ((GameCubeButtons)ButtonFlags & GameCubeButtons.Unused0x8000) != 0; }
        public StageParams Params { get; set; } = new StageParams();

        public StageEntry Copy()
        {
            var copy = new StageEntry
            {
                ButtonFlags = ButtonFlags,
                ListAlt = ListAlt.Copy(),
                Params = Params.Copy()
            };
            return copy;
        }
    }

    public static class StageEntryListExtensions
    {
        public static List<StageEntry> Copy(this List<StageEntry> list)
        {
            var newList = new List<StageEntry>();
            var paramsCopied = new List<(StageParams OldRef, StageParams NewRef)>();
            foreach (var item in list)
            {
                var newItem = new StageEntry
                {
                    ButtonFlags = item.ButtonFlags,
                    ListAlt = item.ListAlt.Copy()
                };
                // Preserve references
                var existingParams = paramsCopied.Any(x => x.OldRef == item.Params);
                if (existingParams)
                {
                    newItem.Params = paramsCopied.FirstOrDefault(x => x.OldRef == item.Params).NewRef;
                }
                else
                {
                    newItem.Params = item.Params.Copy();
                    paramsCopied.Add((item.Params, newItem.Params));
                }
                newList.Add(newItem);
            }
            return newList;
        }
    }

    public class StageParams
    {
        public string Name { get; set; } = "Unknown";
        public string PacName { get; set; } = "Unknown";
        public string PacFile { get; set; } = null;
        public string TrackList { get => TrackListFile != null ? Path.GetFileNameWithoutExtension(TrackListFile) : string.Empty; }
        public string TrackListFile { get; set; } = null;
        public string Module { get; set; } = string.Empty;
        public string ModuleFile { get; set; } = null;
        [JsonIgnore] public RGBAPixel CharacterOverlay { get; set; } = new RGBAPixel { R = 0, G = 0, B = 0, A = 0 };
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

        public StageParams Copy()
        {
            var copy = JsonConvert.DeserializeObject<StageParams>(JsonConvert.SerializeObject(this));
            copy.CharacterOverlay = CharacterOverlay;
            return copy;
        }
    }

    public class Substage
    {
        public string Name { get; set; } = "00";
        public string PacFile { get; set; } = null;

        public Substage Copy()
        {
            return new Substage
            {
                Name = Name,
                PacFile = PacFile
            };
        }
    }

    public class ListAlt
    {
        public string BinFileName { get; set; } = "Unknown";
        public string BinFilePath { get; set; } = string.Empty;
        public string Name { get; set; } = "Unknown";
        public BitmapImage Image { get; set; } = null;
        [JsonIgnore] public byte[] JpegData
        {
            get
            {
                var bitmap = Image.ToBitmap();
                var pixelData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                var pixelFormat = PixelFormats.Bgra32;
                var bitmapSource = BitmapSource.Create(bitmap.Width, bitmap.Height, 1, 1, pixelFormat, null, pixelData.Scan0, pixelData.Stride * bitmap.Height, pixelData.Stride);
                var resizedBitmap = new TransformedBitmap(bitmapSource, new ScaleTransform(160.0 / bitmap.Width, 120.0 / bitmap.Height));
                var encoder = new JpegBitmapEncoder();
                using (MemoryStream outStream = new MemoryStream())
                {
                    encoder.Frames.Add(BitmapFrame.Create(resizedBitmap));
                    encoder.Save(outStream);
                    return outStream.ToArray();
                }
            }
        }

        public ListAlt Copy()
        {
            var copy = new ListAlt
            {
                BinFileName = BinFileName,
                BinFilePath = BinFilePath,
                Name = Name,
                Image = Image
            };
            return copy;
        }
    }
}
