using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Snack = new()
    {
        Id = "Snack",
        DisplayName = "Snack",
        Description = "Gain {Power}.",
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
                    new GridTraversal { Direction = GridDirection.Orthogonal, MaxDistance = 0 }
                }
            }
        }
    };
}
