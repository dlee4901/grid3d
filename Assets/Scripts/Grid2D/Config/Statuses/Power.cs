using System.Collections.Generic;

public static partial class StatusConfigs
{
    public static readonly StatusConfig Power = new()
    {
        Id = "Power",
        Trigger = new Trigger { On = GameEvent.DamageDealt, When = Is.AbilityDamage },
        Effects = new List<AbilityEffect> { new ModifyDamageEffect { Amount = Var.Count } }
    };
}
