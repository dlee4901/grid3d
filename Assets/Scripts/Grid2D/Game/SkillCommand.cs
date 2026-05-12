using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class SkillCommand : ICommand
{
    public Skill Skill { get; }
    public QueryContext Source { get; }
    public IReadOnlyList<(int, int)> Targets { get; }

    public SkillCommand(Skill skill, QueryContext source, IReadOnlyList<(int, int)> targets)
    {
        Skill = skill;
        Source = source;
        Targets = targets;
    }

    public bool ApplyTo(GridState state)
    {
        // TODO: resolve skill effects against state. For now, log and accept.
        Debug.Log($"[SkillCommand] {Skill.Id} from {Source.SourcePosition} → [{string.Join(", ", Targets.Select(t => t.ToString()))}]");
        return true;
    }
}
