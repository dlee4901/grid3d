public class IdleState : PlayerInputStateBase
{
    public IdleState(PlayerInputContext ctx) : base(ctx) {}

    public override void OnEnter()
        => Ctx.Renderer.HighlightAvailableEntities();

    public override void OnPositionSelected(QueryContext clicked)
        => Ctx.Input.Select(clicked);

    public override void OnSelectionChanged(QueryContext? ctx)
        => Ctx.Controller.TransitionTo(new SelectedState(Ctx));
}
