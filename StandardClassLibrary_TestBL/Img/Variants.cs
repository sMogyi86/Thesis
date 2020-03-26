using System;
using System.Collections.Generic;

namespace MARGO.BL.Img
{
    public class Variants<T> where T : struct
    {
        public int Width { get; }
        public int Height { get; }
        internal T[] Data { get; }
        public ReadOnlyMemory<T> Memory { get; }
        public IReadOnlyDictionary<T, int> Stats { get; set; }

        public Variants(int width, int height)
        {
            Width = width > 0 ? width : throw new ArgumentOutOfRangeException(nameof(width));
            Height = height > 0 ? height : throw new ArgumentOutOfRangeException(nameof(height));
            Memory = new ReadOnlyMemory<T>(Data = new T[Width * Height]);
        }
    }
}
