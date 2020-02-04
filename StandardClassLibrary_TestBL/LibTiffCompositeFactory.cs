using BitMiracle.LibTiff.Classic;
using System;
using System.IO;

namespace StandardClassLibraryTestBL
{
    public interface ICompositeFactory
    {
        MemoryStream CreateComposite(CompositeParts from);
    }

    internal sealed class LibTiffCompositeFactory : ICompositeFactory
    {
        private const int SAMPLESPERPIXEL = 3;

        public MemoryStream CreateComposite(CompositeParts parts)
        {
            var tiff = Tiff.ClientOpen("in-memory RGB composite", "w", new MemoryStream(), new TiffStream());

            if (tiff is null)
                throw new IOException("Can not create composite-stream!");

            try
            {
                tiff.SetField(TiffTag.IMAGEWIDTH, parts.Width);
                tiff.SetField(TiffTag.IMAGELENGTH, parts.Height);
                tiff.SetField(TiffTag.SAMPLESPERPIXEL, SAMPLESPERPIXEL);
                tiff.SetField(TiffTag.BITSPERSAMPLE, 8);
                tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
                tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                tiff.SetField(TiffTag.ROWSPERSTRIP, 1);

                var red = parts.Red.Span;
                var green = parts.Green.Span;
                var blue = parts.Blue.Span;

                byte[] rowData = new byte[tiff.ScanlineSize()];
                for (int rowIndex = 0; rowIndex < parts.Height; rowIndex++)
                {
                    for (int pixelIndex = 0; pixelIndex < parts.Width; pixelIndex++)
                    {
                        rowData[pixelIndex * SAMPLESPERPIXEL + 0] = red[rowIndex * parts.Width + pixelIndex];
                        rowData[pixelIndex * SAMPLESPERPIXEL + 1] = green[rowIndex * parts.Width + pixelIndex];
                        rowData[pixelIndex * SAMPLESPERPIXEL + 2] = blue[rowIndex * parts.Width + pixelIndex];
                    }

                    if (!tiff.WriteScanline(rowData, rowIndex))
                        throw new IOException(@"Image data were NOT encoded and written successfully!");
                }

                //https://stackoverflow.com/questions/23162083/writing-a-multipaged-tif-file-from-memorystream-vs-filestream/23227792#23227792
                tiff.Flush();

                if (!tiff.WriteDirectory())
                    throw new IOException("The current directory was NOT written successfully!");

                return new MemoryStream(((MemoryStream)tiff.Clientdata()).GetBuffer(), false);
            }
            finally
            {
                // https://stackoverflow.com/questions/7274340/how-to-use-bit-miracle-libtiff-net-to-write-the-image-to-a-memorystream/8427942#8427942
                tiff.Close();
                tiff.Dispose();
            }
        }
    }

    public class CompositeParts
    {
        public int Width { get; }
        public int Height { get; }
        public ReadOnlyMemory<byte> Red { get; }
        public ReadOnlyMemory<byte> Green { get; }
        public ReadOnlyMemory<byte> Blue { get; }

        public CompositeParts(int width, int height, ReadOnlyMemory<byte> red, ReadOnlyMemory<byte> green, ReadOnlyMemory<byte> blue)
        {
            Width = width;
            Height = height;
            Red = red;
            Green = green;
            Blue = blue;
        }
    }
}