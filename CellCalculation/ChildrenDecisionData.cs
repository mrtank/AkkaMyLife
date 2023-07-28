namespace CellCalculation
{
    using System;
    using System.Collections.Generic;
    using Akka.Actor;

    internal class ChildrenDecisionData
    {
        public ChildrenDecisionData(IActorRef self, Guid guid)
        {
            Acks = new List<(IActorRef, Guid)> {(self, guid)};
        }

        public List<(IActorRef, Guid)> Acks { get; }
    }
}