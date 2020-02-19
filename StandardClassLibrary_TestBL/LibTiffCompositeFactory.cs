using BitMiracle.LibTiff.Classic;
using System;
using System.IO;

namespace StandardClassLibraryTestBL
{
    public interface ICompositeFactory
    {
        MemoryStream CreateTiff(TiffParts from);
    }

    internal sealed class LibTiffCompositeFactory : ICompositeFactory
    {
        public MemoryStream CreateTiff(TiffParts parts)
        {
            var tiff = Tiff.ClientOpen("in-memory RGB composite", "w", new MemoryStream(), new TiffStream());

            if (tiff is null)
                throw new IOException("Can not create composite-stream!");

            try
            {
                int samplesPerPixel = parts.IsMono ? 1 : 3;
                tiff.SetField(TiffTag.IMAGEWIDTH, parts.Width);
                tiff.SetField(TiffTag.IMAGELENGTH, parts.Height);
                tiff.SetField(TiffTag.SAMPLESPERPIXEL, samplesPerPixel);
                tiff.SetField(TiffTag.BITSPERSAMPLE, 8);
                tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
                tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                tiff.SetField(TiffTag.PHOTOMETRIC, parts.IsMono ? Photometric.MINISBLACK : Photometric.RGB);
                tiff.SetField(TiffTag.ROWSPERSTRIP, 1);

                if (parts.IsMono)
                    this.WriteMono(parts, tiff);
                else
                    this.WriteRGB(parts, tiff);

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

        private void WriteMono(TiffParts parts, Tiff tiff)
        {
            var mono = parts.Mono.Span;

            byte[] rowData = new byte[tiff.ScanlineSize()];
            for (int rowIndex = 0; rowIndex < parts.Height; rowIndex++)
            {
                for (int pixelIndex = 0; pixelIndex < parts.Width; pixelIndex++)
                    rowData[pixelIndex] = mono[rowIndex * parts.Width + pixelIndex];

                if (!tiff.WriteScanline(rowData, rowIndex))
                    throw new IOException(@"Image data were NOT encoded and written successfully!");
            }
        }

        private void WriteRGB(TiffParts parts, Tiff tiff)
        {
            int samplesPerPixel = 3;

            var red = parts.Red.Span;
            var green = parts.Green.Span;
            var blue = parts.Blue.Span;

            byte[] rowData = new byte[tiff.ScanlineSize()];
            for (int rowIndex = 0; rowIndex < parts.Height; rowIndex++)
            {
                for (int pixelIndex = 0; pixelIndex < parts.Width; pixelIndex++)
                {
                    rowData[pixelIndex * samplesPerPixel + 0] = red[rowIndex * parts.Width + pixelIndex];
                    rowData[pixelIndex * samplesPerPixel + 1] = green[rowIndex * parts.Width + pixelIndex];
                    rowData[pixelIndex * samplesPerPixel + 2] = blue[rowIndex * parts.Width + pixelIndex];
                }

                if (!tiff.WriteScanline(rowData, rowIndex))
                    throw new IOException(@"Image data were NOT encoded and written successfully!");
            }
        }
    }

    public class TiffParts
    {
        public bool IsMono { get; }
        public int Width { get; }
        public int Height { get; }
        public ReadOnlyMemory<byte> Red { get; }
        public ReadOnlyMemory<byte> Green { get; }
        public ReadOnlyMemory<byte> Blue { get; }
        public ReadOnlyMemory<byte> Mono { get; }

        private TiffParts(int width, int height, bool isMono)
        {
            Width = width;
            Height = height;
            IsMono = isMono;
        }

        public TiffParts(int width, int height, ReadOnlyMemory<byte> mono)
            : this(width, height, true)
        {
            Mono = mono;
        }

        public TiffParts(int width, int height, ReadOnlyMemory<byte> red, ReadOnlyMemory<byte> green, ReadOnlyMemory<byte> blue)
            : this(width, height, false)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }
    }
}