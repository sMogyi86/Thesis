using System;

namespace StandardClassLibraryTestBL
{
    public interface IProcessingFunctions
    {
        //void CalculateVariants(ReadOnlyMemory<byte> source, Memory<int> destination, ReadOnlyMemory<int> offsetsValues, int start, int length); // , double weight
        void CalculateVariants(ReadOnlyMemory<byte> source, Memory<int> destination, ReadOnlyMemory<int> offsetsValues); // , double weight
        void ReclassToByte(ReadOnlyMemory<int> source, Memory<byte> destination, double ratio);
        //void ReclassToRGB(Memory<int> variants, double ratio);
        //void SplitToRGB(ReadOnlyMemory<int> source, Memory<byte> red, Memory<byte> green, Memory<byte> blue);
        IRasterLayer Cut(IRasterLayer raster, int topLextX, int topLeftY, int bottomRightX, int bottomRightY, string id = null);
    }

    internal class ProcessingFunctions : IProcessingFunctions
    {
        public void CalculateVariants(ReadOnlyMemory<byte> source, Memory<int> destination, ReadOnlyMemory<int> offsetsValues) // , double weight
        {
            int length = source.Length;
            int filterSize = offsetsValues.Length;

            var offsets = offsetsValues.Span;
            var from = source.Span;
            var to = destination.Span;
            for (int i = 0; i < length; i++)
            {
                int sum = 0;
                for (int j = 0; j < filterSize; j++)
                {
                    int k = i + offsets[j];

                    if (k < 0)
                        k += length;

                    if (k >= length)
                        k -= length;

                    sum += from[k];
                }
                int mean = sum / filterSize;

                sum = 0;
                for (int j = 0; j < filterSize; j++)
                {
                    int k = i + offsets[j];

                    if (k < 0)
                        k += length;

                    if (k >= length)
                        k -= length;

                    int d = mean - from[k];
                    sum += (d * d);
                }
                int variance = sum / filterSize;

                to[i] += variance; // * weight;
            }
        }

        public IRasterLayer Cut(IRasterLayer raster, int topLeftX, int topLeftY, int bottomRightX, int bottomRightY, string id = null)
        {
            int dx = bottomRightX - topLeftX;
            int dy = bottomRightY - topLeftY;
            int newLength = dx * dy;

            var buffer = new byte[newLength];
            var memory = new Memory<byte>(buffer);
            var target = memory.Span;

            int i = 0;
            var source = raster.Data.Span;
            int rowStartPos = topLeftY * raster.Width + topLeftX;
            for (int y = 0; y < dy; y++)
            {
                for (int x = 0; x < dx; x++)
                {
                    target[i++] = source[rowStartPos + x];
                }

                rowStartPos += raster.Width;
            }

            return new RasterLayer(id is null ? $"{raster.ID}_{nameof(Cut)}" : id, buffer, dx, dy);
        }

        public void ReclassToByte(ReadOnlyMemory<int> source, Memory<byte> destination, double ratio)
        {
            var bytes = destination.Span;
            var variants = source.Span;
            for (int i = 0; i < variants.Length; i++)
                bytes[i] = (byte)Math.Round(variants[i] * ratio, MidpointRounding.AwayFromZero);
        }

        //public void ReclassToRGB(Memory<int> variants, double ratio)
        //{
        //    var vars = variants.Span;
        //    for (int i = 0; i < vars.Length; i++)
        //    {
        //        vars[i] = (int)Math.Round(vars[i] * ratio, MidpointRounding.AwayFromZero);
        //    }
        //}

        //public void SplitToRGB(ReadOnlyMemory<int> source, Memory<byte> red, Memory<byte> green, Memory<byte> blue)
        //{
        //    var src = source.Span;
        //    var r = red.Span;
        //    var g = green.Span;
        //    var b = blue.Span;

        //    for (int i = 0; i < src.Length; i++)
        //    {
        //        r[i] = (byte)(src[i] >> 16);
        //        g[i] = (byte)(src[i] >> 8);
        //        b[i] = (byte)src[i];
        //    }
        //}
    }
}