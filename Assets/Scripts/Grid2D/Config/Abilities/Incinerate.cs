using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Incinerate = new()
    {
        Id = "Incinerate",
        DisplayName = "Incinerate",
        Description = "Blast a cone for 4 damage, falling to 2 at its far end. Each target takes 1 more per Burn stack it has.",
        Cooldown = 2,
        Effects = new List<AbilityEffect>
        {
            new DamageEffect { Relation = Occupant.Units & ~Occupant.Self, Amount = 5 - Var.Distance + Var.TargetStatus("Burn"), Area = true }
        },
        Targeting = new DirectionTargeting
        {
            Grouping = new[] {1, 0, 2, 0, 3, 0, 4, 0},
            EffectArea = new GridSelection
            {
                MinDistance = 1,
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal
                    {
                        Direction = GridDirection.North, Linear = true, MaxDistance = 1, Passthrough = Occupant.Any,
                        Chain = new GridTraversal { Direction = GridDirection.NorthCone, Linear = false, MaxDistance = 2, Passthrough = Occupant.Any }
                    },
                    new GridTraversal
                    {
                        Direction = GridDirection.East, Linear = true, MaxDistance = 1, Passthrough = Occupant.Any,
                        Chain = new GridTraversal { Direction = GridDirection.EastCone, Linear = false, MaxDistance = 2, Passthrough = Occupant.Any }
                    },
                    new GridTraversal
                    {
                        Direction = GridDirection.South, Linear = true, MaxDistance = 1, Passthrough = Occupant.Any,
                        Chain = new GridTraversal { Direction = GridDirection.SouthCone, Linear = false, MaxDistance = 2, Passthrough = Occupant.Any }
                    },
                    new GridTraversal
                    {
                        Direction = GridDirection.West, Linear = true, MaxDistance = 1, Passthrough = Occupant.Any,
                        Chain = new GridTraversal { Direction = GridDirection.WestCone, Linear = false, MaxDistance = 2, Passthrough = Occupant.Any }
                    }
                }
            }
        }
    };
}
