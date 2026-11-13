using System.Collections.Generic;

public abstract class EffectConfig : INameId
{
    public string Id { get; init; }
    public List<AbilityEffect> Effects { get; init; } = new();
}
