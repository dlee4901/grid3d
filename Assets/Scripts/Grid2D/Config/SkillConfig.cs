using System.Collections.Generic;
using Newtonsoft.Json;

public class SkillConfig : INameId
{
    public string Id { get; set; }
    public string Type { get; set; } = "Default";
    public int Cost { get; set; } = 1;
    public List<SkillBindings> Bindings { get; set; }
    [JsonConverter(typeof(SkillSelection))] public SkillSelection Selection { get; set; }
    
    public int Warmup { get; set; } = 0;
    public int Cooldown { get; set; } = 0;
    public int Delay { get; set; } = 0;
    public bool Locked { get; set; } = false;
}

public class SkillBindings
{
    List<SkillTrigger> Triggers { get; set; }
    List<SkillEffect> Effects { get; set; }
}