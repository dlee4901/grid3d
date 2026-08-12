public class SelectedState : PlayerInputStateBase
{
    private readonly GridSource _selected;
    public GridSource Selected => _selected;

    public SelectedState(PlayerInputContext ctx, GridSource selected) : base(ctx) { _selected = selected; }

    public bool Actionable
        => _selected.Grid.IsAvailableControllable(_selected.Position);
    
    public override void OnEnter()
    {
        Ctx.Renderer.ClearHighlights();
        if (!Actionable)
            Ctx.Renderer.HighlightPositions(Ctx.Grid.GetControllableEntityPositions(), GridHighlightType.AvailableEntities);
    }
    public override void OnPositionSelected(GridSource clicked) => Ctx.Controller.TransitionTo(new SelectedState(Ctx, clicked));

    public override void OnAbilityActivate(Ability ability, GridSource source)
    {
        if (!Actionable) return;
        Ctx.Controller.TransitionTo(new TargetingState(Ctx, ability, source));
    }
    
    public override void OnAbilityPreview(Ability ability, GridSource source)
    {
        if (!Actionable) return;
        var steps = ability.Targeting.GetSelectableSteps(source);
        Ctx.Renderer.ClearHighlights();
        Ctx.Renderer.HighlightPositions(steps, GridHighlightType.AbilityRange);
    }
    public override void OnAbilityCancelPreview() => Ctx.Renderer.ClearHighlights();

    public override void OnCancel() => Ctx.Controller.TransitionTo(new IdleState(Ctx));
}
