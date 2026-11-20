using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig KnifeStab = new()
    {
        Id = "Knife Stab",
        DisplayName = "Knife Stab",
        Description = "Stab an adjacent unit for {Damage} damage.",
        ManaCost = 1,
        Effects = new List<AbilityEffect>
        {
            new DamageEffect { Relation = Occupant.Units & ~Occupant.Self, Amount = 3 }
        },
        Targeting = new PositionTargeting
        {
            SelectableArea = new GridSelection
            {
                MinDistance = 1,
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal { Direction = GridDirection.Straight, Linear = false, MaxDistance = 1, Passthrough = Occupant.Any }
                }
            }
        }
    };
}
