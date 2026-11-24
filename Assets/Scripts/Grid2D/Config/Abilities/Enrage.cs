using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Enrage = new()
    {
        Id = "Enrage",
        DisplayName = "Enrage",
        Description = "When this unit takes damage, gain {Power}.",
        Trigger = new Trigger { On = GameEvent.Damaged },
        Effects = new List<AbilityEffect>
        {
            new StatusEffect { Relation = Occupant.Self, Status = "Power", Amount = 1 }
        }
    };
}
