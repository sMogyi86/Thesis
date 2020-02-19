using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StandardClassLibraryTestBL
{
    internal static class Offsets
    {
        private readonly static IDictionary<int, IDictionary<int, int[]>> CACHE = new Dictionary<int, IDictionary<int, int[]>>();

        public static ReadOnlyCollection<int> CalculateOffsetsFor(int width, int range)
        {
            if (!CACHE.TryGetValue(width, out IDictionary<int, int[]> offsetsDict))
            {
                offsetsDict = CACHE[width] = new Dictionary<int, int[]>();
            }

            if (!offsetsDict.TryGetValue(range, out int[] offsets))
            {
                int halfRange = range / 2;

                offsets = new int[range * range];
                int c = 0;
                for (int rowIdx = -halfRange; rowIdx < halfRange; rowIdx++)
                {
                    for (int columnIdx = -halfRange; columnIdx < halfRange; columnIdx++)
                    {
                        offsets[c] = rowIdx * width + columnIdx;
                        c++;
                    }
                }

                offsetsDict[range] = offsets;
            }

            return new ReadOnlyCollection<int>(offsets);
        }
    }
}
