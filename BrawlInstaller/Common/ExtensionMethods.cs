using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using BrawlLib.Internal;

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
}
