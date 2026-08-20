// public enum AbilityEffectType { ReplaceAbility, AddAbility, Damage, InflictStatus, GainShield }
// public enum Effect {Position, Health, Shield, Damage, Counter}

using System;

public readonly struct EffectContext
{
    public readonly GridState State;
    public readonly Entity Source;
    public readonly GridPosition SourcePosition;

    public EffectContext(GridState state, Entity source, GridPosition sourcePosition)
    {
        State = state;
        Source = source;
        SourcePosition = sourcePosition;
    }
}

[Flags]
public enum TargetRelation
{
    None = 0,
    Self = 1 << 0,
    Enemy = 1 << 1,
    Ally = 1 << 2,
    Neutral = 1 << 3,
    Any = Self | Enemy | Ally | Neutral
}

public abstract class AbilityEffect
{
    public TargetRelation Relation { get; set; } = TargetRelation.Any;
    public abstract void Apply(EffectContext ctx, Entity target);
}