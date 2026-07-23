public class SelectedState : PlayerInputStateBase
{
    private readonly QueryContext _selected;
    public QueryContext Selected => _selected;

    public SelectedState(PlayerInputContext ctx, QueryContext selected) : base(ctx) { _selected = selected; }

    public bool Actionable
        => _selected.Grid.IsAvailableControllable(_selected.Grid.ToPosition1D(_selected.SourcePosition));
    
    public override void OnEnter()
    {
        Ctx.Renderer.ClearHighlights();
        if (!Actionable)
            Ctx.Renderer.HighlightPositions(Ctx.Grid.GetControllableEntities(), GridHighlightType.AvailableEntities);
    }
    public override void OnPositionSelected(QueryContext clicked) => Ctx.Controller.TransitionTo(new SelectedState(Ctx, clicked));

    public override void OnAbilityActivate(Ability ability, QueryContext source)
    {
        if (!Actionable) return;
        Ctx.Controller.TransitionTo(new TargetingState(Ctx, ability, source));
    }
    
    public override void OnAbilityPreview(Ability ability, QueryContext ctx)
    {
        if (!Actionable) return;
        var (areas, _) = ability.Targeting.GetSelectablePositions(ctx);
        Ctx.Renderer.ClearHighlights();
        Ctx.Renderer.HighlightPositions(areas, GridHighlightType.AbilityRange);
    }
    public override void OnAbilityCancelPreview() => Ctx.Renderer.ClearHighlights();

    public override void OnCancel() => Ctx.Controller.TransitionTo(new IdleState(Ctx));
}
