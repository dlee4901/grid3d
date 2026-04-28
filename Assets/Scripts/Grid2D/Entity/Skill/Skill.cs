using System.Collections.Generic;

public class Skill : INameId
{
    // Identifiers
    public SkillConfig Config { get; }
    
    public string Id => Config.Id;
    public int Cost => Config.Cost;
    public List<SkillBindings> Bindings => Config.Bindings;
    public SkillSelection Selection => Config.Selection;
    
    // State
    public int Cooldown { get; private set; }
    public int Delay { get; private set; }
    public bool Locked { get; private set; }
    
    static Skill()
    {
        MemberRegistry<Skill>.Register<string>("Id", e => e.Id);
        MemberRegistry<Skill>.Register<int>("Cost", e => e.Cost);
    }
    
    public static Skill Create(SkillConfig config)
    {
        return new Skill(config);
    }
    
    private Skill(SkillConfig config)
    {
        Config = config;
        Cooldown = config.Warmup;
        Delay = config.Delay;
        Locked = config.Locked;
    }
    
    public void Trigger()
    {
        if (Cooldown != 0)
            return;
        Cooldown = Config.Cooldown;
    }
    
    public void TurnUpdate()
    {
        if (Cooldown != 0)
            Cooldown--;
    }
}