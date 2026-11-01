public class IdleState : PlayerInputStateBase
{
    public IdleState(PlayerInputContext ctx) : base(ctx) {}

    public override void OnEnter()
    {
        Ctx.Renderer.ClearHighlights();
        Ctx.Renderer.HighlightPositions(Ctx.Grid.GetAvailableEntityPositions(Ctx.Dispatcher.LocalSeat), HighlightType.AvailableSources);
    }

    public override void OnPositionSelected(GridSource clicked)
        => Ctx.Controller.TransitionTo(new SelectedState(Ctx, clicked));
}
