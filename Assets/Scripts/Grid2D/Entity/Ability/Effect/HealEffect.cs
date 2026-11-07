public sealed class HealEffect : AbilityEffect
{
    public Value Amount { get; init; } = 0;

    internal override void Apply(in EffectContext ctx)
    {
        if (ctx.Target.TryGetComponent<HealthComponent>(out var health)) health.ApplyHeal(Amount.Eval(ctx));
    }
}
