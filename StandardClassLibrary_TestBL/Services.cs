namespace StandardClassLibraryTestBL
{
    public static class Services
    {
        public static IIOService GetIO()
            => new TiffIO();

        public static ICompositeFactory GetCompositeFactory()
            => new LibTiffCompositeFactory();

        public static IProcessingFunctions GetProcessingFunctions()
            => new ProcessingFunctions();
    }
}