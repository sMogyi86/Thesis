using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MARGO.Common
{
    public class StringColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte r = byte.MinValue;
            byte g = byte.MinValue;
            byte b = byte.MinValue;
            byte a = byte.MaxValue;

            if (value is string hex)
            {
                if (hex.Length == 7)
                {
                    r = byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
                    g = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
                    b = byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);
                }

                if (hex.Length == 9)
                {
                    a = byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
                    r = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
                    g = byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);
                    b = byte.Parse(hex.Substring(7, 2), NumberStyles.HexNumber);
                }                
            }

            return Color.FromArgb(a, r, g, b);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
                return color.ToString();
            else
                return string.Empty;
        }
    }
}
