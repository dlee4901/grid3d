using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Fireball = new()
    {
        Id = "Fireball",
        DisplayName = "Fireball",
        Description = "Deal {Damage} damage and apply {Burning} in an area.",
        Effects = new List<AbilityEffect>()
        {
            new DamageEffect()
            {
                Relation = Occupant.Units & ~Occupant.Self,
                Amount = 3,
                Area = true
            },
            new TerrainEffect()
            {
                Relation = Occupant.Any,
                Terrain = TerrainType.Burning
            }
        },
        Targeting = new PositionTargeting()
        {
            EffectArea = new GridSelection()
            {
                Traversals = new List<GridTraversal>()
                {
                    new GridTraversal()
                    {
                        Direction = GridDirection.All,
                        Linear = false,
                        MaxDistance = 1,
                        Passthrough = Occupant.Any
                    }
                }
            },
            SelectableArea = new GridSelection()
            {
                Traversals = new List<GridTraversal>()
                {
                    new GridTraversal()
                    {
                        Direction = GridDirection.Orthogonal,
                        Linear = false,
                        MaxDistance = 2,
                        Passthrough = Occupant.Any
                    }
                }
            }
        }
    };
}
