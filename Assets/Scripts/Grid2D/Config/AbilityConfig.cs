using System.Collections.Generic;

public class AbilityConfig : EffectConfig
{
    public Keyword Keywords { get; init; }
    public AbilityTargeting Targeting { get; init; }
    public Trigger Trigger { get; init; }
    public List<Reaction> Reactions { get; init; } = new();
    public GridSelection Area { get; init; }

    public int? Counter { get; init; }
    public string CounterStatus { get; init; }
    public bool IsPassive => Targeting == null;

    private int? _cooldown;

    public int Warmup { get; init; } = 0;
    public int Cooldown { get => _cooldown ?? (IsPassive ? 0 : 1); init => _cooldown = value; }
    public int Delay { get; init; } = 0;
    public bool Locked { get; init; } = false;
}
