using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig AxeSwing = new()
    {
        Id = "Axe Swing",
        ManaCost = 1,
        Effects = new List<AbilityEffect>
        {
            new CooldownEffect { Relation = Occupant.Units & ~Occupant.Self, Turns = 1, When = Var.Distance >= 2 },
            new DamageEffect { Relation = Occupant.Units & ~Occupant.Self, Amount = Value.If(Var.Distance <= 1, 5, 3) }
        },
        Targeting = new DirectionTargeting
        {
            MaxHits = 1,
            EffectArea = new GridSelection
            {
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal
                    {
                        Direction = GridDirection.Line, Linear = true, MaxDistance = 3, Passthrough = Occupant.Any
                    }
                }
            }
        }
    };
}
