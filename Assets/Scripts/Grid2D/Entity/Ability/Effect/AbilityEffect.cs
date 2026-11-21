using System.Collections.Generic;

public readonly struct EffectContext
{
    public readonly GridSource Origin;
    public readonly Ability Ability;
    public readonly ITriggered Triggered;
    public readonly TriggerEvent Event;
    public readonly IReadOnlyList<GridPosition> Path;
    public readonly GridPosition Position;
    public readonly Entity Target;
    public readonly int Distance;

    public GridState State => Origin.Grid;
    public Entity Source => Origin.Entity;
    public DamageRoll Damage => Event?.Damage;

    internal EffectContext(GridSource origin, Ability ability, ITriggered triggered = null, TriggerEvent evt = null,
                           IReadOnlyList<GridPosition> path = null)
        : this(origin, ability, triggered, evt, path, origin.Position, origin.Entity, 0) { }

    private EffectContext(GridSource origin, Ability ability, ITriggered triggered, TriggerEvent evt,
                          IReadOnlyList<GridPosition> path, GridPosition position, Entity target, int distance)
    {
        Origin = origin;
        Ability = ability;
        Triggered = triggered;
        Event = evt;
        Path = path;
        Position = position;
        Target = target;
        Distance = distance;
    }

    internal EffectContext At(GridPosition position, Entity target, int distance)
        => new(Origin, Ability, Triggered, Event, Path, position, target, distance);
}

public abstract class AbilityEffect
{
    public Occupant Relation { get; init; } = Occupant.Units;
    public Condition When { get; init; } = Condition.Always;
    public string DescriptionToken { get; init; }
    protected virtual string DefaultDescriptionToken => null;
    internal bool MatchesToken(string token) => (DescriptionToken ?? DefaultDescriptionToken) == token;
    internal abstract void Apply(in EffectContext ctx);

    public virtual bool TryGetValueRange(GridState state, out int min, out int max)
    {
        min = max = 0;
        return false;
    }

    internal bool Affects(in EffectContext ctx)
        => (Relation & Occupants.Of(ctx.Source, ctx.Target)) != 0 && When.Eval(ctx);
}
