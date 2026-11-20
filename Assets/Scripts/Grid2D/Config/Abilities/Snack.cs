using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Snack = new()
    {
        Id = "Snack",
        DisplayName = "Snack",
        Description = "Gain {Power}.",
        ManaCost = 1,
        Cooldown = 2,
        Effects = new List<AbilityEffect>
        {
            new StatusEffect { Relation = Occupant.Self, Status = "Power", Amount = 1 }
        },
        Targeting = new FillTargeting
        {
            EffectArea = new GridSelection
            {
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal { Direction = GridDirection.Straight, MaxDistance = 0 }
                }
            }
        }
    };
}
