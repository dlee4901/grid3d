#nullable enable

public readonly struct QueryContext
{
    public readonly IReadOnlyGridState Grid;
    public readonly (int, int) SourcePosition;
    public readonly IReadOnlyEntity? SourceEntity;

    public QueryContext(IReadOnlyGridState grid, (int, int) sourcePosition, IReadOnlyEntity? sourceEntity = null)
    {
        Grid = grid;
        SourcePosition = sourcePosition;
        SourceEntity = sourceEntity;
    }
}
