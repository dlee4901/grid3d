using System.Collections.Generic;

public static partial class StatusConfigs
{
    public static readonly StatusConfig Power = new()
    {
        Id = "Power",
        DisplayName = "Power",
        Description = "Damage dealt by this unit's abilities is increased by 1 per stack.",
        Trigger = new Trigger { On = GameEvent.DamageDealt, When = Is.AbilityDamage },
        Effects = new List<AbilityEffect>
        {
            new ModifyDamageEffect { Amount = Var.Count }
        }
    };
}
