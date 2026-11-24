using System.Collections.Generic;

public static partial class StatusConfigs
{
    public static readonly StatusConfig Rabies = new()
    {
        Id = "Rabies",
        DisplayName = "Rabies",
        Description = "At the end of this unit's turn, lose one stack. At 0 stacks, this unit dies. Applying it again removes one stack.",
        Counter = StatusCounter.Decrement,
        Trigger = new Trigger { On = GameEvent.TurnEnd },
        Effects = new List<AbilityEffect>
        {
            new StatusEffect { Relation = Occupant.Self, Status = "Rabies", Amount = -1 }
        },
        OnExpire = new List<AbilityEffect>
        {
            new KillEffect { Relation = Occupant.Self }
        }
    };
}
