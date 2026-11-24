using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig SummonThorns = new()
    {
        Id = "Summon Thorns",
        DisplayName = "Summon Thorns",
        Description = "Grow {Thorns} in an area.",
        Cooldown = 2,
        Effects = new List<AbilityEffect>
        {
            new TerrainEffect { Relation = Occupant.Any, Terrain = TerrainType.Thorns, Rounds = 3 }
        },
        Targeting = new PositionTargeting
        {
            SelectableArea = new GridSelection
            {
                MinDistance = 1,
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal { Direction = GridDirection.Orthogonal, Linear = false, MaxDistance = 2, Passthrough = Occupant.Any }
                }
            },
            EffectArea = new GridSelection
            {
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal { Direction = GridDirection.All, Linear = true, MaxDistance = 1, Passthrough = Occupant.Any }
                }
            }
        }
    };
}
