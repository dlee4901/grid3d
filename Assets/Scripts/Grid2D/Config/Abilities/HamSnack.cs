using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig HamSnack = new()
    {
        Id = "Ham Snack",
        ManaCost = 1,
        Cooldown = 2,
        Effects = new List<AbilityEffect>
        {
            new HealEffect { Relation = Occupant.Self, Amount = 3 }
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
