using BrawlInstaller.Enums;
using BrawlLib.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BrawlInstaller.Resources
{
    [ValueConversion(typeof(int), typeof(byte))]
    public class ByteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (byte)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class FilePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultur)
        {
            if (value != null)
                return Path.GetFileName((string)value);
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultur)
        {
            if (value != null)
                return (string)value;
            return null;
        }
    }

    [ValueConversion(typeof(string), typeof(int))]
    public class HexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return $"0x{(int)value:X2}";
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int) return value;
            if (value == null || (string)value == "")
            {
                return null;
            }
            var parsed = int.TryParse(((string)value), NumberStyles.Integer, null, out int result);
            if (parsed) return result;
            parsed = int.TryParse(((string)value).Replace("0x", ""), NumberStyles.HexNumber, null, out result);
            if (parsed) return result;
            else return null;
        }
    }

    [ValueConversion(typeof(string), typeof(ushort))]
    public class HexShortConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return $"0x{(ushort)value:X4}";
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ushort) return value;
            if (value == null || (string)value == "")
            {
                return null;
            }
            var parsed = ushort.TryParse(((string)value), NumberStyles.Integer, null, out ushort result);
            if (parsed) return result;
            parsed = ushort.TryParse(((string)value).Replace("0x", ""), NumberStyles.HexNumber, null, out result);
            if (parsed) return result;
            else return null;
        }
    }

    [ValueConversion(typeof(string), typeof(uint))]
    public class HexUIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return $"0x{(uint)value:X8}";
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is uint) return value;
            if (value == null || (string)value == "")
            {
                return null;
            }
            var parsed = uint.TryParse(((string)value), NumberStyles.Integer, null, out uint result);
            if (parsed) return result;
            parsed = uint.TryParse(((string)value).Replace("0x", ""), NumberStyles.HexNumber, null, out result);
            if (parsed) return result;
            else return null;
        }
    }

    [ValueConversion(typeof(object), typeof(bool))]
    public class  NullBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == false)
            {
                return null;
            }
            return true;
        }
    }

    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Hidden;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Hidden)
            {
                return null;
            }
            return true;
        }
    }

    [ValueConversion(typeof(uint), typeof(GameCubeButtons))]
    public class GameCubeButtonConverter : IValueConverter
    {
        private GameCubeButtons target;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GameCubeButtons mask = (GameCubeButtons)parameter;
            this.target = (GameCubeButtons)value;
            return ((mask & this.target) != 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            this.target ^= (GameCubeButtons)parameter;
            return (uint)this.target;
        }
    }

    [ValueConversion(typeof(Color), typeof(RGBAPixel))]
    public class ColorRGBAPixelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parsed = (RGBAPixel)value;
            var color = new Color
            {
                R = parsed.R,
                G = parsed.G,
                B = parsed.B,
                A = parsed.A
            };
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parsed = (Color)value;
            var color = new RGBAPixel
            {
                R = parsed.R,
                G = parsed.G,
                B = parsed.B,
                A = parsed.A
            };
            return color;
        }
    }
}
