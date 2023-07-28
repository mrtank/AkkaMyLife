namespace CellCalculation
{
    using Akka.Actor;

    internal class ChildrenDeciderResult
    {
        public ChildrenDeciderResult(bool haveResult)
        {
            HaveResult = haveResult;
        }

        public bool HaveResult { get; }

        public IActorRef MaxGuidedActor { get; set; }
    }
}