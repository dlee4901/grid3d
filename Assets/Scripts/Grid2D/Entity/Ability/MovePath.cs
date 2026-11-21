using System.Collections.Generic;

public static class MovePath
{
    public static bool CanTrace(Ability ability)
    {
        if (ability.Targeting is not PositionTargeting { Targets: 1 }) return false;
        foreach (var effect in ability.Effects)
            if (effect is MoveEffect { Teleport: false }) return true;
        return false;
    }

    public static int Budget(GridSteps selectable)
    {
        var budget = 0;
        foreach (var position in selectable.GetPositions())
        {
            var distance = selectable.GetShortestDistance(position);
            if (distance > budget) budget = distance;
        }
        return budget;
    }

    public static bool CanStep(GridSource origin, Ability ability, GridSteps selectable, IReadOnlyList<GridPosition> path, GridPosition next)
    {
        if (path.Count >= Budget(selectable) || !selectable.Contains(next) || next.Equals(origin.Position)) return false;
        foreach (var position in path)
            if (position.Equals(next)) return false;
        var from = path.Count == 0 ? origin.Position : path[path.Count - 1];
        var adjacent = ability.Targeting.GetSelectableSteps(new GridSource(origin.Grid, from, origin.Entity));
        return adjacent.GetShortestDistance(next) == 1;
    }

    public static GridSteps NextSteps(GridSource origin, Ability ability, GridSteps selectable, IReadOnlyList<GridPosition> path)
    {
        var next = new GridSteps();
        var from = path.Count == 0 ? origin.Position : path[path.Count - 1];
        foreach (var position in selectable.GetPositions())
            if (CanStep(origin, ability, selectable, path, position))
                next.Add(new GridStep(position, DirectionOf(from, position), 1));
        return next;
    }

    private static GridDirection DirectionOf(GridPosition from, GridPosition to)
    {
        var dx = System.Math.Sign(to.Dim2.x - from.Dim2.x);
        var dy = System.Math.Sign(to.Dim2.y - from.Dim2.y);
        return (dx, dy) switch
        {
            (0, 1) => GridDirection.North,
            (1, 1) => GridDirection.NorthEast,
            (1, 0) => GridDirection.East,
            (1, -1) => GridDirection.SouthEast,
            (0, -1) => GridDirection.South,
            (-1, -1) => GridDirection.SouthWest,
            (-1, 0) => GridDirection.West,
            _ => GridDirection.NorthWest
        };
    }

    public static bool IsValid(GridSource origin, Ability ability, GridPosition target, IReadOnlyList<GridPosition> path)
    {
        if (!CanTrace(ability) || path.Count == 0 || !path[path.Count - 1].Equals(target)) return false;
        var selectable = ability.Targeting.GetSelectableSteps(origin);
        var traced = new List<GridPosition>();
        foreach (var next in path)
        {
            if (!CanStep(origin, ability, selectable, traced, next)) return false;
            traced.Add(next);
        }
        return true;
    }
}
