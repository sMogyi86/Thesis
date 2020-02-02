using BitMiracle.LibTiff.Classic;
using System;
using System.IO;

namespace StandardClassLibraryTestBL
{
    public interface IComposite
    {
        MemoryStream CreateComposite();
    }

    internal sealed class CompositeTIFF : IComposite, IDisposable
    {
        private const int SAMPLESPERPIXEL = 3;

        private readonly int myWidth, myHeight;
        private readonly byte[] myRed, myGreen, myBlue;

        private Tiff myTiff;

        public CompositeTIFF(int width, int height, byte[] red, byte[] green, byte[] blue)
        {
            myWidth = width;
            myHeight = height;
            myRed = red;
            myGreen = green;
            myBlue = blue;

            Open();
        }

        public void Dispose()
        {
            Close();
        }

        public MemoryStream CreateComposite()
        {
            byte[] rowData = new byte[myTiff.ScanlineSize()];
            for (int rowIndex = 0; rowIndex < myHeight; rowIndex++)
            {
                for (int pixelIndex = 0; pixelIndex < myWidth; pixelIndex++)
                {
                    rowData[pixelIndex * SAMPLESPERPIXEL + 0] = myRed[rowIndex * myWidth + pixelIndex];
                    rowData[pixelIndex * SAMPLESPERPIXEL + 1] = myGreen[rowIndex * myWidth + pixelIndex];
                    rowData[pixelIndex * SAMPLESPERPIXEL + 2] = myBlue[rowIndex * myWidth + pixelIndex];
                }

                if (!myTiff.WriteScanline(rowData, rowIndex))
                    throw new IOException(@"Image data were NOT encoded and written successfully!");
            }

            if (!myTiff.WriteDirectory())
                throw new IOException("The current directory was NOT written successfully!");

            myTiff.Flush();

            return (MemoryStream)myTiff.Clientdata();
        }

        private void Open()
        {
            this.Close();

            myTiff = Tiff.ClientOpen("in-memory RGB composite", "w", new MemoryStream(), new TiffStream());

            myTiff.SetField(TiffTag.IMAGEWIDTH, myWidth);
            myTiff.SetField(TiffTag.IMAGELENGTH, myHeight);
            myTiff.SetField(TiffTag.SAMPLESPERPIXEL, SAMPLESPERPIXEL);
            myTiff.SetField(TiffTag.BITSPERSAMPLE, 8);
            myTiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
            myTiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
            myTiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
            myTiff.SetField(TiffTag.ROWSPERSTRIP, 1);
        }

        private void Close()
        {
            if (myTiff != null)
            {
                myTiff.Close();
                myTiff.Dispose();
            }
        }
    }
}