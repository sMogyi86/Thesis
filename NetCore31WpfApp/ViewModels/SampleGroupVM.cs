﻿using System.Linq;
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
        public uint Color { get; }
        public int Count => myPoints.Count;

        public uint ID => Color;
        public IEnumerable<int> Indexes => myPoints.Select(p => myCalcIndex(p));

        public SampleGroupVM(string name, uint color, Func<Point, int> calcIndex)
        {
            Name = name;
            Color = color;
            myCalcIndex = calcIndex;
        }

        internal SampleGroupVM(string name, uint color, Func<Point, int> calcIndex, IEnumerable<Point> points)
            : this(name, color, calcIndex)
        {
            foreach (var point in points)
                myPoints.Add(point);
        }

        internal SampleGroupVM EmptyCopy() => new SampleGroupVM(Name, Color, myCalcIndex);

        public void AddPoint(Point? point)
        {
            if (point.HasValue && !myPoints.Contains(point.Value))
            {
                myPoints.Add(point.Value);
                this.RaisePropertyChanged(nameof(Count));
            }
        }

        public void Clear()
            => myPoints.Clear();
    }
}