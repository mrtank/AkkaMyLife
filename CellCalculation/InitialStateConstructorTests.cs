namespace CellCalculation
{
    using Akka.TestKit.NUnit3;
    using NUnit.Framework;
    using System.Collections.Generic;
    using FluentAssertions;

    [TestFixture]
    class InitialStateConstructorTests: TestKit
    {

        public InitialStateConstructorTests() : base(@"
akka {
        loggers = [
            ""Akka.Event.TraceLogger, Akka"",
            ""Akka.TestKit.TestEventListener, Akka.TestKit""
        ]

        loglevel = DEBUG

        actor
        {
          debug
          {
            receive = on      # log any received message
            autoreceive = on  # log automatically received messages, e.g. PoisonPill
            lifecycle = on    # log actor lifecycle changes
            event-stream = on # log subscription changes for Akka.NET event stream
            unhandled = on    # log unhandled messages sent to actors
          }
        }
}")
        {
        }


        [Test]
        public void CreateOneNodeSystem()
        {
            InitialStateConstructorMock constructor = new InitialStateConstructorMock(Sys);
            bool x = true;
            bool o = false;
            constructor.Create(1, 1, new[,]
            {
                {o, o, o},
                {o, x, o},
                {o, o, o}
            });
            constructor.GetEdges().Should().Equal();
        }

        [Test]
        public void CreateTwoNodeSystem()
        {
            InitialStateConstructorMock constructor = new InitialStateConstructorMock(Sys);
            bool x = true;
            bool o = false;
            constructor.Create(1, 1, new[,]
            {
                {o, o, o, o},
                {o, x, x, o},
                {o, o, o, o}
            });
            var edge = new NodeEdges((1, 1));
            edge.Add((1, 2));
            constructor.GetEdges().Should().BeEquivalentTo(new List<NodeEdges> {edge});
        }

        [Test]
        public void CreateThreeNodeSystem()
        {
            InitialStateConstructorMock constructor = new InitialStateConstructorMock(Sys);
            bool x = true;
            bool o = false;
            constructor.Create(1, 1, new[,]
            {
                {o, o, o, o, o},
                {o, x, x, x, o},
                {o, o, o, o, o}
            });
            var edge1 = new NodeEdges((1, 1));
            edge1.Add((1, 2));
            var edge2 = new NodeEdges((1, 2));
            edge2.Add((1, 3));
            constructor.GetEdges().Should().BeEquivalentTo(new List<NodeEdges> {edge1, edge2});
        }

        [Test]
        public void SimpleTwoParentedNode()
        {
            InitialStateConstructorMock constructor = new InitialStateConstructorMock(Sys);
            bool x = true;
            bool o = false;
            constructor.Create(1, 1, new[,]
            {
                {o, o, o, o, o},
                {o, x, x, o, o},
                {o, o, x, x, o},
                {o, o, o, o, o}
            });
            var edge1 = new NodeEdges((1, 1));
            edge1.Add((1, 2));
            edge1.Add((2, 2));
            var edge2 = new NodeEdges((1, 2));
            edge2.Add((2, 3));
            constructor.GetEdges().Should().BeEquivalentTo(new List<NodeEdges> {edge1, edge2});
        }

        [Test]
        public void ConstructsTwoNode()
        {
            InitialStateConstructorMock constructor = new InitialStateConstructorMock(Sys, TestActor);
            bool x = true;
            bool o = false;
            constructor.Create(1, 1, new[,]
            {
                {o, o, o, o},
                {o, x, x, o},
                {o, o, o, o}
            });
            ExpectMsg<CreateMessage>();
            ExpectMsg<SetMockProperties>();
        }

        [Test]
        public void ConstructsBlinker()
        {
            var constructor = new InitialStateConstructorMock(Sys, TestActor);
            bool x = true;
            bool o = false;
            constructor.Create(1, 1, new[,]
            {
                {o, o, o, o, o},
                {o, x, x, x, o},
                {o, o, o, o, o}
            });
            ExpectMsg<CreateMessage>(x => x.ParentX == 1 && x.ParentY == 1 && x.X == 1 && x.Y == 1);
            ExpectMsg<SetMockProperties>(x => x.IsLiving);
            ExpectMsg<CreateMessage>(x => x.ParentX == 2 && x.ParentY == 2 && x.X == 2 && x.Y == 1);
            ExpectMsg<SetMockProperties>(x => x.IsLiving);
        }
    }
}
