using BitMiracle.LibTiff.Classic;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;

namespace StandardClassLibrary_TestBL
{
    public class TestServices
    {
        public byte[] GetImageAsByteArray(string imageName)
        {
            byte[] image = { };

            using (Stream stream = new FileStream(imageName, FileMode.Open, FileAccess.Read))
            {
                var length = stream.Length;
                image = new byte[length];
                stream.Read(image, 0, (int)length);
            }

            return image;
        }

        public IReadOnlyDictionary<TiffTag, FieldValue[]> GetTagValues(string imageName)
        {
            var tiff = Tiff.Open(imageName, "r");

            Dictionary<TiffTag, FieldValue[]> tiffTagValues = new Dictionary<TiffTag, FieldValue[]>();

            foreach (var tiffTag in TiffTag.GetValues(typeof(TiffTag)).Cast<TiffTag>())
            {
                tiffTagValues[tiffTag] = tiff.GetField(tiffTag);
            }

            tiff.Close();

            return tiffTagValues;
        }

        // http://www.simplesystems.org/libtiff/libtiff.html#fio
        public void Testing(string imageName)
        {
            var tiff = Tiff.Open(imageName, "r");

            if (tiff != null)
            {
                //var offsets = (ulong[])tiff.GetField(TiffTag.STRIPOFFSETS)[0].Value;

                int dirCount = 0;

                do
                {
                    dirCount++;

                    int imagelength = (int)tiff.GetField(TiffTag.IMAGELENGTH)[0].Value;

                    int scanlineSize = tiff.ScanlineSize();

                    byte[] buf = new byte[scanlineSize];

                    for (int rowCounter = 0; rowCounter < imagelength; rowCounter++)
                    {
                        tiff.ReadScanline(buf, rowCounter);

                        if (buf.Any(b => b != 0))
                        {

                        }
                    }


                    //tiff.SetupStrips();

                    //tiff.ReadScanline()

                    //for (int i = 0; i < tiff.NumberOfStrips(); i++)
                    //{
                    //    var offset = offsets[i];

                    //    var count = tiff.StripSize();

                    //    var buffer = new byte[count];

                    //    tiff.ReadEncodedStrip(i, buffer, offset, count);
                    //}
                } while (tiff.ReadDirectory());
            }





            tiff.Close();
        }
    }
}