namespace CellCalculation
{
    using System.Collections.Generic;
    using System.Linq;
    using Akka.Actor;

    internal class FullMapper
    {
        private Dictionary<(int, int), IActorRef> _return;
        public Dictionary<(int, int), IActorRef> Map(List<IActorRef[,]> neighboursAckResponseList, int x, int y)
        {
            _return = new Dictionary<(int, int), IActorRef>();
            CheckCell(neighboursAckResponseList.First(), neighboursAckResponseList, x, y);
            return _return;
        }

        private void CheckCell(IActorRef[,] actual, List<IActorRef[,]> neighboursAckResponseList, int x, int y)
        {
            _return.Add((x, y), actual[1, 1]);
            foreach (IActorRef actualNeighbour in actual.OfType<IActorRef>().Where(z => z != actual[1, 1]))
            {
                if (_return.ContainsValue(actualNeighbour))
                    continue;

                foreach (
                    IActorRef[,] nextIteration in
                    neighboursAckResponseList.Where(
                        m => m != actual && m.OfType<IActorRef>().Any(s => s == actualNeighbour) && !_return.ContainsValue(m[1, 1])))
                    for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        if (nextIteration[1, 1] == actual[j, i])
                            CheckCell(nextIteration, neighboursAckResponseList, x + (i - 1), y + (j - 1));
            }
        }
    }
}