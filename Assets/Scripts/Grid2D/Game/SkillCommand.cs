public sealed class SkillCommand : ICommand
{
    public string SkillId { get; }
    public int SourcePosition { get; }
    public int[] Targets { get; }

    public SkillCommand(string skillId, int sourcePosition, int[] targets)
    {
        SkillId = skillId;
        SourcePosition = sourcePosition;
        Targets = targets;
    }

    public bool ApplyTo(GridState state)
    {
        var entity = state.GetEntity(SourcePosition);
        if (entity == null)
        {
            GridLog.Warning($"[SkillCommand] no entity at {SourcePosition}");
            return false;
        }
        if (!entity.TryGetComponent<SkillComponent>(out var skills)) return false;
        if (!skills.Dictionary.TryGetValue(SkillId, out var skill))
        {
            GridLog.Warning($"[SkillCommand] entity {entity.Id} has no skill {SkillId}");
            return false;
        }
        return skill.Execute(state, SourcePosition, Targets);
    }
}
