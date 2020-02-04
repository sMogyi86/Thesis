using System;

namespace StandardClassLibraryTestBL
{
    public interface IRaster
    {
        string ID { get; }
        ReadOnlyMemory<byte> Data { get; }
        int With { get; }
        int Height { get; }
    }
    internal sealed class Raster : IRaster
    {
        public string ID { get; }
        public ReadOnlyMemory<byte> Data { get; }
        public int With { get; }
        public int Height { get; }

        public Raster(string name, byte[] data, int width, int height)
        {
            ID = name;
            Data = data ?? throw new ArgumentNullException(nameof(data));
            With = width;
            Height = height;
        }
    }
}