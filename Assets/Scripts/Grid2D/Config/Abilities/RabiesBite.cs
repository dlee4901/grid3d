using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig RabiesBite = new()
    {
        Id = "Rabies Bite",
        DisplayName = "Rabies Bite",
        Description = "Bite an adjacent unit, giving it {Rabies}.",
        Effects = new List<AbilityEffect>
        {
            new StatusEffect { Relation = Occupant.Units & ~Occupant.Self, Status = "Rabies", Amount = 4 }
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
