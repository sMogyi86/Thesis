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

        //public MemoryStream CreateComposite(IRaster r, IRaster g, IRaster b)
        //{
        //    MemoryStream memoryStream;

        //    //using (var tiff = Tiff.Open(@"D:\Segment\New folder\alma.tiff", "w"))
        //    {
        //        var tiff = Tiff.ClientOpen("in-memory", "w", new MemoryStream(), new TiffStream());

        //        int width = r.With;
        //        int height = r.Height;
        //        int samplePerPixel = 3;

        //        tiff.SetField(TiffTag.IMAGEWIDTH, width);
        //        tiff.SetField(TiffTag.IMAGELENGTH, height);
        //        tiff.SetField(TiffTag.SAMPLESPERPIXEL, samplePerPixel);
        //        tiff.SetField(TiffTag.BITSPERSAMPLE, 8);
        //        tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
        //        tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
        //        tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
        //        tiff.SetField(TiffTag.ROWSPERSTRIP, 1);

        //        int bytesPerRow = tiff.ScanlineSize();
        //        byte[] rowData = new byte[bytesPerRow];
        //        for (int rowIndex = 0; rowIndex < height; rowIndex++)
        //        {
        //            for (int pixelIndex = 0; pixelIndex < width; pixelIndex++)
        //            {
        //                rowData[pixelIndex * samplePerPixel + 0] = r.Data[rowIndex * width + pixelIndex];
        //                rowData[pixelIndex * samplePerPixel + 1] = g.Data[rowIndex * width + pixelIndex];
        //                rowData[pixelIndex * samplePerPixel + 2] = b.Data[rowIndex * width + pixelIndex];
        //            }

        //            if (!tiff.WriteScanline(rowData, rowIndex))
        //                throw new IOException(@"Image data were NOT encoded and written successfully!");
        //        }

        //        if (!tiff.WriteDirectory())
        //            throw new IOException("The current directory was NOT written successfully!");

        //        var v1 = tiff.Flush();

        //        memoryStream = (MemoryStream)tiff.Clientdata();
        //        memoryStream.Seek(0, SeekOrigin.Begin);

        //        //using (FileStream fileStream = new FileStream(@"D:\Segment\New folder\korte.tiff", FileMode.OpenOrCreate, FileAccess.Write))
        //        //{

        //        //    memoryStream.CopyTo(fileStream); // TODO Async

        //        //    fileStream.Flush();
        //        //}

        //        //tiff.Close();
        //    }

        //    return memoryStream;
        //}

        public void Testing()
        {
            ReadOnlySpan<byte> rs;

            ReadOnlyMemory<byte> rm;

            Span<byte> s;


        }
    }
}