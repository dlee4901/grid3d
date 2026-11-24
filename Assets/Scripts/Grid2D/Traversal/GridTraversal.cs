#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

public enum GridDirection {North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest, NorthCone, EastCone, SouthCone, WestCone, Vertical, Horizontal, Diagonal, Orthogonal, All, Custom}
public enum GridDirectionFacing {North, East, South, West}

// [Flags] 
// public enum EntityPassthrough
// {
//     None = 0,
//     Self = 1 << 0,
//     Enemy = 1 << 1,
//     Ally = 1 << 2,
//     Neutral = 1 << 3,
//     Any = Self | Enemy | Ally | Neutral
// }

public class GridTraversal
{
    public const int UnidirectionalCount = 8;
    
    public GridDirection Direction { get; init; }
    public bool[] CustomDirection { get; init; } = new bool[UnidirectionalCount];

    public const int Unlimited = -1;

    public int MaxDistance { get; init; } = 0;
    public bool Linear { get; init; } = true;
    public bool AbsoluteDirection { get; init; } = false;

    public int StartWidth { get; init; } = 0;
    public bool DiagonalWidth { get; init; } = false;

    public Occupant Passthrough { get; init; } = Occupant.Empty;

    public GridTraversal? Chain { get; init; }
    public bool ChainAll { get; init; } = false;

    public List<GridStep> GetSteps(GridSource source)
    {
        var steps = new HashSet<GridStep>();
        foreach (var iter in Traverse(source))
            steps = iter;
        return steps.ToList();
    }

    public HashSet<GridStep> GetStepsSet(GridSource source)
    {
        var steps = new HashSet<GridStep>();
        foreach (var iter in Traverse(source))
            steps = iter;
        return steps;
    }

    public List<GridStep> GetTraceSteps(GridSource source)
    {
        return GetTraceSteps(source, new GridStep(source.Position, FacingDirection(source), 0));
    }

    private List<GridStep> GetTraceSteps(GridSource source, GridStep initialGridStep)
    {
        return Expand(source, initialGridStep.Position, initialGridStep.Distance, initialGridStep.Direction).ToList();
    }

    private IEnumerable<HashSet<GridStep>> Traverse(GridSource source)
    {
        var steps = new HashSet<GridStep>();
        var queue = new Queue<GridStep>();
        var chained = new List<GridStep>();
        var direction = FacingDirection(source);
        var initialStep = new GridStep(source.Position, direction, 0);
        steps.Add(initialStep);
        queue.Enqueue(initialStep);
        
        if (StartWidth > 0)
        {
            var widthPositions = GetWidthPositions(source, StartWidth, initialStep.Position, initialStep.Direction);
            foreach (var position in widthPositions)
            {
                var widthStep = new GridStep(position, direction, 0);
                steps.Add(widthStep);
                queue.Enqueue(widthStep);
            }
        }

        yield return steps;

        while (queue.Count > 0)
        {
            var currentStep = queue.Dequeue();
            var expanded = false;
            foreach (var nextStep in Expand(source, currentStep.Position, currentStep.Distance + 1, currentStep.Direction))
            {
                expanded = true;
                if (!steps.Add(nextStep)) continue;
                queue.Enqueue(nextStep);
                yield return steps;
            }
            if (Chain != null && currentStep.Distance > 0 && (ChainAll || !expanded)) chained.Add(currentStep);
        }

        foreach (var end in chained)
            foreach (var step in Chain!.GetStepsSet(new GridSource(source.Grid, end.Position, source.Entity))) 
                if (step.Distance > 0 && !step.Position.Equals(source.Position))
                    steps.Add(new GridStep(step.Position, end.Direction, end.Distance + step.Distance));
        yield return steps;
    }

    private IEnumerable<GridStep> Expand(GridSource source, GridPosition currentPosition, int curDistance, GridDirection gridDirection)
    {
        if (!currentPosition.IsValid()) yield break;
        
        var grid = source.Grid;
        if (MaxDistance != Unlimited && curDistance > MaxDistance) yield break;

        var directionVectors = GetDirectionVectors(gridDirection);
        var currentPosition2D = currentPosition.Dim2;

        for (var i = 0; i < directionVectors.Length; i++)
        {
            var (xOffset, yOffset) = directionVectors[i];
            var position = new GridPosition(grid, (currentPosition2D.x + xOffset, currentPosition2D.y + yOffset));
            if (position.Equals(currentPosition)) continue;

            if (!position.IsValid()) continue;
            if (!CanEnter(grid, position, source.Entity)) continue;
            //if (collideMask != null && (entity = grid.GetEntity(tile)) != null && collideMask(entity)) continue;

            if (Linear) yield return new GridStep(position, (GridDirection)i, curDistance);
            else        yield return new GridStep(position, gridDirection, curDistance);
        }
    }

    private bool CanEnter(GridState grid, GridPosition position, Entity? sourceEntity)
        => (Passthrough & Occupants.At(grid, position, sourceEntity)) != 0;

    private List<GridPosition> GetWidthPositions(GridSource source, int width, GridPosition startPosition, GridDirection gridDirection)//(int, int)[] directionVectors)
    {
        List<GridPosition> widthPositions = new();
        
        var directionVectors = GetDirectionVectors(gridDirection);
        
        var zeroTuple = (0, 0);
        var leftShift = zeroTuple;
        var rightShift = zeroTuple;
        for (var i = 1; i <= width; i++)
        {
            if (!directionVectors[0].Equals(zeroTuple) || !directionVectors[4].Equals(zeroTuple))
            {
                leftShift = (i, 0);
                rightShift = (-i, 0);
            }
            if (!directionVectors[2].Equals(zeroTuple) || !directionVectors[6].Equals(zeroTuple))
            {
                leftShift = (0, i);
                rightShift = (0, -i);
            }
            
            if (DiagonalWidth)
            {
                if (!directionVectors[1].Equals(zeroTuple) || !directionVectors[5].Equals(zeroTuple))
                {
                    leftShift = (i, -i);
                    rightShift = (-i, i);
                }
            
                if (!directionVectors[3].Equals(zeroTuple) || !directionVectors[7].Equals(zeroTuple))
                {
                    leftShift = (-i, -i);
                    rightShift = (i, i);
                }
            }
            
            var newPosition = startPosition.Add(leftShift);
            if (newPosition.IsValid()) widthPositions.Add(newPosition);
            newPosition = startPosition.Add(rightShift);
            if (newPosition.IsValid()) widthPositions.Add(newPosition);
        }
        return widthPositions;
    }

    private GridDirection FacingDirection(GridSource source)
    {
        if (AbsoluteDirection) return Direction;
        var facing = source.Entity?.Facing ?? GridDirectionFacing.North;
        return Rotate(Direction, facing);
    }

    public static GridDirection Rotate(GridDirection direction, GridDirectionFacing facing)
    {
        var quarterRotations = (int)facing;
        if (direction < GridDirection.NorthCone) return (GridDirection)(((int)direction + quarterRotations * 2) % UnidirectionalCount);
        if (direction <= GridDirection.WestCone) return GridDirection.NorthCone + ((int)direction - (int)GridDirection.NorthCone + quarterRotations) % 4;
        if (direction is GridDirection.Vertical or GridDirection.Horizontal && quarterRotations % 2 == 1)
            return direction == GridDirection.Vertical ? GridDirection.Horizontal : GridDirection.Vertical;
        return direction;
    }

    private (int, int)[] GetDirectionVectors(GridDirection gridDirection)
    {
        var unitVectors = new (int, int)[UnidirectionalCount];
        var absoluteDirections = GetAbsoluteDirections(gridDirection);
        for (var i = 0; i < UnidirectionalCount; i++)
        {
            var xOffset = 0;
            var yOffset = 0;
            if (absoluteDirections[i])
            {
                if (i > 4)               xOffset = -1;
                else if (i > 0 && i < 4) xOffset = 1;
                if (i > 2 && i < 6)      yOffset = -1;
                else if (i < 2 || i > 6) yOffset = 1;
            }
            unitVectors[i] = (xOffset, yOffset);
        }
        return unitVectors;
    }

    public bool[] DirectionMask() => GetAbsoluteDirections(Direction);

    private bool[] GetAbsoluteDirections(GridDirection gridDirection)
    {
        var absoluteDirections = new bool[UnidirectionalCount];//List<bool>{false, false, false, false, false, false, false, false};
        switch (gridDirection)
        {
            case GridDirection.North:
                absoluteDirections[0] = true;
                break;
            case GridDirection.NorthEast:
                absoluteDirections[1] = true;
                break;
            case GridDirection.East:
                absoluteDirections[2] = true;
                break;
            case GridDirection.SouthEast:
                absoluteDirections[3] = true;
                break;
            case GridDirection.South:
                absoluteDirections[4] = true;
                break;
            case GridDirection.SouthWest:
                absoluteDirections[5] = true;
                break;
            case GridDirection.West:
                absoluteDirections[6] = true;
                break;
            case GridDirection.NorthWest:
                absoluteDirections[7] = true;
                break;
            case GridDirection.NorthCone:
                absoluteDirections[7] = true;
                absoluteDirections[0] = true;
                absoluteDirections[1] = true;
                break;
            case GridDirection.EastCone:
                absoluteDirections[1] = true;
                absoluteDirections[2] = true;
                absoluteDirections[3] = true;
                break;
            case GridDirection.SouthCone:
                absoluteDirections[3] = true;
                absoluteDirections[4] = true;
                absoluteDirections[5] = true;
                break;
            case GridDirection.WestCone:
                absoluteDirections[5] = true;
                absoluteDirections[6] = true;
                absoluteDirections[7] = true;
                break;
            case GridDirection.Vertical:
                absoluteDirections[0] = true;
                absoluteDirections[4] = true;
                break;
            case GridDirection.Horizontal:
                absoluteDirections[2] = true;
                absoluteDirections[6] = true;
                break;
            case GridDirection.Diagonal:
                for (var i = 1; i < UnidirectionalCount; i += 2) absoluteDirections[i] = true;
                break;
            case GridDirection.Orthogonal:
                for (var i = 0; i < UnidirectionalCount; i += 2) absoluteDirections[i] = true;
                break;
            case GridDirection.All:
                for (var i = 0; i < UnidirectionalCount; i++) absoluteDirections[i] = true;
                break;
            case GridDirection.Custom:
                for (var i = 0; i < UnidirectionalCount; i++) absoluteDirections[i] = CustomDirection[i];
                break;
            default:
                return absoluteDirections;
        }
        return absoluteDirections;
    }
}
