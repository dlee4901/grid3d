public class IdleState : PlayerInputState
{
    public IdleState(PlayerInputContext ctx) : base(ctx) {}

    public override void OnEnter()
    {
        Ctx.Highlights.Clear();
        Ctx.Highlights.SetSelected(null);
        Ctx.Highlights.Highlight(Ctx.Grid.GetAvailableEntityPositions(Ctx.Dispatcher.LocalSeat), HighlightType.AvailableSources);
    }

    public override void OnPositionSelected(GridSource clicked)
        => Ctx.Controller.TransitionTo(new SelectedState(Ctx, clicked));
}
