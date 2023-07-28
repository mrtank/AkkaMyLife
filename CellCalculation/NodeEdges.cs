using System.Collections.Generic;
using System.Linq;

namespace CellCalculation
{
    using System;

    internal class NodeEdges: IEquatable<NodeEdges>
    {
        public NodeEdges((int, int) from)
        {
            From = from;
            To = new List<(int, int)>();
        }

        public (int, int) From { get; }

        public List<(int, int)> To { get; }

        public void Add((int, int) to)
        {
            To.Add(to);
        }

        public bool ToContains(int i, int j)
        {
            return To.Any(x => x.Item1 == i && x.Item2 == j);
        }

        public bool Equals(NodeEdges other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            foreach ((int, int) to in To)
                if (!other.To.Any(x => Equals(to, x)))
                    return false;
            return Equals(From, other.From);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((NodeEdges) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return From.GetHashCode() * 397;
            }
        }
    }
}