public readonly struct EffectContext
{
    public readonly GridSource Origin;
    public readonly Ability Ability;
    public readonly Trait Trait;
    public readonly TriggerEvent Event;
    public readonly GridPosition Position;
    public readonly Entity Target;
    public readonly int Distance;

    public GridState State => Origin.Grid;
    public Entity Source => Origin.Entity;
    public DamageRoll Damage => Event?.Damage;

    internal EffectContext(GridSource origin, Ability ability, Trait trait = null, TriggerEvent evt = null)
        : this(origin, ability, trait, evt, origin.Position, origin.Entity, 0) { }

    private EffectContext(GridSource origin, Ability ability, Trait trait, TriggerEvent evt,
                          GridPosition position, Entity target, int distance)
    {
        Origin = origin;
        Ability = ability;
        Trait = trait;
        Event = evt;
        Position = position;
        Target = target;
        Distance = distance;
    }

    internal EffectContext At(GridPosition position, Entity target, int distance)
        => new(Origin, Ability, Trait, Event, position, target, distance);
}

public abstract class AbilityEffect
{
    public Occupant Relation { get; init; } = Occupant.Units;
    public Condition When { get; init; } = Condition.Always;
    internal abstract void Apply(in EffectContext ctx);

    internal bool Affects(in EffectContext ctx)
        => (Relation & Occupants.Of(ctx.Source, ctx.Target)) != 0 && When.Eval(ctx);
}
