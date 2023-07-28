namespace CellCalculation
{
    using Akka.Actor;
    using FluentAssertions;
    using NUnit.Framework;
    using System.Collections.Generic;
    using Akka.TestKit.NUnit3;

    [TestFixture]
    public class NeighbourCounterTest: TestKit
    {
        private Dictionary<(int, int), IActorRef> _map;

        [SetUp]
        public void Setup()
        {
            _map = new Dictionary<(int, int), IActorRef>();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    _map[(j, i)] = ActorOf<Cell>();
                }
        }

        [Test]
        public void NeighboursTopParent()
        {
            var neighbourCounter = new NeighbourCounter();
            var ret = neighbourCounter.Neighbours(_map, 1, 2);
            ret.Should().Equal((0, 1), (0, 2), (1, 1), (2, 1), (2, 2));
        }

        [Test]
        public void NeighboursBottomParent()
        {
            var neighbourCounter = new NeighbourCounter();
            var ret = neighbourCounter.Neighbours(_map, 1, 0);
            ret.Should().Equal((0, 0), (0, 1), (1, 1), (2, 0), (2, 1));
        }

        [Test]
        public void NeighboursLeftParent()
        {
            var neighbourCounter = new NeighbourCounter();
            var ret = neighbourCounter.Neighbours(_map, 2, 1);
            ret.Should().Equal((1, 0), (1, 1), (1, 2), (2, 0), (2, 2));
        }

        [Test]
        public void NeighboursTopRightParent()
        {
            var neighbourCounter = new NeighbourCounter();
            var ret = neighbourCounter.Neighbours(_map, 0, 2);
            ret.Should().Equal((0, 1), (1, 1), (1, 2));
        }

        [Test]
        public void NeighboursTopParentWithHole()
        {
            _map.Remove((0, 2));
            var neighbourCounter = new NeighbourCounter();
            var ret = neighbourCounter.Neighbours(_map, 1, 2);
            ret.Should().Equal((0, 1), (1, 1), (2, 1), (2, 2));
        }

        [Test]
        public void PossibleParentZero()
        {
            var neighbourCounter = new NeighbourCounter();
            neighbourCounter.PossibleParentNeighbours(_map, 0, 0).Should().Equal();
        }

        [Test]
        public void PossibleParentFourCorners()
        {
            var neighbourCounter = new NeighbourCounter();
            neighbourCounter.PossibleParentNeighbours(_map, 1, 1).Should().Equal((0, 0), (0, 2), (2, 0), (2, 2));
        }
    }
}
