using System.Threading.Tasks;

namespace CellCalculation
{
    using Akka.Actor;
    using System;
    using System.Collections.Generic;
    using Akka.Configuration;
    using BlazorReporter;

    public class Manager: IDisposable
    {
        private ActorSystem _sys = ActorSystem.Create("Foo");
        //private readonly IBlazorFeeder _logger = new BlazorFeeder();
        private readonly IBlazorFeeder _logger = new NullLogger();

        public Manager()
        {
            Logger.Feeder = _logger;
            CreateSys();
        }

        private void CreateSys()
        {
            //stdout-loglevel = DEBUG
            //log-config-on-start = on
            _sys = ActorSystem.Create("Foo", ConfigurationFactory.ParseString(@"
akka {
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

        loggers = [""CellCalculation.Logger, CellCalculation""]
    }"));
        }

        public void Start(Action<int, int, bool> setColor)
        {
            Task.Run(
                _logger.InitConnectionAsync
            ).GetAwaiter().GetResult();
            //CreateStableIn1Step(setColor);
            //CreateAcorn(setColor);
            CreateBlinker(setColor);
            //CreateLine(setColor);
            //CreateShortLine(setColor);
            //CreateBlinkerWithInitialStateConstructor(setColor);
        }

        private void CreateLine(Action<int, int, bool> setColor)
        {
            var actor = _sys.ActorOf<CellMock>();
            var actor2 = _sys.ActorOf<CellMock>();
            var actor3 = _sys.ActorOf<CellMock>();
            var actor4 = _sys.ActorOf<CellMock>();
            var actor5 = _sys.ActorOf<CellMock>();
            var actor6 = _sys.ActorOf<CellMock>();
            actor.Tell(new CreateMessage(setColor, 25, 11, 25, 10, 3, _logger));
            actor.Tell(new SetMockProperties(new List<IActorRef> {actor2}, null, new[,]
            {
                {null, null, null},
                {null, actor, actor2},
                {null, null, null}
            }));
            actor2.Tell(new CreateMessage(setColor, 26, 11, 26, 10, 3, _logger));
            actor2.Tell(new SetMockProperties(new List<IActorRef> {actor3}, actor, new[,]
            {
                {null, null, null},
                {actor, actor2, actor3},
                {null, null, null}
            }));
            actor3.Tell(new CreateMessage(setColor, 26, 11, 27, 10, 3, _logger));
            actor3.Tell(new SetMockProperties(new List<IActorRef> {actor4}, actor2, new[,]
            {
                {null, null, null},
                {actor2, actor3, actor4},
                {null, null, null}
            }));
            actor4.Tell(new CreateMessage(setColor, 27, 11, 28, 10, 3, _logger));
            actor4.Tell(new SetMockProperties(new List<IActorRef> {actor5}, actor3, new[,]
            {
                {null, null, null},
                {actor3, actor4, actor5},
                {null, null, null}
            }));
            actor5.Tell(new CreateMessage(setColor, 28, 11, 29, 10, 3, _logger));
            actor5.Tell(new SetMockProperties(new List<IActorRef> {actor6}, actor4, new[,]
            {
                {null, null, null},
                {actor4, actor5, actor6},
                {null, null, null}
            }));
            actor6.Tell(new CreateMessage(setColor, 29, 11, 30, 10, 3, _logger));
            actor6.Tell(new SetMockProperties(new List<IActorRef>(), actor5, new[,]
            {
                {null, null, null},
                {actor5, actor6, null},
                {null, null, null}
            }));
        }

        private void CreateShortLine(Action<int, int, bool> setColor)
        {
            var actor = _sys.ActorOf<CellMock>();
            var actor2 = _sys.ActorOf<CellMock>();
            var actor3 = _sys.ActorOf<CellMock>();
            var actor4 = _sys.ActorOf<CellMock>();
            actor.Tell(new CreateMessage(setColor, 25, 11, 25, 10, 3, _logger));
            actor.Tell(new SetMockProperties(new List<IActorRef> {actor2}, null, new[,]
            {
                {null, null, null},
                {null, actor, actor2},
                {null, null, null}
            }));
            actor2.Tell(new CreateMessage(setColor, 26, 11, 26, 10, 3, _logger));
            actor2.Tell(new SetMockProperties(new List<IActorRef> {actor3}, actor, new[,]
            {
                {null, null, null},
                {actor, actor2, actor3},
                {null, null, null}
            }));
            actor3.Tell(new CreateMessage(setColor, 26, 11, 27, 10, 3, _logger));
            actor3.Tell(new SetMockProperties(new List<IActorRef> {actor4}, actor2, new[,]
            {
                {null, null, null},
                {actor2, actor3, actor4},
                {null, null, null}
            }));
            actor4.Tell(new CreateMessage(setColor, 27, 11, 28, 10, 3, _logger));
            actor4.Tell(new SetMockProperties(new List<IActorRef>(), actor3, new[,]
            {
                {null, null, null},
                {actor3, actor4, null},
                {null, null, null}
            }));
        }

        private void CreateBlinker(Action<int, int, bool> setColor)
        {
            var actor = _sys.ActorOf<CellMock>();
            var actor2 = _sys.ActorOf<CellMock>();
            var actor3 = _sys.ActorOf<CellMock>();
            actor3.Tell(new CreateMessage(setColor, 3, 3, 2, 2, 3, _logger));
            actor3.Tell(new SetMockProperties(new List<IActorRef>(), actor2, new[,]
            {
                {null, null, null},
                {null, actor3, null},
                {null, actor2, null}
            }));
            actor2.Tell(new CreateMessage(setColor, 3, 4, 2, 3, 3, _logger));
            actor2.Tell(new SetMockProperties(new List<IActorRef> {actor3}, actor, new[,]
            {
                {null, actor3, null},
                {null, actor2, null},
                {null, actor, null}
            }));
            actor.Tell(new CreateMessage(setColor, 3, 5, 2, 4, 3, _logger));
            actor.Tell(new SetMockProperties(new List<IActorRef> {actor2}, null, new[,]
            {
                {null, actor2, null},
                {null, actor, null},
                {null, null, null}
            }));
        }

        private void CreateAcorn(Action<int, int, bool> setColor)
        {
            var actor = _sys.ActorOf<CellMock>();
            var actor2 = _sys.ActorOf<CellMock>();
            var actor3 = _sys.ActorOf<CellMock>();
            var actor4 = _sys.ActorOf<CellMock>();
            var actor5 = _sys.ActorOf<CellMock>();
            var actor6 = _sys.ActorOf<CellMock>();
            var actor7 = _sys.ActorOf<CellMock>();
            var actor8 = _sys.ActorOf<CellMock>();
            actor.Tell(new CreateMessage(setColor, 20, 21, 20, 20, 3, _logger));
            actor.Tell(new SetMockProperties(new List<IActorRef> {actor2}, null, new[,]
            {
                {null, null, null},
                {actor2, actor, null},
                {null, null, null}
            }));
            actor2.Tell(new CreateMessage(setColor, 19, 21, 19, 20, 3, _logger));
            actor2.Tell(new SetMockProperties(new List<IActorRef> {actor3}, actor, new[,]
            {
                {null, null, null},
                {actor3, actor2, actor},
                {null, null, null}
            }));
            actor3.Tell(new CreateMessage(setColor, 19, 21, 18, 20, 3, _logger));
            actor3.Tell(new SetMockProperties(new List<IActorRef> {actor4}, actor2, new[,]
            {
                {actor4, null, null},
                {null, actor3, actor2},
                {null, null, null}
            }));
            actor4.Tell(new CreateMessage(setColor, 18, 18, 17, 19, 3, _logger));
            actor4.Tell(new SetMockProperties(new List<IActorRef> {actor5}, actor3, new[,]
            {
                {null, null, null},
                {actor5, actor4, null},
                {null, null, actor3}
            }));
            actor5.Tell(new CreateMessage(setColor, 17, 18, 16, 19, 3, _logger));
            actor5.Tell(new SetMockProperties(new List<IActorRef> {actor6, actor7}, actor4, new[,]
            {
                {actor6, null, null},
                {null, actor5, actor4},
                {actor7, null, null}
            }, false));
            actor6.Tell(new CreateMessage(setColor, 14, 17, 15, 18, 3, _logger));
            actor6.Tell(new SetMockProperties(new List<IActorRef>(), actor5, new[,]
            {
                {null, null, null},
                {null, actor6, null},
                {null, null, actor5}
            }));
            actor7.Tell(new CreateMessage(setColor, 15, 21, 15, 20, 3, _logger));
            actor7.Tell(new SetMockProperties(new List<IActorRef> {actor8}, actor5, new[,]
            {
                {null, null, actor5},
                {actor8, actor7, null},
                {null, null, null}
            }));
            actor8.Tell(new CreateMessage(setColor, 15, 21, 14, 20, 3, _logger));
            actor8.Tell(new SetMockProperties(new List<IActorRef>(), actor7, new[,]
            {
                {null, null, null},
                {null, actor8, actor7},
                {null, null, null}
            }));
        }

        private void CreateStableIn1Step(Action<int, int, bool> setColor)
        {
            var actor = _sys.ActorOf<CellMock>();
            var actor2 = _sys.ActorOf<CellMock>();
            var actor3 = _sys.ActorOf<CellMock>();
            actor3.Tell(new CreateMessage(setColor, 10, 11, 10, 10, 3, _logger));
            actor3.Tell(new SetMockProperties(new List<IActorRef>(), actor2, new[,]
            {
                {null, null, null},
                {null, actor3, null},
                {null, actor2, actor}
            }));
            actor2.Tell(new CreateMessage(setColor, 11, 11, 10, 11, 3, _logger));
            actor2.Tell(new SetMockProperties(new List<IActorRef> {actor3}, actor, new[,]
            {
                {null, actor3, null},
                {null, actor2, actor},
                {null, null, null}
            }));
            actor.Tell(new CreateMessage(setColor, 12, 12, 11, 11, 3, _logger));
            actor.Tell(new SetMockProperties(new List<IActorRef> {actor2}, null, new[,]
            {
                {actor3, null, null},
                {actor2, actor, null},
                {null, null, null}
            }));
        }

        public void CreateBlinkerWithInitialStateConstructor(Action<int, int, bool> setColor)
        {
            var initialStateConstuctor = new InitialStateConstructor(_sys, setColor, _logger);
            var x = true;
            var o = false;
            initialStateConstuctor.Create(1, 1, new bool[,]
            {
                {o, o, o, o, o},
                {o, x, x, x, o},
                {o, o, o, o, o}
            });
        }

        public void Dispose()
        {
            _sys?.Dispose();
            _logger?.Dispose();
        }
    }
}
