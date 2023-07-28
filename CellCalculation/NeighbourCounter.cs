namespace CellCalculation
{
    using System.Collections.Generic;
    using System.Linq;
    using Akka.Actor;

    public class NeighbourCounter
    {
        public int NeighbourCount(Dictionary<(int, int), IActorRef> extendedNeighbours, int x, int y)
        {
            return Neighbours(extendedNeighbours, x, y).Count();
        }

        public IEnumerable<(int, int)> Neighbours(Dictionary<(int, int), IActorRef> extendedNeighbours, int x, int y)
        {
            for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
            {
                var key = (x - 1 + i, y - 1 + j);
                if (extendedNeighbours.ContainsKey(key) && (i != 1 || j != 1))
                    yield return key;
            }
        }

        public IEnumerable<(int, int)> PossibleParentNeighbours(
            Dictionary<(int, int), IActorRef> extendedNeighbours, int x, int y)
        {
            return Neighbours(extendedNeighbours, x, y).Where(z => IsLivingNeighbour(z, extendedNeighbours));
        }

        private bool IsLivingNeighbour((int neighbourX, int neighbourY) xyNeighbourPair, Dictionary<(int, int), IActorRef> extendedNeighbours)
        {
            var neighbourCount = NeighbourCount(extendedNeighbours, xyNeighbourPair.neighbourX, xyNeighbourPair.neighbourY);
            return neighbourCount == 2 || neighbourCount == 3;
        }
    }
}