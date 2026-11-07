using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig AxeSwing = new()
    {
        Id = "Axe Swing",
        ManaCost = 1,
        Effects = new List<AbilityEffect>
        {
            new CooldownEffect { Relation = Occupant.Any & ~Occupant.Self, Turns = 2, When = Var.Distance >= 2 },
            new DamageEffect { Relation = Occupant.Any & ~Occupant.Self, Amount = Value.If(Var.Distance <= 1, 5, 3) }
        },
        Targeting = new DirectionTargeting
        {
            Grouping = new[] {1, 0, 2, 0, 3, 0, 4, 0},
            MaxHits = 1,
            EffectArea = new GridSelection
            {
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal
                    {
                        Direction = GridDirection.Straight, Linear = true, MaxDistance = 3, Passthrough = Occupant.Any
                    }
                }
            }
        }
    };
}
