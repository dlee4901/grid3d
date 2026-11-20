using System.Collections.Generic;

public abstract class EffectConfig : INameId
{
    public string Id { get; init; }
    public string DisplayName { get; init; }
    public string Description { get; init; }
    public List<AbilityEffect> Effects { get; init; } = new();
}
