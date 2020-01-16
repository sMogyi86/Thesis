using BitMiracle.LibTiff.Classic;
using System.Collections.Generic;
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

            return tiffTagValues;
        }
    }
}