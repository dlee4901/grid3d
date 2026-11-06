public sealed class HealEffect : AbilityEffect
{
    public int Amount { get; init; }

    internal override void Apply(EffectContext ctx, Entity target)
    {
        if (target.TryGetComponent<HealthComponent>(out var health)) health.ApplyHeal(Amount);
    }
}
