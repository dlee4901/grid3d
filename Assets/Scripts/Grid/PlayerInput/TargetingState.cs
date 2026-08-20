using System.Collections.Generic;
using System.Linq;

public class TargetingState : PlayerInputStateBase
{
    private readonly Ability _ability;
    private readonly GridSource _source;
    private readonly List<GridPosition> _targets = new();
    private readonly GridSteps _selectable;

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
        Ctx.Renderer.ClearHighlights();
        Ctx.Renderer.HighlightPositions(_selectable, GridHighlightType.SelectableTargets);
    }

    public override void OnPositionSelected(GridSource clicked)
    {
        if (!IsValidTarget(clicked.Position))
        {
            Ctx.Controller.TransitionTo(new SelectedState(Ctx, clicked));
            return;
        }
        _targets.Add(clicked.Position);
        Ctx.Controller.LogState($"Target added: {clicked.Position} ({_targets.Count}/{_ability.Targeting.Targets})");
        if (_targets.Count >= _ability.Targeting.Targets) Confirm();
    }

    public override void OnHover(GridSource? hovered)
    {
        Ctx.Renderer.ClearHighlights();
        Ctx.Renderer.HighlightPositions(_selectable, GridHighlightType.SelectableTargets);
        if (!hovered.HasValue || !_selectable.Contains(hovered.Value.Position)) return;
        if (_ability.Targeting.GetEffectSteps(_source, hovered.Value.Position, out var list))
            Ctx.Renderer.HighlightPositions(list, GridHighlightType.EffectPreview);
    }

    public override void OnAbilityActivate(Ability ability, GridSource source)
    {
        if (ability.Id == _ability.Id) OnCancel();
        else Ctx.Controller.TransitionTo(new TargetingState(Ctx, ability, source));
    }
    
    public override void OnAbilityPreview(Ability ability, GridSource source) {}
    public override void OnAbilityCancelPreview() {}

    public override void OnCancel()
        => Ctx.Controller.TransitionTo(new SelectedState(Ctx, _source));

    private bool IsValidTarget(GridPosition position) => _selectable.Contains(position);

    private void Confirm()
    {
        var issuer = 0;
        if (_source.Entity != null && _source.Entity.TryGetComponent<ControlComponent>(out var ctrl))
            issuer = ctrl.PlayerController;
            
        var targets1D = new int[_targets.Count];
        for (var i = 0; i < _targets.Count; i++) targets1D[i] = _targets[i].Dim1;
        var ok = Ctx.Dispatcher.Submit(new AbilityCommand(issuer, _ability.Id, _source.Position.Dim1, targets1D));
        
        Ctx.Controller.LogState($"AbilityCommand: {_ability.Id} ok={ok}");
        Ctx.Controller.TransitionTo(new IdleState(Ctx));
    }
}
