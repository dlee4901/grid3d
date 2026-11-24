using System.Collections.Generic;

public class EntityConfig : INameId
{
    public const int MaxAbilities = 5;

    public string Id { get; init; }
    public int Cost { get; init; } = 0;
    public int Health { get; init; } = 0;
    public List<string> Abilities { get; init; }
    public List<(string Id, int Count)> Statuses { get; init; } = new();
}
