namespace CellCalculation
{
    using Akka.Actor;
    using System.Collections.Generic;
    using System.Linq;

    public class NextState
    {
        private readonly NeighbourCounter _neighbourCounter = new NeighbourCounter();
        private int _y;
        private int _x;

        public List<Todo> Calculate(Dictionary<(int, int), IActorRef> extendedNeighbours, int x, int y)
        {
            var result = new List<Todo>();
            _x = x;
            _y = y;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    result.AddRange(CalculateForCell(extendedNeighbours, x + j - 1, y + i - 1));

            return result;
        }

        private IEnumerable<Todo> CalculateForCell(Dictionary<(int, int), IActorRef> extendedNeighbours, int x, int y)
        {
            int neighbourCount = _neighbourCounter.NeighbourCount(extendedNeighbours, x, y);
            if (neighbourCount == 3 && !extendedNeighbours.ContainsKey((x, y)))
                yield return new CreateChild(x, y, _neighbourCounter.PossibleParentNeighbours(extendedNeighbours, x, y).Count());
            if (x == _x && neighbourCount != 2 && neighbourCount != 3 && _y == y)
                yield return new Suicide();
            if (neighbourCount != 2 && neighbourCount != 3 && extendedNeighbours.ContainsKey((x, y)) && (x != _x || y != _y))
                yield return new KillNeighbour(x, y);
        }

    }
}
