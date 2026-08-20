using System.Collections.Generic;

public enum AbilityType { Default, Move, Passive }

public class AbilityConfig : INameId
{
    public string Id { get; set; }
    public AbilityType Type { get; set; } = AbilityType.Default;
    public int ManaCost { get; set; } = 1;
    public AbilityTargeting Targeting { get; set; }
    public List<AbilityEffect> Effects { get; set; } = new();
    //public List<AbilityBindings> Bindings { get; set; }

    public int Warmup { get; set; } = 0;
    public int Cooldown { get; set; } = 0;
    public int Delay { get; set; } = 0;
    public bool Locked { get; set; } = false;
}

// public class AbilityBindings
// {
//     List<AbilityTrigger> Triggers { get; set; }
//     List<AbilityEffect> Effects { get; set; }
// }