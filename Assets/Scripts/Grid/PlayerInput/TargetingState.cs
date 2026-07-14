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

    public override void OnEnter()
    {
        //Ctx.Input.Lock();
        Ctx.Renderer.HighlightSelectableTargets(_ability, _source);
    }

    public override void OnPositionSelected(QueryContext clicked)
    {
        if (!IsValidTarget(clicked.SourcePosition))
        {
            Ctx.Input.Select(clicked);
            return;
        }
        _targets.Add(clicked.SourcePosition);
        Ctx.Controller.LogState($"Target added: {clicked.SourcePosition} ({_targets.Count}/{_ability.Selection.SelectionAmount})");
        if (_targets.Count >= _ability.Selection.SelectionAmount) Confirm();
    }
    
    public override void OnSelectionChanged(QueryContext? ctx)
    {
        OnCancel();
    }

    // Clicking an ability icon while already aiming:
    //   same ability  -> toggle off (OnCancel -> SelectedState, since a unit is still selected)
    //   different one -> switch to aiming that ability from the same source
    public override void OnAbilityActivate(Ability ability, QueryContext source)
    {
        if (ability.Id == _ability.Id) OnCancel();
        else Ctx.Controller.TransitionTo(new TargetingState(Ctx, ability, source));
    }

    // Hovering another icon must NOT clobber the active aim preview.
    public override void OnAbilityPreview(Ability ability, QueryContext ctx) {}
    public override void OnAbilityCancelPreview() {}

    public override void OnCancel()
    {
        var next = Ctx.Input.HasSelection
            ? (IPlayerInputState)new SelectedState(Ctx)
            : new IdleState(Ctx);
        Ctx.Controller.TransitionTo(next);
    }

    private bool IsValidTarget((int, int) pos)
    {
        var (areas, _) = _ability.Selection.GetSelectablePositions(_source);
        return areas.Contains(pos);
    }

    private void Confirm()
    {
        var sourcePos1D = _source.Grid.ToPosition1D(_source.SourcePosition);
        var targets1D = _targets.Select(t => _source.Grid.ToPosition1D(t)).ToArray();
        // Local placeholder for the issuer; the dispatcher seam will stamp it from the
        // authenticated local seat once networking is wired in.
        int issuer = 0;
        if (_source.SourceEntity != null
            && _source.SourceEntity.TryGetComponent<ControlComponent>(out var ctrl))
            issuer = ctrl.PlayerController;
        var ok = Ctx.Dispatcher.Submit(new AbilityCommand(issuer, _ability.Id, sourcePos1D, targets1D));
        Ctx.Controller.LogState($"AbilityCommand: {_ability.Id} ok={ok}");
        // Grid/UI refresh is handled centrally by TurnExecutor.CommandApplied -> GridManager.
        // var next = Ctx.Input.HasSelection
        //     ? (IPlayerInputState)new EntitySelectedState(Ctx)
        //     : ;
        Ctx.Controller.TransitionTo(new IdleState(Ctx));
        Ctx.Input.ClearSelection();
    }
}
