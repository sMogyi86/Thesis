using System;
using System.Collections.Generic;
using System.Text;

namespace StandardClassLibraryTestBL
{
    public class Variants<T> where T : struct
    {
        internal T[] Data { get; }
        public ReadOnlyMemory<T> Memory { get; }
        public IReadOnlyDictionary<T, int> Stats { get; set; }

        public Variants(int length)
        {
            Memory = new ReadOnlyMemory<T>(Data = new T[length]);
        }
    }
}
