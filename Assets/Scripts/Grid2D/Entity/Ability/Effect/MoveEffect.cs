public sealed class MoveEffect : AbilityEffect
{
    public bool Teleport { get; init; }

    public MoveEffect() => Relation = Occupant.Empty;

    internal override void Apply(in EffectContext ctx)
    {
        var from = ctx.Source.Position;
        if (Teleport)
        {
            ctx.State.ChangeEntityPosition(from, ctx.Position, true);
            return;
        }
        var path = ctx.Ability?.Targeting.GetSelectableSteps(ctx.Origin).PathTo(from, ctx.Position);
        if (path == null)
        {
            ctx.State.ChangeEntityPosition(from, ctx.Position);
            return;
        }
        foreach (var next in path)
        {
            if (!ctx.State.ChangeEntityPosition(from, next)) return;
            from = next;
        }
    }
}
