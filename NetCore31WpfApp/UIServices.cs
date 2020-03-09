using Microsoft.Win32;

namespace NetCore31WpfApp
{
    class UIServices
    {
        public System.Collections.Generic.IEnumerable<string> OpenTiff()
        {
            OpenFileDialog ofd =
                           new OpenFileDialog()
                           {
                               InitialDirectory = @"c:\Users\z0040rwz\Documents\Private\OE NIK\_SzD\DATA\LE07_L1TP_188027_20011220_20170201_01_T2\",
                               Filter = @"GeoTIFF files(*.tif)|*.tif",
                               Multiselect = true,
                           };
            return
                ofd.ShowDialog().Value
                ? ofd.FileNames
                : System.Linq.Enumerable.Empty<string>();
        }
    }
}