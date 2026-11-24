public static class Is
{
    public static readonly Condition AbilityDamage = new FuncCondition(ctx => ctx.Damage != null && ctx.Damage.FromAbility);
    public static readonly Condition AreaDamage = new FuncCondition(ctx => ctx.Damage != null && ctx.Damage.Area);
    public static Condition DamageFrom(string id) => new FuncCondition(ctx => ctx.Damage?.Cause == id);
    public static Condition OnTerrain(TerrainType terrain)
        => new FuncCondition(ctx => ctx.State.TryGetTerrain(ctx.Position, out var current) && current == terrain);
    public static Condition Unit(string id) => new FuncCondition(ctx => ctx.Target?.Id == id);

    public static readonly Condition FirstInLine = new FuncCondition(ctx =>
    {
        if (ctx.Target == null || ctx.Source == null) return false;
        var from = ctx.Source.Position.Dim2;
        var to = ctx.Target.Position.Dim2;
        var dx = System.Math.Sign(to.x - from.x);
        var dy = System.Math.Sign(to.y - from.y);
        for (var step = (x: from.x + dx, y: from.y + dy); step != to; step = (step.x + dx, step.y + dy))
            if (ctx.State.GetEntity(new GridPosition(ctx.State, step)) != null) return false;
        return true;
    });

    public static readonly Condition FromBehind = new FuncCondition(ctx =>
    {
        if (ctx.Target == null || ctx.Source == null) return false;
        var (x, y) = ctx.Target.Position.Dim2;
        var behind = ctx.Target.Facing switch
        {
            GridDirectionFacing.North => (x, y - 1),
            GridDirectionFacing.East => (x - 1, y),
            GridDirectionFacing.South => (x, y + 1),
            _ => (x + 1, y)
        };
        return ctx.Source.Position.Dim2 == behind;
    });

    public static readonly Condition FromStatusSource = new FuncCondition(ctx => ctx.Event?.Other != null && ctx.Event.Other.Uid == StatusParam(ctx));
    public static readonly Condition SubjectIsStatusSource = new FuncCondition(ctx => ctx.Event?.Subject != null && ctx.Event.Subject.Uid == StatusParam(ctx));

    public static readonly Condition FromStatusDirection = new FuncCondition(ctx =>
    {
        var attacker = ctx.Event?.Other;
        if (attacker == null || ctx.Source == null) return false;
        var (ax, ay) = attacker.Position.Dim2;
        var (hx, hy) = ctx.Source.Position.Dim2;
        if (ax != hx && ay != hy) return false;
        return (int)GridUtil.DirectionBetween(ctx.Source.Position, attacker.Position) == StatusParam(ctx);
    });

    private static int StatusParam(in EffectContext ctx)
    {
        if (ReactionInstance.OwnerOf(ctx.Triggered) is Status status) return status.Param;
        return -1;
    }
}
