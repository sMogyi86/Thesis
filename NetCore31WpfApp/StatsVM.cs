using MARGO.BL;
using MARGO.MVVM;
using System;
using System.Collections.Generic;
using System.Text;

namespace MARGO
{
    class StatsVM : ObservableBase
    {
        private Project Project => Project.Instance;
    }
}
