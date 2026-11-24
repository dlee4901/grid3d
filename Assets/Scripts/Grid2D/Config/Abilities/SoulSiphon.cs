using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig SoulSiphon = new()
    {
        Id = "Soul Siphon",
        DisplayName = "Soul Siphon",
        Description = "Gains 1 stack whenever another unit dies. When this unit dies, it deals damage equal to its stacks to the unit that killed it.",
        Counter = 0,
        Reactions = new List<Reaction>
        {
            new()
            {
                Trigger = new Trigger { On = GameEvent.Died, Subject = Occupant.Units & ~Occupant.Self },
                Effects = new List<AbilityEffect>
                {
                    new AbilityCounterEffect { Relation = Occupant.Self, Ability = "Soul Siphon" }
                }
            },
            new()
            {
                Trigger = new Trigger { On = GameEvent.Died },
                Effects = new List<AbilityEffect>
                {
                    new DamageEffect { Relation = Occupant.Units & ~Occupant.Self, Amount = Var.Count }
                }
            }
        }
    };
}
