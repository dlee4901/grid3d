using System.Collections.Generic;

public sealed class Status : ITriggered
{
    public StatusConfig Config { get; }

    public string Id => Config.Id;
    public Trigger Trigger => Config.Trigger;
    public GridSelection Area => Config.Area;
    public List<AbilityEffect> Effects => Config.Effects;
    public int Count { get; private set; }
    public int Param { get; internal set; }

    internal Status(StatusConfig config, int count, int param)
    {
        Config = config;
        Count = count;
        Param = param;
    }

    internal void Adjust(int amount) => Count += amount;

    internal Status Copy() => new(Config, Count, Param);
}
