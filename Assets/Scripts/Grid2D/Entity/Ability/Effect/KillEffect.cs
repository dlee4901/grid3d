public sealed class KillEffect : AbilityEffect
{
    internal override void Apply(in EffectContext ctx)
    {
        if (ctx.Target == null) return;
        var killer = ctx.Source == ctx.Target ? null : ctx.Source;
        ctx.State.Kill(ctx.Target, killer, ctx.Distance);
    }
}
