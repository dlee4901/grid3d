public class EntitySelectedState : PlayerInputStateBase
{
    public EntitySelectedState(PlayerInputContext ctx) : base(ctx) { }

    public override void OnTileClicked(QueryContext clicked)
        => Ctx.Input.Select(clicked);

    public override void OnGridSelectionChanged(QueryContext? ctx)
    {
        if (!ctx.HasValue || ctx.Value.SourceEntity == null)
            Ctx.Controller.TransitionTo(new IdleState(Ctx));
    }

    public override void OnSkillActivate(Skill skill, QueryContext source)
        => Ctx.Controller.TransitionTo(new AimingSkillState(Ctx, skill, source));
}
