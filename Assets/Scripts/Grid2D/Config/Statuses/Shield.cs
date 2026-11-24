using System.Collections.Generic;

public static partial class StatusConfigs
{
    public static readonly StatusConfig Shield = new()
    {
        Id = "Shield",
        DisplayName = "Shield",
        Description = "Blocks the next ability damage coming straight from the shielded direction. Diagonal and area damage are not blocked.",
        Counter = StatusCounter.None,
        Param = StatusParam.Direction,
        Trigger = new Trigger { On = GameEvent.DamageTaken, When = Is.AbilityDamage & !Is.AreaDamage & Is.FromStatusDirection },
        Effects = new List<AbilityEffect>
        {
            new ModifyDamageEffect { Prevent = true },
            new StatusEffect { Relation = Occupant.Self, Status = "Shield", Amount = -1 }
        }
    };
}
