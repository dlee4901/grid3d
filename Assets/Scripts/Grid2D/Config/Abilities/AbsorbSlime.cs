using System.Collections.Generic;

public static partial class AbilityConfigs
{
    private static readonly Condition IsSlime = Is.Unit("Slime");

    public static readonly AbilityConfig AbsorbSlime = new()
    {
        Id = "Absorb Slime",
        DisplayName = "Absorb Slime",
        Description = "Absorb an adjacent Slime: gain all of its statuses and {Power}, then kill it.",
        Effects = new List<AbilityEffect>
        {
            new AbsorbStatusesEffect { Relation = Occupant.Units & ~Occupant.Self, When = IsSlime },
            new StatusEffect { Relation = Occupant.Units & ~Occupant.Self, When = IsSlime, OnCaster = true, Status = "Power" },
            new KillEffect { Relation = Occupant.Units & ~Occupant.Self, When = IsSlime }
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
