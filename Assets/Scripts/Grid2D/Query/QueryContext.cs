#nullable enable

using System;

public readonly struct QueryContext : IEquatable<QueryContext>
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

    public bool Equals(QueryContext other)
        => SourcePosition == other.SourcePosition
           && ReferenceEquals(SourceEntity, other.SourceEntity)
           && ReferenceEquals(Grid, other.Grid);

    public override bool Equals(object? obj) => obj is QueryContext other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Grid, SourcePosition, SourceEntity);
    public static bool operator ==(QueryContext a, QueryContext b) => a.Equals(b);
    public static bool operator !=(QueryContext a, QueryContext b) => !a.Equals(b);
}
