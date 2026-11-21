using System;
using System.Collections.Generic;

public class Ability : INameId, ITriggered
{
    public AbilityConfig Config { get; }

    public string Id => Config.Id;
    public int ManaCost => Config.ManaCost;
    public Keyword Keywords => Config.Keywords;
    public AbilityTargeting Targeting => Config.Targeting;
    public Trigger Trigger => Config.Trigger;
    public GridSelection Area => Config.Area;
    public bool IsCastable => Trigger == null;
    public int Count => 1;
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

    internal bool Execute(GridState state, GridPosition sourcePosition, List<GridPosition> targetPositions,
                          IReadOnlyList<GridPosition> path = null)
    {
        var source = state.GetEntity(sourcePosition);
        if (source == null) return false;
        var origin = new GridSource(state, sourcePosition, source);
        if (!Targeting.GetEffectSteps(origin, targetPositions, out var steps)) return false;
        if (path != null && !MovePath.IsValid(origin, this, targetPositions[0], path))
        {
            GridLog.Warning($"[Ability] {Id} rejected traced path");
            return false;
        }
        EffectResolver.Run(new EffectContext(origin, this, path: path), steps, Effects);
        return true;
    }

    internal void RecordCast()
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
