using System.Collections.Generic;

public static partial class TerrainConfigs
{
    public static readonly TerrainConfig Burning = new()
    {
        Id = "Burning",
        DisplayName = "Burning",
        Description = "If unit on square, apply {Burn} then become {Plain}.",
        Effects = new List<AbilityEffect>
        {
            new StatusEffect { Relation = Occupant.Self, Status = "Burn", Amount = 1 },
            new TerrainEffect { Relation = Occupant.Self, Terrain = TerrainType.Plain }
        }
    };
}
