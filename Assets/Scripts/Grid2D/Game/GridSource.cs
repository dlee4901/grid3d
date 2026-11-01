#nullable enable

using System;

public readonly struct GridSource : IEquatable<GridSource>
{
    public readonly GridState Grid;
    public readonly GridPosition Position;
    public readonly Entity? Entity;

    public GridSource(GridState grid, GridPosition position, Entity? entity = null)
    {
        Grid = grid;
        Position = position;
        Entity = entity;
    }

    public bool Equals(GridSource other)
        => Position.Equals(other.Position)
           && ReferenceEquals(Entity, other.Entity)
           && ReferenceEquals(Grid, other.Grid);

    public override bool Equals(object? obj) => obj is GridSource other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Grid, Position, Entity);
    public static bool operator ==(GridSource a, GridSource b) => a.Equals(b);
    public static bool operator !=(GridSource a, GridSource b) => !a.Equals(b);
}
