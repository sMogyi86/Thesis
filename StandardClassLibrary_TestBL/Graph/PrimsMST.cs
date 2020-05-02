using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MARGO.BL.Graph
{
    public interface IMST
    {
        bool Terminated { get; }
        byte CurrentLevel { get; }
        IEnumerable<int> Items { get; }
        void DoStep();
    }

    internal class PrimsMST : IMST
    {
        private readonly List<int[]> myReachables = new List<int[]>();
        //private List<int[]> myClosests = new List<int[]>();
        private readonly List<int> myItems = new List<int>();
        private readonly ReadOnlyMemory<byte> myValueField;
        private bool myCanStep = true;
        private int myLastCoupledIdx;


        public bool Terminated => !myCanStep;
        public byte CurrentLevel { get; private set; }
        public IEnumerable<int> Items => myItems;



        private static readonly int[] OFFSETS = new int[4];
        private static bool INITIALIZED = false;
        public static void Initalize(int dataWidth)
        {
            if (!INITIALIZED)
            {
                OFFSETS[0] = 1;
                OFFSETS[1] = dataWidth;
                OFFSETS[2] = -1;
                OFFSETS[3] = -dataWidth;

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
            CurrentLevel = myValueField.Span[myLastCoupledIdx];
        }



        public void DoStep()
        {
            if (myCanStep)
            {
                var field = myValueField.Span;

                DiscoverReachables(field);
                if (myCanStep = AnyReachable())
                {
                    FindClosests();

                    do
                    {
                        CleanReachablesFrom(TryPickOne(field));
                    } while (!WasSuccessfulPick() && AnyReachable());

                    myCanStep = WasSuccessfulPick();
                }
            }
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DiscoverReachables(ReadOnlySpan<byte> valueField)
        {
            static int[] Dist(int currentIdx, int otherIdx, ReadOnlySpan<byte> values)
            {
                if (otherIdx >= values.Length)
                    otherIdx -= values.Length;

                if (otherIdx < 0)
                    otherIdx += values.Length;

                int d = values[otherIdx] - values[currentIdx];
                return new int[2] { (d < 0 ? -d : d), otherIdx };
            }

            foreach (var offset in OFFSETS)
            {
                int otherIdx = myLastCoupledIdx + offset;

                if (!myItems.Contains(otherIdx))
                    myReachables.Add(Dist(myLastCoupledIdx, otherIdx, valueField));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FindClosests()
        {
            myReachables.Sort(
                   (pairA, pairB) =>
                       pairA[0]
                       .CompareTo(pairB[0])
               );

            //int count = 0;
            //int shortestDistance = int.MaxValue;
            //foreach (var pair in myReachables)
            //{
            //    int d = pair[0];
            //    if (d < shortestDistance)
            //    {
            //        shortestDistance = d;
            //        count = 1;
            //    }
            //    else if (d == shortestDistance)
            //    {
            //        count++;
            //    }
            //}

            //myClosests = new List<int[]>(count);
            //foreach (var pair in myReachables)
            //    if (pair[0] == shortestDistance)
            //        myClosests.Add(pair);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int TryPickOne(ReadOnlySpan<byte> valueField)
        {
            var picked = myReachables.First();
            int pIdx = picked[1];

            if (FieldsSemaphore.TryTake(pIdx))
            {
                myLastCoupledIdx = pIdx;
                myItems.Add(myLastCoupledIdx);
                CurrentLevel = valueField[myLastCoupledIdx];
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
            for (int i = myReachables.Count - 1; i >= 0; i--)
            {
                //var pair = myReachables[i];
                if (myReachables[i][1] == removeIdx)
                {
                    myReachables.RemoveAt(i);
                    //myReachables.Remove(pair);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyReachable()
            => myReachables.Any();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WasSuccessfulPick()
            => myLastCoupledIdx != -1;
    }
}