using System.Collections.Generic;

public class Ability : INameId
{
    public AbilityConfig Config { get; }

    public string Id => Config.Id;
    public int ManaCost => Config.ManaCost;
    public List<AbilityBindings> Bindings => Config.Bindings;
    public AbilitySelection Selection => Config.Selection;

    public int Cooldown { get; private set; }
    public int Delay { get; private set; }
    public bool Locked { get; private set; }

    static Ability()
    {
        MemberRegistry<Ability>.Register<string>("Id", e => e.Id);
        MemberRegistry<Ability>.Register<int>("Cost", e => e.ManaCost);
    }

    public static Ability Create(AbilityConfig config) => config.Type switch
    {
        AbilityType.Move => new MoveAbility(config),
        _                => new Ability(config),
    };

    protected Ability(AbilityConfig config)
    {
        Config = config;
        Cooldown = config.Warmup;
        Delay = config.Delay;
        Locked = config.Locked;
    }

    public virtual bool Execute(GridState state, int sourcePosition, IReadOnlyList<int> targets)
    {
        GridLog.Info($"[Ability] {Id} from {sourcePosition} → [{string.Join(", ", targets)}] (no-op stub)");
        return true;
    }

    public void Trigger()
    {
        if (Cooldown != 0) return;
        Cooldown = Config.Cooldown;
    }

    public void TurnUpdate()
    {
        if (Cooldown > 0) Cooldown--;
    }
}
