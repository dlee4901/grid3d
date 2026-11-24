using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig CrossbowShot = new()
    {
        Id = "Crossbow Shot",
        DisplayName = "Crossbow Shot",
        Description = "Fire up to 4 squares in a line, dealing {Damage} damage.",
        Keywords = Keyword.Opener,
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
                        Direction = GridDirection.Orthogonal,
                        Linear = true,
                        MaxDistance = 4,
                        Passthrough = Occupant.Any
                    }
                }
            }
        }
    };
}
