using System;
using System.Collections.Generic;
using System.Text;

namespace StandardClassLibraryTestBL
{
    public static class Services
    {
        public static IIOService GetIO()
            => new TiffIO();

        public static IComposite GetComposite(int width, int height, byte[] red, byte[] green, byte[] blue)
            => new CompositeTIFF(width, height, red, green, blue);
    }
}
