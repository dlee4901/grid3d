using System.Collections.Generic;

public class MoveSkill : Skill
{
    public MoveSkill(SkillConfig config) : base(config) { }

    public override bool Execute(GridState state, int sourcePosition, IReadOnlyList<int> targets)
    {
        if (targets.Count < 1) return false;
        return state.ChangeEntityPosition(sourcePosition, targets[0]);
    }
}
