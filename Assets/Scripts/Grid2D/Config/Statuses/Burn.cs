using System.Collections.Generic;

public static partial class StatusConfigs
{
    public static readonly StatusConfig Burn = new()
    {
        Id = "Burn",
        Trigger = new Trigger { On = GameEvent.TurnStart },
        Effects = new List<AbilityEffect>
        {
            new DamageEffect { Relation = Occupant.Self, Amount = 1 },
            new StatusEffect { Relation = Occupant.Self, Status = "Burn", Amount = -1 }
        }
    };
}
