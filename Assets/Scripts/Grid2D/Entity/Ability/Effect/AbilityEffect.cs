// public enum AbilityEffectType { ReplaceAbility, AddAbility, Damage, InflictStatus, GainShield }
// public enum Effect {Position, Health, Shield, Damage, Counter}

public readonly struct EffectContext
{
    public readonly GridState State;
    public readonly Entity Source;
    public readonly GridPosition SourcePosition;
    public readonly Ability Ability;
    public readonly Entity Target;
    public readonly int Distance;

    public EffectContext(GridState state, Entity source, GridPosition sourcePosition, Ability ability,
                         Entity target = null, int distance = 0)
    {
        State = state;
        Source = source;
        SourcePosition = sourcePosition;
        Ability = ability;
        Target = target;
        Distance = distance;
    }

    public EffectContext For(Entity target, int distance)
        => new(State, Source, SourcePosition, Ability, target, distance);
}

public abstract class AbilityEffect
{
    public Occupant Relation { get; init; } = Occupant.Units;
    public Condition When { get; init; } = Condition.Always;
    internal abstract void Apply(in EffectContext ctx);

    internal bool Affects(in EffectContext ctx)
        => (Relation & Occupants.Between(ctx.Source, ctx.Target)) != 0 && When.Eval(ctx);
}