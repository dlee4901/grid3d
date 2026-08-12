#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

public enum GridDirection {North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest, Vertical, Horizontal, Diagonal, Straight, Line, Mask}
public enum GridDirectionFacing {North, East, South, West}
[Flags] public enum EntityPassthrough {None, Ally, Enemy, All=Ally|Enemy}

public class GridTraversal
{
    public const int UnidirectionalCount = 8;
    
    public GridDirection Direction { get; set; }
    public bool[] DirectionMask { get; set; } = new bool[UnidirectionalCount];

    public int MaxDistance { get; set; } = 0;
    public bool AbsoluteDirection { get; set; } = false;
    public bool Linear { get; set; } = true;

    public int StartWidth { get; set; } = 0;
    public int DeltaWidth { get; set; } = 0;
    public int DeltaWidthStep { get; set; } = 1;
    public int DeltaWidthDistanceOffset { get; set; } = 0;

    public EntityPassthrough Passthrough { get; set; } = EntityPassthrough.None;
    public PredicateConfig? PassthroughQuery { get; set; }

    public GridTraversal? Chain { get; set; }
    public int ChainOffset { get; set; } = 0; // IF (n > 0) n ~ distance ELSE maxDistReached + n ~ maxDistReached

    public List<GridStep> GetSteps(GridSource ctx)
    {
        var steps = new HashSet<GridStep>();
        foreach (var iter in Traverse(ctx))
            steps = iter;
        return steps.ToList();
    }

    public HashSet<GridStep> GetStepsSet(GridSource ctx)
    {
        var steps = new HashSet<GridStep>();
        foreach (var iter in Traverse(ctx))
            steps = iter;
        return steps;
    }

    public List<GridStep> GetTraceSteps(GridSource ctx)
    {
        return GetTraceSteps(ctx, new GridStep(ctx.Position, Direction, 0));
    }

    private List<GridStep> GetTraceSteps(GridSource ctx, GridStep initialGridStep)
    {
        return Expand(ctx, initialGridStep.Position, initialGridStep.Distance, initialGridStep.Direction).ToList();
    }

    private IEnumerable<HashSet<GridStep>> Traverse(GridSource ctx)
    {
        var steps = new HashSet<GridStep>();
        var queue = new Queue<GridStep>();
        var initialStep = new GridStep(ctx.Position, Direction, 0);
        steps.Add(initialStep);
        queue.Enqueue(initialStep);
        while (queue.Count > 0)
        {
            var currentStep = queue.Dequeue();
            foreach (var nextStep in Expand(ctx, currentStep.Position, currentStep.Distance + 1, currentStep.Direction))
            {
                if (!steps.Add(nextStep)) continue;
                queue.Enqueue(nextStep);
                foreach (var widthStep in Widen(ctx, nextStep)) steps.Add(widthStep);
                yield return steps;
            }
        }
    }

    private IEnumerable<GridStep> Expand(GridSource ctx, GridPosition currentPosition, int curDistance, GridDirection gridDirection)
    {
        if (!currentPosition.IsValid()) yield break;
        
        var grid = ctx.Grid;
        var maxDistance = MaxDistance < 0 || MaxDistance > grid.X * grid.Y ? grid.X * grid.Y : MaxDistance;
        if (curDistance > maxDistance) yield break;

        var directionFacing = ctx.Entity?.Facing ?? GridDirectionFacing.North;
        var unitVectors = GetUnitVectors(gridDirection, directionFacing);
        
        var currentPosition2D = currentPosition.Dim2;
        var traverseTiles = unitVectors.Select(vec => (currentPosition2D.x + vec.Item1, currentPosition2D.y + vec.Item2)).ToList();

        for (var i = 0; i < traverseTiles.Count; i++)
        {
            var position = new GridPosition(grid, traverseTiles[i]);
            if (position.Equals(currentPosition)) continue;

            if (!position.IsValid()) continue;
            Entity entity;
            if ((entity = grid.GetEntity(position)) != null && IsColliding(entity, ctx.Entity)) continue;
            //if (collideMask != null && (entity = grid.GetEntity(tile)) != null && collideMask(entity)) continue;

            yield return new GridStep(position, Linear ? (GridDirection)i : gridDirection, curDistance);

            if (Chain == null || curDistance < ChainOffset) continue;
            foreach (var step in Chain.Expand(ctx, position, curDistance, Chain.Direction))
                yield return step;
        }
    }
    
    private IEnumerable<GridStep> Widen(GridSource ctx, GridStep gridStep)
    {
        if (StartWidth <= 0 && DeltaWidth <= 0) yield break;
        if (DeltaWidthDistanceOffset >= gridStep.Distance) yield break;
        
        var directionFacing = ctx.Entity?.Facing ?? GridDirectionFacing.North;
        var unitVectors = GetUnitVectors(gridStep.Direction, directionFacing);
        
        var width = StartWidth + DeltaWidth * (gridStep.Distance - DeltaWidthDistanceOffset) / DeltaWidthStep;
        var widthPositions = GetWidthPositions(ctx, width, gridStep.Position, unitVectors);
        
        foreach (var position in widthPositions) yield return new GridStep(position, gridStep.Direction, gridStep.Distance);
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

    private List<GridPosition> GetWidthPositions(GridSource ctx, int width, GridPosition startPosition, (int, int)[] unitVectors)
    {
        var grid = ctx.Grid;
        List<GridPosition> widthPositions = new();
        var zeroTuple = (0, 0);
        var leftShift = zeroTuple;
        var rightShift = zeroTuple;
        for (var i = 1; i <= width; i++)
        {
            if (!unitVectors[0].Equals(zeroTuple) || !unitVectors[4].Equals(zeroTuple))
            {
                leftShift = (i, 0);
                rightShift = (-i, 0);
            }
            if (!unitVectors[1].Equals(zeroTuple) || !unitVectors[5].Equals(zeroTuple))
            {
                leftShift = (i, -i);
                rightShift = (-i, i);
            }
            if (!unitVectors[2].Equals(zeroTuple) || !unitVectors[6].Equals(zeroTuple))
            {
                leftShift = (0, i);
                rightShift = (0, -i);
            }
            if (!unitVectors[3].Equals(zeroTuple) || !unitVectors[7].Equals(zeroTuple))
            {
                leftShift = (-i, -i);
                rightShift = (i, i);
            }
            var newPosition = startPosition.Add(leftShift); //GridUtil.TupleArithmetic(startPosition, leftTile, GridUtil.ArithmeticOperation.Add));
            if (newPosition.IsValid()) widthPositions.Add(newPosition);
            newPosition = startPosition.Add(rightShift);
            if (newPosition.IsValid()) widthPositions.Add(newPosition);
        }
        return widthPositions;
    }

    private (int, int)[] GetUnitVectors(GridDirection gridDirection, GridDirectionFacing gridDirectionFacing=GridDirectionFacing.North)
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
            case GridDirection.Line:
                for (var i = 0; i < UnidirectionalCount; i++) absoluteDirections[i] = true;
                break;
            case GridDirection.Diagonal:
                for (var i = 1; i < UnidirectionalCount; i += 2) absoluteDirections[i] = true;
                break;
            case GridDirection.Straight:
                for (var i = 0; i < UnidirectionalCount; i += 2) absoluteDirections[i] = true;
                break;
            case GridDirection.Horizontal:
                absoluteDirections[2] = true;
                absoluteDirections[6] = true;
                break;
            case GridDirection.Vertical:
                absoluteDirections[0] = true;
                absoluteDirections[4] = true;
                break;
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
            case GridDirection.Mask:
                for (var i = 0; i < UnidirectionalCount; i++) absoluteDirections[i] = DirectionMask[i];
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
