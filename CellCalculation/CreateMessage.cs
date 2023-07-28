namespace CellCalculation
{
    using System;
    using BlazorReporter;

    public record CreateMessage(Action<int, int, bool> SetColor, int ParentX, int ParentY, int X, int Y, int ParentCount, IBlazorFeeder Logger)
    {
    }
}