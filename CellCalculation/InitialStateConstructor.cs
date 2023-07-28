namespace CellCalculation
{
    using System;
    using System.Collections.Generic;
    using Akka.Actor;
    using System.Linq;
    using BlazorReporter;

    class InitialStateConstructor
    {
        private readonly ActorSystem _sys;
        private readonly Action<int, int, bool> _setColor;
        protected IActorRef _testActor = null;
        protected List<NodeEdges> _edges = new();
        private readonly IBlazorFeeder _logger;

        public InitialStateConstructor(ActorSystem sys, Action<int, int, bool> setColor, IBlazorFeeder logger)
        {
            _sys = sys;
            _setColor = setColor;
            _logger = logger;
        }

        public void Create(long x, long y, bool[,] bools)
        {
            IActorRef[,] nodeRefs = new IActorRef[bools.GetUpperBound(0) + 1, bools.GetUpperBound(1) + 1];
            for (int i = 0; i < bools.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < bools.GetUpperBound(1) + 1; j++)
                {
                    if (!bools[i, j])
                        continue;

                    nodeRefs[i, j] = _testActor ?? _sys.ActorOf(Props.Create<CellMock>());
                    var nodeEdges = new NodeEdges((i, j));
                    _edges.Add(nodeEdges);
                    if (j < bools.GetUpperBound(1) - 1 && bools[i, j+1] && !_edges.Any(z => z.ToContains(i, j+1)))
                        nodeEdges.Add((i, j+1));
                    if (j > 0 && i < bools.GetUpperBound(0) - 1 && bools[i+1, j-1] && !_edges.Any(z => z.ToContains(i+1, j-1)))
                        nodeEdges.Add((i+1, j-1));
                    if (i < bools.GetUpperBound(0) - 1 && bools[i+1, j] && !_edges.Any(z => z.ToContains(i+1, j)))
                        nodeEdges.Add((i+1, j));
                    if (j < bools.GetUpperBound(1) - 1 && i < bools.GetUpperBound(0) - 1 && bools[i+1, j+1] && !_edges.Any(z => z.ToContains(i+1, j+1)))
                        nodeEdges.Add((i+1, j+1));

                    if (nodeEdges.To.Count == 0)
                        _edges.Remove(nodeEdges);
                }
            }

            if (_edges.Count == 0)
                return;

            var (parentX, parentY) = _edges[0].To[0];
            var parent = nodeRefs[parentX, parentY];
            foreach (NodeEdges nodeEdges in _edges)
            {
                var children = new List<IActorRef>();
                foreach ((int toX, int toY) in nodeEdges.To)
                    children.Add(nodeRefs[toX, toY]);

                var (actualX, actualY) = nodeEdges.From;
                IActorRef actual = nodeRefs[actualX, actualY];
                actual.Tell(new CreateMessage(_setColor, 1, 1, 1, 1, 3, _logger));
                actual.Tell(new SetMockProperties(children, parent, new[,]
                {
                    {
                        nodeRefs[actualX - 1, actualY - 1], nodeRefs[actualX, actualY - 1],
                        nodeRefs[actualX + 1, actualY - 1]
                    },
                    {nodeRefs[actualX - 1, actualY], nodeRefs[actualX, actualY], nodeRefs[actualX + 1, actualY]},
                    {
                        nodeRefs[actualX - 1, actualY + 1], nodeRefs[actualX, actualY + 1],
                        nodeRefs[actualX + 1, actualY + 1]
                    }
                }));

                parent = actual;
                //IActorRef mock = _sys.ActorOf(Props.Create<CellMock>());
                //mock.Tell(new CreateMessage(_setColor, 1, 1, 1, 1, 1));
                //mock.Tell(new SetMockProperties(new List<IActorRef>(), null, new IActorRef[0,0]));
            }
        }
    }
}
