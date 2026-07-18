public class IdleState : PlayerInputStateBase
{
    public IdleState(PlayerInputContext ctx) : base(ctx) {}

    public override void OnEnter()
        => Ctx.Renderer.HighlightAvailableEntities();

    public override void OnPositionSelected(QueryContext clicked)
        => Ctx.Controller.TransitionTo(new SelectedState(Ctx, clicked));
}
