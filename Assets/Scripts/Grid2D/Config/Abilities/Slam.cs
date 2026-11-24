using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Slam = new()
    {
        Id = "Slam",
        DisplayName = "Slam",
        Description = "Slam an adjacent unit for {Damage} damage.",
        Effects = new List<AbilityEffect>
        {
            new DamageEffect { Relation = Occupant.Units & ~Occupant.Self, Amount = 1 }
        },
        Targeting = new PositionTargeting
        {
            SelectableArea = new GridSelection
            {
                MinDistance = 1,
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal { Direction = GridDirection.Orthogonal, Linear = true, MaxDistance = 1, Passthrough = Occupant.Any }
                }
            }
        }
    };
}
