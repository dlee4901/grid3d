using System.Collections.Generic;
using System.Linq;

public abstract class AbilityInputState : PlayerInputState
{
    protected readonly Ability Ability;
    protected readonly List<GridPosition> Targets;
    private readonly GridSource _source;
    private readonly GridSteps _selectable;
    private bool _holding;

    protected AbilityInputState(PlayerInputContext ctx, Ability ability, GridSource source, List<GridPosition> targets) : base(ctx)
    {
        Ability = ability;
        _source = source;
        Targets = targets ?? new List<GridPosition>();
        _selectable = ability.Targeting.GetSelectableSteps(source);
    }

    public string AbilityId => Ability.Id;
    public GridSource Source => _source;
    protected int TargetCount => Ability.Targeting.Targets;
    public bool Castable => Ctx.Grid.CanCastAt(_source.Position, Ability, Ctx.Dispatcher.LocalSeat);

    public override void OnEnter()
    {
        Ctx.Highlights.SetSelected(_source.Position);
        ShowSelection(Targets);
    }

    public override void OnPress(GridSource pressed)
    {
        _holding = IsValidTarget(pressed.Position);
        if (_holding) ShowSelection(WithLast(pressed.Position));
    }

    public override void OnPositionSelected(GridSource clicked)
    {
        var wasHolding = _holding;
        _holding = false;
        if (!IsValidTarget(clicked.Position))
        {
            if (wasHolding) ShowSelection(Targets);
            else Ctx.Controller.TransitionTo(new PositionSelectedState(Ctx, clicked));
            return;
        }
        OnTargetSelected(clicked.Position);
    }

    protected abstract void OnTargetSelected(GridPosition position);

    public override void OnAbilityActivate(Ability ability, GridSource source)
    {
        if (ability.Id == Ability.Id) Ctx.Controller.TransitionTo(new PositionSelectedState(Ctx, _source));
        else Ctx.Controller.TransitionTo(new AbilitySelectedState(Ctx, ability, source));
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

    protected void ShowSelection(List<GridPosition> targets)
    {
        Ctx.Highlights.Clear();
        Ctx.Highlights.Highlight(_selectable, HighlightType.AvailableTargets);
        if (Ability.Targeting is DirectionTargeting) Ctx.Highlights.DirectionChevrons(_selectable);
        if (targets.Count == 0)
        {
            Ctx.Manager.ClearPreview();
            return;
        }
        if (Ability.Targeting.GetEffectSteps(_source, targets[targets.Count - 1], out var list))
        {
            if (Ability.Targeting is DirectionTargeting { MaxHits: > 0 }) ShowProjectiles(list);
            else Ctx.Highlights.Highlight(list, HighlightType.EffectArea);
        }
        if (targets.Count != TargetCount
            || !Ctx.Manager.Preview(seat => new AbilityCommand(seat, Ability.Id, _source.Position.Dim1, ToIndices(targets))))
            Ctx.Manager.ClearPreview();
    }

    protected static int[] ToIndices(IEnumerable<GridPosition> positions) => positions.Select(p => p.Dim1).ToArray();

    private bool IsValidTarget(GridPosition position) => _selectable.Contains(position);

    private void ShowProjectiles(GridSteps steps)
    {
        foreach (var ray in steps.GetSteps().GroupBy(step => step.Direction))
        {
            var impact = ray.OrderBy(step => step.Distance).Last().Position;
            Ctx.Highlights.Highlight(new HashSet<GridPosition> { impact }, HighlightType.EffectArea);
            Ctx.Highlights.Projectile(_source.Position, impact);
        }
    }
}
