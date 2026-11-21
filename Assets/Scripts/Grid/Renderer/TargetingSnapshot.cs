using System.Collections.Generic;

public readonly struct TargetingSnapshot
{
    public readonly GridSource Source;
    public readonly Ability Ability;
    public readonly GridSteps Selectable;
    public readonly IReadOnlyList<GridPosition> Targets;
    public readonly IReadOnlyList<GridPosition> Path;
    public readonly GridSteps NextSteps;

    public TargetingSnapshot(GridSource source, Ability ability, GridSteps selectable, IReadOnlyList<GridPosition> targets,
                             IReadOnlyList<GridPosition> path = null, GridSteps nextSteps = null)
    {
        Source = source;
        Ability = ability;
        Selectable = selectable;
        Targets = targets;
        Path = path;
        NextSteps = nextSteps;
    }

    public int TargetCount => Targets?.Count ?? 0;
    public GridPosition LastTarget => Targets[Targets.Count - 1];
}
