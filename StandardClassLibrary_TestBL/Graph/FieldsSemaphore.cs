using System;
using System.Threading;

namespace MARGO.BL.Graph
{
    internal sealed class FieldsSemaphore : IDisposable
    {
        private readonly SemaphoreSlim mySemaphoreSlim = new SemaphoreSlim(1);
        private readonly bool[] myNodesTaken;



        public FieldsSemaphore(int length) { myNodesTaken = new bool[length]; }



        /// <summary>
        /// Synchronously blocks the current thread,
        /// to check if the given <paramref name="idx"/> is taken already by another-one.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public bool TryTake(int idx)
        {
            mySemaphoreSlim.Wait();
            try
            {
                return myNodesTaken[idx] ? false : (myNodesTaken[idx] = true);
            }
            finally
            {
                mySemaphoreSlim.Release();
            }
        }

        public void Dispose()
            => mySemaphoreSlim.Dispose();
    }
}