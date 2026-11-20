using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig ShieldBash = new()
    {
        Id = "Shield Bash",
        DisplayName = "Shield Bash",
        Description = "Bash an adjacent unit for {Damage} damage and push it back {Push} squares.",
        ManaCost = 1,
        Effects = new List<AbilityEffect>
        {
            new DamageEffect { Relation = Occupant.Units & ~Occupant.Self, Amount = 2 },
            new PushEffect { Relation = Occupant.Units & ~Occupant.Self, Distance = 2 }
        },
        Targeting = new PositionTargeting
        {
            SelectableArea = new GridSelection
            {
                MinDistance = 1,
                Selectable = Occupant.Units,
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal { Direction = GridDirection.Straight, Linear = false, MaxDistance = 1, Passthrough = Occupant.Any }
                }
            }
        }
    };
}
