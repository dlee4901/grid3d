using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Fireball = new()
    {
        Id = "Fireball",
        DisplayName = "Fireball",
        Description = "Deal {Damage} damage and apply {Burning} in an area.",
        ManaCost = 2,
        Effects = new List<AbilityEffect>()
        {
            new TerrainEffect()
            {
                Relation = Occupant.Any,
                Terrain = TerrainType.Burning
            },
            new DamageEffect()
            {
                Relation = Occupant.Units & ~Occupant.Self,
                Amount = 3
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
                        Direction = GridDirection.Line,
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
                        Direction = GridDirection.Straight,
                        Linear = false,
                        MaxDistance = 2,
                        Passthrough = Occupant.Any
                    }
                }
            }
        }
    };
}
