public sealed class StatusEffect : AbilityEffect
{
    public string Status { get; init; }
    public Value Amount { get; init; } = 1;

    internal override void Apply(in EffectContext ctx)
    {
        if (ctx.Target == null || !ctx.Target.TryGetComponent<TraitComponent>(out var traits)) return;
        if (!IdRegistry<StatusConfig>.TryGet(Status, out var config))
        {
            GridLog.Error($"[StatusEffect] unknown status '{Status}'");
            return;
        }
        traits.AdjustStatus(config, Amount.Eval(ctx));
    }
}
