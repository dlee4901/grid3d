using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Blaze = new()
    {
        Id = "Blaze",
        DisplayName = "Blaze",
        Description = "Burn deals no damage to this unit. Counts this unit's Burn stacks; each adds 1 damage to its abilities.",
        CounterStatus = "Burn",
        Reactions = new List<Reaction>
        {
            new()
            {
                Trigger = new Trigger { On = GameEvent.DamageTaken, When = Is.DamageFrom("Burn") },
                Effects = new List<AbilityEffect>
                {
                    new ModifyDamageEffect { Prevent = true }
                }
            },
            new()
            {
                Trigger = new Trigger { On = GameEvent.DamageDealt, When = Is.AbilityDamage },
                Effects = new List<AbilityEffect>
                {
                    new ModifyDamageEffect { Amount = Var.SourceCounter("Blaze") }
                }
            }
        }
    };
}
