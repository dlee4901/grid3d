public sealed class DamageEffect : AbilityEffect
{
    public Value Amount { get; init; } = 0;

    internal override void Apply(in EffectContext ctx)
    {
        if (!ctx.Target.TryGetComponent<HealthComponent>(out var health)) return;
        if (health.ApplyDamage(Amount.Eval(ctx))) ctx.State.RemoveEntity(ctx.Target.Position);
    }
}
