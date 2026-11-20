public static class DamageBreakdown
{
    public static DamageRoll ForCaster(GridState state, Entity caster, int baseAmount)
    {
        var roll = new DamageRoll(baseAmount, null, true);
        if (caster == null || !caster.TryGetComponent<TraitComponent>(out var traits)) return roll;
        var origin = new GridSource(state, caster.Position, caster);
        var dealt = new TriggerEvent(GameEvent.DamageDealt, caster, null, 0, roll);
        foreach (var trait in traits.All)
        {
            var trigger = trait.Trigger;
            if (trigger == null || trigger.On != GameEvent.DamageDealt || (trigger.Subject & Occupant.Self) == 0) continue;
            var ctx = new EffectContext(origin, null, trait, dealt);
            if (!trigger.When.Eval(ctx)) continue;
            foreach (var effect in trait.Effects)
                if (effect is ModifyDamageEffect && effect.When.Eval(ctx)) effect.Apply(ctx);
        }
        return roll;
    }
}
