using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig SwordSlash = new()
    {
        Id = "Sword Slash",
        ManaCost = 1,
        Effects = new List<AbilityEffect>
        {
            new DamageEffect { Relation = Occupant.Units & ~Occupant.Self, Amount = 4 }
        },
        Targeting = new DirectionTargeting
        {
            Grouping = new[] {1, 0, 2, 0, 3, 0, 4, 0},
            EffectArea = new GridSelection
            {
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal { Direction = GridDirection.NorthCone, MaxDistance = 1, Linear = false, Passthrough = Occupant.Any },
                    new GridTraversal { Direction = GridDirection.EastCone, MaxDistance = 1, Linear = false, Passthrough = Occupant.Any },
                    new GridTraversal { Direction = GridDirection.SouthCone, MaxDistance = 1, Linear = false, Passthrough = Occupant.Any },
                    new GridTraversal { Direction = GridDirection.WestCone, MaxDistance = 1, Linear = false, Passthrough = Occupant.Any }
                }
            }
        }
    };
}
