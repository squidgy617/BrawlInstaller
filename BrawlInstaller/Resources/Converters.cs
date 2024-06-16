using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

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
}
