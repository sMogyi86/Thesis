using MARGO.BL.Img;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MARGO.BL.Segment
{
    public interface IClassifier
    {
        IReadOnlyDictionary<int, byte[]> CreateCategorySmaples(SampleType sType, IEnumerable<ISampleGroup> samples, IEnumerable<IEnumerable<int>> segments, IEnumerable<ISegmentStats> stats);
        void CreateSample(SampleType sType, IEnumerable<int> segment, IEnumerable<ISegmentStats> stats, Span<byte> sampleVector);
        int Classify(Span<short> segmentSample);
        (IReadOnlyDictionary<int, byte> CategoryMapping, IReadOnlyDictionary<byte, int> ColorMapping) CreateMappings(IEnumerable<int> categories);
    }

    internal abstract class Classifier : IClassifier
    {
        public IReadOnlyDictionary<int, byte[]> CreateCategorySmaples(SampleType sType, IEnumerable<ISampleGroup> samples, IEnumerable<IEnumerable<int>> segments, IEnumerable<ISegmentStats> segmentStats)
        {
            var categorySamples = new Dictionary<int, byte[]>(samples.Count());

            var sampleSegments = SegmentsFromIndexes(samples, segments);

            Span<byte> sampleVector = stackalloc byte[segmentStats.Count()];
            foreach (var sample in sampleSegments)
            {
                CreateSample(sType, sample.Value, segmentStats, sampleVector);
                categorySamples[sample.Key] = sampleVector.ToArray();
            }

            return categorySamples;
        }
        private IReadOnlyDictionary<int, IEnumerable<int>> SegmentsFromIndexes(IEnumerable<ISampleGroup> categorySamples, IEnumerable<IEnumerable<int>> segments)
        {
            var sampleSegments = new Dictionary<int, IEnumerable<int>>(categorySamples.Count());

            foreach (var categorySample in categorySamples)
            {
                sampleSegments[categorySample.ID] = Enumerable.Empty<int>();

                foreach (var index in categorySample.Indexes)
                    sampleSegments[categorySample.ID] = sampleSegments[categorySample.ID].Concat(segments.First(segment => segment.Contains(index)));
            }

            return sampleSegments;
        }

        public void CreateSample(SampleType sType, IEnumerable<int> segment, IEnumerable<ISegmentStats> stats, Span<byte> sampleVector)
        {
            int i = 0;
            foreach (var stat in stats)
            {
                stat.Segment = segment;
                sampleVector[i++] = stat.GetSample(sType);
            }
        }

        public (IReadOnlyDictionary<int, byte> CategoryMapping, IReadOnlyDictionary<byte, int> ColorMapping) CreateMappings(IEnumerable<int> categories)
        {
            int categoriesCount;
            if ((categoriesCount = categories.Count()) > byte.MaxValue + 1)
                throw new NotSupportedException();

            var categoryMapping = new Dictionary<int, byte>(categories.Count());
            var colorMapping = new Dictionary<byte, int>(categories.Count());

            byte d = (byte)(byte.MaxValue / (byte)categoriesCount);
            byte c = 0;
            foreach (var category in categories)
            {
                categoryMapping[category] = c;
                colorMapping[c] = category;
                c += d;
            }

            return (categoryMapping, colorMapping);
        }

        public abstract int Classify(Span<short> segmentSample);
    }


    internal class MinDistClassifier : Classifier
    {
        private static IReadOnlyDictionary<int, Memory<short>> CATEGORY_SAMPLES;
        public static void Initialize(IReadOnlyDictionary<int, byte[]> categorySmaples)
        {
            var csTmp = new Dictionary<int, Memory<short>>(categorySmaples.Count);

            int l = categorySmaples.First().Value.Length;
            Span<short> buffer = stackalloc short[Vector<short>.Count];
            buffer.Fill(0);
            foreach (var kvp in categorySmaples)
            {
                for (int i = 0; i < l; i++)
                    buffer[i] = kvp.Value[i];

                csTmp[kvp.Key] = buffer.ToArray();
            }

            CATEGORY_SAMPLES = csTmp;
        }

        //public MinDistClassifier()
        //{
        //    if (CATEGORY_SAMPLES is null)
        //        throw new InvalidOperationException($"Call static '{nameof(Initialize)}()' first!");
        //}

        public override int Classify(Span<short> segmentSample)
        {
            double minDist = double.MaxValue;
            int categoryID = 0;

            double d;
            foreach (var category in CATEGORY_SAMPLES)
            {
                d = Distance(category.Value.Span, segmentSample);

                if (d < minDist)
                {
                    minDist = d;
                    categoryID = category.Key;
                }
            }

            return categoryID;
        }

        private double Distance(Span<short> category, Span<short> segment)
        {
            Vector<ushort> delta = Vector.AsVectorUInt16(Vector.Subtract(new Vector<short>(category), new Vector<short>(segment))); // a-b
            return Math.Sqrt(Vector.Dot(Vector.Multiply(delta, delta), Vector<ushort>.One)); // Sqrt{Sum[(a-b)ˇ2]}
        }
    }
}