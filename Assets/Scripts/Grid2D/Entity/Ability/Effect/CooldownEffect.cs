public sealed class CooldownEffect : AbilityEffect
{
    public Value Turns { get; init; } = 0;

    internal override void Apply(in EffectContext ctx)
    {
        if (ctx.Ability == null)
        {
            GridLog.Error("[CooldownEffect] used outside an ability");
            return;
        }
        ctx.Ability.ApplyCooldown(Turns.Eval(ctx));
    }
}
