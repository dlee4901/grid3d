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
        Ctx.Renderer.HighlightSelectableTargets(_ability, _source);
    }

    public override void OnPositionSelected(QueryContext clicked)
    {
        if (!IsValidTarget(clicked.SourcePosition))
        {
            Ctx.Controller.TransitionTo(new SelectedState(Ctx, clicked));
            return;
        }
        _targets.Add(clicked.SourcePosition);
        Ctx.Controller.LogState($"Target added: {clicked.SourcePosition} ({_targets.Count}/{_ability.Selection.SelectionAmount})");
        if (_targets.Count >= _ability.Selection.SelectionAmount) Confirm();
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
        var (areas, _) = _ability.Selection.GetSelectablePositions(_source);
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
