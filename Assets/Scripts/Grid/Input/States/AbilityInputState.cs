using System.Collections.Generic;

public abstract class AbilityInputState : PlayerInputState
{
    protected readonly Ability Ability;
    protected readonly List<GridPosition> Targets;
    protected readonly List<GridPosition> Path;
    private readonly GridSource _source;
    private readonly GridSteps _selectable;
    private readonly GridSteps _valid;
    private readonly bool _traceable;
    private List<GridPosition> _trace;
    private bool _holding;

    protected AbilityInputState(PlayerInputContext ctx, Ability ability, GridSource source, List<GridPosition> targets,
                                List<GridPosition> path = null) : base(ctx)
    {
        Ability = ability;
        _source = source;
        Targets = targets ?? new List<GridPosition>();
        Path = path;
        _selectable = AreaOf(ability, source);
        _valid = ability.IsCastable ? ValidPicks(ability, source, _selectable) : _selectable;
        _traceable = MovePath.CanTrace(ability);
    }

    public string AbilityId => Ability.Id;
    public GridSource Source => _source;
    protected int TargetCount => Ability.Targeting?.Targets ?? 0;
    public bool Castable => Ctx.Dispatcher.Enabled && Ctx.Grid.CanCastAt(_source.Position, Ability, Ctx.Dispatcher.LocalSeat);

    public override void OnEnter() => Show(Targets, Path);

    protected bool StartsTrace(GridPosition position) => _traceable && position.Equals(_source.Position);

    public override void OnPress(GridSource pressed)
    {
        if (StartsTrace(pressed.Position))
        {
            _trace = new List<GridPosition>();
            ShowTrace();
            return;
        }
        _holding = IsValidTarget(pressed.Position);
        if (_holding) Show(WithLast(pressed.Position));
    }

    public override void OnDrag(GridSource dragged)
    {
        if (_trace == null) return;
        var position = dragged.Position;
        var index = _trace.IndexOf(position);
        if (position.Equals(_source.Position)) _trace.Clear();
        else if (index >= 0) _trace.RemoveRange(index + 1, _trace.Count - index - 1);
        else if (MovePath.CanStep(_source, Ability, _selectable, _trace, position)) _trace.Add(position);
        else return;
        ShowTrace();
    }

    private void ShowTrace()
    {
        var nextSteps = MovePath.NextSteps(_source, Ability, _selectable, _trace);
        if (_trace.Count == 0) Show(Targets, null, nextSteps);
        else Show(new List<GridPosition> { _trace[_trace.Count - 1] }, _trace, nextSteps);
    }

    protected bool TryFinishTrace()
    {
        var trace = _trace;
        _trace = null;
        if (trace is not { Count: > 0 }) return false;
        Ctx.Controller.TransitionTo(new PreviewState(Ctx, Ability, _source, new List<GridPosition> { trace[trace.Count - 1] }, trace));
        return true;
    }

    public override void OnPositionSelected(GridSource clicked)
    {
        if (TryFinishTrace()) return;
        var wasHolding = _holding;
        _holding = false;
        if (!IsValidTarget(clicked.Position))
        {
            if (wasHolding) Show(Targets);
            else Ctx.Controller.TransitionTo(new PositionSelectedState(Ctx, clicked));
            return;
        }
        OnTargetSelected(clicked.Position);
    }

    protected abstract void OnTargetSelected(GridPosition position);

    public override void OnAbilityActivate(Ability ability, GridSource source)
    {
        if (ability.Id == Ability.Id) Ctx.Controller.TransitionTo(new PositionSelectedState(Ctx, _source));
        else Ctx.Controller.TransitionTo(Select(ability, source));
    }

    protected bool IsDuplicate(GridPosition position)
        => Ability.Targeting is PositionTargeting { Distinct: true } && Targets.Contains(position);

    protected List<GridPosition> WithLast(GridPosition position)
    {
        var targets = new List<GridPosition>(Targets);
        if (targets.Count >= TargetCount) targets.RemoveAt(targets.Count - 1);
        targets.Add(position);
        return targets;
    }

    protected List<GridPosition> WithoutLast()
    {
        var targets = new List<GridPosition>(Targets);
        if (targets.Count > 0) targets.RemoveAt(targets.Count - 1);
        return targets;
    }

    protected void Show(IReadOnlyList<GridPosition> targets, IReadOnlyList<GridPosition> path = null, GridSteps nextSteps = null)
        => Ctx.Display.Show(new TargetingSnapshot(_source, Ability, _selectable, _valid, targets, path, nextSteps));

    protected bool IsValidTarget(GridPosition position) => Ability.IsCastable && _valid.Contains(position);

    private static GridSteps ValidPicks(Ability ability, GridSource source, GridSteps selectable)
    {
        var valid = new GridSteps();
        foreach (var position in selectable.GetPositions())
        {
            if (!ability.CanAffect(source, position)) continue;
            foreach (var step in selectable.GetStepsAtPosition(position)) valid.Add(step);
        }
        return valid;
    }

    private static GridSteps AreaOf(Ability ability, GridSource source)
    {
        if (ability.Targeting != null) return ability.Targeting.GetSelectableSteps(source);
        return ability.Trigger?.Range?.GetGridSteps(source) ?? new GridSteps();
    }
}
