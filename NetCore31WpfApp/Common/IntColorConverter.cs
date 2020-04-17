using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MARGO.Common
{
    public class IntColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int argb)
            {
                var bytes = BitConverter.GetBytes(argb);

                return Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
            }

            return Color.FromArgb(byte.MaxValue, byte.MinValue, byte.MinValue, byte.MinValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int argb = int.MinValue;

            if (value is Color color)
            {
                if (BitConverter.IsLittleEndian)
                    argb = BitConverter.ToInt32(stackalloc byte[4] { color.A, color.R, color.G, color.B });
                else
                    argb = BitConverter.ToInt32(stackalloc byte[4] { color.B, color.G, color.R, color.A, });
            }

            return argb;
        }
    }
}
