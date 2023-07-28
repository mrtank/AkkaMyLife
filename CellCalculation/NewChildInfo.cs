namespace CellCalculation
{
    using Akka.Actor;

    internal class NewChildInfo
    {
        public NewChildInfo(IActorRef newChild, CreateChild createChild)
        {
            NewChild = newChild;
            CreateChild = createChild;
        }

        public CreateChild CreateChild { get; }
        public IActorRef NewChild { get; }
    }
}