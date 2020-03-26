using MARGO.BL.Img;

namespace MARGO.BL
{
    public static class Services
    {
        public static IIOService GetIO()
            => new TiffIO();

        public static IImageFactory GetImageFactory()
            => new LibTiffImageFactory();

        public static IImageFunctions GetProcessingFunctions()
            => new ImageFunctions();
    }
}