using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MARGO.BL
{
    internal class Runner
    {
        public async Task RunAsync(int volume, int sliceCount, Action<int, int> action)
        {
            var lengths = this.Intervals(volume, sliceCount);
            var tasks = new Task[sliceCount];

            int start = 0;
            int i = 0;
            foreach (var l in lengths)
            {
                int sectionStart = start;

                tasks[i++] = Task.Run(() => action(sectionStart, l));

                start += l;
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private IEnumerable<int> Intervals(int num, int count)
        {
            int normalSize = num / count;
            int ext = normalSize + 1;
            int remain = num % count;

            var slices = new int[count];
            int i = 0;
            for (int j = 0; j < remain; j++)
                slices[i++] = ext;

            for (int j = 0; j < (count - remain); j++)
                slices[i++] = normalSize;

            return slices;
        }

        //private IReadOnlyDictionary<int, int> Slice(int num, int count)
        //{
        //    int nomalSize = num / count;
        //    int remain = num % count;

        //    return new Dictionary<int, int>(2)
        //    {
        //        { count-remain, nomalSize},
        //        { remain, nomalSize+1}
        //    };
        //}
    }
}