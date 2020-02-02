using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StandardClassLibraryTestBL
{
    internal sealed class CompositeTIFF : IDisposable
    {
        private readonly int myWidth, myHeight;
        private readonly byte[] myRed, myGreen, myBlue;

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
        }

        
    }
}
