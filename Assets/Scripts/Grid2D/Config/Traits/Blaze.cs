using System.Collections.Generic;

public static partial class TraitConfigs
{
    public static readonly TraitConfig Blaze = new()
    {
        Id = "Blaze",
        Trigger = new Trigger { On = GameEvent.DamageTaken, When = Is.DamageFrom("Burn") },
        Effects = new List<AbilityEffect>
        {
            new ModifyDamageEffect { Prevent = true },
            new StatusEffect { Relation = Occupant.Self, Status = "Power", Amount = 1 }
        }
    };
}
