using System;
using System.Windows;

namespace MARGO.Common
{
    interface IUIHelper
    {
        Action<Point?> Handle { get; }
        //void TimerElapsedAt(Point? point);
    }
}