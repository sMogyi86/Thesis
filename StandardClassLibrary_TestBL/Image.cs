using System;
using System.IO;

namespace MARGO.BL
{
    public class Image
    {
        public string Name { get; }
        public ImageParts Parts { get; }

        private readonly Func<ImageParts, MemoryStream> myBuilder;
        private readonly Lazy<Stream> myLazy;
        public Stream Stream => myLazy.Value;

        internal Image(string name, ImageParts parts, Func<ImageParts, MemoryStream> builder)
        {
            Name = name;
            Parts = parts;
            myBuilder = builder;
            myLazy = new Lazy<Stream>(() => myBuilder(Parts));
        }

        public override string ToString()
            => Name;
    }
}