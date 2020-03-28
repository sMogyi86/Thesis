using System;

namespace MARGO.BL.Img
{
    public class RasterLayer
    {
        public string ID { get; }
        public ReadOnlyMemory<byte> Memory { get; }
        public int Width { get; }
        public int Height { get; }

        public RasterLayer(string id, ReadOnlyMemory<byte> data, int width, int height)
        {
            ID = id;
            Memory = data;
            Width = width;
            Height = height;
        }

        public override string ToString()
            => ID;
    }
}