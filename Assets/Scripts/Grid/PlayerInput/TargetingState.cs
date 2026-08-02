using System.Collections.Generic;
using System.Linq;

public class TargetingState : PlayerInputStateBase
{
    private readonly Ability _ability;
    private readonly QueryContext _source;
    private readonly List<GridPosition> _targets = new();
    private readonly GridSteps _selectable;

    public TargetingState(PlayerInputContext ctx, Ability ability, QueryContext source) : base(ctx)
    {
        _ability = ability;
        _source = source;
        _selectable = ability.Targeting.GetSelectableSteps(source);
    }

    public string AbilityId => _ability.Id;
    public QueryContext Source => _source;

    public override void OnEnter()
    {
        Ctx.Renderer.ClearHighlights();
        Ctx.Renderer.HighlightPositions(_selectable, GridHighlightType.SelectableTargets);
    }

    public override void OnPositionSelected(QueryContext clicked)
    {
        if (!IsValidTarget(clicked.SourcePosition))
        {
            Ctx.Controller.TransitionTo(new SelectedState(Ctx, clicked));
            return;
        }
        _targets.Add(clicked.SourcePosition);
        Ctx.Controller.LogState($"Target added: {clicked.SourcePosition} ({_targets.Count}/{_ability.Targeting.Targets})");
        if (_targets.Count >= _ability.Targeting.Targets) Confirm();
    }

    public override void OnHover(QueryContext? hovered)
    {
        Ctx.Renderer.ClearHighlights();
        Ctx.Renderer.HighlightPositions(_selectable, GridHighlightType.SelectableTargets);
        if (!hovered.HasValue || !_selectable.Contains(hovered.Value.SourcePosition)) return;
        if (_ability.Targeting.GetEffectSteps(_source, hovered.Value.SourcePosition, out var list))
            Ctx.Renderer.HighlightPositions(list, GridHighlightType.EffectPreview);
    }

    public override void OnAbilityActivate(Ability ability, QueryContext source)
    {
        if (ability.Id == _ability.Id) OnCancel();
        else Ctx.Controller.TransitionTo(new TargetingState(Ctx, ability, source));
    }
    
    public override void OnAbilityPreview(Ability ability, QueryContext ctx) {}
    public override void OnAbilityCancelPreview() {}

    public override void OnCancel()
        => Ctx.Controller.TransitionTo(new SelectedState(Ctx, _source));

    private bool IsValidTarget(GridPosition position) => _selectable.Contains(position);

    private void Confirm()
    {
        var issuer = 0;
        if (_source.SourceEntity != null
            && _source.SourceEntity.TryGetComponent<ControlComponent>(out var ctrl))
            issuer = ctrl.PlayerController;
            
        var targets1D = new int[_targets.Count];
        for (var i = 0; i < _targets.Count; i++) targets1D[i] = _targets[i].Dim1;
        var ok = Ctx.Dispatcher.Submit(new AbilityCommand(issuer, _ability.Id, _source.SourcePosition.Dim1, targets1D));
        
        Ctx.Controller.LogState($"AbilityCommand: {_ability.Id} ok={ok}");
        Ctx.Controller.TransitionTo(new IdleState(Ctx));
    }
}
