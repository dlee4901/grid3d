using System.Collections.Generic;

public class Ability : INameId
{
    public AbilityConfig Config { get; }

    public string Id => Config.Id;
    public int ManaCost => Config.ManaCost;
    public AbilityTargeting Targeting => Config.Targeting;
    public List<AbilityEffect> Effects => Config.Effects;

    public int Cooldown { get; private set; }
    public int Delay { get; private set; }
    public bool Locked { get; private set; }

    internal Ability(AbilityConfig config)
    {
        Config = config;
        Cooldown = config.Warmup;
        Delay = config.Delay;
        Locked = config.Locked;
    }

    internal bool Execute(GridState state, GridPosition sourcePosition, List<GridPosition> targetPositions)
    {
        var source = state.GetEntity(sourcePosition);
        if (source == null) return false;
        var origin = new GridSource(state, sourcePosition, source);
        if (!Targeting.GetEffectSteps(origin, targetPositions, out var steps)) return false;
        EffectResolver.Run(new EffectContext(origin, this), steps, Effects);
        return true;
    }


    internal void Trigger()
    {
        if (Cooldown != 0) return;
        Cooldown = Config.Cooldown;
    }

    internal void ApplyCooldown(int turns)
    {
        if (turns > Cooldown) Cooldown = turns;
    }

    internal void TurnUpdate()
    {
        if (Cooldown > 0) Cooldown--;
    }
}
