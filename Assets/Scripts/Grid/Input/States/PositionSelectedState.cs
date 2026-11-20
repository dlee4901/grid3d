public class PositionSelectedState : PlayerInputState
{
    private readonly GridSource _selected;
    public GridSource Selected => _selected;

    public PositionSelectedState(PlayerInputContext ctx, GridSource selected) : base(ctx) { _selected = selected; }

    public bool Actionable
        => _selected.Grid.IsAvailable(_selected.Position, Ctx.Dispatcher.LocalSeat);

    public override void OnEnter()
    {
        Ctx.Manager.ClearPreview();
        Ctx.Highlights.Clear();
        Ctx.Highlights.SetSelected(_selected.Position);
        if (!Actionable)
            Ctx.Highlights.Highlight(Ctx.Grid.GetAvailableEntityPositions(Ctx.Dispatcher.LocalSeat), HighlightType.AvailableSources);
    }

    public override void OnPositionSelected(GridSource clicked)
    {
        if (clicked.Position.Equals(_selected.Position))
            Ctx.Controller.TransitionTo(new IdleState(Ctx));
        else
            Ctx.Controller.TransitionTo(new PositionSelectedState(Ctx, clicked));
    }

    public override void OnAbilityActivate(Ability ability, GridSource source)
    {
        Ctx.Controller.TransitionTo(new AbilitySelectedState(Ctx, ability, source));
    }

    public override void OnCancel() => Ctx.Controller.TransitionTo(new IdleState(Ctx));
}
