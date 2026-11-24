public sealed class SpawnEffect : AbilityEffect
{
    public string Unit { get; init; }
    public bool Exact { get; init; }
    public System.Collections.Generic.List<(string Id, int Count)> Statuses { get; init; } = new();

    public SpawnEffect() => Relation = Occupant.Empty;

    internal override void Apply(in EffectContext ctx)
    {
        if (ctx.Target != null) return;
        var entity = Exact ? ctx.Source?.Copy() : Create(ctx);
        if (entity == null || !ctx.State.SetEntityPosition(ctx.Position, entity)) return;
        foreach (var (id, count) in Statuses)
            if (IdRegistry<StatusConfig>.TryGet(id, out var status)) ctx.State.ApplyStatus(entity, status, count);
    }

    private Entity Create(in EffectContext ctx)
    {
        var id = Unit ?? ctx.Source?.Id;
        if (!IdRegistry<EntityConfig>.TryGet(id, out var config))
        {
            GridLog.Error($"[SpawnEffect] unknown unit '{id}'");
            return null;
        }
        var player = 0;
        if (ctx.Source != null && ctx.Source.TryGetComponent<ControlComponent>(out var control)) player = control.PlayerController;
        return Entity.Create(config, player);
    }
}
