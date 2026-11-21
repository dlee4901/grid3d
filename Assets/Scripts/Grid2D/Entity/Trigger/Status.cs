using System.Collections.Generic;

public sealed class Status : ITriggered
{
    public StatusConfig Config { get; }

    public string Id => Config.Id;
    public Trigger Trigger => Config.Trigger;
    public GridSelection Area => Config.Area;
    public List<AbilityEffect> Effects => Config.Effects;
    public int Count { get; private set; }

    internal Status(StatusConfig config, int count)
    {
        Config = config;
        Count = count;
    }

    internal void Adjust(int amount) => Count += amount;
}
