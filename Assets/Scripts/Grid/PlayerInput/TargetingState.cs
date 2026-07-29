using System.Collections.Generic;
using System.Linq;

public class TargetingState : PlayerInputStateBase
{
    private readonly Ability _ability;
    private readonly QueryContext _source;
    private readonly List<(int, int)> _targets = new();
    private readonly (List<(int, int)> areas, List<int> splits) _selectable;

    public TargetingState(PlayerInputContext ctx, Ability ability, QueryContext source) : base(ctx)
    {
        _ability = ability;
        _source = source;
        _selectable = ability.TargetingOld.GetSelectablePositions(source);
    }

    public string AbilityId => _ability.Id;
    public QueryContext Source => _source;

    public override void OnEnter()
    {
        Ctx.Renderer.ClearHighlights();
        Ctx.Renderer.HighlightPositions(_selectable.areas, GridHighlightType.SelectableTargets);
    }

    public override void OnPositionSelected(QueryContext clicked)
    {
        if (!IsValidTarget(clicked.SourcePosition))
        {
            Ctx.Controller.TransitionTo(new SelectedState(Ctx, clicked));
            return;
        }
        _targets.Add(clicked.SourcePosition);
        Ctx.Controller.LogState($"Target added: {clicked.SourcePosition} ({_targets.Count}/{_ability.TargetingOld.SelectionAmount})");
        if (_targets.Count >= _ability.TargetingOld.SelectionAmount) Confirm();
    }

    public override void OnHover(QueryContext? hovered)
    {
        Ctx.Renderer.ClearHighlights();
        Ctx.Renderer.HighlightPositions(_selectable.areas, GridHighlightType.SelectableTargets);
        if (hovered.HasValue)
        {
            var tentative = new List<(int, int)>(_targets) { hovered.Value.SourcePosition };
            if (_ability.TargetingOld.TryGetEffectPositions(_source, tentative, out var effect, selectable: _selectable))
                Ctx.Renderer.HighlightPositions(effect, GridHighlightType.EffectPreview);
        }
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

    private bool IsValidTarget((int, int) pos) => _selectable.areas.Contains(pos);

    private void Confirm()
    {
        var sourcePos1D = _source.Grid.ToPosition1D(_source.SourcePosition);
        var targets1D = _targets.Select(t => _source.Grid.ToPosition1D(t)).ToArray();
        int issuer = 0;
        if (_source.SourceEntity != null
            && _source.SourceEntity.TryGetComponent<ControlComponent>(out var ctrl))
            issuer = ctrl.PlayerController;
        var ok = Ctx.Dispatcher.Submit(new AbilityCommand(issuer, _ability.Id, sourcePos1D, targets1D));
        Ctx.Controller.LogState($"AbilityCommand: {_ability.Id} ok={ok}");
        Ctx.Controller.TransitionTo(new IdleState(Ctx));
    }
}
