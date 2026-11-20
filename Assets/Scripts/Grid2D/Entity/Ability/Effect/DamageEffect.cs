public sealed class DamageEffect : AbilityEffect
{
    public Value Amount { get; init; } = 0;

    protected override string DefaultDescriptionToken => "Damage";

    public override bool TryGetValueRange(GridState state, out int min, out int max) => Amount.TryGetRange(out min, out max);

    internal override void Apply(in EffectContext ctx)
    {
        if (ctx.Target == null || !ctx.Target.TryGetComponent<HealthComponent>(out var health)) return;
        var state = ctx.State;
        var distance = ctx.Source.Position.StepsTo(ctx.Target.Position);
        var hit = new DamageRoll(Amount.Eval(ctx), ctx.Trait?.Id, ctx.Ability != null);
        state.Triggers.Fire(new TriggerEvent(GameEvent.DamageDealt, ctx.Source, ctx.Target, distance, hit));
        state.Triggers.Fire(new TriggerEvent(GameEvent.DamageTaken, ctx.Target, ctx.Source, distance, hit));
        if (hit.Prevented || hit.Amount <= 0) return;
        var killed = health.ApplyDamage(hit.Amount);
        state.Triggers.Emit(new TriggerEvent(GameEvent.Damaged, ctx.Target, ctx.Source, distance, hit));
        if (!killed) return;
        state.RemoveEntity(ctx.Target.Position);
        state.Triggers.Emit(new TriggerEvent(GameEvent.Died, ctx.Target, ctx.Source, distance, hit));
    }
}
