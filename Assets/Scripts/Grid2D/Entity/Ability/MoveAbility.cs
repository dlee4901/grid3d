using System.Collections.Generic;

public class MoveAbility : Ability
{
    public MoveAbility(AbilityConfig config) : base(config) { }

    public override bool Execute(GridState state, GridPosition sourcePosition, List<GridPosition> targetPositions)
    {
        if (targetPositions.Count != 1) return false;
        return state.ChangeEntityPosition(sourcePosition, targetPositions[0]);
    }
}
