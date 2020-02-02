using BitMiracle.LibTiff.Classic;
using System.Linq;
using System.IO;

namespace StandardClassLibraryTestBL
{
    public interface IIOService
    {
        IRaster Read(string ID);
    }

    internal sealed class TiffIO : IIOService
    {
        public IRaster Read(string imagePath)
        {
            using (var tiff = Tiff.Open(imagePath, "r"))
            {
                if (tiff != null)
                {
                    int bytesPerRow = tiff.ScanlineSize();

                    int height = (int)tiff.GetField(TiffTag.IMAGELENGTH)[0].Value;

                    var planarConfig = tiff.GetField(TiffTag.PLANARCONFIG)[0].Value;
                    if (PlanarConfig.CONTIG != (PlanarConfig)planarConfig)
                        throw new IOException($"Unknown planarConfig! [{planarConfig}]");

                    byte[] raster = new byte[bytesPerRow * height];
                    for (int rowIndex = 0; rowIndex < height; rowIndex++)
                    {
                        int offset = rowIndex * bytesPerRow;

                        if (!tiff.ReadScanline(raster, offset, rowIndex, 0))
                            throw new IOException("Image data were NOT read and decoded successfully!");
                    }

                    tiff.Close();

                    return new Raster(imagePath, raster, bytesPerRow, height);
                }
            }

            throw new IOException($"Can't open image! [{imagePath}]");
        }



    }
}