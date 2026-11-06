using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig CrossbowShot = new()
    {
        Id = "Crossbow Shot",
        ManaCost = 1,
        Effects = new List<AbilityEffect>
        {
            new DamageEffect { Relation = Occupant.Any & ~Occupant.Self, Amount = 2 }
        },
        Targeting = new DirectionTargeting
        {
            Grouping = new[] {1, 0, 2, 0, 3, 0, 4, 0},
            MaxHits = 1,
            EffectArea = new GridSelection
            {
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal
                    {
                        Direction = GridDirection.Straight,
                        Linear = true,
                        MaxDistance = 4,
                        Passthrough = Occupant.Any
                    }
                }
            }
        }
    };
}
