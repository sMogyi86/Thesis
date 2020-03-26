using System;
using System.Collections.Generic;
using System.Linq;

namespace MARGO.BL.Graph
{
    internal class GraphFuctions
    {
        /// <summary>
        /// Creates all the nodes
        /// </summary>
        /// <param name="values"></param>
        /// <param name="nodes"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        public void CreateNodes(ReadOnlyMemory<byte> values, Memory<Node> nodes, int start, int length)
        {
            var from = values.Span;
            var to = nodes.Span;
            for (int i = start; i < (start + length); i++)
                to[i] = new Node(i, from[i]);
        }

        /// <summary>
        /// Creates all the edges
        /// </summary>
        /// <param name="dataWidth"></param>
        /// <param name="nodes"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        public void InitializeEdges(int dataWidth, ReadOnlyMemory<Node> nodes, int start, int length)
        {
            void Bound(Node current, ReadOnlySpan<Node> nodes, int otherOffset)
            {
                int otherIdx = current.Index + otherOffset;

                if (otherIdx > nodes.Length)
                    otherIdx -= nodes.Length;

                var neighbour = nodes[otherIdx];
                int d = neighbour - current;

                Edge edge;
                if (d > 0)
                    edge = new Edge(current, neighbour, d);
                else
                    edge = new Edge(neighbour, current, -d);

                current.AddNeighbour(edge, neighbour);
                neighbour.AddNeighbour(edge, current);
            }

            var span = nodes.Span;
            for (int i = start; i < (start + length); i++)
            {
                var currentNode = span[i];
                Bound(currentNode, span, 1); // right
                Bound(currentNode, span, dataWidth); // bottom
            }
        }

        public void CreateSeeds(Func<int, bool> blockingTryTake, ReadOnlyMemory<Node> nodes, ReadOnlyMemory<byte> minimas, ICollection<IMST> seeds, int start, int length)
        {
            var nodesSpan = nodes.Span;
            var minimasSpan = minimas.Span;
            for (int i = start; i < (start + length); i++)
                if (minimasSpan[i] == byte.MaxValue)
                    seeds.Add(new PrimsMST(nodesSpan[i], blockingTryTake));
        }

        public void Flood(IEnumerable<IMST> seeds)
        {
            do
            {
                foreach (var mst in seeds.Where(s => !s.Terminated).ToList())
                {
                    mst.DoStep();
                }
            } while (seeds.Any(s => !s.Terminated));
        }
    }
}
