public class IdleState : PlayerInputState
{
    public IdleState(PlayerInputContext ctx) : base(ctx) {}

    public override void OnEnter()
    {
        Ctx.Manager.ClearPreview();
        Ctx.Highlights.Clear();
        Ctx.Highlights.SetSelected(null);
        Ctx.Highlights.Highlight(Ctx.Grid.GetAvailableEntityPositions(Ctx.Dispatcher.LocalSeat), HighlightType.AvailableSources);
    }

    public override void OnPositionSelected(GridSource clicked)
        => Ctx.Controller.TransitionTo(new PositionSelectedState(Ctx, clicked));
}
