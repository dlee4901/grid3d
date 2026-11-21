public class IdleState : PlayerInputState
{
    public IdleState(PlayerInputContext ctx) : base(ctx) {}

    public override void OnEnter()
    {
        Ctx.Display.ShowSources(null, Ctx.Grid.GetAvailableEntityPositions(Ctx.Dispatcher.LocalSeat));
    }

    public override void OnPositionSelected(GridSource clicked)
        => Ctx.Controller.TransitionTo(new PositionSelectedState(Ctx, clicked));
}
