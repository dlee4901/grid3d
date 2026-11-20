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

    internal void OnCast(Ability cast)
    {
        cast.Trigger();
        if ((cast.Keywords & Keyword.Opener) != 0) return;
        foreach (var ability in _list)
            if (ability != cast && (ability.Keywords & Keyword.Finisher) == 0) ability.ApplyCooldown(1);
    }
}