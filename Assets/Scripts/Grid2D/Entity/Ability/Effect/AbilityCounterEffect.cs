public sealed class AbilityCounterEffect : AbilityEffect
{
    public string Ability { get; init; }
    public int Amount { get; init; } = 1;
    public int Max { get; init; } = int.MaxValue;

    internal override void Apply(in EffectContext ctx)
    {
        if (ctx.Target == null || !ctx.Target.TryGetComponent<AbilityComponent>(out var abilities)) return;
        if (abilities.Dictionary.TryGetValue(Ability, out var ability)) ability.AdjustCount(Amount, Max);
    }
}
