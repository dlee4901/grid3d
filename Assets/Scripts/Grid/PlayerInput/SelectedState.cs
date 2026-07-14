public class SelectedState : PlayerInputStateBase
{
    public SelectedState(PlayerInputContext ctx) : base(ctx) { }

    public override void OnEnter() => Ctx.Renderer.ClearHighlights();
    public override void OnPositionSelected(QueryContext clicked) => Ctx.Input.Select(clicked);

    public override void OnAbilityActivate(Ability ability, QueryContext source) => Ctx.Controller.TransitionTo(new TargetingState(Ctx, ability, source));

    public override void OnAbilityPreview(Ability ability, QueryContext ctx) => Ctx.Renderer.HighlightAbilityRange(ability, ctx);

    public override void OnAbilityCancelPreview() => Ctx.Controller.TransitionTo(new IdleState(Ctx));

    public override void OnCancel() => Ctx.Controller.TransitionTo(new IdleState(Ctx));
}
