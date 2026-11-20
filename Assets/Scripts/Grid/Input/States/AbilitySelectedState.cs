using System.Collections.Generic;

public class AbilitySelectedState : AbilityInputState
{
    public AbilitySelectedState(PlayerInputContext ctx, Ability ability, GridSource source, List<GridPosition> targets = null)
        : base(ctx, ability, source, targets) {}

    protected override void OnTargetSelected(GridPosition position)
    {
        if (IsDuplicate(position))
        {
            ShowSelection(Targets);
            return;
        }
        var targets = WithLast(position);
        Ctx.Controller.LogState($"Target selected: {position} ({targets.Count}/{TargetCount})");
        if (targets.Count == TargetCount)
        {
            Ctx.Controller.TransitionTo(new PreviewState(Ctx, Ability, Source, targets));
            return;
        }
        Targets.Add(position);
        ShowSelection(Targets);
    }

    public override void OnCancel() => Ctx.Controller.TransitionTo(new PositionSelectedState(Ctx, Source));
}
