// ReSharper disable PossibleUnintendedReferenceComparison
namespace CellCalculation
{
    using Akka.Actor;
    using System.Threading;
    using System.Threading.Tasks;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BlazorReporter;

    public class Cell : ReceiveActor, IWithUnboundedStash, ILogReceive
    {
        private CancellationTokenSource _cancel = new CancellationTokenSource();
        protected IActorRef[,] _neighbours = new IActorRef[3,3];
        private readonly List<IActorRef[,]> _neighbourResponses = new List<IActorRef[,]>();
        private readonly List<IActorRef> _childrenAcked = new List<IActorRef>();
        protected List<IActorRef> _children = new List<IActorRef>();
        private Action<int, int, bool> _setColor;
        private int _initialNeghbourAckedCount;
        private int _y;
        private int _x;
        internal IActorRef _parent;
        private readonly ChildrenDecider _childrenDecider = new ChildrenDecider();
        private readonly NeighbourCounter _neighbourCounter = new NeighbourCounter();
        private int _newChildInfoAckCountDown;
        private IBlazorFeeder _gameOfLifeLogger;
        public const int WAITTIMEINMILLISEC = 500;

        public IStash Stash { get; set; }

        public Cell()
        {
            Become(InitState);
        }

        protected virtual void InitState()
        {
            Receive<CreateMessage>(createMessage =>
            {
                Init(createMessage);
                if (createMessage.ParentCount <= _initialNeghbourAckedCount)
                    BecomeReady();
            });
            Receive<NeighbourInfo>(NeighbourInfo, shouldHandle:null);
            ReceiveAny(o => Stash.Stash());
        }

        private void NeighbourInfo(NeighbourInfo info)
        {
            _neighbours[info.I, info.J] = Sender;
            _initialNeghbourAckedCount++;
            if (info.ParentCount <= _initialNeghbourAckedCount)
                BecomeReady();
        }

        private void Ready()
        {
            Receive<NeighboursAckLeafToRoot>(NeighboursAckLeafToRoot, shouldHandle:null);
            Receive<NeighboursAckRootToLeaf>(NeighboursAckRootToLeaf, shouldHandle:null);
            Receive<CreateChild>(CreateChild);
            Receive<NewChildInfo>(ResponseCreateChildInfo, shouldHandle:null);
        }

        private void NeighboursAckLeafToRoot(NeighboursAckLeafToRoot neighboursAck)
        {
            _neighbourResponses.AddRange(neighboursAck.ResponseList);
            if (IsParentDead() || Sender != _parent)
                _childrenAcked.Add(Sender);
            // roots waits for all children to Ack back. These acks will contain tree information to build up fullMap
            // after fullMap at hand, Ack back to children with confidence to be able to calculate next state.
            // children have to wait for parent ack to calculate own next state.
            // leaves start chaining up in BecomeReady
            if (_childrenAcked.Count < _children.Count)
                return;
            if (!IsParentDead())
            {
                _parent.Tell(new NeighboursAckLeafToRoot(CreateNeighbourChain(_neighbourResponses)));
                return;
            }
            HandleNextState(_neighbourResponses);
        }

        private void NeighboursAckRootToLeaf(NeighboursAckRootToLeaf neighboursAck)
        {
            HandleNextState(neighboursAck.ResponseList);
        }

        private void HandleNextState(List<IActorRef[,]> neighbourResponses)
        {
            List<IActorRef[,]> usedForMap = CreateNeighbourChain(neighbourResponses);
            foreach (IActorRef child in _children)
                child.Tell(new NeighboursAckRootToLeaf(usedForMap));
            Dictionary<(int, int), IActorRef> cells = new FullMapper().Map(usedForMap, _x, _y);
            var nextState = new NextState();
            UseCalculationResults(cells, nextState.Calculate(cells, _x, _y));
        }

        private List<IActorRef[,]> CreateNeighbourChain(List<IActorRef[,]> neighbourResponses)
        {
            List<IActorRef[,]> usedForMap = new List<IActorRef[,]> {_neighbours};
            usedForMap.AddRange(neighbourResponses);
            return usedForMap;
        }

        private void UseCalculationResults(Dictionary<(int, int), IActorRef> cells, List<Todo> calculationResult)
        {
            _newChildInfoAckCountDown = CalculateNewChildInfoAckCount(calculationResult);
            if (calculationResult.Any(x => x is Suicide))
            {
                Suicide();
                return;
            }

            SendOutCreateChildren(cells, calculationResult);

            RemoveNeighbours(calculationResult);

            if (_newChildInfoAckCountDown == 0)
                HandleMessageStashing();
        }

        private void RemoveNeighbours(List<Todo> calculationResult)
        {
            foreach (KillNeighbour killNeighbour in calculationResult.OfType<KillNeighbour>())
            {
                var (i, j) = _childrenDecider.WhereIsItToMeXYtoIJ(_x, _y, killNeighbour.X, killNeighbour.Y);
                _children.Remove(_neighbours[i, j]);
                _neighbours[i, j] = null;
            }
        }

        private void SendOutCreateChildren(Dictionary<(int, int), IActorRef> cells, List<Todo> calculationResult)
        {
            foreach (CreateChild createChild in calculationResult.OfType<CreateChild>())
            {
                IEnumerable<(int, int)> possibleParents = _neighbourCounter.PossibleParentNeighbours(cells,
                    createChild.NewX, createChild.NewY);
                foreach ((int, int) xyPair in possibleParents)
                    cells[xyPair].Tell(createChild);
            }
        }

        private int CalculateNewChildInfoAckCount(List<Todo> calculationResult)
        {
            var ret = 0;
            foreach (CreateChild createChild in calculationResult.OfType<CreateChild>())
                ret += createChild.ParentCount;
            return ret;
        }

        protected void Suicide()
        {
            _setColor(_x, _y, false);
            _gameOfLifeLogger.LogSuicide(_x, _y, Self.Path.ToStringWithAddress());
            Become(Dead);
        }

        private bool IsParentDead()
        {
            if (_parent == null)
                return true;
            bool isNotDead = false;
            foreach (IActorRef neighbour in _neighbours)
                isNotDead |= neighbour == _parent;
            return !isNotDead;
        }

        private void ResponseCreateChildInfo(NewChildInfo newChildInfo)
        {
            var (i, j) = _childrenDecider.WhereAmIToItXYtoIJ(newChildInfo.CreateChild.NewX, newChildInfo.CreateChild.NewY, _x, _y);
            newChildInfo.NewChild.Tell(new NeighbourInfo(i, j, newChildInfo.CreateChild.ParentCount));
            (i, j) = _childrenDecider.WhereIsItToMeXYtoIJ(_x, _y, newChildInfo.CreateChild.NewX, newChildInfo.CreateChild.NewY);
            _neighbours[i, j] = newChildInfo.NewChild;
        }

        private void CreateChild(CreateChild createChild)
        {
            ChildrenDeciderResult result = _childrenDecider.Tell(createChild, Sender);
            if (!result.HaveResult)
                return;
            if (result.MaxGuidedActor != Self)
            {
                _newChildInfoAckCountDown++;
                return;
            }
            var child = NewChild(createChild);
            child.Tell(new CreateMessage(_setColor, _x, _y, createChild.NewX, createChild.NewY, createChild.ParentCount, _gameOfLifeLogger));
            if (createChild.ParentCount > 1)
                _childrenDecider.NotifyOthers(child, createChild);
            if (_newChildInfoAckCountDown == 1)
                HandleMessageStashing();
            else
                _newChildInfoAckCountDown--;
        }

        private IActorRef NewChild(CreateChild createChild)
        {
            var child = Context.ActorOf<Cell>();
            _children.Add(child);
            var (i, j) = _childrenDecider.WhereIsItToMeXYtoIJ(_x, _y, createChild.NewX, createChild.NewY);
            _neighbours[i, j] = child;
            return child;
        }

        private void HandleMessageStashing()
        {
            var self = Self; // closure
            Task.Run(Paint, _cancel.Token).ContinueWith<object>(x =>
                {
                    if (x.IsCanceled || x.IsFaulted)
                        return new Failed();
                    return new Finished();
                }, TaskContinuationOptions.ExecuteSynchronously)
                .PipeTo(self);

            // switch behavior
            Become(Working);
        }

        protected void Init(CreateMessage createMessage)
        {
            _setColor = createMessage.SetColor;
            _x = createMessage.X;
            _y = createMessage.Y;
            if (!Sender.IsNobody())
            {
                var (i, j) = _childrenDecider.WhereIsItToMeXYtoIJ(_x, _y, createMessage.ParentX, createMessage.ParentY);
                _neighbours[i, j] = Sender;
            }
            _neighbours[1, 1] = Self;
            _parent = Sender;
            _initialNeghbourAckedCount++;
            _setColor(_x, _y, true);
            _gameOfLifeLogger = createMessage.Logger;
            _gameOfLifeLogger.LogCreate(_x, _y, Self.Path.ToStringWithAddress());
        }

        private async Task Paint()
        {
            _childrenAcked.Clear();
            _neighbourResponses.Clear();
            _setColor(_x, _y, true);
            await Task.Delay(WAITTIMEINMILLISEC);
        }

        private void Working()
        {
            Receive<Cancel>(cancel => {
                _cancel.Cancel(); // cancel work
                BecomeReady();
            });
            Receive<Failed>(f => BecomeReady());
            Receive<Finished>(f => BecomeReady());
            ReceiveAny(o => Stash.Stash());
        }

        protected void Dead()
        {
            // possible feature: option for dead cells to pipe acks forward.
            // for that ChainWithHole in FullMapperTests.cs have to work.
            //Receive<CreateChild>(x => CreateChild(x));
            //Receive<NewChildInfo>(ResponseCreateChildInfo, shouldHandle:null);
            ReceiveAny(any => Stash.UnstashAll());
        }

        protected void BecomeReady()
        {
            _cancel = new CancellationTokenSource();
            if (0 == _children.Count)
                _parent?.Tell(new NeighboursAckLeafToRoot(new List<IActorRef[,]> {_neighbours}));
            Stash.UnstashAll();
            Become(Ready);
        }
    }
}
