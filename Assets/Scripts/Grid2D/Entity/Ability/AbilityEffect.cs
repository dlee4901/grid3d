// public enum AbilityEffectType { ReplaceAbility, AddAbility, Damage, InflictStatus, GainShield }
// public enum Effect {Position, Health, Shield, Damage, Counter}

public readonly struct EffectContext
{
    public readonly GridState State;
    public readonly Entity Source;
    public readonly GridPosition SourcePosition;

    public EffectContext(GridState state, Entity source, GridPosition
        sourcePosition)
    {
        State = state;
        Source = source;
        SourcePosition = sourcePosition;
    }
}

public abstract class AbilityEffect
{
    public abstract void Apply(EffectContext ctx, Entity target);
}

// public class DamageEffect : AbilityEffect
// {
//     public int Amount { get; set; }
//
//     public override void Apply(EffectContext ctx, Entity target)
//         => Damage.Apply(ctx.State, target, Amount);
// }