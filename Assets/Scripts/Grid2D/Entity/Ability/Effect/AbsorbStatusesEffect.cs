public sealed class AbsorbStatusesEffect : AbilityEffect
{
    internal override void Apply(in EffectContext ctx)
    {
        if (ctx.Target == null || ctx.Source == null || !ctx.Target.TryGetComponent<StatusComponent>(out var statuses)) return;
        foreach (var status in new System.Collections.Generic.List<Status>(statuses.Statuses))
            ctx.State.ApplyStatus(ctx.Source, status.Config, status.Count, status.Param);
    }
}
