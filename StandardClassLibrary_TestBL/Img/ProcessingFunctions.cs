using MARGO.BL.Graph;
using MARGO.BL.Segment;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MARGO.BL.Img  
{
    public interface IProcessingFunctions
    {
        RasterLayer Cut(RasterLayer raster, int topLextX, int topLeftY, int bottomRightX, int bottomRightY, string id);
        void CalculateVariants(ReadOnlyMemory<byte> source, Memory<int> destination, ReadOnlyMemory<int> offsetsValues, int start, int length); // , double weight
        void PopulateStats<T>(Variants<T> variants) where T : struct;
        void ReclassToByte(Variants<int> source, Variants<byte> destination);
        void ReclassToByteLog(Variants<int> source, Variants<byte> destination);
        void FindMinimas(ReadOnlyMemory<byte> variants, ICollection<int> minimas, ReadOnlyMemory<int> offsetsValues, int start, int length);
        void CreateSampleLayer(IEnumerable<IMST> segments, ReadOnlyMemory<byte> source, Memory<byte> target, SampleType smapleType);
    }

    internal class ProcessingFunctions : IProcessingFunctions
    {
        public RasterLayer Cut(RasterLayer raster, int topLeftX, int topLeftY, int bottomRightX, int bottomRightY, string id)
        {
            int dx = bottomRightX - topLeftX;
            int dy = bottomRightY - topLeftY;
            int newLength = dx * dy;

            var memory = new Memory<byte>(new byte[newLength]);
            var target = memory.Span;

            int i = 0;
            var source = raster.Memory.Span;
            int rowStartPos = topLeftY * raster.Width + topLeftX;
            for (int y = 0; y < dy; y++)
            {
                for (int x = 0; x < dx; x++)
                    target[i++] = source[rowStartPos + x];

                rowStartPos += raster.Width;
            }

            return new RasterLayer(id is null ? $"{raster.ID}_{nameof(Cut)}" : id, memory, dx, dy);
        }

        public void CalculateVariants(ReadOnlyMemory<byte> source, Memory<int> destination, ReadOnlyMemory<int> offsetsValues, int start, int length) // , double weight
        {
            int srcLength = source.Length;
            int filterSize = offsetsValues.Length;

            var offsets = offsetsValues.Span;
            var from = source.Span;
            var to = destination.Span;
            for (int i = start; i < (start + length); i++)
            {
                int sum = 0;
                for (int j = 0; j < filterSize; j++)
                {
                    int k = i + offsets[j];

                    if (k < 0)
                        k += srcLength;

                    if (k >= srcLength)
                        k -= srcLength;

                    sum += from[k];
                }
                int mean = sum / filterSize;

                sum = 0;
                for (int j = 0; j < filterSize; j++)
                {
                    int k = i + offsets[j];

                    if (k < 0)
                        k += srcLength;

                    if (k >= srcLength)
                        k -= srcLength;

                    int d = mean - from[k];
                    sum += (d * d);
                }
                int variance = sum / filterSize;

                to[i] += variance; // * weight;
            }
        }

        public void PopulateStats<T>(Variants<T> variants) where T : struct
        {
            var span = variants.Memory.Span;
            //https://stackoverflow.com/questions/935621/whats-the-difference-between-sortedlist-and-sorteddictionary
            var varsDict = new SortedDictionary<T, int>();
            for (int i = 0; i < span.Length; i++)
            {
                if (varsDict.ContainsKey(span[i]))
                    varsDict[span[i]]++;
                else
                    varsDict[span[i]] = 1;
            }
            variants.Stats = varsDict;
        }

        public void ReclassToByte(Variants<int> source, Variants<byte> destination)
        {
            double ratio = ((double)byte.MaxValue) / source.Data.Max();

            Dictionary<int, byte> mapping = new Dictionary<int, byte>(source.Stats.Count);
            foreach (var integer in source.Stats.Keys)
                mapping[integer] = (byte)Math.Round(ratio * integer, MidpointRounding.AwayFromZero);

            this.Reclass(source, destination, mapping);
        }

        public void ReclassToByteLog(Variants<int> source, Variants<byte> destination)
        {
            double b = Math.Pow(source.Data.Max(), 1.0 / byte.MaxValue);

            var mapping = new Dictionary<int, byte>(source.Stats.Count);
            foreach (var integer in source.Stats.Keys)
                mapping[integer] = (byte)Math.Round(Math.Log(integer, b), MidpointRounding.AwayFromZero);

            this.Reclass(source, destination, mapping);
        }

        private void Reclass(Variants<int> source, Variants<byte> destination, IReadOnlyDictionary<int, byte> mapping)
        {
            for (int i = 0; i < source.Data.Length; i++)
                destination.Data[i] = mapping[source.Data[i]];

            var bytesStats = new Dictionary<byte, int>(source.Stats.Count);
            foreach (var integer in source.Stats.Keys)
                bytesStats[mapping[integer]] = source.Stats[integer];

            destination.Stats = bytesStats;
        }

        public void FindMinimas(ReadOnlyMemory<byte> variants, ICollection<int> minimas, ReadOnlyMemory<int> offsetsValues, int start, int length)
        {
            int srcLength = variants.Length;
            int filterSize = offsetsValues.Length;

            var offsets = offsetsValues.Span;
            var from = variants.Span;

            for (int i = start; i < (start + length); i++)
            {
                int minIdx = i;
                for (int j = 0; j < filterSize; j++)
                {
                    int k = i + offsets[j];

                    if (k < 0)
                        k += srcLength;

                    if (k >= srcLength)
                        k -= srcLength;

                    if (from[k] < from[minIdx])
                    {
                        minIdx = k;
                    }
                    else if (from[k] == from[minIdx])
                    {
                        minIdx = k >= minIdx ? k : minIdx;
                    }
                }

                if (minIdx == i)
                    minimas.Add(minIdx);
            }
        }

        public void CreateSampleLayer(IEnumerable<IMST> segments, ReadOnlyMemory<byte> source, Memory<byte> target, SampleType smapleType)
        {
            var to = target.Span;

            var stats = new SegmentStats(source);
            foreach (var segment in segments)
            {
                stats.Segment = segment.Items;
                byte sample = stats.GetSample(smapleType);
                foreach (var idx in stats.Segment)
                    to[idx] = sample;
            }
        }
    }
}