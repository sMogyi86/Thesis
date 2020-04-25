using MARGO.BL.Img;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MARGO.Common
{
    public class IntColorConverter : IValueConverter
    {
        private readonly ColorBuilder cb = new ColorBuilder();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is uint argb)
            {
                Color color;
                if (BitConverter.IsLittleEndian)
                    color = Color.FromArgb(cb.AFromLITTLEEndian(argb),
                                       cb.RFromLITTLEEndian(argb),
                                       cb.GFromLITTLEEndian(argb),
                                       cb.BFromLITTLEEndian(argb));
                else
                    color = Color.FromArgb(cb.AFromBIGEndian(argb),
                                       cb.RFromBIGEndian(argb),
                                       cb.GFromBIGEndian(argb),
                                       cb.BFromBIGEndian(argb));
                return color;
            }

            return Color.FromArgb(byte.MaxValue, byte.MinValue, byte.MinValue, byte.MinValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            uint argb = 0;

            if (value is Color color)
            {
                if (BitConverter.IsLittleEndian)
                    argb = cb.ConstructLITTLEEndian(color.A, color.R, color.G, color.B);
                else
                    argb = cb.ConstructBIGEndian(color.A, color.R, color.G, color.B);
            }

            return argb;
        }
    }
}
