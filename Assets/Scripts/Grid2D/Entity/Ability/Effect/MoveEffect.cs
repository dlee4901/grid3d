public sealed class MoveEffect : AbilityEffect
{
    public bool Teleport { get; init; }
    public bool Flying { get; init; }

    public MoveEffect() => Relation = Occupant.Empty;

    internal override void Apply(in EffectContext ctx)
    {
        var from = ctx.Source.Position;
        if (Teleport)
        {
            ctx.State.ChangeEntityPosition(from, ctx.Position, true, !Flying);
            return;
        }
        var path = ctx.Path ?? ctx.Ability?.Targeting.GetSelectableSteps(ctx.Origin).PathTo(from, ctx.Position);
        if (path == null)
        {
            ctx.State.ChangeEntityPosition(from, ctx.Position, applyTerrain: !Flying);
            return;
        }
        foreach (var next in path)
        {
            if (ctx.State.GetEntity(next) != null) continue;
            if (!ctx.State.ChangeEntityPosition(from, next, applyTerrain: !Flying)) return;
            from = next;
        }
    }
}
