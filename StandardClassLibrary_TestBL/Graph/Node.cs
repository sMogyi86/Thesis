using System;
using System.Collections.Generic;
using System.Text;

namespace MARGO.BL.Graph
{
    public interface INode
    {
        int Index { get; }
        byte Value { get; }
        bool Taken { get; set; }
        IReadOnlyDictionary<IEdge, INode> Neighbours { get; }
        IEnumerable<INode> Children { get; }
    }

    internal sealed class Node : INode
    {
        public int Index { get; }
        public byte Value { get; }
        public bool Taken { get; set; } = false;

        public IReadOnlyDictionary<IEdge, INode> Neighbours => (IReadOnlyDictionary<IEdge, INode>)myNeighbours;
        public IReadOnlyDictionary<Edge, Node> NeighboursInternal => myNeighbours;
        private readonly Dictionary<Edge, Node> myNeighbours = new Dictionary<Edge, Node>(4);

        public IEnumerable<INode> Children => myChildren;
        private readonly List<Node> myChildren = new List<Node>();



        public Node(int index, byte value)
        {
            Index = index;
            Value = value;
        }

        public void AddNeighbour(Edge edge, Node node)
            => myNeighbours[edge] = node;

        public void AddChild(Node node)
            => myChildren.Add(node);


        public static int operator -(Node end, Node start)
            => end.Value - start.Value;

        public static bool operator ==(Node end, Node start)
            => end.Index == start.Index;

        public static bool operator !=(Node end, Node start)
            => !(end == start);



        //public override bool Equals(object obj) => (obj is Node other) && other == this;

        //public override int GetHashCode() => this.Index.GetHashCode();
    }
}