using MARGO.BL.Img;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MARGO.BL.Segment
{
    public interface IClassifier
    {
        IReadOnlyDictionary<uint, byte[]> CreateCategorySamples(SampleType sType, IEnumerable<ISampleGroup> samples, IEnumerable<IEnumerable<int>> segments, IEnumerable<ISegmentStats> stats);
        void CreateSample(SampleType sType, IEnumerable<int> segment, IEnumerable<ISegmentStats> stats, Span<byte> sampleVector);
        uint Classify(Span<short> segmentSample);
        (IReadOnlyDictionary<uint, byte> CategoryMapping, IReadOnlyDictionary<byte, uint> ColorMapping) CreateMappings(IEnumerable<uint> categories);
    }

    internal abstract class Classifier : IClassifier
    {
        public IReadOnlyDictionary<uint, byte[]> CreateCategorySamples(SampleType sType, IEnumerable<ISampleGroup> samples, IEnumerable<IEnumerable<int>> segments, IEnumerable<ISegmentStats> segmentStats)
        {
            var categorySamples = new Dictionary<uint, byte[]>(samples.Count());

            var sampleSegments = SegmentsFromIndexes(samples, segments);

            Span<byte> sampleVector = stackalloc byte[segmentStats.Count()];
            foreach (var sample in sampleSegments)
            {
                CreateSample(sType, sample.Value, segmentStats, sampleVector);
                categorySamples[sample.Key] = sampleVector.ToArray();
            }

            return categorySamples;
        }
        private IReadOnlyDictionary<uint, IEnumerable<int>> SegmentsFromIndexes(IEnumerable<ISampleGroup> categorySamples, IEnumerable<IEnumerable<int>> segments)
        {
            var sampleSegments = new Dictionary<uint, IEnumerable<int>>(categorySamples.Count());

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

        public (IReadOnlyDictionary<uint, byte> CategoryMapping, IReadOnlyDictionary<byte, uint> ColorMapping) CreateMappings(IEnumerable<uint> categories)
        {
            int categoriesCount;
            if ((categoriesCount = categories.Count()) > byte.MaxValue)
                throw new NotSupportedException();

            var categoryMapping = new Dictionary<uint, byte>(categories.Count());
            var colorMapping = new Dictionary<byte, uint>(categories.Count());

            //byte d = (byte)(byte.MaxValue / (byte)categoriesCount);
            byte c = 0;
            foreach (var category in categories)
            {
                categoryMapping[category] = c;
                colorMapping[c] = category;
                c++;
            }

            return (categoryMapping, colorMapping);
        }

        public abstract uint Classify(Span<short> segmentSample);
    }


    internal class MinDistClassifier : Classifier
    {
        private static IReadOnlyDictionary<uint, Memory<short>> CATEGORY_SAMPLES;
        public static void Initialize(IReadOnlyDictionary<uint, byte[]> categorySmaples)
        {
            var csTmp = new Dictionary<uint, Memory<short>>(categorySmaples.Count);

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

        public override uint Classify(Span<short> segmentSample)
        {
            double minDist = double.MaxValue;
            uint categoryID = 0;

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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double Distance(Span<short> category, Span<short> segment)
        {
            Vector<ushort> delta = Vector.AsVectorUInt16(Vector.Subtract(new Vector<short>(category), new Vector<short>(segment))); // a-b
            return Math.Sqrt(Vector.Dot(Vector.Multiply(delta, delta), Vector<ushort>.One)); // Sqrt{Sum[(a-b)ˇ2]}
        }
    }
}