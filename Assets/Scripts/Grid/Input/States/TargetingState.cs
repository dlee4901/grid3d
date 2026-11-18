using System.Collections.Generic;
using System.Linq;

public class TargetingState : PlayerInputState
{
    private readonly Ability _ability;
    private readonly GridSource _source;
    private readonly List<GridPosition> _targets = new();
    private readonly GridSteps _selectable;
    private bool _holding;

    public TargetingState(PlayerInputContext ctx, Ability ability, GridSource source) : base(ctx)
    {
        _ability = ability;
        _source = source;
        _selectable = ability.Targeting.GetSelectableSteps(source);
    }

    public string AbilityId => _ability.Id;
    public GridSource Source => _source;

    public override void OnEnter()
    {
        Ctx.Highlights.SetSelected(_source.Position);
        ShowSelectable();
    }

    private void ShowSelectable()
    {
        Ctx.Highlights.Clear();
        Ctx.Highlights.Highlight(_selectable, HighlightType.AvailableTargets);
        if (_ability.Targeting is DirectionTargeting) Ctx.Highlights.DirectionChevrons(_selectable);
    }

    public override void OnExit() => Ctx.Manager.ClearPreview();

    public override void OnPress(GridSource pressed) => _holding = IsValidTarget(pressed.Position);

    public override void OnPositionSelected(GridSource clicked)
    {
        var wasHolding = _holding;
        _holding = false;
        if (!IsValidTarget(clicked.Position))
        {
            if (wasHolding)
            {
                Ctx.Controller.LogState("Hold released outside selection area");
                return;
            }
            Ctx.Controller.TransitionTo(new SelectedState(Ctx, clicked));
            return;
        }
        if (_ability.Targeting is PositionTargeting { Distinct: true } && _targets.Contains(clicked.Position)) return;
        _targets.Add(clicked.Position);
        Ctx.Controller.LogState($"Target added: {clicked.Position} ({_targets.Count}/{_ability.Targeting.Targets})");
        if (_targets.Count >= _ability.Targeting.Targets) Confirm();
    }

    public override void OnHover(GridSource? hovered)
    {
        ShowSelectable();
        if (!hovered.HasValue || !_selectable.Contains(hovered.Value.Position))
        {
            Ctx.Manager.ClearPreview();
            return;
        }
        if (_ability.Targeting.GetEffectSteps(_source, hovered.Value.Position, out var list))
        {
            if (_ability.Targeting is DirectionTargeting { MaxHits: > 0 }) ShowProjectiles(list);
            else Ctx.Highlights.Highlight(list, HighlightType.EffectArea);
        }
        if (!TryPreview(hovered.Value.Position)) Ctx.Manager.ClearPreview();
    }

    private void ShowProjectiles(GridSteps steps)
    {
        foreach (var ray in steps.GetSteps().GroupBy(step => step.Direction))
        {
            var impact = ray.OrderBy(step => step.Distance).Last().Position;
            Ctx.Highlights.Highlight(new HashSet<GridPosition> { impact }, HighlightType.EffectArea);
            Ctx.Highlights.Projectile(_source.Position, impact);
        }
    }

    private bool TryPreview(GridPosition hovered)
    {
        if (_targets.Count + 1 != _ability.Targeting.Targets) return false;
        var targets = ToIndices(_targets.Append(hovered));
        return Ctx.Manager.Preview(seat => new AbilityCommand(seat, _ability.Id, _source.Position.Dim1, targets));
    }

    private static int[] ToIndices(IEnumerable<GridPosition> positions) => positions.Select(p => p.Dim1).ToArray();

    public override void OnAbilityActivate(Ability ability, GridSource source)
    {
        if (ability.Id == _ability.Id) OnCancel();
        else if (Ctx.Grid.CanCastAt(source.Position, ability, Ctx.Dispatcher.LocalSeat)) Ctx.Controller.TransitionTo(new TargetingState(Ctx, ability, source));
    }
    
    public override void OnAbilityPreview(Ability ability, GridSource source) {}
    public override void OnAbilityCancelPreview() {}

    public override void OnCancel()
        => Ctx.Controller.TransitionTo(new SelectedState(Ctx, _source));

    private bool IsValidTarget(GridPosition position) => _selectable.Contains(position);

    private void Confirm()
    {
        var targets1D = ToIndices(_targets);
        var ok = Ctx.Dispatcher.Submit(seat => new AbilityCommand(seat, _ability.Id, _source.Position.Dim1, targets1D));
        
        Ctx.Controller.LogState($"AbilityCommand: {_ability.Id} ok={ok}");
        Ctx.Controller.TransitionTo(new IdleState(Ctx));
    }
}
