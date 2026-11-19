using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig CrossbowShot = new()
    {
        Id = "Crossbow Shot",
        ManaCost = 1,
        Effects = new List<AbilityEffect>
        {
            new DamageEffect { Relation = Occupant.Units & ~Occupant.Self, Amount = 2 }
        },
        Targeting = new DirectionTargeting
        {
            MaxHits = 1,
            EffectArea = new GridSelection
            {
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal
                    {
                        Direction = GridDirection.Diagonal,
                        Linear = true,
                        MaxDistance = 4,
                        Passthrough = Occupant.Any
                    }
                }
            }
        }
    };
}
