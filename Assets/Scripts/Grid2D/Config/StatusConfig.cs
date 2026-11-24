using System.Collections.Generic;

public enum StatusCounter { Increment, Decrement, None }
public enum StatusParam { None, SourceUid, Direction }

public sealed class StatusConfig : EffectConfig
{
    public Trigger Trigger { get; init; }
    public GridSelection Area { get; init; }
    public List<Reaction> Reactions { get; init; } = new();
    public StatusCounter Counter { get; init; } = StatusCounter.Increment;
    public StatusParam Param { get; init; } = StatusParam.None;
    public bool OnePerSource { get; init; }
    public List<AbilityEffect> OnExpire { get; init; } = new();
}
