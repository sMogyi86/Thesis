using System;

namespace MARGO.BL
{
    public class ImageParts
    {
        public bool IsMono { get; }
        public int Width { get; }
        public int Height { get; }
        public ReadOnlyMemory<byte> Red { get; }
        public ReadOnlyMemory<byte> Green { get; }
        public ReadOnlyMemory<byte> Blue { get; }
        public ReadOnlyMemory<byte> Mono { get; }

        private ImageParts(int width, int height, bool isMono)
        {
            Width = width;
            Height = height;
            IsMono = isMono;
        }

        public ImageParts(int width, int height, ReadOnlyMemory<byte> mono)
            : this(width, height, true)
        {
            Mono = mono;
        }

        public ImageParts(int width, int height, ReadOnlyMemory<byte> red, ReadOnlyMemory<byte> green, ReadOnlyMemory<byte> blue)
            : this(width, height, false)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }
    }
}
