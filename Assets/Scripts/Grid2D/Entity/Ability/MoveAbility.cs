using System.Collections.Generic;

public class MoveAbility : Ability
{
    public MoveAbility(AbilityConfig config) : base(config) { }

    public override bool Execute(GridState state, int sourcePosition, IReadOnlyList<int> targets)
    {
        if (targets.Count < 1) return false;
        return state.ChangeEntityPosition(sourcePosition, targets[0]);
    }
}
