using System.Collections.Generic;

public static partial class TraitConfigs
{
    public static readonly TraitConfig Retaliate = new()
    {
        Id = "Retaliate",
        Trigger = new Trigger { On = GameEvent.Damaged, When = Is.AbilityDamage },
        Effects = new List<AbilityEffect>
        {
            new DamageEffect { Relation = Occupant.Enemy, Amount = 1, When = Var.Distance <= 1 }
        }
    };
}
