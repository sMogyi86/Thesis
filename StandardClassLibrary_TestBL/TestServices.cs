using BitMiracle.LibTiff.Classic;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace StandardClassLibraryTestBL
{
    public class TestServices
    {
        IIOService iOService = new TiffIO();

        public IReadOnlyDictionary<TiffTag, FieldValue[]> GetTagValues(string imageName)
        {
            var tiff = Tiff.Open(imageName, "r");

            Dictionary<TiffTag, FieldValue[]> tiffTagValues = new Dictionary<TiffTag, FieldValue[]>();

            foreach (TiffTag tiffTag in System.Enum.GetValues(typeof(TiffTag)).Cast<TiffTag>())
            {
                tiffTagValues[tiffTag] = tiff.GetField(tiffTag);
            }

            tiff.Close();

            return tiffTagValues;
        }

        // http://www.simplesystems.org/libtiff/libtiff.html#fio
        public void Testing(string imageName)
        {
            //https://stackoverflow.com/questions/20181132/edit-raw-pixel-data-of-writeablebitmap
            //var raster = iOService.Read(imageName);

            //https://stackoverflow.com/questions/6024172/is-it-possible-to-intercept-console-output

            var tiff = Tiff.Open(imageName, "w");

            TiffRgbaImage tiffRgbaImage = TiffRgbaImage.Create(tiff, true, out var msg);

            // https://stackoverflow.com/questions/19219651/c-sharp-can-i-use-libtiff-to-output-tiff-encoded-jpeg-in-ycbcr

            // https://stackoverflow.com/questions/48884728/libtiff-reading-and-writing-rgba-image-in-c

            tiff
        }

    }
}