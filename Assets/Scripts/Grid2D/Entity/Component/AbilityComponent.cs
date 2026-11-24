using System.Collections.Generic;

public class AbilityComponent : IEntityComponent
{
    private readonly List<Ability> _list = new();
    private readonly Dictionary<string, Ability> _dictionary = new();
    public IReadOnlyList<Ability> List => _list;
    public IReadOnlyDictionary<string, Ability> Dictionary => _dictionary;
    
    public AbilityComponent(List<string> abilityIds)
    {
        foreach (var abilityId in abilityIds)
        {
            if (!IdRegistry<AbilityConfig>.TryGet(abilityId, out var config)) continue;
            var ability = new Ability(config);
            _list.Add(ability);
            _dictionary.Add(abilityId, ability);
        }
    }

    private AbilityComponent() { }

    public IEntityComponent Copy()
    {
        var copy = new AbilityComponent();
        foreach (var ability in _list)
        {
            var clone = ability.Copy();
            copy._list.Add(clone);
            copy._dictionary.Add(clone.Id, clone);
        }
        return copy;
    }

    internal void OnCast(Ability cast) => cast.RecordCast();
}