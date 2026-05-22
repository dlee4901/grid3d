using System.Collections.Generic;

public class Skill : INameId
{
    public SkillConfig Config { get; }

    public string Id => Config.Id;
    public int Cost => Config.Cost;
    public List<SkillBindings> Bindings => Config.Bindings;
    public SkillSelection Selection => Config.Selection;

    public int Cooldown { get; private set; }
    public int Delay { get; private set; }
    public bool Locked { get; private set; }

    static Skill()
    {
        MemberRegistry<Skill>.Register<string>("Id", e => e.Id);
        MemberRegistry<Skill>.Register<int>("Cost", e => e.Cost);
    }

    public static Skill Create(SkillConfig config) => config.Type switch
    {
        "Move" => new MoveSkill(config),
        _      => new Skill(config),
    };

    protected Skill(SkillConfig config)
    {
        Config = config;
        Cooldown = config.Warmup;
        Delay = config.Delay;
        Locked = config.Locked;
    }

    public virtual bool Execute(GridState state, int sourcePosition, IReadOnlyList<int> targets)
    {
        GridLog.Info($"[Skill] {Id} from {sourcePosition} → [{string.Join(", ", targets)}] (no-op stub)");
        return true;
    }

    public void Trigger()
    {
        if (Cooldown != 0) return;
        Cooldown = Config.Cooldown;
    }

    public void TurnUpdate()
    {
        if (Cooldown != 0) Cooldown--;
    }
}
