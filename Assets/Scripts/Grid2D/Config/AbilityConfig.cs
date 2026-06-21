using System.Collections.Generic;
using Newtonsoft.Json;

public class AbilityConfig : INameId
{
    public string Id { get; set; }
    public string Type { get; set; } = "Default";
    public int Cost { get; set; } = 1;
    public List<AbilityBindings> Bindings { get; set; }
    [JsonConverter(typeof(AbilitySelection))] public AbilitySelection Selection { get; set; }
    
    public int Warmup { get; set; } = 0;
    public int Cooldown { get; set; } = 0;
    public int Delay { get; set; } = 0;
    public bool Locked { get; set; } = false;
}

public class AbilityBindings
{
    List<AbilityTrigger> Triggers { get; set; }
    List<AbilityEffect> Effects { get; set; }
}