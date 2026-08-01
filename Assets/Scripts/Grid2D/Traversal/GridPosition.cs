public class GridPosition
{
    public int Position1D { get; private set; }
    public (int, int) Position2D { get; private set; }
    
    private int _gridX;
    private int _gridY;
    
    public GridPosition(IReadOnlyGridState grid)
    {
        _gridX = grid.X;
        _gridY = grid.Y;
    }
    
    public GridPosition(IReadOnlyGridState grid, int position1D) : this(grid)
    {
        Position1D = position1D;
        
    }
}