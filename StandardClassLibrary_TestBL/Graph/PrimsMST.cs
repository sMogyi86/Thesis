﻿using System;
using System.Collections.Generic;
using System.Linq;

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
        private static readonly int[] OFFSETS = new int[4];
        private readonly List<int[]> myReachables = new List<int[]>();
        private readonly List<int> myItems = new List<int>();
        private readonly ReadOnlyMemory<byte> myValueField;
        private readonly Func<int, bool> myBlockingTryTake; // for the optimistic concurrency
        private bool myCanStep = true;
        private int myLastCoupledIdx;


        public bool Terminated => !myCanStep;
        public int SumValue { get; private set; } = 0;
        public IEnumerable<int> Items => myItems;



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

        public PrimsMST(int rootIdx, ReadOnlyMemory<byte> valueField, Func<int, bool> blockingTryTake)
        {
            if (!INITIALIZED)
                throw new InvalidOperationException($"Call the static '{nameof(Initalize)}()' first!");

            myLastCoupledIdx = rootIdx;
            myValueField = valueField;
            myBlockingTryTake = blockingTryTake;
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



        void DiscoverReachables()
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

        void SortReachables()
            => myReachables.Sort(
                    (pairA, pairB) =>
                        pairA[0]
                        .CompareTo(pairB[0])
                );

        int TryPickOne()
        {
            var picked = myReachables.First();
            int pIdx = picked[1];

            if (myBlockingTryTake == null || myBlockingTryTake(pIdx))
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

        private void CleanReachablesFrom(int removeIdx)
        {
            foreach (var pair in myReachables.Where(pair => pair[1] == removeIdx).ToList())
                myReachables.Remove(pair);
        }

        private bool AnyReachable()
            => myReachables.Any();

        private bool WasSuccessfulPick()
            => myLastCoupledIdx != -1;
    }
}