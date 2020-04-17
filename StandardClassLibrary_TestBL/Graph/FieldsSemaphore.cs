using System.Runtime.CompilerServices;

namespace MARGO.BL.Graph
{
    internal static class FieldsSemaphore
    {
        private static bool[] NODESTAKEN;



        public static void Initialize(int length) { NODESTAKEN = new bool[length]; }



        /// <summary>
        /// Synchronously blocks the current thread,
        /// to check if the given <paramref name="idx"/> is taken already by another-one.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        /// 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryTake(int idx)
        {
            lock (idx.ToString())
            {
                return NODESTAKEN[idx] ? false : (NODESTAKEN[idx] = true);
            }
        }
    }
}