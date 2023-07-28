namespace CellCalculation
{
    using System;

    public record CreateChild(int NewX, int NewY, int ParentCount): Todo
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
    }
}