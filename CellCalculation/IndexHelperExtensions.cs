namespace CellCalculation
{
    using Akka.Actor;
    using System;

    public static class IndexHelperExtensions
    {
        public static IActorRef Get(this IActorRef[,] self, int y, int x)
        {
            try
            {
                return self[y, x];
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }
    }
}
