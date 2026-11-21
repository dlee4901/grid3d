public static class DamageBreakdown
{
    public static DamageRoll ForCaster(GridState state, Entity caster, int baseAmount)
    {
        var roll = new DamageRoll(baseAmount, null, true);
        if (caster == null) return roll;
        var origin = new GridSource(state, caster.Position, caster);
        var dealt = new TriggerEvent(GameEvent.DamageDealt, caster, null, 0, roll);
        foreach (var triggered in TriggerSystem.Of(caster))
        {
            var trigger = triggered.Trigger;
            if (trigger == null || trigger.On != GameEvent.DamageDealt || (trigger.Subject & Occupant.Self) == 0) continue;
            var ctx = new EffectContext(origin, null, triggered, dealt);
            if (!trigger.When.Eval(ctx)) continue;
            foreach (var effect in triggered.Effects)
                if (effect is ModifyDamageEffect && effect.When.Eval(ctx)) effect.Apply(ctx);
        }
        return roll;
    }
}
