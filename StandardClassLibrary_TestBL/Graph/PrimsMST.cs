using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MARGO.BL.Graph
{
    public interface IMST
    {
        bool Terminated { get; }
        int SumValue { get; }
        IEnumerable<int> Items { get; }
        void DoStep();
    }

    internal class PrimsMST : IMST
    {
        private readonly List<int[]> myReachables = new List<int[]>();
        private readonly List<int> myItems = new List<int>();
        private readonly ReadOnlyMemory<byte> myValueField;
        private bool myCanStep = true;
        private int myLastCoupledIdx;


        public bool Terminated => !myCanStep;
        public int SumValue { get; private set; } = 0;
        public IEnumerable<int> Items => myItems;



        private static readonly int[] OFFSETS = new int[4];
        private static bool ISPARALLEL;
        private static bool INITIALIZED = false;
        public static void Initalize(int dataWidth, bool isParallel)
        {
            if (!INITIALIZED)
            {
                OFFSETS[0] = 1;
                OFFSETS[1] = dataWidth;
                OFFSETS[2] = -1;
                OFFSETS[3] = -dataWidth;

                ISPARALLEL = isParallel;

                INITIALIZED = true;
            }
        }

        public PrimsMST(int rootIdx, ReadOnlyMemory<byte> valueField)
        {
            if (!INITIALIZED)
                throw new InvalidOperationException($"Call the static '{nameof(Initalize)}()' first!");

            myLastCoupledIdx = rootIdx;
            myValueField = valueField;
            myItems.Add(myLastCoupledIdx);
        }



        public void DoStep()
        {
            if (myCanStep)
            {
                DiscoverReachables();
                if (myCanStep = AnyReachable())
                {
                    SortReachables();

                    do
                    {
                        CleanReachablesFrom(TryPickOne());
                    } while (!WasSuccessfulPick() && AnyReachable());

                    myCanStep = WasSuccessfulPick();
                }
            }
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DiscoverReachables()
        {
            int[] Dist(int currentIdx, ReadOnlySpan<byte> values, int otherOffset)
            {
                int otherIdx = currentIdx + otherOffset;

                if (otherIdx >= values.Length)
                    otherIdx -= values.Length;

                if (otherIdx < 0)
                    otherIdx += values.Length;

                int d = values[otherIdx] - values[currentIdx];
                return new int[2] { (d < 0 ? -d : d), otherIdx };
            }

            var span = myValueField.Span;

            foreach (var offset in OFFSETS)
                myReachables.Add(Dist(myLastCoupledIdx, span, offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SortReachables()
            => myReachables.Sort(
                    (pairA, pairB) =>
                        pairA[0]
                        .CompareTo(pairB[0])
                );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int TryPickOne()
        {
            var picked = myReachables.First();
            int pIdx = picked[1];

            if (ISPARALLEL && FieldsSemaphore.TryTake(pIdx))
            {
                myLastCoupledIdx = pIdx;
                myItems.Add(myLastCoupledIdx);
                SumValue += picked[0];
            }
            else
            {
                myLastCoupledIdx = -1;
            }

            return pIdx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CleanReachablesFrom(int removeIdx)
        {
            foreach (var pair in myReachables.Where(pair => pair[1] == removeIdx).ToList())
                myReachables.Remove(pair);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyReachable()
            => myReachables.Any();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WasSuccessfulPick()
            => myLastCoupledIdx != -1;
    }
}