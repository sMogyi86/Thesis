using System.Linq;
using System;
using MARGO.BL.Segment;
using System.Collections.Generic;
using System.Windows;

namespace MARGO.ViewModels
{
    public class SampleGroupVM : ObservableBase, ISampleGroup
    {
        private readonly ICollection<Point> myPoints = new List<Point>();
        private readonly Func<Point, int> myCalcIndex;

        public string Name { get; }
        public int Color { get; }
        public int Count => myPoints.Count;

        public int ID => Color;
        public IEnumerable<int> Indexes => myPoints.Select(p => myCalcIndex(p));

        public SampleGroupVM(string name, int color, Func<Point, int> calcIndex)
        {
            Name = name;
            Color = color;
            myCalcIndex = calcIndex;
        }

        internal SampleGroupVM(string name, int color, Func<Point, int> calcIndex, IEnumerable<Point> points)
            : this(name, color, calcIndex)
        {
            foreach (var point in points)
                myPoints.Add(point);
        }

        public void AddPoint(Point? point)
        {
            if (point.HasValue && !myPoints.Contains(point.Value))
            {
                myPoints.Add(point.Value);
                this.RaisePropertyChanged(nameof(Count));
            }
        }
    }
}