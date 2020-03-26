using System;
using System.Collections.Generic;

namespace MARGO.BL.Img
{
    internal static class Offsets
    {
        private readonly static IDictionary<int, IDictionary<byte, int[]>> CACHE = new Dictionary<int, IDictionary<byte, int[]>>();

        public static ReadOnlyMemory<int> CalculateOffsetsFor(int width, byte range)
        {
            if (!CACHE.TryGetValue(width, out IDictionary<byte, int[]> offsetsDict))
            {
                offsetsDict = CACHE[width] = new Dictionary<byte, int[]>();
            }

            if (!offsetsDict.TryGetValue(range, out int[] offsets))
            {
                int halfRange = range / 2;

                offsets = new int[range * range];
                int c = 0;
                for (int rowIdx = -halfRange; rowIdx <= halfRange; rowIdx++)
                {
                    for (int columnIdx = -halfRange; columnIdx <= halfRange; columnIdx++)
                    {
                        offsets[c] = rowIdx * width + columnIdx;
                        c++;
                    }
                }

                offsetsDict[range] = offsets;
            }

            return offsets;
        }
    }
}
