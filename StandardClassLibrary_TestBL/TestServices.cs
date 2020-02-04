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
        

        public void Testing()
        {



        }
    }
}