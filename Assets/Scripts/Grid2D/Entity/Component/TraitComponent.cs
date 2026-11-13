using System.Collections.Generic;

public class TraitComponent : IEntityComponent
{
    private readonly List<Trait> _traits = new();
    private readonly List<Status> _statuses = new();
    public IReadOnlyList<Trait> Traits => _traits;
    public IReadOnlyList<Status> Statuses => _statuses;

    public TraitComponent(List<string> traitIds)
    {
        if (traitIds == null) return;
        foreach (var id in traitIds)
            if (IdRegistry<TraitConfig>.TryGet(id, out var config)) _traits.Add(new Trait(config));
    }

    internal IEnumerable<Trait> All
    {
        get
        {
            foreach (var trait in _traits) yield return trait;
            foreach (var status in _statuses) yield return status;
        }
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
