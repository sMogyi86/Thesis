using Microsoft.Win32;
using System;
using System.Linq;

namespace NetCore31WpfApp
{
    class UIServices
    {
        public string OpenTiff()
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
                ? ofd.FileNames.First()
                : String.Empty;
        }
    }        
}