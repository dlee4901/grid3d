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
            var ability = Ability.Create(config);
            _list.Add(ability);
            _dictionary.Add(abilityId, ability);
        }
    }
}