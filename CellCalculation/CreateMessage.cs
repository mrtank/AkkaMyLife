namespace CellCalculation
{
    using System;
    using BlazorReporter;

    public class CreateMessage
    {

        public CreateMessage(Action<int, int, bool> setColor, int parentX, int parentY, int x, int y, int parentCount, IBlazorFeeder logger)
        {
            SetColor = setColor;
            ParentX = parentX;
            ParentY = parentY;
            ParentCount = parentCount;
            X = x;
            Y = y;
            Logger = logger;
        }

        public Action<int, int, bool> SetColor { get; }
        public int X { get; }
        public int Y { get; }
        public int ParentX { get; }
        public int ParentY { get; }
        public int ParentCount { get; }
        public IBlazorFeeder Logger { get; }
    }
}