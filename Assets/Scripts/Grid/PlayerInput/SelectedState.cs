public class SelectedState : PlayerInputStateBase
{
    public SelectedState(PlayerInputContext ctx) : base(ctx) { }

    public override void OnPositionSelected(QueryContext clicked)
        => Ctx.Input.Select(clicked);

    public override void OnSkillActivate(Skill skill, QueryContext source)
        => Ctx.Controller.TransitionTo(new TargetingState(Ctx, skill, source));
    
    public override void OnCancel() 
        => Ctx.Controller.TransitionTo(new IdleState(Ctx));
}
