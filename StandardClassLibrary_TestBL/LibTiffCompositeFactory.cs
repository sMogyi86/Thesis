using BitMiracle.LibTiff.Classic;
using System;
using System.IO;

namespace StandardClassLibraryTestBL
{
    public interface ICompositeStream : IDisposable
    {
        MemoryStream Stream { get; }
    }

    public interface ICompositeFactory
    {
        ICompositeStream CreateComposite(CompositeParts from);
    }

    internal sealed class LibTiffCompositeFactory : ICompositeFactory
    {
        private class LibTiffComposite : ICompositeStream
        {
            private readonly Tiff myTiff;

            public MemoryStream Stream { get; }

            public LibTiffComposite(Tiff tiff, MemoryStream stream)
            {
                myTiff = tiff ?? throw new ArgumentNullException(nameof(tiff));
                Stream = stream ?? throw new ArgumentNullException(nameof(stream));
                Stream.Seek(0, SeekOrigin.Begin);
            }

            public void Dispose()
            {
                myTiff.Close();
                myTiff.Dispose();

                Stream.Close();
                Stream.Dispose();
            }
        }

        private const int SAMPLESPERPIXEL = 3;

        public ICompositeStream CreateComposite(CompositeParts parts)
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

                byte[] rowData = new byte[tiff.ScanlineSize()];
                for (int rowIndex = 0; rowIndex < parts.Height; rowIndex++)
                {
                    for (int pixelIndex = 0; pixelIndex < parts.Width; pixelIndex++)
                    {
                        rowData[pixelIndex * SAMPLESPERPIXEL + 0] = parts.Red[rowIndex * parts.Width + pixelIndex];
                        rowData[pixelIndex * SAMPLESPERPIXEL + 1] = parts.Green[rowIndex * parts.Width + pixelIndex];
                        rowData[pixelIndex * SAMPLESPERPIXEL + 2] = parts.Blue[rowIndex * parts.Width + pixelIndex];
                    }

                    if (!tiff.WriteScanline(rowData, rowIndex))
                        throw new IOException(@"Image data were NOT encoded and written successfully!");
                }

                if (!tiff.WriteDirectory())
                    throw new IOException("The current directory was NOT written successfully!");

                tiff.Flush();

                return new LibTiffComposite(tiff, (MemoryStream)tiff.Clientdata());
            }
            catch
            {
                tiff.Close();
                tiff.Dispose();

                throw;
            }
        }
    }

    public class CompositeParts
    {
        public int Width { get; }
        public int Height { get; }
        public byte[] Red { get; }
        public byte[] Green { get; }
        public byte[] Blue { get; }

        public CompositeParts(int width, int height, byte[] red, byte[] green, byte[] blue)
        {
            Width = width;
            Height = height;
            Red = red ?? throw new ArgumentNullException(nameof(red));
            Green = green ?? throw new ArgumentNullException(nameof(green));
            Blue = blue ?? throw new ArgumentNullException(nameof(blue));
        }
    }
}