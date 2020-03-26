using System;
using System.Collections.Generic;
using System.Linq;

namespace MARGO.BL.Graph
{
    public interface IMST
    {
        bool Terminated { get; }
        int Value { get; }
        INode Root { get; }
        void DoStep();
    }

    internal class PrimsMST : IMST
    {
        private readonly List<(Edge Edge, Node Node)> myReachable = new List<(Edge Edge, Node Node)>();
        private readonly Func<int, bool> myBlockingTryTake; // for the optimistic concurrency
        private bool myCanStep = true;
        private Node myLastCoupled;


        public bool Terminated => !myCanStep;
        public int Value { get; private set; } = 0;
        public INode Root { get; }


        public PrimsMST(Node root, Func<int, bool> blockingTryTake)
        {
            Root = myLastCoupled = root;
            myBlockingTryTake = blockingTryTake;
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
            foreach (var neigbourh in myLastCoupled.NeighboursInternal.Where(neigbourh => !neigbourh.Value.Taken))
                myReachable.Add((neigbourh.Key, neigbourh.Value));
        }

        void SortReachables()
            => myReachable.Sort(
                    (tpl1, tpl2) =>
                        tpl1.Edge.Weight
                        .CompareTo(tpl2.Edge.Weight)
                );

        Node TryPickOne()
        {
            var picked = myReachable.First();

            if (myBlockingTryTake(picked.Node.Index))
            {
                myLastCoupled = picked.Node;
                myLastCoupled.Taken = true;
                var throughEdge = picked.Edge;
                throughEdge.GetTheOtherNode(myLastCoupled).AddChild(myLastCoupled);
                Value += throughEdge.Weight;
            }
            else
            {
                myLastCoupled = null;
            }

            return picked.Node;
        }

        private void CleanReachablesFrom(Node remove)
        {
            foreach (var tpl in myReachable.Where(tpl => tpl.Node == remove).ToList())
                myReachable.Remove(tpl);
        }

        private bool AnyReachable()
            => myReachable.Any();

        private bool WasSuccessfulPick()
            => myLastCoupled != null;
    }
}