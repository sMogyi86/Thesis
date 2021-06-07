using MARGO.BL.Img;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MARGO.BL.Segment
{
    public interface ISegmentStats
    {
        //string LayerID { get; }
        IEnumerable<int> Segment { get; set; }
        byte GetSample(SampleType sType);
    }

    class SegmentStats : ISegmentStats
    {
        private const ushort MaxStackLimit = 4 * 1024; // n*K*B(yte) = 4KB :D

        private int count;
        private IEnumerable<int> myIndexes;

        //public string LayerID { get; }
        public IEnumerable<int> Segment
        {
            get { return myIndexes; }
            set
            {
                myIndexes = value;
                count = myIndexes.Count();
            }
        }

        private readonly ReadOnlyMemory<byte> myLayer;
        public SegmentStats(ReadOnlyMemory<byte> layer)
        {
            //LayerID = layerId string layerId,
            myLayer = layer;
        }

        public byte GetSample(SampleType sType)
            => sType switch
            {
                SampleType.Mean => Mean(),
                SampleType.Median => Median(),
                SampleType.Mode => Mode(),
                SampleType.Range => Range(),
                _ => throw new ArgumentException($"Unknown sample type. [{sType}]", nameof(sType)),
            };


        private byte Mean()
        {
            int value = 0;

            var layer = myLayer.Span;
            foreach (var idx in Segment)
                value += layer[idx];

            value /= count;

            return (byte)value;
        }

        private byte Median()
        {
            Span<byte> values = count <= MaxStackLimit ? stackalloc byte[count] : new byte[count];

            var layer = myLayer.Span;
            int i = 0;
            foreach (int idx in Segment)
                values[i++] = layer[idx];

            ShellSort(values);

            return values[count / 2];
        }

        private byte Mode()
        {
            Span<byte> values = count <= MaxStackLimit ? stackalloc byte[count] : new byte[count];

            var layer = myLayer.Span;
            int i = 0;
            foreach (int idx in Segment)
                values[i++] = layer[idx];

            ShellSort(values);

            i = 0;
            byte mode = values[i];
            int maxCount = 0;
            while (i < count)
            {
                byte current = values[i];
                int currCount = 0;

                while (i < count && current == values[i])
                {
                    currCount++;
                    i++;
                }

                if (currCount > maxCount)
                {
                    maxCount = currCount;
                    mode = current;
                }
            }


            return mode;
        }

        private byte Range()
        {
            byte min = byte.MaxValue;
            byte max = byte.MinValue;

            var layer = myLayer.Span;
            foreach (int idx in Segment)
            {
                byte value = layer[idx];

                if (value < min)
                    min = value;

                if (value > max)
                    max = value;
            }

            return (byte)(max - min);
        }

        // requirements: non-recursive, in place
        private void ShellSort(Span<byte> values)
        {
            int dist = values.Length / 2;

            while (dist >= 1)
            {
                for (int i = dist; i < values.Length; i++)
                {
                    int j = i - dist;
                    byte aid = values[i];

                    while (j >= 0 && values[j] > aid)
                    {
                        values[j + dist] = values[j];
                        j -= dist;
                    }

                    values[j + dist] = aid;
                }

                dist--;
            }
        }
    }
}
