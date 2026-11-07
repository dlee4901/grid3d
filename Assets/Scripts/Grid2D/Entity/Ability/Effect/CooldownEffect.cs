public sealed class CooldownEffect : AbilityEffect
{
    public Value Turns { get; init; } = 0;

    internal override void Apply(in EffectContext ctx) => ctx.Ability.ApplyCooldown(Turns.Eval(ctx));
}
