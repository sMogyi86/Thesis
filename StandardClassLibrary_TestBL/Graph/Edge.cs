using System;
using System.Collections.Generic;
using System.Text;

namespace MARGO.BL.Graph
{
    public interface IEdge
    {
        INode StartNode { get; }
        INode EndNode { get; }
        int Weight { get; }
    }

    internal sealed class Edge : IEdge
    {
        public INode StartNode => myStart;
        private readonly Node myStart;

        public INode EndNode => myEnd;
        private readonly Node myEnd;

        public int Weight { get; }



        public Edge(Node star, Node end, int weight)
        {
            myStart = star;
            myEnd = end;
            Weight = weight;
        }



        public Node GetTheOtherNode(Node node)
            => node == myStart ? myEnd : myStart;
    }
}