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

        }

        public void Testing(IRaster r, IRaster g, IRaster b)
        {
            using (var tiff = Tiff.Open(@"D:\Segment\New folder\alma.tiff", "w"))
            {
                int width = r.With;
                int height = r.Height;
                int samplePerPixel = 3;

                tiff.SetField(TiffTag.IMAGEWIDTH, width);
                tiff.SetField(TiffTag.IMAGELENGTH, height);
                tiff.SetField(TiffTag.SAMPLESPERPIXEL, samplePerPixel);
                tiff.SetField(TiffTag.BITSPERSAMPLE, 8);
                tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
                tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);

                int bytesPerRow = tiff.ScanlineSize();
                tiff.SetField(TiffTag.ROWSPERSTRIP, 1);

                byte[] rowData = new byte[bytesPerRow];
                for (int rowIndex = 0; rowIndex < height; rowIndex++)
                {
                    for (int pixelIndex = 0; pixelIndex < width; pixelIndex++)
                    {
                        rowData[pixelIndex * samplePerPixel + 0] = r.Data[rowIndex * width + pixelIndex];
                        rowData[pixelIndex * samplePerPixel + 1] = g.Data[rowIndex * width + pixelIndex];
                        rowData[pixelIndex * samplePerPixel + 2] = b.Data[rowIndex * width + pixelIndex];
                    }

                    if (!tiff.WriteScanline(rowData, rowIndex))
                        throw new IOException(@"Image data were NOT encoded and written successfully!");
                }

                tiff.Close();



            }

        }
    }
}