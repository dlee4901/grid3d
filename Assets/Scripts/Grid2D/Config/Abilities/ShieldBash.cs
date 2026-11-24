using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig ShieldBash = new()
    {
        Id = "Shield Bash",
        DisplayName = "Shield Bash",
        Cooldown = 2,
        Description = "Gain {Shield} facing a direction. Push an adjacent unit there back {Push} squares, then deal {Damage} damage to it.",
        Effects = new List<AbilityEffect>
        {
            new StatusEffect { Relation = Occupant.Any, OnCaster = true, Status = "Shield" },
            new PushEffect { Relation = Occupant.Units & ~Occupant.Self, Distance = 2 },
            new DamageEffect { Relation = Occupant.Units & ~Occupant.Self, Amount = 1 }
        },
        Targeting = new DirectionTargeting
        {
            EffectArea = new GridSelection
            {
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal { Direction = GridDirection.Orthogonal, Linear = true, MaxDistance = 1, Passthrough = Occupant.Any }
                }
            }
        }
    };
}
