using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MARGO.BL
{
    internal class Runner
    {
        public async Task RunAsync(int volume, int sliceCount, Action<int, int> action)
        {
            var lengths = this.Lengths(volume, sliceCount);
            var tasks = new Task[sliceCount];

            int start = 0;
            int i = 0;
            foreach (var l in lengths)
            {
                int sectionStart = start;

                tasks[i++] = Task.Run(() => action(sectionStart, l));

                start += l;
            }

            await TryCatchAsync(Task.WhenAll(tasks)).ConfigureAwait(false);

        }

        public async Task ScheduleAsync<T>(IEnumerable<T> volume, int maxCount, Action<T> action)
        {
            int count = volume.Count();
            int i = 0;
            do
            {
                var slice = volume.Skip(i * maxCount).Take(maxCount);
                var tasks = new Task[slice.Count()];

                int j = 0;
                foreach (var data in slice)
                    tasks[j++] = Task.Run(() => action(data));

                await TryCatchAsync(Task.WhenAll(tasks)).ConfigureAwait(false);

                i++;
            } while ((count -= maxCount) > 0);
        }

        public async Task<T[]> PerformAsync<T>(int volume, int sliceCount, Func<int, int, T> func)
        {
            var lengths = this.Lengths(volume, sliceCount);
            var tasks = new Task<T>[sliceCount];

            int start = 0;
            int i = 0;
            foreach (var l in lengths)
            {
                int sectionStart = start;

                tasks[i++] = Task.Run(() => func(sectionStart, l));

                start += l;
            }

            return await TryCatchAsyncT(Task.WhenAll(tasks)).ConfigureAwait(false);
        }

        /// <summary>
        /// Calculates the most equal lengths for cutting the volume into the give number of pieces.
        /// </summary>
        /// <param name="fullLength">the full length of the volume</param>
        /// <param name="pieceCount">the desired number of pieces</param>
        /// <returns></returns>
        private IEnumerable<int> Lengths(int fullLength, int pieceCount)
        {
            int normalSize = fullLength / pieceCount;
            int ext = normalSize + 1;
            int remain = fullLength % pieceCount;

            var lengths = new int[pieceCount];
            int i = 0;
            for (int j = 0; j < remain; j++)
                lengths[i++] = ext;

            for (int j = 0; j < (pieceCount - remain); j++)
                lengths[i++] = normalSize;

            return lengths;
        }

        private async Task<T> TryCatchAsyncT<T>(Task<T> task)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw task.Exception;
            }
        }

        private async Task TryCatchAsync(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw task.Exception;
            }
        }
    }
}