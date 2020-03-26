using System;

namespace MARGO.BL.Img
{
    public class RasterLayer
    {
        public string ID { get; }
        public ReadOnlyMemory<byte> Memory { get; }
        public int Width { get; }
        public int Height { get; }

        public RasterLayer(string id, byte[] data, int width, int height)
        {
            ID = id;
            Memory = data ?? throw new ArgumentNullException(nameof(data));
            Width = width;
            Height = height;
        }

        public override string ToString()
            => ID;
    }
}