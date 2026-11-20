using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig AxeSwing = new()
    {
        Id = "Axe Swing",
        DisplayName = "Axe Swing",
        Description = "Deal {FarDamage} damage to first target hit. If target is adjacent, deal {NearDamage} and reduce cooldown to {Cooldown}.",
        ManaCost = 1,
        Cooldown = 2,
        Effects = new List<AbilityEffect>
        {
            new CooldownEffect { Relation = Occupant.Units & ~Occupant.Self, Turns = 1, When = Var.Distance < 2 },
            new DamageEffect { DescriptionToken = "NearDamage", Relation = Occupant.Units & ~Occupant.Self, Amount = 5, When = Var.Distance <= 1 },
            new DamageEffect { DescriptionToken = "FarDamage", Relation = Occupant.Units & ~Occupant.Self, Amount = 3, When = Var.Distance >= 2 }
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
