using System;

public readonly struct GridPosition : IEquatable<GridPosition>
{
    public readonly int Dim1;
    public readonly (int x, int y) Dim2;
    
    private readonly int _gridX;
    private readonly int _gridY;
    
    private const int NullDim = -1;
    
    public GridPosition(int gridX, int gridY, int dim1)
    {
        _gridX = gridX;
        _gridY = gridY;
        Dim1 = NullDim;
        Dim2 = (NullDim, NullDim);
        if (!IsValidPosition(dim1)) return;
        Dim1 = dim1;
        Dim2 = (dim1 % _gridX, dim1 / _gridX);
    }
    
    public GridPosition(int gridX, int gridY, (int x, int y) dim2)
    {
        _gridX = gridX;
        _gridY = gridY;
        Dim1 = NullDim;
        Dim2 = (NullDim, NullDim);
        if (!IsValidPosition(dim2)) return;
        Dim2 = dim2;
        Dim1 = dim2.y * _gridX + dim2.x;
    }
    
    public GridPosition(IReadOnlyGridState grid, int dim1) : this(grid.X, grid.Y, dim1)
    {
        if (!IsValidPosition(dim1)) return;
        Dim1 = dim1;
        Dim2 = (dim1 % _gridX, dim1 / _gridX);
    }
    
    public GridPosition(IReadOnlyGridState grid, (int x, int y) dim2) : this(grid.X, grid.Y, dim2)
    {
        if (!IsValidPosition(dim2)) return;
        Dim2 = dim2;
        Dim1 = dim2.y * _gridX + dim2.x;
    }
    
    public override bool Equals(object obj) 
    => obj is GridPosition other && Equals(other);
    
    public bool Equals(GridPosition other) 
    => Dim1 == other.Dim1 && Dim2 == other.Dim2 && _gridX == other._gridX && _gridY == other._gridY;
    
    public GridPosition Add(int dim1)
    {
        return new GridPosition(_gridX, _gridY, Dim1 + dim1);
    }
    
    public GridPosition Add((int x, int y) dim2)
    {
        return new GridPosition(_gridX, _gridY, (Dim2.x + dim2.x, Dim2.y + dim2.y));
    }
    
    public bool IsValid() => IsValidPosition(Dim1) && IsValidPosition(Dim2);
    
    private bool IsValidPosition(int dim1) => dim1 >= 0 && dim1 < _gridX * _gridY;
    private bool IsValidPosition((int x, int y) dim2) => dim2.x >= 0 && dim2.x < _gridX && dim2.y >= 0 && dim2.y < _gridY;
}