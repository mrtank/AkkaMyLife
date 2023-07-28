namespace CellCalculation
{
    using System.Collections.Generic;
    using Akka.Actor;

    internal record SetMockProperties(List<IActorRef> Children, IActorRef Parent, IActorRef[,] Neighbours, bool IsLiving = true)
    {
    }
}