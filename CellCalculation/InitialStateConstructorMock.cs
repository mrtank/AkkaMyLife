using System.Collections.Generic;

namespace CellCalculation
{
    using Akka.Actor;
    using BlazorReporter;

    internal class InitialStateConstructorMock : InitialStateConstructor
    {
        public InitialStateConstructorMock(ActorSystem sys, IActorRef testActor = null): base(sys, (a, b, c) => {}, new NullLogger())
        {
            _testActor = testActor;
        }

        public List<NodeEdges> GetEdges()
        {
            return _edges;
        }
    }
}