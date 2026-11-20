using System;
using System.Collections.Generic;

public class Ability : INameId
{
    public AbilityConfig Config { get; }

    public string Id => Config.Id;
    public int ManaCost => Config.ManaCost;
    public Keyword Keywords => Config.Keywords;
    public AbilityTargeting Targeting => Config.Targeting;
    public List<AbilityEffect> Effects => Config.Effects;

    public int Cooldown { get; private set; }
    public int CastsThisTurn { get; private set; }
    public int Delay { get; private set; }
    public bool Locked { get; private set; }

    private int? _castCooldown;

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
        var cooldown = _castCooldown ?? Config.Cooldown;
        _castCooldown = null;
        CastsThisTurn++;
        if (CastsThisTurn == 1 && (Keywords & Keyword.Double) != 0) return;
        ApplyCooldown(cooldown);
    }

    internal void SetCastCooldown(int turns) => _castCooldown = turns;

    internal void ApplyCooldown(int turns)
    {
        if (turns > Cooldown) Cooldown = turns;
    }

    internal void TurnUpdate()
    {
        Cooldown = Math.Max(Cooldown - 1, 0);
        CastsThisTurn = 0;
    }
}
