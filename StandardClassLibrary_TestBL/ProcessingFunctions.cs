using System;
using System.Collections.Generic;
using System.Text;

namespace StandardClassLibraryTestBL
{
    public interface IProcessingFunctions
    {
        void CalculateVariants(IRaster raster, CalculateVariantsParam parameters, Memory<double> destination);
    }

    internal class ProcessingFunctions : IProcessingFunctions
    {
        // TODO mirroring
        public void CalculateVariants(IRaster raster, CalculateVariantsParam parameters, Memory<double> destination)
        {
            int range = parameters.FilterMatrixSize;
            int extraPixelsCount = (range / 2) * raster.With;

            int startIdx = parameters.Start - extraPixelsCount;
            if (startIdx < 0)
            {

            }

            int length = parameters.Length + 2 * extraPixelsCount;
            if ((startIdx + length) > raster.Data.Length)
            {

            }

            var offsets = Offsets.CalculateOffsetsFor(raster.With, range);

            var from = raster.Data.Slice(startIdx, length).Span;
            var to = destination.Span; // .Slice(startIdx, length)
            for (int i = extraPixelsCount; i < extraPixelsCount + parameters.Length; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < offsets.Count; j++)
                {
                    sum += from[i + offsets[j]];
                }
                double mean = sum / offsets.Count;

                sum = 0.0;
                for (int j = 0; j < offsets.Count; j++)
                {
                    sum += Math.Pow(mean - from[i + offsets[j]], 2);
                }
                double variance = sum / offsets.Count;

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