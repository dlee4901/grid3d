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
    public EntityRelation Relation { get; init; } = EntityRelation.Any;
    internal abstract void Apply(EffectContext ctx, Entity target);

    internal bool Affects(Entity source, Entity target) => Relation.HasFlag(RelationBetween(source, target));

    private static EntityRelation RelationBetween(Entity source, Entity target)
    {
        if (ReferenceEquals(source, target)) return EntityRelation.Self;
        if (!source.TryGetComponent<ControlComponent>(out var sourceControl)
            || !target.TryGetComponent<ControlComponent>(out var targetControl)
            || sourceControl.PlayerController == 0
            || targetControl.PlayerController == 0)
            return EntityRelation.Neutral;
        return sourceControl.IsAlly(targetControl) ? EntityRelation.Ally : EntityRelation.Enemy;
    }
}