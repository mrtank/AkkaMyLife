// ReSharper disable ExpressionIsAlwaysNull
namespace CellCalculation
{
    using System.Collections.Generic;
    using System.Linq;
    using Akka.Actor;
    using Akka.TestKit.NUnit3;
    using NUnit.Framework;
    using FluentAssertions;

    [TestFixture]
    public class NextStateTest: TestKit
    {
        [Test]
        public void UnderPopulation()
        {
            IActorRef[,] input = new IActorRef[5, 5];
            input[2, 2] = ActorOf<Cell>("foo");
            var result = RunCalculation(input);
            result.Should().ContainItemsAssignableTo<Suicide>().And.NotBeEmpty();
        }

        [Test]
        public void OverPopulation()
        {
            IActorRef[,] input = new IActorRef[5, 5];
            input[2, 2] = ActorOf<Cell>("foo");
            input[1, 1] = ActorOf<Cell>();
            input[1, 2] = ActorOf<Cell>();
            input[1, 3] = ActorOf<Cell>();
            input[3, 2] = ActorOf<Cell>();
            var result = RunCalculation(input);
            result.OfType<Suicide>().Should().NotBeEmpty();
        }

        [Test]
        public void StayingStill()
        {
            IActorRef[,] input = new IActorRef[5, 5];
            input[2, 2] = ActorOf<Cell>("foo");
            input[1, 1] = ActorOf<Cell>();
            input[1, 2] = ActorOf<Cell>();
            input[2, 1] = ActorOf<Cell>();
            var result = RunCalculation(input);
            result.Should().BeEmpty();
        }

        [Test]
        public void CreateChild()
        {
            IActorRef[,] input = new IActorRef[5, 5];
            input[2, 2] = ActorOf<Cell>("foo");
            input[2, 1] = ActorOf<Cell>();
            input[2, 3] = ActorOf<Cell>();
            var result = RunCalculation(input);
            result.OfType<CreateChild>().Should().HaveCount(2);
            foreach (CreateChild createChild in result.OfType<CreateChild>())
                createChild.ParentCount.Should().Be(1);
        }

        [Test]
        public void CreateChildMovedAway()
        {
            Dictionary<(int, int), IActorRef> extendedNeighbours = new Dictionary<(int, int), IActorRef>
            {
                {(10, 7), ActorOf<Cell>()},
                {(10, 6), ActorOf<Cell>("foo")},
                {(10, 8), ActorOf<Cell>()}
            };
            NextState nextState = new NextState();
            var result = nextState.Calculate(extendedNeighbours, 10, 6);
            result.OfType<CreateChild>().Should().HaveCount(2);
            foreach (CreateChild createChild in result.OfType<CreateChild>())
                createChild.NewY.Should().Be(7);
        }

        [Test]
        public void LShapedCreateChild()
        {
            Dictionary<(int, int), IActorRef> extendedNeighbours = new Dictionary<(int, int), IActorRef>
            {
                {(10, 11), ActorOf<Cell>()},
                {(10, 10), ActorOf<Cell>()},
                {(11, 11), ActorOf<Cell>()}
            };
            CheckFor(10, 11, extendedNeighbours);
            CheckFor(10, 10, extendedNeighbours);
            CheckFor(11, 11, extendedNeighbours);
        }

        private void CheckFor(int x, int y, Dictionary<(int, int), IActorRef> extendedNeighbours)
        {
            NextState nextState = new NextState();
            var result = nextState.Calculate(extendedNeighbours, x, y);
            result.Should().ContainItemsAssignableTo<CreateChild>().And.HaveCount(1);
            foreach (CreateChild createChild in result.OfType<CreateChild>())
            {
                createChild.NewX.Should().Be(11);
                createChild.NewY.Should().Be(10);
                createChild.ParentCount.Should().Be(3);
            }
        }

        [Test]
        public void CalculateNeighbourState()
        {
            IActorRef[,] input = new IActorRef[5, 5];
            input[2, 2] = ActorOf<Cell>("foo");
            input[2, 1] = ActorOf<Cell>();
            input[2, 3] = ActorOf<Cell>();
            var result = RunCalculation(input);
            result.OfType<KillNeighbour>().Should().HaveCount(2);
            foreach (KillNeighbour killNeighbour in result.OfType<KillNeighbour>())
                killNeighbour.X.Should().Be(2);
        }

        [Test]
        public void IntegrationalTestWithNeighbourCounter()
        {
            int? x = 1;
            int? o = null;
            Dictionary<(int, int), IActorRef> extendedNeighbours = CreateFromMap(new[,]
            {
                {o, o, o, o, o},
                {o, o, o, o, o},
                {o, x, x, x, o},
                {o, o, o, o, o},
                {o, o, o, o, o}
            });
            NextState nextState = new NextState();
            var result = nextState.Calculate(extendedNeighbours, 2, 2);
            result.OfType<CreateChild>().Should().HaveCount(2);
            var neighbourCounter = new NeighbourCounter();
            foreach (CreateChild createChild in result.OfType<CreateChild>())
                neighbourCounter.PossibleParentNeighbours(extendedNeighbours, createChild.NewX, createChild.NewY)
                    .Should()
                    .Equal((2, 2));
        }

        [Test]
        public void HaveTwoPossibleParents()
        {
            int? x = 1;
            int? o = null;
            Dictionary<(int, int), IActorRef> extendedNeighbours = CreateFromMap(new[,]
            {
                {x, x, o, x, x},
                {x, x, o, x, x},
                {o, o, o, o, o},
                {o, o, x, o, o},
                {o, o, o, o, o}
            });
            NextState nextState = new NextState();
            var result = nextState.Calculate(extendedNeighbours, 1, 1);
            result.OfType<CreateChild>().Should().HaveCount(2);
            var neighbourCounter = new NeighbourCounter();
            foreach (CreateChild createChild in result.OfType<CreateChild>().Take(1))
                neighbourCounter.PossibleParentNeighbours(extendedNeighbours, createChild.NewX, createChild.NewY)
                    .Should()
                    .Equal((0, 1), (1, 1));
            foreach (CreateChild createChild in result.OfType<CreateChild>().Skip(1))
                neighbourCounter.PossibleParentNeighbours(extendedNeighbours, createChild.NewX, createChild.NewY)
                    .Should()
                    .Equal((1, 1), (3, 1));
        }

        private Dictionary<(int, int), IActorRef> CreateFromMap(int?[,] input)
        {
            Dictionary<(int, int), IActorRef> ret = new Dictionary<(int, int), IActorRef>();
            for (int i = 0; i <= input.GetUpperBound(0); i++)
            for (int j = 0; j <= input.GetUpperBound(1); j++)
                if (input[i, j].HasValue)
                    ret.Add((j, i), ActorOf<Cell>());
            return ret;
        }

        private List<Todo> RunCalculation(IActorRef[,] input)
        {
            var stateCalculator = new NextState();
            var extendedNeighbours = new Dictionary<(int, int), IActorRef>();
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                {
                    if (input[i, j] == null)
                        continue;
                    extendedNeighbours.Add((i, j), input[i, j]);
                }
            var result = stateCalculator.Calculate(extendedNeighbours, 2, 2);
            return result;
        }
    }
}
