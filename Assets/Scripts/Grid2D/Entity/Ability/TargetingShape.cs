using System.Collections.Generic;

public static class TargetingShape
{
    private const int BoardSize = 21;
    private const int Center = BoardSize / 2;

    private static readonly (int X, int Y)[] AimOrder =
        { (0, 1), (1, 1), (1, 0), (1, -1), (0, -1), (-1, -1), (-1, 0), (-1, 1) };

    private static GridState _board;

    private static GridState Board
    {
        get
        {
            if (_board != null) return _board;
            var definition = new GridDefinition { Id = "TargetingShape", X = BoardSize, Y = BoardSize };
            definition.Init();
            _board = new GridState(definition);
            return _board;
        }
    }

    public static bool ReachesAnywhere(Ability ability)
    {
        foreach (var effect in ability.Effects)
            if (effect is MoveEffect { Teleport: true }) return true;
        var area = ability.Targeting is PositionTargeting position ? position.SelectableArea : ability.Targeting?.EffectArea;
        if (area?.Traversals == null) return false;
        foreach (var traversal in area.Traversals)
            if (!traversal.Linear && traversal.MaxDistance == GridTraversal.Unlimited) return true;
        return false;
    }

    public static int DirectionMask(AbilityTargeting targeting)
    {
        if (targeting is DirectionTargeting { Grouping: { } grouping })
        {
            var grouped = 0;
            for (var i = 0; i < grouping.Length && i < GridTraversal.UnidirectionalCount; i++)
                if (grouping[i] != 0) grouped |= 1 << i;
            return grouped;
        }
        var area = targeting is PositionTargeting position ? position.SelectableArea : targeting?.EffectArea;
        if (area?.Traversals == null) return 0;
        var mask = 0;
        foreach (var traversal in area.Traversals)
        {
            if (traversal.MaxDistance == 0 || area.MaxDistance == 0) continue;
            var directions = traversal.DirectionMask();
            for (var i = 0; i < directions.Length; i++)
                if (directions[i]) mask |= 1 << i;
        }
        return mask;
    }

    public static bool TryGetArea(AbilityTargeting targeting, out List<(int X, int Y)> cells, out bool fromCaster)
    {
        cells = new List<(int X, int Y)>();
        fromCaster = targeting is not PositionTargeting;
        if (targeting?.EffectArea == null) return false;

        var center = new GridPosition(Board, (Center, Center));
        var source = new GridSource(Board, center);
        GridSteps steps;
        if (targeting is DirectionTargeting direction)
        {
            if (!TryGetAim(direction.GetSelectableSteps(source), out var aim)
                || !direction.GetEffectSteps(source, aim, out steps)) return false;
        }
        else steps = targeting.EffectArea.GetGridSteps(source);

        var seen = new HashSet<(int, int)>();
        for (var group = 0; group < steps.GroupCount; group++)
        foreach (var position in steps.GetPositions(group))
        {
            var offset = (position.Dim2.x - Center, position.Dim2.y - Center);
            if (fromCaster && offset == (0, 0)) continue;
            if (seen.Add(offset)) cells.Add(offset);
        }
        return cells.Count > 0;
    }

    private static bool TryGetAim(GridSteps selectable, out GridPosition aim)
    {
        foreach (var (x, y) in AimOrder)
        {
            aim = new GridPosition(Board, (Center + x, Center + y));
            if (selectable.Contains(aim)) return true;
        }
        aim = default;
        return false;
    }
}
