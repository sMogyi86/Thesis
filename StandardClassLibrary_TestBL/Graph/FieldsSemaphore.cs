using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MARGO.BL.Graph
{
    internal static class FieldsSemaphore
    {
        private static bool[] NODESTAKEN;
        private static Func<int, bool> TRYTAKE;
        static SpinLock SPINLOCK;



        public static void Initialize(int length, bool concurrent)
        {
            NODESTAKEN = new bool[length];

            if (concurrent)
            {
                SPINLOCK = new SpinLock();
                TRYTAKE = idx =>
                {
                    lock (Convert.ToString(idx))
                    {
                        return NODESTAKEN[idx] ? false : (NODESTAKEN[idx] = true);
                    }
                    //bool lockTaken = false;
                    //try
                    //{
                    //    SPINLOCK.Enter(ref lockTaken);
                    //    return NODESTAKEN[idx] ? false : (NODESTAKEN[idx] = true);
                    //}
                    //finally
                    //{
                    //    if (lockTaken)
                    //        SPINLOCK.Exit();
                    //}
                };
            }
            else
            {
                TRYTAKE = idx => NODESTAKEN[idx] ? false : (NODESTAKEN[idx] = true);
            }
        }


        /// <summary>
        /// Synchronously blocks the current thread,
        /// to check if the given <paramref name="idx"/> is taken already by another-one.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        /// 
        public static bool TryTake(int idx)
            => TRYTAKE(idx);
    }
}