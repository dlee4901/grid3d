using System.Collections.Generic;

public static class RegistryValidator
{
    public static List<string> Validate(GridDefinition map, IEnumerable<TeamData> teams)
    {
        var issues = new List<string>();
        var checkedEntities = new HashSet<string>();

        if (map?.EntityStartPositions != null)
            foreach (var spec in map.EntityStartPositions)
                CheckEntity(spec.EntityId, $"Map '{map.Id}'", checkedEntities, issues);

        foreach (var team in teams)
            foreach (var unitId in team.UnitStartPositions.Values)
                CheckEntity(unitId, $"Team '{team.Id}'", checkedEntities, issues);

        return issues;
    }

    private static void CheckEntity(string entityId, string source, HashSet<string> seen, List<string> issues)
    {
        if (!IdRegistry<EntityConfig>.TryGet(entityId, out var config))
        {
            issues.Add($"{source} references unregistered entity '{entityId}'");
            return;
        }
        if (!seen.Add(entityId) || config.Abilities == null) return;
        foreach (var abilityId in config.Abilities)
            if (!IdRegistry<AbilityConfig>.TryGet(abilityId, out _))
                issues.Add($"Entity '{entityId}' references unregistered ability '{abilityId}'");
    }
}
