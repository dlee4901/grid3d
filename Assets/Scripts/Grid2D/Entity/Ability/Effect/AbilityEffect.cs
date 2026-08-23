// public enum AbilityEffectType { ReplaceAbility, AddAbility, Damage, InflictStatus, GainShield }
// public enum Effect {Position, Health, Shield, Damage, Counter}

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

public abstract class AbilityEffect
{
    public EntityRelation Relation { get; set; } = EntityRelation.Any;
    public abstract void Apply(EffectContext ctx, Entity target);
}