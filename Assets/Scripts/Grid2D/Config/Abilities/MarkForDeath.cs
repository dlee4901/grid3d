using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig MarkForDeath = new()
    {
        Id = "Mark for Death",
        DisplayName = "Mark for Death",
        Description = "Give a unit within 4 squares {Marked}. Only one unit can be marked at a time.",
        Cooldown = 2,
        Effects = new List<AbilityEffect>
        {
            new StatusEffect { Relation = Occupant.Units & ~Occupant.Self, Status = "Marked" }
        },
        Targeting = new PositionTargeting
        {
            SelectableArea = new GridSelection
            {
                MinDistance = 1,
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal { Direction = GridDirection.Orthogonal, Linear = false, MaxDistance = 4, Passthrough = Occupant.Any }
                }
            }
        }
    };
}
