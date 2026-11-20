public sealed class CooldownEffect : AbilityEffect
{
    public Value Turns { get; init; } = 0;

    protected override string DefaultDescriptionToken => "Cooldown";

    public override bool TryGetValueRange(GridState state, out int min, out int max) => Turns.TryGetRange(out min, out max);

    internal override void Apply(in EffectContext ctx)
    {
        if (ctx.Ability == null)
        {
            GridLog.Error("[CooldownEffect] used outside an ability");
            return;
        }
        ctx.Ability.SetCastCooldown(Turns.Eval(ctx));
    }
}
