﻿using System;
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
using BrawlLib.Internal;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlInstaller.Common
{
    public static class BitmapExtensions
    {
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
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
    }

    public static class BitmapImageExtensions
    {
        public static void Save(this BitmapImage image, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
    }

    public static class EnumExtensions
    {
        public static KeyValuePair<string, T> GetKeyValuePair<T>(this T obj)
        {
            return new KeyValuePair<string, T>(obj.GetDescription(), obj);
        }

        public static List<KeyValuePair<string, T>> GetKeyValueList<T>(this T obj)
        {
            var keyValueList = new List<KeyValuePair<string, T>>();
            foreach(T item in Enum.GetValues(typeof(T)))
            {
                keyValueList.Add(item.GetKeyValuePair());
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
}
