using BitMiracle.LibTiff.Classic;
using System;
using System.IO;

namespace StandardClassLibraryTestBL
{
    public interface ICompositeStream : IDisposable
    {
        Stream Stream { get; }
    }

    public interface ICompositeFactory
    {
        ICompositeStream CreateComposite(CompositeParts from, Stream to);
    }

    internal sealed class LibTiffCompositeFactory : ICompositeFactory
    {
        private class LibTiffComposite : ICompositeStream
        {
            private readonly Tiff myTiff;

            public Stream Stream { get; }


            public LibTiffComposite(Tiff tiff, Stream stream)
            {
                myTiff = tiff ?? throw new ArgumentNullException(nameof(tiff));
                Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            }

            public void Dispose()
            {
                myTiff.Close();
                myTiff.Dispose();
                // TODO Stream.Dispose(); ???
            }
        }

        private const int SAMPLESPERPIXEL = 3;

        public ICompositeStream CreateComposite(CompositeParts from, Stream to)
        {
            var tiff = Tiff.ClientOpen("in-memory RGB composite", "w", to, new TiffStream());

            if (tiff is null)
                throw new IOException("Can not create composite-stream!");

            try
            {
                tiff.SetField(TiffTag.IMAGEWIDTH, from.Width);
                tiff.SetField(TiffTag.IMAGELENGTH, from.Height);
                tiff.SetField(TiffTag.SAMPLESPERPIXEL, SAMPLESPERPIXEL);
                tiff.SetField(TiffTag.BITSPERSAMPLE, 8);
                tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
                tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                tiff.SetField(TiffTag.ROWSPERSTRIP, 1);

                byte[] rowData = new byte[tiff.ScanlineSize()];
                for (int rowIndex = 0; rowIndex < from.Height; rowIndex++)
                {
                    for (int pixelIndex = 0; pixelIndex < from.Width; pixelIndex++)
                    {
                        rowData[pixelIndex * SAMPLESPERPIXEL + 0] = from.Red.Span[rowIndex * from.Width + pixelIndex];
                        rowData[pixelIndex * SAMPLESPERPIXEL + 1] = from.Green.Span[rowIndex * from.Width + pixelIndex];
                        rowData[pixelIndex * SAMPLESPERPIXEL + 2] = from.Blue.Span[rowIndex * from.Width + pixelIndex];
                    }

                    if (!tiff.WriteScanline(rowData, rowIndex))
                        throw new IOException(@"Image data were NOT encoded and written successfully!");
                }

                if (!tiff.WriteDirectory())
                    throw new IOException("The current directory was NOT written successfully!");

                tiff.Flush();

                return new LibTiffComposite(tiff, (Stream)tiff.Clientdata());
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