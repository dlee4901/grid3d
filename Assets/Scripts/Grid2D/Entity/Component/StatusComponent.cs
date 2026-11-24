using System.Collections.Generic;

public class StatusComponent : IEntityComponent
{
    private readonly List<Status> _statuses = new();
    public IReadOnlyList<Status> Statuses => _statuses;

    public StatusComponent(List<(string Id, int Count)> statuses = null)
    {
        if (statuses != null)
            foreach (var (id, count) in statuses)
                if (IdRegistry<StatusConfig>.TryGet(id, out var config)) Apply(config, count, 0);
    }

    private StatusComponent() { }

    public IEntityComponent Copy()
    {
        var copy = new StatusComponent();
        foreach (var status in _statuses) copy._statuses.Add(status.Copy());
        return copy;
    }

    public Status Find(StatusConfig config) => _statuses.Find(status => status.Config == config);

    internal bool Apply(StatusConfig config, int amount, int param)
    {
        var status = Find(config);
        if (status == null)
        {
            if (amount > 0) _statuses.Add(new Status(config, config.Counter == StatusCounter.None ? 1 : amount, param));
            return false;
        }
        if (amount > 0)
        {
            if (config.Param != StatusParam.None) status.Param = param;
            if (config.Counter == StatusCounter.Increment) status.Adjust(amount);
            else if (config.Counter == StatusCounter.Decrement) status.Adjust(-1);
        }
        else status.Adjust(amount);
        if (status.Count > 0) return false;
        _statuses.Remove(status);
        return true;
    }

    internal void Remove(Status status) => _statuses.Remove(status);
}
