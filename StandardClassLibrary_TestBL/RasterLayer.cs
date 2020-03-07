using System;

namespace StandardClassLibraryTestBL
{
    public interface IRasterLayer
    {
        string ID { get; }
        ReadOnlyMemory<byte> Data { get; }
        int Width { get; }
        int Height { get; }
    }
    public class RasterLayer : IRasterLayer
    {
        public string ID { get; }
        public ReadOnlyMemory<byte> Data { get; }
        public int Width { get; }
        public int Height { get; }

        public RasterLayer(string id, byte[] data, int width, int height)
        {
            ID = id;
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Width = width;
            Height = height;
        }
    }
}