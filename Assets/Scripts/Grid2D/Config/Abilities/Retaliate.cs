using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Retaliate = new()
    {
        Id = "Retaliate",
        DisplayName = "Retaliate",
        Description = "When hit by an ability, deal {Damage} damage back to an adjacent attacker.",
        Trigger = new Trigger
        {
            On = GameEvent.Damaged,
            When = Is.AbilityDamage,
            Range = new GridSelection
            {
                MinDistance = 1,
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal { Direction = GridDirection.Straight, MaxDistance = 1, Linear = false, Passthrough = Occupant.Any }
                }
            }
        },
        Effects = new List<AbilityEffect>
        {
            new DamageEffect { Relation = Occupant.Enemy, Amount = 1 }
        }
    };
}
