namespace CellCalculation
{
    internal class CellMock : Cell
    {
        protected override void InitState()
        {
            Receive<SetMockProperties>(SetMockProperties, shouldHandle: null);
            Receive<CreateMessage>(Init, shouldHandle:null);
            ReceiveAny(o => Stash.Stash());
        }

        private void SetMockProperties(SetMockProperties setMockProperties)
        {
            _children = setMockProperties.Children;
            _parent = setMockProperties.Parent;
            _neighbours = setMockProperties.Neighbours;
            if (setMockProperties.IsLiving)
                BecomeReady();
            else
                Suicide();
        }
    }
}