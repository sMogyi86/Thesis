using BitMiracle.LibTiff.Classic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StandardClassLibraryTestBL
{
    internal sealed class CompositeTIFF : IDisposable
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
        }

        public void Dispose()
        {
            Close();
        }

        public MemoryStream CreateComposite()
        {
            MemoryStream memoryStream;

            int bytesPerRow = myTiff.ScanlineSize();
            byte[] rowData = new byte[bytesPerRow];
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

            memoryStream = (MemoryStream)myTiff.Clientdata();
            //memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        private void Close()
        {
            if (myTiff != null)
            {
                myTiff.Close();
                myTiff.Dispose();
            }
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
    }
}
