namespace CellCalculation
{
    using System.Collections.Generic;
    using Akka.Actor;

    internal record NeighboursAckRootToLeaf(List<IActorRef[,]> ResponseList)
    {
    }
}
