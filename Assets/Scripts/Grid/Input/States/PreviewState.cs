using System.Collections.Generic;

public class PreviewState : AbilityInputState
{
    public PreviewState(PlayerInputContext ctx, Ability ability, GridSource source, List<GridPosition> targets,
                        List<GridPosition> path = null)
        : base(ctx, ability, source, targets, path) {}

    public override void OnPress(GridSource pressed)
    {
        if (StartsTrace(pressed.Position)) base.OnPress(pressed);
    }

    public override void OnPositionSelected(GridSource clicked)
    {
        if (TryFinishTrace()) return;
        var position = clicked.Position;
        if (Targets.Count > 0 && position.Equals(Targets[Targets.Count - 1])) Confirm();
        else if (IsValidTarget(position) && !IsDuplicate(position))
            Ctx.Controller.TransitionTo(new PreviewState(Ctx, Ability, Source, WithLast(position)));
        else if (!position.Equals(Source.Position) && Ctx.Grid.GetEntity(position) != null)
            Ctx.Controller.TransitionTo(new PositionSelectedState(Ctx, clicked));
        else Back();
    }

    protected override void OnTargetSelected(GridPosition position) => Back();

    public override void OnAbilityActivate(Ability ability, GridSource source)
    {
        if (ability.Id == Ability.Id)
        {
            Back();
            return;
        }
        base.OnAbilityActivate(ability, source);
    }

    public override void OnCancel() => Back();

    public void Confirm()
    {
        if (!Castable) return;
        var ok = Ctx.Dispatcher.Submit(seat => AbilityCommand.From(seat, Ability.Id, Source.Position, Targets, Path,
                                                                          Ctx.Turns.ElapsedMs(seat)));
        Ctx.Controller.LogState($"AbilityCommand: {Ability.Id} ok={ok}");
        Ctx.Controller.TransitionTo(new IdleState(Ctx));
    }

    private void Back() => Ctx.Controller.TransitionTo(new AbilitySelectedState(Ctx, Ability, Source, WithoutLast()));
}
