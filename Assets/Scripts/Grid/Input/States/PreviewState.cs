using System.Collections.Generic;

public class PreviewState : AbilityInputState
{
    public PreviewState(PlayerInputContext ctx, Ability ability, GridSource source, List<GridPosition> targets,
                        List<GridPosition> path = null)
        : base(ctx, ability, source, targets, path) {}

    public override void OnPress(GridSource pressed) {}

    public override void OnPositionSelected(GridSource clicked) => Back();

    protected override void OnTargetSelected(GridPosition position) => Back();

    public override void OnAbilityActivate(Ability ability, GridSource source) => Back();

    public override void OnCancel() => Back();

    public void Confirm()
    {
        if (!Castable) return;
        var ok = Ctx.Dispatcher.Submit(seat => AbilityCommand.From(seat, Ability.Id, Source.Position, Targets, Path));
        Ctx.Controller.LogState($"AbilityCommand: {Ability.Id} ok={ok}");
        Ctx.Controller.TransitionTo(new IdleState(Ctx));
    }

    private void Back() => Ctx.Controller.TransitionTo(new AbilitySelectedState(Ctx, Ability, Source, WithoutLast()));
}
