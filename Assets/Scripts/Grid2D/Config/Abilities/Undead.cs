using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Undead = new()
    {
        Id = "Undead",
        DisplayName = "Undead",
        Description = "At the start of the game, gain {Revive}.",
        Trigger = new Trigger { On = GameEvent.GameStart },
        Effects = new List<AbilityEffect>
        {
            new StatusEffect { Relation = Occupant.Self, Status = "Revive" }
        }
    };
}
