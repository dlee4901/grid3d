public sealed class StatusEffect : AbilityEffect
{
    public string Status { get; init; }
    public Value Amount { get; init; } = 1;
    public bool OnCaster { get; init; }

    protected override string DefaultDescriptionToken => Status;

    public override bool TryGetValueRange(GridState state, out int min, out int max) => Amount.TryGetRange(out min, out max);

    internal override void Apply(in EffectContext ctx)
    {
        var recipient = OnCaster ? ctx.Source : ctx.Target;
        if (recipient == null) return;
        if (!IdRegistry<StatusConfig>.TryGet(Status, out var config))
        {
            GridLog.Error($"[StatusEffect] unknown status '{Status}'");
            return;
        }
        ctx.State.ApplyStatus(recipient, config, Amount.Eval(ctx), ParamFor(config, ctx));
    }

    private static int ParamFor(StatusConfig config, in EffectContext ctx) => config.Param switch
    {
        StatusParam.SourceUid => ctx.Source?.Uid ?? 0,
        StatusParam.Direction => (int)GridUtil.DirectionBetween(ctx.Origin.Position, ctx.Position),
        _ => 0
    };
}
