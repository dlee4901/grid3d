using System;
using System.Collections.Generic;

public class Ability : INameId, ITriggered
{
    public AbilityConfig Config { get; }

    public string Id => Config.Id;
    public Keyword Keywords => Config.Keywords;
    public AbilityTargeting Targeting => Config.Targeting;
    public Trigger Trigger => Config.Trigger;
    public GridSelection Area => Config.Area;
    public bool IsCastable => !Config.IsPassive;
    public bool HasCounter => Config.Counter.HasValue || Config.CounterStatus != null;
    public int Count { get; private set; }
    public List<AbilityEffect> Effects => Config.Effects;

    public int Cooldown { get; private set; }
    public int Delay { get; private set; }
    public bool Locked { get; private set; }

    private int? _castCooldown;

    internal Ability(AbilityConfig config)
    {
        Config = config;
        Count = config.Counter ?? 1;
        Cooldown = config.Warmup;
        Delay = config.Delay;
        Locked = config.Locked;
    }

    private Ability(Ability source)
    {
        Config = source.Config;
        Count = source.Count;
        Cooldown = source.Cooldown;
        Delay = source.Delay;
        Locked = source.Locked;
        _castCooldown = source._castCooldown;
    }

    internal Ability Copy() => new(this);

    internal bool Execute(GridState state, GridPosition sourcePosition, List<GridPosition> targetPositions,
                          IReadOnlyList<GridPosition> path = null)
    {
        var source = state.GetEntity(sourcePosition);
        if (source == null) return false;
        var origin = new GridSource(state, sourcePosition, source);
        if (!Targeting.GetEffectSteps(origin, targetPositions, out var steps)) return false;
        foreach (var target in targetPositions)
        {
            if (CanAffect(origin, target)) continue;
            GridLog.Warning($"[Ability] {Id} rejected target that affects nothing");
            return false;
        }
        if (path != null && !MovePath.IsValid(origin, this, targetPositions[0], path))
        {
            GridLog.Warning($"[Ability] {Id} rejected traced path");
            return false;
        }
        EffectResolver.Run(new EffectContext(origin, this, path: path), steps, Effects);
        return true;
    }

    public bool CanAffect(GridSource origin, GridPosition target)
        => Targeting != null && Targeting.GetPickSteps(origin, target, out var steps)
           && EffectResolver.AffectsAny(new EffectContext(origin, this), steps, Effects);

    public bool HasTarget(GridSource origin)
    {
        if (Targeting == null) return false;
        foreach (var position in Targeting.GetSelectableSteps(origin).GetPositions())
            if (CanAffect(origin, position)) return true;
        return false;
    }

    public int CounterFor(Entity holder)
    {
        if (Config.CounterStatus == null) return Count;
        if (holder == null || !holder.TryGetComponent<StatusComponent>(out var statuses)) return 0;
        foreach (var status in statuses.Statuses)
            if (status.Id == Config.CounterStatus) return status.Count;
        return 0;
    }

    internal void AdjustCount(int amount, int max) => Count = Math.Clamp(Count + amount, 0, max);

    internal void RecordCast()
    {
        var cooldown = _castCooldown ?? Config.Cooldown;
        _castCooldown = null;
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
    }
}
