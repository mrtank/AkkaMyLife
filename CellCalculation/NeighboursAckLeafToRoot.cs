namespace CellCalculation
{
    using System.Collections.Generic;
    using Akka.Actor;

    internal record NeighboursAckLeafToRoot(List<IActorRef[,]> ResponseList)
    {
    }
}