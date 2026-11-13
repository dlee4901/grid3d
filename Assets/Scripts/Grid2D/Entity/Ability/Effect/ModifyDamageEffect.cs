public sealed class ModifyDamageEffect : AbilityEffect
{
    public Value Amount { get; init; } = 0;
    public bool Prevent { get; init; }

    public ModifyDamageEffect() => Relation = Occupant.Self;

    internal override void Apply(in EffectContext ctx)
    {
        if (ctx.Damage == null)
        {
            GridLog.Error("[ModifyDamageEffect] used outside a damage trigger");
            return;
        }
        if (Prevent) ctx.Damage.Prevented = true;
        else ctx.Damage.Amount += Amount.Eval(ctx);
    }
}
