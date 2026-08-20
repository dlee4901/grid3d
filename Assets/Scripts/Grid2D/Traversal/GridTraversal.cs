#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

public enum GridDirection {North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest, NorthCone, EastCone, SouthCone, WestCone, Vertical, Horizontal, Diagonal, Straight, Line, Custom}
public enum GridDirectionFacing {North, East, South, West}
[Flags] public enum EntityPassthrough {None, Ally, Enemy, All=Ally|Enemy}

public class GridTraversal
{
    public const int UnidirectionalCount = 8;
    
    public GridDirection Direction { get; set; }
    public bool[] CustomDirection { get; set; } = new bool[UnidirectionalCount];

    public int MaxDistance { get; set; } = 0;
    public bool Linear { get; set; } = true;
    public bool AbsoluteDirection { get; set; } = false;

    public int StartWidth { get; set; } = 0;
    public bool DiagonalWidth { get; set; } = false;

    public EntityPassthrough Passthrough { get; set; } = EntityPassthrough.None;
    public PredicateConfig? PassthroughQuery { get; set; }

    public GridTraversal? Chain { get; set; }
    public int ChainOffset { get; set; } = 0; // IF (n > 0) n ~ distance ELSE maxDistReached + n ~ maxDistReached

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
        return GetTraceSteps(source, new GridStep(source.Position, Direction, 0));
    }

    private List<GridStep> GetTraceSteps(GridSource source, GridStep initialGridStep)
    {
        return Expand(source, initialGridStep.Position, initialGridStep.Distance, initialGridStep.Direction).ToList();
    }

    private IEnumerable<HashSet<GridStep>> Traverse(GridSource source)
    {
        var steps = new HashSet<GridStep>();
        var queue = new Queue<GridStep>();
        var initialStep = new GridStep(source.Position, Direction, 0);
        steps.Add(initialStep);
        queue.Enqueue(initialStep);
        
        if (StartWidth > 0)
        {
            var widthPositions = GetWidthPositions(source, StartWidth, initialStep.Position, initialStep.Direction);
            foreach (var position in widthPositions)
            {
                var widthStep = new GridStep(position, Direction, 0);
                steps.Add(widthStep);
                queue.Enqueue(widthStep);
            }
        }
            
        while (queue.Count > 0)
        {
            var currentStep = queue.Dequeue();
            foreach (var nextStep in Expand(source, currentStep.Position, currentStep.Distance + 1, currentStep.Direction))
            {
                if (!steps.Add(nextStep)) continue;
                queue.Enqueue(nextStep);
                yield return steps;
            }
        }
    }

    private IEnumerable<GridStep> Expand(GridSource source, GridPosition currentPosition, int curDistance, GridDirection gridDirection)
    {
        if (!currentPosition.IsValid()) yield break;
        
        var grid = source.Grid;
        var maxDistance = MaxDistance < 0 || MaxDistance > grid.X * grid.Y ? grid.X * grid.Y : MaxDistance;
        if (curDistance > maxDistance) yield break;

        var directionFacing = source.Entity?.Facing ?? GridDirectionFacing.North;
        var directionVectors = GetDirectionVectors(gridDirection, directionFacing);
        var currentPosition2D = currentPosition.Dim2;

        for (var i = 0; i < directionVectors.Length; i++)
        {
            var (xOffset, yOffset) = directionVectors[i];
            var position = new GridPosition(grid, (currentPosition2D.x + xOffset, currentPosition2D.y + yOffset));
            if (position.Equals(currentPosition)) continue;

            if (!position.IsValid()) continue;
            Entity entity;
            if ((entity = grid.GetEntity(position)) != null && IsColliding(entity, source.Entity)) continue;
            //if (collideMask != null && (entity = grid.GetEntity(tile)) != null && collideMask(entity)) continue;

            if (Linear) yield return new GridStep(position, (GridDirection)i, curDistance);
            else        yield return new GridStep(position, gridDirection, curDistance);

            if (Chain == null || curDistance < ChainOffset) continue;
            foreach (var step in Chain.Expand(source, position, curDistance, Chain.Direction))
                yield return step;
        }
    }

    private bool IsColliding(Entity targetEntity, IReadOnlyEntity? sourceEntity=null)
    {
        Func<Entity, bool>? predicate = null;
        if (PassthroughQuery != null) predicate = PredicateFactory<Entity>.Create(PassthroughQuery);

        if (sourceEntity != null && sourceEntity.TryGetComponent<ControlComponent>(out var sourceControl) && targetEntity.TryGetComponent<ControlComponent>(out var targetControl))
        {
            if ((!Passthrough.HasFlag(EntityPassthrough.Enemy) && !sourceControl.IsAlly(targetControl))
            || (!Passthrough.HasFlag(EntityPassthrough.Ally) && sourceControl.IsAlly(targetControl)))
                return predicate?.Invoke(targetEntity) ?? true;
        }
        //if (!Passthrough.HasFlag(EntityPassthrough.Unit) && targetEntity.GetType() == typeof(Unit)) return true;
        //if (!Passthrough.HasFlag(EntityPassthrough.Obstacle) && targetEntity.GetType() != typeof(Unit)) return true;
        return false;
    }

    private List<GridPosition> GetWidthPositions(GridSource source, int width, GridPosition startPosition, GridDirection gridDirection)//(int, int)[] directionVectors)
    {
        List<GridPosition> widthPositions = new();
        
        var directionFacing = source.Entity?.Facing ?? GridDirectionFacing.North;
        var directionVectors = GetDirectionVectors(gridDirection, directionFacing);
        
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

    private (int, int)[] GetDirectionVectors(GridDirection gridDirection, GridDirectionFacing gridDirectionFacing=GridDirectionFacing.North)
    {
        var unitVectors = new (int, int)[UnidirectionalCount];
        var absoluteDirections = GetAbsoluteDirections(gridDirection, gridDirectionFacing);
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

    private bool[] GetAbsoluteDirections(GridDirection gridDirection, GridDirectionFacing gridDirectionFacing=GridDirectionFacing.North)
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
            case GridDirection.Straight:
                for (var i = 0; i < UnidirectionalCount; i += 2) absoluteDirections[i] = true;
                break;
            case GridDirection.Line:
                for (var i = 0; i < UnidirectionalCount; i++) absoluteDirections[i] = true;
                break;
            case GridDirection.Custom:
                for (var i = 0; i < UnidirectionalCount; i++) absoluteDirections[i] = CustomDirection[i];
                break;
            default:
                return absoluteDirections;
        }
        var shift = gridDirectionFacing switch
        {
            GridDirectionFacing.East => 6,
            GridDirectionFacing.South => 4,
            GridDirectionFacing.West => 2,
            _ => 0
        };
        if (shift == 0) return absoluteDirections;
  
        var buffer = new bool[UnidirectionalCount];
        Array.Copy(absoluteDirections, shift, buffer, 0, UnidirectionalCount - shift);
        Array.Copy(absoluteDirections, 0, buffer, UnidirectionalCount - shift, shift);
        return buffer;
    }
}
