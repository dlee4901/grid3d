using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Backstab = new()
    {
        Id = "Backstab",
        DisplayName = "Backstab",
        Description = "Stab an adjacent unit for {Damage} damage. From behind, kill it instead.",
        Keywords = Keyword.Opener,
        Effects = new List<AbilityEffect>
        {
            new DamageEffect { Relation = Occupant.Units & ~Occupant.Self, Amount = 4, When = !Is.FromBehind },
            new KillEffect { Relation = Occupant.Units & ~Occupant.Self, When = Is.FromBehind }
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
