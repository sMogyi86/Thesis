using BitMiracle.LibTiff.Classic;
using System.IO;
using System.Threading.Tasks;

namespace MARGO.BL
{
    public interface IIOService
    {
        RasterLayer Load(string ID);
        Task Save(Image image, string ID);
    }

    internal sealed class TiffIO : IIOService
    {
        public RasterLayer Load(string ID)
        {
            using (var tiff = Tiff.Open(ID, "r"))
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
                    
                    return new RasterLayer(Path.GetFileNameWithoutExtension(ID), raster, bytesPerRow, height);
                }
            }

            throw new IOException($"Can't open image! [{ID}]");
        }

        public async Task Save(Image image, string ID)
        {
            using (FileStream fileStream = new FileStream(ID, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                await image.Stream.CopyToAsync(fileStream).ConfigureAwait(false);
                await fileStream.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}