using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using BrawlInstaller.Classes;
using BrawlInstaller.Enums;
using BrawlLib.Internal;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.ResourceNodes.ProjectPlus;
using Newtonsoft.Json;

namespace BrawlInstaller.Common
{
    public static class BitmapExtensions
    {
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            return ToBitmapImage(bitmap, ImageFormat.Png);
        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap, ImageFormat format)
        {
            if (bitmap != null)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, format);
                    memory.Position = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return bitmapImage;
                }
            }
            return null;
        }
    }

    public static class BitmapImageExtensions
    {
        public static Bitmap ToBitmap(this BitmapImage image)
        {
            return ToBitmap(image, new PngBitmapEncoder());
        }

        public static Bitmap ToBitmap(this BitmapImage image, BitmapEncoder encoder)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(outStream);
                var bitmap = new Bitmap(outStream);
                return new Bitmap(bitmap);
            }
        }

        public static byte[] ToByteArray(this BitmapImage image, BitmapEncoder encoder)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(outStream);
                return outStream.ToArray();
            }
        }
    }

    public static class FCFGNodeExtensions
    {
        public static FighterAttributes ToFighterAttributes(this FCFGNode node)
        {
            var fighterAttributes = new FighterAttributes
            {
                CanCrawl = node.CanCrawl,
                CanFTilt = node.CanFTilt,
                CanGlide = node.CanGlide,
                CanWallCling = node.CanWallCling,
                CanWallJump = node.CanWallJump,
                CanZAir = node.CanZAir,
                AirJumpCount = node.AirJumpCount,
                DAIntoCrouch = node.DAIntoCrouch,
                FSmashCount = node.FSmashCount,
                HasRapidJab = node.HasRapidJab,
                JabCount = node.JabCount,
                JabFlag = node.JabFlag,
                HasCostume00 = node.HasCostume00,
                HasCostume01 = node.HasCostume01,
                HasCostume02 = node.HasCostume02,
                HasCostume03 = node.HasCostume03,
                HasCostume04 = node.HasCostume04,
                HasCostume05 = node.HasCostume05,
                HasCostume06 = node.HasCostume06,
                HasCostume07 = node.HasCostume07,
                HasCostume08 = node.HasCostume08,
                HasCostume09 = node.HasCostume09,
                HasCostume10 = node.HasCostume10,
                HasCostume11 = node.HasCostume11,
                UnknownFlagA = node.UnknownFlagA,
                UnknownFlagB = node.UnknownFlagB,
                UnknownFlagC = node.UnknownFlagC,
                UnknownFlagD = node.UnknownFlagD,
                HasPac = node.HasPac,
                HasModule = node.HasModule,
                MotionEtcType = node.MotionEtcType,
                UnknownLoadFlagA = node.UnknownLoadFlagA,
                UnknownLoadFlagB = node.UnknownLoadFlagB,
                EntryLoadType = node.EntryLoadType,
                ResultLoadType = node.ResultLoadType,
                FinalLoadType = node.FinalLoadType,
                FinalSmashMusic = node.FinalSmashMusic,
                AIController = node.AIController,
                EntryFlag = node.EntryFlag,
                IkPhysics = node.IkPhysics,
                TextureLoader = node.TextureLoader,
                ThrownType = node.ThrownType,
                GrabSize = node.GrabSize,
                WorkManage = node.WorkManage,
                Version = node.Version
            };
            return fighterAttributes;
        }
    }

    public static class SLTCNodeExtensions
    {
        public static SlotAttributes ToSlotAttributes(this SLTCNode node)
        {
            var slotAttributes = new SlotAttributes
            {
                SetSlot = node.SetSlot,
                CharSlot1 = node.CharSlot1,
                CharSlot2 = node.CharSlot2,
                CharSlot3 = node.CharSlot3,
                CharSlot4 = node.CharSlot4,
                Records = node.Records,
                AnnouncerID = node.AnnouncerID,
                CameraDistance1 = node.CameraDistance1,
                CameraDistance2 = node.CameraDistance2,
                CameraDistance3 = node.CameraDistance3,
                CameraDistance4 = node.CameraDistance4,
                Size = node._size.ReverseEndianness(),
                Version = node._version.ReverseEndianness(),
                Unknown0x25 = node._unknown0x25,
                Unknown0x26 = node._unknown0x26,
                Unknown0x27 = node._unknown0x27,
                Unknown0x2C = node._unknown0x2C.ReverseEndianness()
            };
            return slotAttributes;
        }
    }

    public static class COSCNodeExtensions
    {
        public static CosmeticAttributes ToCosmeticAttributes(this COSCNode node)
        {
            var cosmeticAttributes = new CosmeticAttributes
            {
                HasSecondary = node.HasSecondary,
                CharSlot1 = node.CharSlot1,
                CharSlot2 = node.CharSlot2,
                AnnouncerID = node.AnnouncerID,
                Size = node._size.ReverseEndianness(),
                Version = node._version.ReverseEndianness(),
                Unknown0x11 = node._unknown0x11,
                Unknown0x15 = node._unknown0x15,
                Unknown0x16 = node._unknown0x16,
                Unknown0x17 = node._unknown0x17,
                Unknown0x1C = node._unknown0x1C.ReverseEndianness()
            };
            return cosmeticAttributes;
        }
    }

    public static class CSSCNodeExtensions
    {
        public static CSSSlotAttributes ToCSSSlotAttributes(this CSSCNode node)
        {
            var cssSlotAttributes = new CSSSlotAttributes
            {
                SetPrimarySecondary = node.SetPrimarySecondary,
                CharSlot1 = node.CharSlot1,
                CharSlot2 = node.CharSlot2,
                SetCosmeticSlot = node.SetCosmeticSlot,
                CosmeticSlot = node.CosmeticSlot,
                Records = node.Records,
                WiimoteSFX = node.WiimoteSFX,
                Unknown0x18 = node._unknown0x18,
                Version = node._version,
                Status = node.Status,
                Size = node._size
            };
            return cssSlotAttributes;
        }
    }

    public static class STEXNodeExtensions
    {
        public static StageParams ToStageParams(this STEXNode node)
        {
            var stageParams = new StageParams
            {
                Name = node.Name,
                IsFlat = node.IsFlat,
                IsFixedCamera = node.IsFixedCamera,
                IsSlowStart = node.IsSlowStart,
                PacName = node.StageName,
                Module = node.Module,
                CharacterOverlay = node.CharacterOverlay,
                SoundBank = node.SoundBank,
                EffectBank = node.EffectBank,
                MemoryAllocation = node.MemoryAllocation,
                WildSpeed = node.WildSpeed,
                IsDualLoad = node.IsDualLoad,
                IsDualShuffle = node.IsDualShuffle,
                IsOldSubStage = node.IsOldSubstage,
                VariantType = node.SubstageVarianceType,
                SubstageRange = node.SubstageRange
            };
            foreach(var child in node.Children)
            {
                stageParams.Substages.Add(new Substage { Name = child.Name });
            }
            return stageParams;
        }
    }

    public static class EnumExtensions
    {
        public static KeyValuePair<string, T> GetKeyValuePair<T>(this T obj)
        {
            return new KeyValuePair<string, T>(obj.GetDescription(), obj);
        }

        public static Dictionary<string, T> GetDictionary<T>(this Type t)
        {
            var keyValueList = new Dictionary<string, T>();
            foreach(T item in Enum.GetValues(typeof(T)))
            {
                var keyValuePair = item.GetKeyValuePair();
                keyValueList.Add(keyValuePair.Key, keyValuePair.Value);
            }
            return keyValueList;
        }
    }

    public static class MDL0Extensions
    {
        public static CLR0Node GetColorSequence(this MDL0Node model)
        {
            var bres = model?.Parent?.Parent;
            if (bres != null && bres.GetType() == typeof(BRRESNode))
            {
                var folder = ((BRRESNode)bres).GetFolder<CLR0Node>();
                if (folder != null)
                {
                    var clr0 = folder.Children.FirstOrDefault(x => x.Name == model.Name);
                    if (clr0 != null)
                        return (CLR0Node)clr0;
                }
            }
            return null;
        }
    }

    public static class ObservableCollectionExtensions
    {
        public static void Move<T>(this ObservableCollection<T> list, int oldIndex, int newIndex)
        {
            var item = list[oldIndex];

            list.RemoveAt(oldIndex);

            if (newIndex > oldIndex) newIndex--;
            // the actual index could have shifted due to the removal

            list.Insert(newIndex, item);
        }

        public static void Move<T>(this ObservableCollection<T> list, T item, int newIndex)
        {
            if (item != null)
            {
                var oldIndex = list.IndexOf(item);
                if (oldIndex > -1)
                {
                    list.RemoveAt(oldIndex);

                    if (newIndex > oldIndex + 1) newIndex--;
                    // the actual index could have shifted due to the removal

                    list.Insert(newIndex, item);
                }
            }
        }

        public static void MoveUp<T>(this ObservableCollection<T> list, T item)
        {
            if (item != null && list.IndexOf(item) != 0)
            {
                var oldIndex = list.IndexOf(item);
                list.Move(item, oldIndex - 1);
            }
        }

        public static void MoveDown<T>(this ObservableCollection<T> list, T item)
        {
            if (item != null && list.IndexOf(item) != list.Count - 1)
            {
                var oldIndex = list.IndexOf(item);
                list.Move(item, oldIndex + 1);
            }
        }
    }

    public static class ListExtensions
    {
        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            var item = list[oldIndex];

            list.RemoveAt(oldIndex);

            if (newIndex > oldIndex) newIndex--;
            // the actual index could have shifted due to the removal

            list.Insert(newIndex, item);
        }

        public static void Move<T>(this List<T> list, T item, int newIndex)
        {
            if (item != null)
            {
                var oldIndex = list.IndexOf(item);
                if (oldIndex > -1)
                {
                    list.RemoveAt(oldIndex);

                    if (newIndex > oldIndex + 1) newIndex--;
                    // the actual index could have shifted due to the removal

                    list.Insert(newIndex, item);
                }
            }
        }

        public static void MoveUp<T>(this List<T> list, T item)
        {
            if (item != null && list.IndexOf(item) != 0)
            {
                var oldIndex = list.IndexOf(item);
                list.Move(item, oldIndex - 1);
            }
        }

        public static void MoveDown<T>(this List<T> list, T item)
        {
            if (item != null && list.IndexOf(item) != list.Count - 1)
            {
                var oldIndex = list.IndexOf(item);
                list.Move(item, oldIndex + 1);
            }
        }
    }

    public static class FighterInfoListExtensions
    {
        public static List<FighterInfo> Copy(this List<FighterInfo> list)
        {
            var newList = new List<FighterInfo>();
            foreach(var item in list)
            {
                newList.Add(item.Copy());
            }
            return newList;
        }
    }

    public static class CosmeticListExtensions
    {
        public static List<Cosmetic> Copy(this List<Cosmetic> list)
        {
            var newList = new List<Cosmetic>();
            foreach(var item in list)
            {
                newList.Add(item.Copy());
            }
            return newList;
        }
    }

    public static class CostumeListExtensions
    {
        public static List<Costume> Copy(this List<Costume> list)
        {
            var newList = new List<Costume>();
            foreach (var item in list)
            {
                newList.Add(item.Copy());
            }
            return newList;
        }
    }

    public static class FighterPacFileListExtensions
    {
        public static List<FighterPacFile> Copy(this List<FighterPacFile> list)
        {
            var newList = new List<FighterPacFile>();
            foreach (var item in list)
            {
                newList.Add(item.Copy());
            }
            return newList;
        }
    }

    public static class FighterInstallOptionListExtensions
    {
        public static List<FighterInstallOption> Copy(this List<FighterInstallOption> list)
        {
            var newList = new List<FighterInstallOption>();
            foreach (var item in list)
            {
                newList.Add(item.Copy());
            }
            return newList;
        }
    }

    public static class FighterTrophyListExtensions
    {
        public static List<FighterTrophy> Copy(this List<FighterTrophy> list)
        {
            var newList = new List<FighterTrophy>();
            var trophiesCopied = new List<(Trophy OldRef, Trophy NewRef)>();
            foreach (var item in list)
            {
                var newItem = new FighterTrophy
                {
                    Type = item.Type
                };
                // Preserve references
                var existingTrophy = trophiesCopied.Any(x => x.OldRef == item.Trophy);
                if (existingTrophy && item?.Trophy != null)
                {
                    newItem.Trophy = trophiesCopied.FirstOrDefault(x => x.OldRef == item.Trophy).NewRef;
                }
                else if (item?.Trophy != null)
                {
                    newItem.Trophy = item.Trophy.Copy();
                    trophiesCopied.Add((item.Trophy, newItem.Trophy));
                }
                var existingOldTrophy = trophiesCopied.Any(x => x.OldRef == item.OldTrophy);
                if (existingOldTrophy && item?.OldTrophy != null)
                {
                    newItem.OldTrophy = trophiesCopied.FirstOrDefault(x => x.OldRef == item.OldTrophy).NewRef;
                }
                else if (item?.OldTrophy != null)
                {
                    newItem.OldTrophy = item.OldTrophy.Copy();
                    trophiesCopied.Add((item.OldTrophy, newItem.OldTrophy));
                }
                newList.Add(newItem);
            }
            return newList;
        }
    }

    public static class ObjectExtensions
    {
        public static bool Compare(this object object1, object object2)
        {
            return JsonConvert.SerializeObject(object1) == JsonConvert.SerializeObject(object2);
        }
    }

    public static class UIntExtensions
    {
        public static uint ReverseEndianness(this uint number)
        {
            byte[] bits = BitConverter.GetBytes(number);
            var revbits = bits.Reverse().ToArray();
            number = BitConverter.ToUInt32(revbits, 0);
            return number;
        }
    }

    public static class StringExtensions
    {
        public static string ConvertToFullwidth(this string input)
        {
            char[] fullwidthChars = new char[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                // Only change actual latin characters
                if (c >= '!' && c <= '~')
                {
                    fullwidthChars[i] = (char)(c + 0xFEE0);
                }
                else
                {
                    fullwidthChars[i] = c;
                }
            }

            return new string(fullwidthChars);
        }
    }

    public static class TyDataListEntryNodeExtensions
    {
        public static Trophy ToTrophy(this TyDataListEntryNode node)
        {
            var trophy = new Trophy
            {
                Name = node.Name,
                Brres = node.BRRES,
                Ids = new BrawlIds
                {
                    Ids = new List<BrawlId>
                    {
                        new BrawlId { Id = node.Id, Type = IdType.Trophy },
                        new BrawlId { Id = node.ThumbnailIndex, Type = IdType.Thumbnail }
                    }
                },
                GameIcon1 = node.GameIcon1,
                GameIcon2 = node.GameIcon2,
                NameIndex = node.NameIndex,
                GameIndex = node.GameIndex,
                DescriptionIndex = node.DescriptionIndex,
                SeriesIndex = node.SeriesIndex,
                CategoryIndex = node.CategoryIndex,
                Unknown0x34 = node.Unknown0x34,
                Unknown0x38 = node.Unknown0x38,
                Unknown0x40 = node.Unknown0x40,
                Unknown0x44 = node.Unknown0x44,
                Unknown0x50 = node.Unknown0x50,
                Unknown0x54 = node.Unknown0x54,
                Unknown0x58 = node.Unknown0x58,
                Unknown0x5C = node.Unknown0x5C,
                Pad0x28 = node.Pad0x28,
                Pad0x2C = node.Pad0x2C,
                Pad0x30 = node.Pad0x30,
                Pad0x3C = node.Pad0x3C,
                Pad0x48 = node.Pad0x48
            };
            return trophy;
        }
    }

    public static class ULongExtensions
    {
        public static ulong SwapBits(this ulong bitmask, int index1, int index2)
        {
            bool bit1 = (bitmask & (1UL << index1)) != 0;
            bool bit2 = (bitmask & (1UL << index2)) != 0;

            if (bit1 == bit2)
            {
                return bitmask;
            }

            // Toggle bits
            bitmask ^= (1UL << index1);
            bitmask ^= (1UL << index2);

            return bitmask;
        }

        public static ulong ToggleBit(this ulong bitmask, int index)
        {
            bitmask ^= (1UL << index);
            return bitmask;
        }

        public static ulong DisableBit(this ulong bitmask, int index)
        {
            bitmask &= (1UL << index);
            return bitmask;
        }

        public static ulong EnableBit(this ulong bitmask, int index)
        {
            bitmask |= (1UL << index);
            return bitmask;
        }

        public static List<int> GetToggledBits(this ulong bitmask)
        {
            List<int> toggledBits = new List<int>();

            for (int i = 0; i < 64; i++)
            {
                if ((bitmask & (1UL << i)) != 0)
                {
                    toggledBits.Add(i + 1);
                }
            }

            return toggledBits;
        }
    }
}
