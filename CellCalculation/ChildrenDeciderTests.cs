namespace CellCalculation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Akka.Actor;
    using BlazorReporter;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    class ChildrenDeciderTests: Akka.TestKit.NUnit3.TestKit
    {
        [Test]
        public void NoDecisionAfterOneMessage()
        {
            ChildrenDecider decider = new ChildrenDecider();
            decider.Tell(new CreateChild(1, 1, 3), ActorOf<Cell>());
            decider.Tell(new CreateChild(1, 1, 3), ActorOf<Cell>()).HaveResult.Should().BeFalse();
        }

        [Test]
        public void DecisionAfterOneMessage()
        {
            ChildrenDecider decider = new ChildrenDecider();
            decider.Tell(new CreateChild(1, 1, 1), ActorOf<Cell>()).HaveResult.Should().BeTrue();
        }

        [Test]
        public void DecisionAfterTwoMessages()
        {
            ChildrenDecider decider = new ChildrenDecider();
            CreateChild[] createChildren = {new CreateChild(1, 1, 3), new CreateChild(1, 1, 3), new CreateChild(1, 1, 3)};
            Dictionary<Guid, IActorRef> guidCreateChildrenMapping =
                createChildren.Zip(createChildren,
                        (child1, child2) => new KeyValuePair<Guid, IActorRef>(child1.Guid, ActorOf<Cell>()))
                    .ToDictionary(x => x.Key, y => y.Value);
            decider.Tell(createChildren[0], guidCreateChildrenMapping[createChildren[0].Guid]);
            decider.Tell(createChildren[1], guidCreateChildrenMapping[createChildren[1].Guid])
                .HaveResult.Should()
                .BeFalse();
            ChildrenDeciderResult ret = decider.Tell(createChildren[2], guidCreateChildrenMapping[createChildren[2].Guid]);
            ret.HaveResult.Should().BeTrue();
            ret.MaxGuidedActor.Should().Be(guidCreateChildrenMapping
                .Single(y => y.Key == guidCreateChildrenMapping.Keys.Max()).Value);
        }

        [Test]
        public void CanCreateTwoForTheSamePlace()
        {
            ChildrenDecider decider = new ChildrenDecider();
            decider.Tell(new CreateChild(1, 1, 3), ActorOf<Cell>());
            decider.Tell(new CreateChild(1, 1, 3), ActorOf<Cell>()).HaveResult.Should().BeFalse();
            decider.Tell(new CreateChild(1, 1, 3), ActorOf<Cell>()).HaveResult.Should().BeTrue();
            decider.NotifyOthers(ActorOf<Cell>(), new CreateChild(1, 1, 3));
            CreateChild[] createChildren = {new CreateChild(1, 1, 3), new CreateChild(1, 1, 3), new CreateChild(1, 1, 3)};
            Dictionary<Guid, IActorRef> guidCreateChildrenMapping =
                createChildren.Zip(createChildren,
                        (child1, child2) => new KeyValuePair<Guid, IActorRef>(child1.Guid, ActorOf<Cell>()))
                    .ToDictionary(x => x.Key, y => y.Value);
            decider.Tell(createChildren[0], guidCreateChildrenMapping[createChildren[0].Guid]);
            decider.Tell(createChildren[1], guidCreateChildrenMapping[createChildren[1].Guid])
                .HaveResult.Should()
                .BeFalse();
            decider.Tell(createChildren[2], guidCreateChildrenMapping[createChildren[2].Guid])
                .MaxGuidedActor
                .Should()
                .Match(
                    x => x == guidCreateChildrenMapping.Single(y => y.Key == guidCreateChildrenMapping.Keys.Max()).Value);
        }

        [Test]
        public void DifferentAckDoesntCount()
        {
            ChildrenDecider decider = new ChildrenDecider();
            decider.Tell(new CreateChild(1, 1, 3), ActorOf<Cell>());
            decider.Tell(new CreateChild(1, 2, 3), ActorOf<Cell>());
            decider.Tell(new CreateChild(1, 1, 3), ActorOf<Cell>()).HaveResult.Should().BeFalse();
            decider.Tell(new CreateChild(1, 2, 3), ActorOf<Cell>()).HaveResult.Should().BeFalse();
        }

        [Test]
        public void NotifyNotNotifyParent()
        {
            ChildrenDecider decider = new ChildrenDecider();
            CreateChild[] createChildren = {new CreateChild(1, 1, 3), new CreateChild(1, 1, 3), new CreateChild(1, 1, 3)};
            Dictionary<Guid, IActorRef> guidCreateChildrenMapping =
                createChildren.Zip(createChildren,
                        (child1, child2) => new KeyValuePair<Guid, IActorRef>(child1.Guid,
                        (child1.Guid == createChildren.Select(x => x.Guid).Max() ? TestActor :
                        ActorOf<Cell>())))
                    .ToDictionary(x => x.Key, y => y.Value);
            decider.Tell(createChildren[0], guidCreateChildrenMapping[createChildren[0].Guid]);
            decider.Tell(createChildren[1], guidCreateChildrenMapping[createChildren[1].Guid]);
            decider.Tell(createChildren[2], guidCreateChildrenMapping[createChildren[2].Guid]);
            decider.NotifyOthers(ActorOf<Cell>(), createChildren[0]);
            ExpectNoMsg(TimeSpan.FromMilliseconds(300));
        }

        [Test]
        public void NotifyNotifyNonParents()
        {
            ChildrenDecider decider = new ChildrenDecider();
            CreateChild[] createChildren = {new CreateChild(1, 1, 3), new CreateChild(1, 1, 3), new CreateChild(1, 1, 3)};
            Dictionary<Guid, IActorRef> guidCreateChildrenMapping =
                createChildren.Zip(createChildren,
                        (child1, child2) => new KeyValuePair<Guid, IActorRef>(child1.Guid,
                        child1.Guid != createChildren.Select(x => x.Guid).Max() ? TestActor :
                            ActorOf<Cell>()))
                    .ToDictionary(x => x.Key, y => y.Value);
            decider.Tell(createChildren[0], guidCreateChildrenMapping[createChildren[0].Guid]);
            decider.Tell(createChildren[1], guidCreateChildrenMapping[createChildren[1].Guid]);
            decider.Tell(createChildren[2], guidCreateChildrenMapping[createChildren[2].Guid]);
            decider.NotifyOthers(ActorOf<Cell>(), createChildren[0]);
            ExpectMsg<NewChildInfo>();
            ExpectMsg<NewChildInfo>();
        }

        [Test]
        public void CalculatesIJTop()
        {
            var child = ActorOf<Cell>();
            var otherParent = ActorOf<Cell>();
            IBlazorFeeder nullFeeder = new NullLogger();
            child.Tell(new CreateMessage((a, b, c) => { }, 10, 10, 11, 11, 3, nullFeeder), otherParent);
            child.Tell(new NeighbourInfo(0, 0, 3), otherParent);
            child.Tell(new NeighbourInfo(0, 0, 3), otherParent);
            child.Tell(new NewChildInfo(TestActor, new CreateChild(11, 10, 3)), ActorOf<Cell>());
            ExpectMsg<NeighbourInfo>(x => x.I == 2 && x.J == 1);
        }

        [Test]
        public void CalculatesIJRight()
        {
            var child = ActorOf<Cell>();
            var otherParent = ActorOf<Cell>();
            IBlazorFeeder nullFeeder = new NullLogger();
            child.Tell(new CreateMessage((a, b, c) => { }, 10, 10, 11, 11, 3, nullFeeder), otherParent);
            child.Tell(new NeighbourInfo(0, 0, 3), otherParent);
            child.Tell(new NeighbourInfo(0, 0, 3), otherParent);
            child.Tell(new NewChildInfo(TestActor, new CreateChild(12, 11, 3)), ActorOf<Cell>());
            ExpectMsg<NeighbourInfo>(x => x.I == 1 && x.J == 0);
        }

        [Test]
        public void WhereAmIToItXYtoIJMeLeft()
        {
            ChildrenDecider childrenDecider = new ChildrenDecider();
            childrenDecider.WhereAmIToItXYtoIJ(11, 10, 10, 10).Should().BeEquivalentTo((1, 0));
        }

        [Test]
        public void WhereAmIToItXYtoIJMeBottom()
        {
            ChildrenDecider childrenDecider = new ChildrenDecider();
            childrenDecider.WhereAmIToItXYtoIJ(10, 10, 10, 11).Should().BeEquivalentTo((2, 1));
        }

        [Test]
        public void WhereIsItToMeXYtoIJMeLeft()
        {
            ChildrenDecider childrenDecider = new ChildrenDecider();
            childrenDecider.WhereIsItToMeXYtoIJ(10, 10, 11, 10).Should().BeEquivalentTo((1, 2));
        }

        [Test]
        public void WhereIsItToMeXYtoIJMeBottom()
        {
            ChildrenDecider childrenDecider = new ChildrenDecider();
            childrenDecider.WhereIsItToMeXYtoIJ(10, 11, 10, 10).Should().BeEquivalentTo((0, 1));
        }
    }
}
