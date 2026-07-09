public class SelectedState : PlayerInputStateBase
{
    public SelectedState(PlayerInputContext ctx) : base(ctx) { }

    public override void OnPositionSelected(QueryContext clicked)
        => Ctx.Input.Select(clicked);

    public override void OnAbilityActivate(Ability ability, QueryContext source)
        => Ctx.Controller.TransitionTo(new TargetingState(Ctx, ability, source));

    public override void OnAbilityPreview(Ability ability, QueryContext ctx)
        => Ctx.GridManager.ShowAbilityPreview(ability, ctx);

    public override void OnAbilityCancelPreview()
        => Ctx.GridManager.ClearAbilityPreview();

    public override void OnCancel()
        => Ctx.Controller.TransitionTo(new IdleState(Ctx));
}
