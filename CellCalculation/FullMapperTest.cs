namespace CellCalculation
{
    using Akka.Actor;
    using Akka.TestKit.NUnit3;
    using FluentAssertions;
    using NUnit.Framework;
    using System.Collections.Generic;

    [TestFixture]
    class FullMapperTest: TestKit
    {
        [Test]
        public void SelfInside()
        {
            FullMapper fullMapper = new FullMapper();
            fullMapper.Map(new List<IActorRef[,]>
            {
                new[,]
                {
                    {null, null, null},
                    {null, ActorOf<Cell>(), null},
                    {null, null, null}
                }
            }, 2, 3).Should().ContainKey((2, 3));
        }

        [Test]
        public void NeighbourInside()
        {
            FullMapper fullMapper = new FullMapper();
            IActorRef mid = ActorOf<Cell>();
            IActorRef right = ActorOf<Cell>();
            fullMapper.Map(new List<IActorRef[,]>
            {
                new[,]
                {
                    {null, null, null},
                    {null, mid, right},
                    {null, null, null}
                },
                new[,]
                {
                    {null, null, null},
                    {mid, right, null},
                    {null, null, null}
                }
            }, 2, 2).Should().ContainKey((3, 2));
        }

        [Test]
        public void Chain()
        {
            FullMapper fullMapper = new FullMapper();
            IActorRef mid = ActorOf<Cell>();
            IActorRef NE = ActorOf<Cell>();
            IActorRef NEN = ActorOf<Cell>();
            IActorRef NENNW = ActorOf<Cell>();
            IActorRef NENNWW = ActorOf<Cell>();
            IActorRef NENNWWSW = ActorOf<Cell>();
            fullMapper.Map(new List<IActorRef[,]>
            {
                new[,]
                {
                    {null, null, NE},
                    {null, mid, null},
                    {null, null, null}
                },
                new[,]
                {
                    {null, NEN, null},
                    {null, NE, null},
                    {mid, null, null}
                },
                new [,]
                {
                    {NENNW, null, null},
                    {null, NEN, null},
                    {null, NE, null}
                },
                new [,]
                {
                    {null, null, null},
                    {NENNWW, NENNW, null},
                    {null, null, NEN}
                },
                new [,]
                {
                    {null, null, null},
                    {null, NENNWW, NENNW},
                    {NENNWWSW, null, null}
                },
                new [,]
                {
                    {null, null, NENNWW},
                    {null, NENNWWSW, null},
                    {null, null, null}
                }
            }, 2, 2).Should().ContainKey((0, 0));
        }

        [Test]
        public void DuplicationNotMatters()
        {
            FullMapper fullMapper = new FullMapper();
            IActorRef mid = ActorOf<Cell>();
            IActorRef NE = ActorOf<Cell>();
            IActorRef NEN = ActorOf<Cell>();
            IActorRef NENNW = ActorOf<Cell>();
            IActorRef NENNWW = ActorOf<Cell>();
            IActorRef NENNWWSW = ActorOf<Cell>();
            var firstResult = fullMapper.Map(new List<IActorRef[,]>
            {
                new[,]
                {
                    {null, null, NE},
                    {null, mid, null},
                    {null, null, null}
                },
                new[,]
                {
                    {null, NEN, null},
                    {null, NE, null},
                    {mid, null, null}
                },
                new [,]
                {
                    {NENNW, null, null},
                    {null, NEN, null},
                    {null, NE, null}
                },
                new [,]
                {
                    {null, null, null},
                    {NENNWW, NENNW, null},
                    {null, null, NEN}
                },
                new [,]
                {
                    {null, null, null},
                    {null, NENNWW, NENNW},
                    {NENNWWSW, null, null}
                },
                new [,]
                {
                    {null, null, NENNWW},
                    {null, NENNWWSW, null},
                    {null, null, null}
                }
            }, 2, 2);
            var secondResult = fullMapper.Map(new List<IActorRef[,]>
            {
                new[,]
                {
                    {null, null, NE},
                    {null, mid, null},
                    {null, null, null}
                },
                new [,]
                {
                    {null, null, null},
                    {NENNWW, NENNW, null},
                    {null, null, NEN}
                },
                new[,]
                {
                    {null, NEN, null},
                    {null, NE, null},
                    {mid, null, null}
                },
                new [,]
                {
                    {NENNW, null, null},
                    {null, NEN, null},
                    {null, NE, null}
                },
                new[,]
                {
                    {null, null, NE},
                    {null, mid, null},
                    {null, null, null}
                },
                new [,]
                {
                    {null, null, null},
                    {null, NENNWW, NENNW},
                    {NENNWWSW, null, null}
                },
                new [,]
                {
                    {null, null, NENNWW},
                    {null, NENNWWSW, null},
                    {null, null, null}
                },
                new[,]
                {
                    {null, null, NE},
                    {null, mid, null},
                    {null, null, null}
                },
                new [,]
                {
                    {null, null, NENNWW},
                    {null, NENNWWSW, null},
                    {null, null, null}
                }
            }, 2, 2);
            secondResult.Should().ContainKeys(firstResult.Keys).And.HaveCount(firstResult.Count).And.ContainValues(firstResult.Values);
            secondResult.Should().BeEquivalentTo(firstResult);
        }

        [Test]
        public void Circle()
        {
            FullMapper fullMapper = new FullMapper();
            IActorRef mid = ActorOf<Cell>();
            IActorRef NE = ActorOf<Cell>();
            IActorRef NEN = ActorOf<Cell>();
            IActorRef NENNW = ActorOf<Cell>();
            IActorRef NENNWW = ActorOf<Cell>();
            IActorRef NENNWWSW = ActorOf<Cell>();
            IActorRef NENNWWSWSE = ActorOf<Cell>();
            fullMapper.Map(new List<IActorRef[,]>
            {
                new[,]
                {
                    {null, null, NE},
                    {null, mid, null},
                    {null, null, null}
                },
                new[,]
                {
                    {null, NEN, null},
                    {null, NE, null},
                    {mid, null, null}
                },
                new [,]
                {
                    {NENNW, null, null},
                    {null, NEN, null},
                    {null, NE, null}
                },
                new [,]
                {
                    {null, null, null},
                    {NENNWW, NENNW, null},
                    {null, null, NEN}
                },
                new [,]
                {
                    {null, null, null},
                    {null, NENNWW, NENNW},
                    {NENNWWSW, null, null}
                },
                new [,]
                {
                    {null, null, NENNWW},
                    {null, NENNWWSW, null},
                    {null, null, NENNWWSWSE}
                },
                new [,]
                {
                    {NENNWWSW, null, null},
                    {null, NENNWWSWSE, null},
                    {null, null, mid}
                }
            }, 2, 2).Should().ContainKey((0, 0));
        }

        [Test]
        public void ChainWithHole()
        {
            var fullMapper = new FullMapper();
            IActorRef mid = ActorOf<Cell>();
            IActorRef NE = ActorOf<Cell>();
            IActorRef NEN = ActorOf<Cell>();
            IActorRef NENNW = ActorOf<Cell>();
            IActorRef NENNWW = ActorOf<Cell>();
            IActorRef NENNWWSW = ActorOf<Cell>();
            fullMapper.Map(new List<IActorRef[,]>
            {
                new[,]
                {
                    {null, null, NE},
                    {null, mid, null},
                    {null, null, null}
                },
                new[,]
                {
                    {null, NEN, null},
                    {null, NE, null},
                    {mid, null, null}
                },
                new [,]
                {
                    {NENNW, null, null},
                    {null, NEN, null},
                    {null, NE, null}
                },
                new [,]
                {
                    {null, null, null},
                    {null, NENNWW, NENNW},
                    {NENNWWSW, null, null}
                },
                new [,]
                {
                    {null, null, NENNWW},
                    {null, NENNWWSW, null},
                    {null, null, null}
                }
            }, 2, 2).Should().NotContainKey((0, 0));
        }
    }
}
