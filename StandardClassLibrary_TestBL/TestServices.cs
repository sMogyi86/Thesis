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
                int dirCount = 0;

                do
                {
                    tiff.SetupStrips();


                    dirCount++;
                } while (tiff.ReadDirectory());
            }


            ////var offsets = (ulong[])tiff.GetField(TiffTag.STRIPOFFSETS)[0].Value;

            //for (int i = 0; i < tiff.NumberOfStrips(); i++)
            //{
            //    //var offset = offsets[i];

            //    var count = tiff.StripSize();

            //    var buffer = new byte[count];

            //    tiff.ReadEncodedStrip(i, buffer, 0, count);
            //}

            tiff.Close();
        }
    }
}