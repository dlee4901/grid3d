using System.Collections.Generic;

public static partial class StatusConfigs
{
    public static readonly StatusConfig Marked = new()
    {
        Id = "Marked",
        DisplayName = "Marked",
        Description = "When the unit that marked this unit deals ability damage to it, this unit dies. Removed if the marker dies.",
        Counter = StatusCounter.None,
        Param = StatusParam.SourceUid,
        OnePerSource = true,
        Reactions = new List<Reaction>
        {
            new()
            {
                Trigger = new Trigger { On = GameEvent.Damaged, When = Is.AbilityDamage & Is.FromStatusSource },
                Effects = new List<AbilityEffect>
                {
                    new KillEffect { Relation = Occupant.Self }
                }
            },
            new()
            {
                Trigger = new Trigger { On = GameEvent.Died, Subject = Occupant.Units & ~Occupant.Self, When = Is.SubjectIsStatusSource },
                Effects = new List<AbilityEffect>
                {
                    new StatusEffect { Relation = Occupant.Self, Status = "Marked", Amount = -1 }
                }
            }
        }
    };
}
