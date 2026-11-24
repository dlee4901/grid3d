using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig KnifeStab = new()
    {
        Id = "Knife Stab",
        DisplayName = "Knife Stab",
        Description = "Stab an adjacent unit for {Damage} damage.",
        Keywords = Keyword.Opener,
        Effects = new List<AbilityEffect>
        {
            new DamageEffect { Relation = Occupant.Units & ~Occupant.Self, Amount = 4 }
        },
        Targeting = new PositionTargeting
        {
            SelectableArea = new GridSelection
            {
                MinDistance = 1,
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal { Direction = GridDirection.Orthogonal, Linear = false, MaxDistance = 1, Passthrough = Occupant.Any }
                }
            }
        }
    };
}
