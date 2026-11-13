using System.Collections.Generic;

public class Trait : INameId
{
    public TraitConfig Config { get; }

    public string Id => Config.Id;
    public Trigger Trigger => Config.Trigger;
    public GridSelection Area => Config.Area;
    public List<AbilityEffect> Effects => Config.Effects;
    public virtual int Count => 1;

    internal Trait(TraitConfig config) => Config = config;
}
