using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Fireball = new()
    {
        Id = "Fireball",
        ManaCost = 2,
        Effects = new List<AbilityEffect>()
        {
            new DamageEffect()
            {
                Relation = Occupant.Any & ~Occupant.Self,
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
                        Direction = GridDirection.Straight,
                        Linear = false,
                        MaxDistance = 2,
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
