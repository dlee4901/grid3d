using System.Collections.Generic;
using System.Linq;

public class TargetingState : PlayerInputStateBase
{
    private readonly Ability _ability;
    private readonly QueryContext _source;
    private readonly List<(int, int)> _targets = new();

    public TargetingState(PlayerInputContext ctx, Ability ability, QueryContext source) : base(ctx)
    {
        _ability = ability;
        _source = source;
    }

    public string AbilityId => _ability.Id;
    public QueryContext Source => _source;

    public override void OnEnter()
    {
        var (areas, _) = _ability.Targeting.GetSelectablePositions(_source);
        Ctx.Renderer.ClearHighlights();
        Ctx.Renderer.HighlightPositions(areas, GridHighlightType.SelectableTargets);
    }

    public override void OnPositionSelected(QueryContext clicked)
    {
        if (!IsValidTarget(clicked.SourcePosition))
        {
            Ctx.Controller.TransitionTo(new SelectedState(Ctx, clicked));
            return;
        }
        _targets.Add(clicked.SourcePosition);
        Ctx.Controller.LogState($"Target added: {clicked.SourcePosition} ({_targets.Count}/{_ability.Targeting.SelectionAmount})");
        if (_targets.Count >= _ability.Targeting.SelectionAmount) Confirm();
    }

    public override void OnHover(QueryContext? hovered)
    {
        var (areas, _) = _ability.Targeting.GetSelectablePositions(_source);
        Ctx.Renderer.ClearHighlights();
        Ctx.Renderer.HighlightPositions(areas, GridHighlightType.SelectableTargets);
        if (hovered.HasValue)
        {
            var tentative = new List<(int, int)>(_targets) { hovered.Value.SourcePosition };
            if (_ability.Targeting.TryGetEffectPositions(_source, tentative, out var effect))
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

    private bool IsValidTarget((int, int) pos)
    {
        var (areas, _) = _ability.Targeting.GetSelectablePositions(_source);
        return areas.Contains(pos);
    }

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
