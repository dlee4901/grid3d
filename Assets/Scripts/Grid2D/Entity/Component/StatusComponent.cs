using System.Collections.Generic;

public class StatusComponent : IEntityComponent
{
    private readonly List<Status> _statuses = new();
    public IReadOnlyList<Status> Statuses => _statuses;

    public StatusComponent(List<(string Id, int Count)> statuses = null)
    {
        if (statuses != null)
            foreach (var (id, count) in statuses)
                if (IdRegistry<StatusConfig>.TryGet(id, out var config)) AdjustStatus(config, count);
    }

    internal void AdjustStatus(StatusConfig config, int amount)
    {
        var index = _statuses.FindIndex(status => status.Config == config);
        if (index < 0)
        {
            if (amount > 0) _statuses.Add(new Status(config, amount));
            return;
        }
        _statuses[index].Adjust(amount);
        if (_statuses[index].Count <= 0) _statuses.RemoveAt(index);
    }
}
