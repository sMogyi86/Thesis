using System;
using System.Collections.Generic;
using System.Text;

namespace StandardClassLibraryTestBL
{
    public static class Services
    {
        public static IIOService GetIO()
            => new TiffIO();

        public static ICompositeFactory GetCompositeFactory()
            => new LibTiffCompositeFactory();
    }
}
