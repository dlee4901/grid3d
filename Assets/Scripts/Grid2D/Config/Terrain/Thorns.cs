using System.Collections.Generic;

public static partial class TerrainConfigs
{
    public static readonly TerrainConfig Thorns = new()
    {
        Id = "Thorns",
        DisplayName = "Thorns",
        Description = "When a unit moves onto this square, it takes {Damage} damage.",
        AppliesOnPlace = false,
        Effects = new List<AbilityEffect>
        {
            new DamageEffect { Relation = Occupant.Self, Amount = 1 }
        }
    };
}
