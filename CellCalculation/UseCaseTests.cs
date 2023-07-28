namespace CellCalculation
{
    using Akka.Actor;
    using Akka.TestKit.NUnit3;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using BlazorReporter;

    [TestFixture]
    class UseCaseTests: TestKit
    {
        public UseCaseTests(): base(@"
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
}") { }

        private readonly Action<int, int, bool> _setColor = (a, b, c) => { };
        private readonly IBlazorFeeder _logger = new NullLogger();

        [Test]
        public void NotYet()
        {
            var child = ActorOf<Cell>();
            child.Tell(new CreateMessage(_setColor, 10, 10, 11, 11, 3, _logger), TestActor);
            ExpectNoMsg(TimeSpan.FromMilliseconds(300));
        }

        [Test]
        public void WaitingForMockProps()
        {
            var testedActor = ActorOf<CellMock>();
            testedActor.Tell(new CreateMessage(_setColor, 10, 10, 11, 11, 1, _logger));
            ExpectNoMsg(TimeSpan.FromMilliseconds(300));
        }

        [Test]
        public void WaitingForChildAck()
        {
            var testedActor = ActorOf<CellMock>();
            var childActor = ActorOf<Cell>();
            testedActor.Tell(new CreateMessage(_setColor, 10, 10, 11, 11, 1, _logger));
            testedActor.Tell(new SetMockProperties(new List<IActorRef> {childActor}, TestActor, new[,]
            {
                {null, null, null},
                {null, testedActor, TestActor},
                {null, null, null}
            }));
            ExpectNoMsg(TimeSpan.FromMilliseconds(300));
        }

        [Test]
        public void WaitingForAllCreateChild()
        {
            var a = Sys.ActorOf<CellMock>();
            var b = Sys.ActorOf<Cell>();
            //var b = Sys.ActorOf<CellMock>();
            //b.Tell(new CreateMessage(_setColor, 9, 10, 10, 10, 1));
            //b.Tell(new SetMockProperties(new List<IActorRef>(), a, new[,]
            //{
            //    {null, null, null},
            //    {null, b, a},
            //    {null, null, null}
            //}));
            var c = Sys.ActorOf<Cell>();
            a.Tell(new CreateMessage(_setColor, 10, 10, 11, 10, 1, _logger));
            a.Tell(new SetMockProperties(new List<IActorRef> {b}, TestActor, new[,]
            {
                {null, null, null},
                {b, a, TestActor},
                {null, null, null}
            }));
            a.Tell(new NeighboursAckLeafToRoot
            ( 
                new List<IActorRef[,]>
                {
                    new[,]
                    {
                        {null, null, null},
                        {null, b, a},
                        {null, null, null}
                    }
                }
            ), b);
            // todo check if NeighbourAck should be delayed after CreateMessage/NeighbourInfo if child should be born in the same step
            ExpectMsg<NeighboursAckLeafToRoot>();
            a.Tell(new NeighboursAckRootToLeaf
            ( 
                new List<IActorRef[,]>
                {
                    new[,]
                    {
                        {null, null, null},
                        {null, b, a},
                        {null, null, null}
                    },
                    new[,]
                    {
                        {null, null, null},
                        {b, a, TestActor},
                        {null, null, null}
                    },
                    new[,]
                    {
                        {null, null, null},
                        {a, TestActor, c},
                        {null, null, null}
                    },
                    new[,]
                    {
                        {null, null, null},
                        {TestActor, c, null},
                        {null, null, null}
                    }
                }
            ), TestActor);
            //ExpectMsgAllOf(typeof(CreateChild), typeof(CreateChild), typeof(CreateChild), typeof(CreateChild));
            ExpectMsg<CreateChild>();
            ExpectMsg<CreateChild>();
            ExpectMsg<CreateChild>();
            ExpectMsg<CreateChild>();
            //ExpectNoMsg(TimeSpan.FromMilliseconds(Cell.WAITTIMEINMILLISEC + 300));
        }

        [Test]
        public void ChildOfNoone()
        {
            var child = ActorOf<Cell>();
            child.Tell(new CreateMessage(_setColor, 10, 10, 11, 11, 1, _logger), TestActor);
            ExpectMsg<NeighboursAckLeafToRoot>();
        }

        [Test]
        public void CalculatesFasterThanCreateMessage()
        {
            var child = ActorOf<Cell>();
            var otherParent = ActorOf<Cell>();
            child.Tell(new NeighbourInfo(0, 0, 3), otherParent);
            child.Tell(new NeighboursAckLeafToRoot (new List<IActorRef[,]>()), otherParent);
            child.Tell(new CreateMessage(_setColor, 10, 10, 11, 11, 3, _logger), TestActor);
            child.Tell(new NeighboursAckLeafToRoot (new List<IActorRef[,]>()), TestActor);
            child.Tell(new NeighbourInfo(0, 0, 3), otherParent);
            child.Tell(new NeighboursAckLeafToRoot (new List<IActorRef[,]>()), otherParent);
            ExpectMsg<NeighboursAckLeafToRoot>();
        }

        [Test]
        public void NeighbourCreateChildFasterThanNeighbourAck()
        {
            var child = ActorOf<Cell>();
            var otherParent = ActorOf<Cell>();
            child.Tell(new NeighbourInfo(0, 0, 3), otherParent);
            child.Tell(new NeighboursAckLeafToRoot (new List<IActorRef[,]>()), otherParent);
            child.Tell(new CreateMessage(_setColor, 10, 10, 11, 11, 3, _logger), TestActor);
            child.Tell(new NeighboursAckLeafToRoot (new List<IActorRef[,]>()), TestActor);
            child.Tell(new NeighbourInfo(0, 0, 3), otherParent);
            child.Tell(new NeighboursAckLeafToRoot (new List<IActorRef[,]>()), otherParent);
            ExpectMsg<NeighboursAckLeafToRoot>();
        }
    }
}
