using System;

namespace StandardClassLibraryTestBL
{
    public interface IRasterLayer
    {
        string ID { get; }
        ReadOnlyMemory<byte> Data { get; }
        int With { get; }
        int Height { get; }
    }
    public class RasterLayer : IRasterLayer
    {
        public string ID { get; }
        public ReadOnlyMemory<byte> Data { get; }
        public int With { get; }
        public int Height { get; }

        public RasterLayer(string name, byte[] data, int width, int height)
        {
            ID = name;
            Data = data ?? throw new ArgumentNullException(nameof(data));
            With = width;
            Height = height;
        }
    }
}