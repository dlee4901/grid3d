public class AbilityConfig : EffectConfig
{
    public int ManaCost { get; init; } = 1;
    public Keyword Keywords { get; init; }
    public AbilityTargeting Targeting { get; init; }

    public int Warmup { get; init; } = 0;
    public int Cooldown { get; init; } = 0;
    public int Delay { get; init; } = 0;
    public bool Locked { get; init; } = false;
}
