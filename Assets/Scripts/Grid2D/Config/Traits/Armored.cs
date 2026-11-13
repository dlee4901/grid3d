using System.Collections.Generic;

public static partial class TraitConfigs
{
    public static readonly TraitConfig Armored = new()
    {
        Id = "Armored",
        Trigger = new Trigger { On = GameEvent.DamageTaken },
        Effects = new List<AbilityEffect> { new ModifyDamageEffect { Amount = -1 } }
    };
}
