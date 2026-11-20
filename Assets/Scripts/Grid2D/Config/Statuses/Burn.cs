using System.Collections.Generic;

public static partial class StatusConfigs
{
    public static readonly StatusConfig Burn = new()
    {
        Id = "Burn",
        DisplayName = "Burn",
        Description = "At the end of this unit's turn, take {Damage} damage and lose one stack.",
        Trigger = new Trigger { On = GameEvent.TurnEnd },
        Effects = new List<AbilityEffect>
        {
            new DamageEffect { Relation = Occupant.Self, Amount = 1 },
            new StatusEffect { Relation = Occupant.Self, Status = "Burn", Amount = -1 }
        }
    };
}
