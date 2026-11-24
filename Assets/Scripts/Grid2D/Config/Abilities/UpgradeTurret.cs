using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig UpgradeTurret = new()
    {
        Id = "Upgrade Turret",
        DisplayName = "Upgrade Turret",
        Description = "Upgrade an adjacent Turret by one level, up to 3.",
        Effects = new List<AbilityEffect>
        {
            new AbilityCounterEffect
            {
                Relation = Occupant.Units & ~Occupant.Self,
                When = Is.Unit("Turret") & (Var.TargetCounter("Upgrade Level") < 3),
                Ability = "Upgrade Level",
                Max = 3
            }
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
