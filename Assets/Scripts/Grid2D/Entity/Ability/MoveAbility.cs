using System.Collections.Generic;

public class MoveAbility : Ability
{
    public MoveAbility(AbilityConfig config) : base(config) { }

    internal override bool Execute(GridState state, GridPosition sourcePosition, List<GridPosition> targetPositions)
    {
        if (targetPositions.Count != 1) return false;
        var source = state.GetEntity(sourcePosition);
        if (source == null) return false;
        if (!Targeting.GetEffectSteps(new GridSource(state, sourcePosition, source), targetPositions, out _)) return false;
        return state.ChangeEntityPosition(sourcePosition, targetPositions[0]);
    }
}
