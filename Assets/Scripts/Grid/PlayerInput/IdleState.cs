public class IdleState : PlayerInputStateBase
{
    public IdleState(PlayerInputContext ctx) : base(ctx) { }

    public override void OnTileClicked(QueryContext clicked)
        => Ctx.Input.Select(clicked);

    public override void OnGridSelectionChanged(QueryContext? ctx)
    {
        if (ctx.HasValue && ctx.Value.SourceEntity != null)
            Ctx.Controller.TransitionTo(new EntitySelectedState(Ctx));
    }
}
