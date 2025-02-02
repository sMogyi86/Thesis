﻿using MARGO.BL.Img;

namespace MARGO.BL
{
    public static class MyServices
    {
        public static IIOService GetIO()
            => new TiffIO();

        public static IImageFactory GetImageFactory()
            => new LibTiffImageFactory();

        public static IProcessingFunctions GetProcessingFunctions()
            => new ProcessingFunctions();
    }
}