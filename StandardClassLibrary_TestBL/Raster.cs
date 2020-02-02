using System;
using System.Collections.Generic;
using System.Text;

namespace StandardClassLibraryTestBL
{
    public interface IRaster
    {
        string ID { get; }
        byte[] Data { get; }
        int With { get; }
        int Height { get; }
    }
    internal sealed class Raster : IRaster
    {
        public string ID { get; }
        public byte[] Data { get; }
        public int With { get; }
        public int Height { get; }

        public Raster(string name, byte[] data, int width, int height)
        {
            ID = name;
            Data = data;
            With = width;
            Height = height;
        }
    }
}