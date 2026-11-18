public class SelectedState : PlayerInputState
{
    private readonly GridSource _selected;
    public GridSource Selected => _selected;

    public SelectedState(PlayerInputContext ctx, GridSource selected) : base(ctx) { _selected = selected; }

    public bool Actionable
        => _selected.Grid.IsAvailable(_selected.Position, Ctx.Dispatcher.LocalSeat);
    
    public override void OnEnter()
    {
        Ctx.Highlights.Clear();
        Ctx.Highlights.SetSelected(_selected.Position);
        if (!Actionable)
            Ctx.Highlights.Highlight(Ctx.Grid.GetAvailableEntityPositions(Ctx.Dispatcher.LocalSeat), HighlightType.AvailableSources);
    }
    public override void OnExit() => Ctx.Manager.ClearPreview();

    public override void OnPositionSelected(GridSource clicked)
    {
        if (clicked.Position.Equals(_selected.Position))
            Ctx.Controller.TransitionTo(new IdleState(Ctx));
        else
            Ctx.Controller.TransitionTo(new SelectedState(Ctx, clicked));
    }

    public override void OnAbilityActivate(Ability ability, GridSource source)
    {
        if (!Ctx.Grid.CanCastAt(source.Position, ability, Ctx.Dispatcher.LocalSeat)) return;
        Ctx.Controller.TransitionTo(new TargetingState(Ctx, ability, source));
    }
    
    public override void OnAbilityPreview(Ability ability, GridSource source)
    {
        if (!Ctx.Grid.CanCastAt(source.Position, ability, Ctx.Dispatcher.LocalSeat)) return;
        var steps = ability.Targeting.GetSelectableSteps(source);
        Ctx.Highlights.Clear();
        Ctx.Highlights.Highlight(steps, HighlightType.SelectionArea);
        PreviewSelfCast(ability, source, steps);
    }

    public override void OnAbilityCancelPreview()
    {
        Ctx.Highlights.Clear();
        Ctx.Manager.ClearPreview();
    }

    private void PreviewSelfCast(Ability ability, GridSource source, GridSteps steps)
    {
        if (ability.Targeting is not FillTargeting || steps.GroupCount == 0) return;
        foreach (var position in steps.GetPositions())
        {
            Ctx.Manager.Preview(seat => new AbilityCommand(seat, ability.Id, source.Position.Dim1, new[] { position.Dim1 }));
            return;
        }
    }

    public override void OnCancel() => Ctx.Controller.TransitionTo(new IdleState(Ctx));
}
