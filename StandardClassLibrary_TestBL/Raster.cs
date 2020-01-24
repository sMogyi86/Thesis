using System;
using System.Collections.Generic;
using System.Text;

namespace StandardClassLibraryTestBL
{
    public interface IRaster
    {
        string Name { get; }
        byte[] Data { get; }
        int With { get; }
        int Height { get; }
    }
    class Raster : IRaster
    {
        public string Name { get; }
        public byte[] Data { get; }
        public int With { get; }
        public int Height { get; }

        public Raster(string name, byte[] data, int width, int height)
        {
            Name = name;
            Data = data;
            With = width;
            Height = height;
        }
    }
}