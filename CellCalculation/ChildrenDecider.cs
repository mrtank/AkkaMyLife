namespace CellCalculation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Akka.Actor;

    internal class ChildrenDecider
    {
        private readonly Dictionary<(int, int), ChildrenDecisionData> _resultsFor = new Dictionary<(int, int), ChildrenDecisionData>();

        public ChildrenDeciderResult Tell(CreateChild createChild, IActorRef self)
        {
            if (createChild.ParentCount == 0 || createChild.ParentCount == 1)
                return new ChildrenDeciderResult(true) {MaxGuidedActor = self};
            var key = (createChild.NewX, createChild.NewY);
            if (!_resultsFor.ContainsKey(key))
            {
                _resultsFor.Add(key, new ChildrenDecisionData(self, createChild.Guid));
                return new ChildrenDeciderResult(false);
            }
            ChildrenDecisionData data = _resultsFor[key];
            data.Acks.Add((self, createChild.Guid));
            var haveResult = createChild.ParentCount <= data.Acks.Count;
            var result = new ChildrenDeciderResult(haveResult);
            if (haveResult)
                result.MaxGuidedActor = data.Acks.Single(x => x.Item2 == data.Acks.Max(y => y.Item2)).Item1;
            return result;
        }

        public void NotifyOthers(IActorRef child, CreateChild createChild)
        {
            var key = (createChild.NewX, createChild.NewY);
            ChildrenDecisionData data = _resultsFor[key];
            foreach (IActorRef other in data.Acks.Select(x => x.Item1).Where(x => x != data.Acks.Single(z => z.Item2 == data.Acks.Max(y => y.Item2)).Item1))
                other.Tell(new NewChildInfo(child, createChild));
            _resultsFor.Remove(key);
        }

        public (int, int) WhereAmIToItXYtoIJ(int itsX, int itsY, int myX, int myY)
        {
            return (1 + myY - itsY, 1 + myX - itsX);
        }

        public (int, int) WhereIsItToMeXYtoIJ(int myX, int myY, int itsX, int itsY)
        {
            return (1 + itsY - myY, 1 + itsX - myX);
        }
    }
}