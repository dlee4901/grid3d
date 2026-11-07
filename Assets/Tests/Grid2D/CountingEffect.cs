public sealed class CountingEffect : AbilityEffect
{
    public int Hits { get; private set; }

    public void Reset() => Hits = 0;

    internal override void Apply(in EffectContext ctx) => Hits++;
}
