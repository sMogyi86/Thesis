using System;
using System.Collections.Generic;
using System.Windows;

namespace MARGO.Common
{
    interface IUIHelper
    {
        Action<Point?> Handle { get; }
        //void TimerElapsedAt(Point? point);
    }

    interface IHaveScript
    {
        Step LastStep { get; set; }
    }
}