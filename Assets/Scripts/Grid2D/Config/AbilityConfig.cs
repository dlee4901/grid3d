using System.Collections.Generic;

public enum AbilityType { Default, Move }

public class AbilityConfig : INameId
{
    public string Id { get; init; }
    public AbilityType Type { get; init; } = AbilityType.Default;
    public int ManaCost { get; init; } = 1;
    public AbilityTargeting Targeting { get; init; }
    public List<AbilityEffect> Effects { get; init; } = new();
    //public List<AbilityBindings> Bindings { get; init; }

    public int Warmup { get; init; } = 0;
    public int Cooldown { get; init; } = 0;
    public int Delay { get; init; } = 0;
    public bool Locked { get; init; } = false;
}

// public class AbilityBindings
// {
//     List<AbilityTrigger> Triggers { get; init; }
//     List<AbilityEffect> Effects { get; init; }
// }