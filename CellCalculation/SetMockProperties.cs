namespace CellCalculation
{
    using System.Collections.Generic;
    using Akka.Actor;

    internal class SetMockProperties
    {
        public SetMockProperties(List<IActorRef> children, IActorRef parent, IActorRef[,] neighbours, bool isLiving = true)
        {
            Children = children;
            Parent = parent;
            Neighbours = neighbours;
            IsLiving = isLiving;
        }

        public IActorRef Parent { get; }
        public IActorRef[,] Neighbours { get; }
        public List<IActorRef> Children { get; }
        public bool IsLiving { get; }
    }
}