using System;

[Flags]
public enum Occupant
{
    None = 0,
    Self = 1 << 0,
    Enemy = 1 << 1,
    Ally = 1 << 2,
    Neutral = 1 << 3,
    Empty = 1 << 4,
    Units = Self | Enemy | Ally | Neutral,
    Any = Units | Empty
}

public static class Occupants
{
    public static Occupant Between(Entity source, Entity target)
    {
        if (ReferenceEquals(source, target)) return Occupant.Self;
        if (!source.TryGetComponent<ControlComponent>(out var sourceControl)
            || !target.TryGetComponent<ControlComponent>(out var targetControl)
            || sourceControl.PlayerController == 0
            || targetControl.PlayerController == 0)
            return Occupant.Neutral;
        return sourceControl.IsAlly(targetControl) ? Occupant.Ally : Occupant.Enemy;
    }

    public static Occupant At(GridState grid, GridPosition position, Entity source)
    {
        var entity = grid.GetEntity(position);
        if (entity == null) return Occupant.Empty;
        if (source == null) return Occupant.Neutral;
        return Between(source, entity);
    }
}
