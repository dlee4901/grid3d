using System.Collections.Generic;

public static partial class AbilityConfigs
{
    private static readonly Value TurretLevel = Var.SourceCounter("Upgrade Level");

    public static readonly AbilityConfig Shoot = new()
    {
        Id = "Shoot",
        DisplayName = "Shoot",
        Description = "At the end of this unit's turn, deal 3 damage to the first unit up to 3 squares ahead. Upgrades: +1 range, then +1 damage, then hits every unit in the line.",
        Cooldown = 2,
        Trigger = new Trigger { On = GameEvent.TurnEnd },
        Area = new GridSelection
        {
            MinDistance = 1,
            Traversals = new List<GridTraversal>
            {
                new GridTraversal { Direction = GridDirection.North, Linear = true, MaxDistance = 4, Passthrough = Occupant.Any }
            }
        },
        Effects = new List<AbilityEffect>
        {
            new DamageEffect
            {
                Relation = Occupant.Units & ~Occupant.Self,
                Amount = 3 + Value.If(TurretLevel >= 2, 1, 0),
                When = (Var.Distance <= 3 + Value.If(TurretLevel >= 1, 1, 0)) & ((TurretLevel >= 3) | Is.FirstInLine)
            }
        }
    };
}
