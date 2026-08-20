public sealed class DamageEffect : AbilityEffect
{
    public int Amount { get; set; }

    public override void Apply(EffectContext ctx, Entity target)
    {
        if (!target.TryGetComponent<HealthComponent>(out var health)) return;
        if (health.ApplyDamage(Amount)) ctx.State.RemoveEntity(target.Position);
    }
}