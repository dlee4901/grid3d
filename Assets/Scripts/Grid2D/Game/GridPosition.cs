using System;

public readonly struct GridPosition : IEquatable<GridPosition>
{
    public readonly int Dim1;
    public readonly (int x, int y) Dim2;
    
    private readonly GridShape _shape;
    
    private const int NullDim = -1;
    
    public GridPosition(GridShape shape, int dim1)
    {
        _shape = shape;
        Dim1 = NullDim;
        Dim2 = (NullDim, NullDim);
        if (!IsValidPosition(dim1)) return;
        Dim1 = dim1;
        Dim2 = (dim1 % shape.X, dim1 / shape.X);
    }
    
    public GridPosition(GridShape shape, (int x, int y) dim2)
    {
        _shape = shape;
        Dim1 = NullDim;
        Dim2 = (NullDim, NullDim);
        if (!IsValidPosition(dim2)) return;
        Dim2 = dim2;
        Dim1 = dim2.y * shape.X + dim2.x;
    }
    
    public GridPosition(GridState grid, int dim1) : this(grid.Definition.Shape, dim1) {}
    
    public GridPosition(GridState grid, (int x, int y) dim2) : this(grid.Definition.Shape, dim2) {}
    
    public override bool Equals(object obj) 
    => obj is GridPosition other && Equals(other);
    
    public bool Equals(GridPosition other) 
    => Dim1 == other.Dim1 && Dim2 == other.Dim2 && _shape?.X == other._shape?.X && _shape?.Y == other._shape?.Y;

    public override int GetHashCode() => HashCode.Combine(Dim1, _shape?.X, _shape?.Y);
    
    public int StepsTo(GridPosition other)
        => Math.Abs(Dim2.x - other.Dim2.x) + Math.Abs(Dim2.y - other.Dim2.y);

    public GridPosition Add(int dim1) => new(_shape, Dim1 + dim1);
    
    public GridPosition Add((int x, int y) dim2) => new(_shape, (Dim2.x + dim2.x, Dim2.y + dim2.y));
    
    public bool IsValid() => IsValidPosition(Dim1) && IsValidPosition(Dim2);
    
    private bool IsValidPosition(int dim1) => _shape != null && _shape.Contains(dim1);
    private bool IsValidPosition((int x, int y) dim2) => _shape != null && _shape.Contains(dim2);
}
