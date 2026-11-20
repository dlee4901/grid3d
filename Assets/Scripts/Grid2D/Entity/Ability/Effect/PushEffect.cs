using System;

public sealed class PushEffect : AbilityEffect
{
    public Value Distance { get; init; } = 1;

    protected override string DefaultDescriptionToken => "Push";

    public override bool TryGetValueRange(GridState state, out int min, out int max) => Distance.TryGetRange(out min, out max);

    internal override void Apply(in EffectContext ctx)
    {
        if (ctx.Target == null) return;
        var origin = ctx.Origin.Position;
        var dx = Math.Sign(ctx.Target.Position.Dim2.x - origin.Dim2.x);
        var dy = Math.Sign(ctx.Target.Position.Dim2.y - origin.Dim2.y);
        var position = ctx.Target.Position;
        for (var i = Distance.Eval(ctx); i > 0; i--)
        {
            var next = new GridPosition(ctx.State, (position.Dim2.x + dx, position.Dim2.y + dy));
            if (!ctx.State.IsTraversable(next)) break;
            ctx.State.ChangeEntityPosition(position, next);
            position = next;
        }
    }
}
