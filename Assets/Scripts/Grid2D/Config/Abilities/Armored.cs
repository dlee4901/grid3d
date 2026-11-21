using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Armored = new()
    {
        Id = "Armored",
        DisplayName = "Armored",
        Description = "Take 1 less damage from abilities.",
        Trigger = new Trigger { On = GameEvent.DamageTaken, When = Is.AbilityDamage },
        Effects = new List<AbilityEffect> { new ModifyDamageEffect { Amount = -1 } }
    };
}
