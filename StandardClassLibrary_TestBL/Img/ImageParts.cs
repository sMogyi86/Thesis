using System;
using System.Collections.Generic;

namespace MARGO.BL.Img
{
    public class ImageParts
    {
        internal enum Plan
        {
            RGB,
            Mono,
            Mapping
        }

        internal Plan Type { get; }
        public int Width { get; }
        public int Height { get; }
        public IReadOnlyDictionary<char, ReadOnlyMemory<byte>> Chanels { get; private set; }
        internal IReadOnlyDictionary<byte, uint> ColorMapping { get; private set; }

        private ImageParts(int width, int height, Plan plan)
        {
            Width = width;
            Height = height;
            Type = plan;
        }

        public ImageParts(int width, int height, ReadOnlyMemory<byte> mono)
            : this(width, height, Plan.Mono)
        {
            Chanels = new Dictionary<char, ReadOnlyMemory<byte>>() { { 'M', mono } };
        }

        public ImageParts(int width, int height, ReadOnlyMemory<byte> red, ReadOnlyMemory<byte> green, ReadOnlyMemory<byte> blue)
            : this(width, height, Plan.RGB)
        {
            Chanels = new Dictionary<char, ReadOnlyMemory<byte>>()
            {
                { 'R', red},
                { 'G', green},
                { 'B', blue}
            };
        }

        public ImageParts(int width, int height, ReadOnlyMemory<byte> chanel, IReadOnlyDictionary<byte, uint> colorMapping)
            : this(width, height, Plan.Mapping)
        {
            Chanels = new Dictionary<char, ReadOnlyMemory<byte>>() { { 'C', chanel } };
            ColorMapping = colorMapping;
        }
    }
}
