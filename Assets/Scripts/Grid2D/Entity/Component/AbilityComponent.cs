using System.Collections.Generic;

public class AbilityComponent : IEntityComponent
{
    public List<Ability> List { get; set; } = new List<Ability>();
    public Dictionary<string, Ability> Dictionary { get; set; } = new Dictionary<string, Ability>();
    
    public AbilityComponent(List<string> abilityIds)
    {
        foreach (var abilityId in abilityIds)
        {
            if (!IdRegistry<AbilityConfig>.TryGet(abilityId, out var config)) continue;
            var ability = Ability.Create(config);
            List.Add(ability);
            Dictionary.Add(abilityId, ability);
        }
    }
}