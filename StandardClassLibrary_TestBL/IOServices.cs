using BitMiracle.LibTiff.Classic;
using System.IO;

namespace StandardClassLibraryTestBL
{
    public interface IIOService
    {
        IRaster Read(string imageName);
    }

    internal class TiffIO : IIOService
    {
        public IRaster Read(string imageName)
        {
            var tiff = Tiff.Open(imageName, "r");

            if (tiff != null)
            {
                int rowSize = tiff.ScanlineSize();
                int rowCount = (int)tiff.GetField(TiffTag.IMAGELENGTH)[0].Value;
                byte[] raster = new byte[rowSize * rowCount];

                var planarConfig = tiff.GetField(TiffTag.PLANARCONFIG)[0].Value;
                if (PlanarConfig.CONTIG != (PlanarConfig)planarConfig)
                    throw new IOException($"Unknown planarConfig! [{planarConfig}]");

                short plane = 0;

                for (int rowCounter = 0; rowCounter < rowCount; rowCounter++)
                {
                    int offset = rowCounter * rowSize;
                    tiff.ReadScanline(raster, offset, rowCounter, plane);
                }

                tiff.Close();

                return new Raster(imageName, raster, rowSize, rowCount);
            }

            throw new IOException($"Can't open image! [{imageName}]");
        }

    }
}