public sealed class NoOpEffect : AbilityEffect
{
    public NoOpEffect() => Relation = Occupant.Self;

    internal override void Apply(in EffectContext ctx) { }
}
