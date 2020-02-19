using System;
using System.Collections.Generic;
using System.Text;

namespace StandardClassLibraryTestBL
{
    public interface IProcessingFunctions
    {
        //void CalculateVariants(IRaster from, CalculateVariantsParam parameters);
    }

    internal class ProcessingFunctions : IProcessingFunctions
    {
        // TODO mirroring
        public void CalculateVariants(IRaster raster, CalculateVariantsParam parameters, Memory<double> destination)
        {
            int range = parameters.FilterMatrixSize;
            int halfRange = range / 2; // extra rows count
            int extraPixelsCount = halfRange * raster.With;

            int startIdx = parameters.Start - extraPixelsCount;
            if (startIdx < 0)
            {

            }

            int length = parameters.Length + 2 * extraPixelsCount;
            if ((startIdx + length) > raster.Data.Length)
            {

            }

            var offsets = new int[range * range];
            int c = 0;
            for (int rowIdx = -halfRange; rowIdx < halfRange; rowIdx++)
            {
                for (int columnIdx = -halfRange; columnIdx < halfRange; columnIdx++)
                {
                    offsets[c] = rowIdx * raster.With + columnIdx;
                    c++;
                }
            }


            var from = raster.Data.Slice(startIdx, length).Span;
            var to = destination.Slice(startIdx, length).Span;
            for (int i = extraPixelsCount; i < extraPixelsCount + parameters.Length; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < offsets.Length; j++)
                {
                    sum += from[i + offsets[j]];
                }
                double mean = sum / offsets.Length;

                sum = 0.0;
                for (int j = 0; j < offsets.Length; j++)
                {
                    sum += Math.Pow(mean - from[i + offsets[j]], 2);
                }
                double variance = sum / offsets.Length;

                to[i] = variance;
            }
        }
    }

    public class CalculateVariantsParam
    {
        public byte FilterMatrixSize { get; }
        public int Start { get; }
        public int Length { get; }
        public double Weight { get; set; }
    }
}