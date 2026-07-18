#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

public enum DirectionType {North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest, Vertical, Horizontal, Diagonal, Straight, Line}
public enum DirectionFacing {North, East, South, West}
[Flags] public enum EntityPassthrough {None, Ally, Enemy, All=Ally|Enemy}

public struct Step : IEquatable<Step>
{
    public (int x, int y) Position;
    public int Distance;
    public DirectionType Direction;

    public Step((int x, int y) position, int distance, DirectionType direction)
    {
        Position = position;
        Distance = distance;
        Direction = direction;
    }

    public bool Equals(Step other)
    {
        return Position.Equals(other.Position) && Direction == other.Direction; //&& Distance == other.Distance;
    }

    public override bool Equals(object? obj)
    {
        return obj is Step other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Position, (int)Direction);
    }
}

public class GridTraversal
{
    public DirectionType Direction { get; set; }

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

    public List<Step> GetSteps(QueryContext ctx)
    {
        var steps = new HashSet<Step>();
        foreach (var iter in Traverse(ctx))
            steps = iter;
        return steps.ToList();
    }

    public HashSet<Step> GetStepsSet(QueryContext ctx)
    {
        var steps = new HashSet<Step>();
        foreach (var iter in Traverse(ctx))
            steps = iter;
        return steps;
    }

    public List<Step> GetTraceSteps(QueryContext ctx)
    {
        return GetTraceSteps(ctx, new Step(ctx.SourcePosition, 0, Direction));
    }

    private List<Step> GetTraceSteps(QueryContext ctx, Step initialStep)
    {
        return Expand(ctx, initialStep.Position, initialStep.Distance, initialStep.Direction).ToList();
    }

    private IEnumerable<HashSet<Step>> Traverse(QueryContext ctx)
    {
        var steps = new HashSet<Step>();
        var queue = new Queue<Step>();
        var initialStep = new Step(ctx.SourcePosition, 0, Direction);
        steps.Add(initialStep);
        queue.Enqueue(initialStep);
        while (queue.Count > 0)
        {
            var currentStep = queue.Dequeue();
            foreach (var nextStep in Expand(ctx, currentStep.Position, currentStep.Distance + 1, currentStep.Direction))
            {
                if (!steps.Add(nextStep)) continue;
                queue.Enqueue(nextStep);
                yield return steps;
            }
        }
    }

    private IEnumerable<Step> Expand(QueryContext ctx, (int, int) currentPosition, int curDistance, DirectionType direction)
    {
        var grid = ctx.Grid;
        var maxDistance = MaxDistance < 0 || MaxDistance > grid.X * grid.Y ? grid.X * grid.Y : MaxDistance;
        if (curDistance > maxDistance) yield break;

        var directionFacing = ctx.SourceEntity?.Facing ?? DirectionFacing.North;
        var unitVectors = GetUnitVectors(direction, directionFacing);
        var traverseTiles = unitVectors.Select(vec => (currentPosition.Item1 + vec.Item1, currentPosition.Item2 + vec.Item2)).ToList();

        for (var i = 0; i < traverseTiles.Count; i++)
        {
            var position = traverseTiles[i];
            if (position == currentPosition) continue;

            if (!grid.IsValidPosition(position)) continue;
            Entity entity;
            if ((entity = grid.GetEntity(position)) != null && IsColliding(entity, ctx.SourceEntity)) continue;
            //if (collideMask != null && (entity = grid.GetEntity(tile)) != null && collideMask(entity)) continue;

            yield return new Step(position, curDistance, Linear ? (DirectionType)i : direction);

            if ((StartWidth <= 0 && DeltaWidth <= 0) || DeltaWidthDistanceOffset >= curDistance) continue;
            var curWidth = StartWidth + DeltaWidth * (curDistance - DeltaWidthDistanceOffset) / DeltaWidthStep;
            var widthTiles = GetWidthTiles(ctx, curWidth, position, unitVectors);
            foreach (var widthTile in widthTiles)
                yield return new Step(widthTile, curDistance, Linear ? (DirectionType)i : direction);

            if (Chain == null || curDistance < ChainOffset) continue;
            foreach (var step in Chain.Expand(ctx, position, curDistance, Chain.Direction))
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

    private List<(int, int)> GetWidthTiles(QueryContext ctx, int width, (int, int) startPosition, (int, int)[] unitVectors)
    {
        var grid = ctx.Grid;
        List<(int, int)> widthTiles = new();
        var zeroTuple = (0, 0);
        var leftTile = zeroTuple;
        var rightTile = zeroTuple;
        for (var i = 1; i <= width; i++)
        {
            if (!unitVectors[0].Equals(zeroTuple) || !unitVectors[4].Equals(zeroTuple))
            {
                leftTile = (i, 0);
                rightTile = (-i, 0);
            }
            if (!unitVectors[1].Equals(zeroTuple) || !unitVectors[5].Equals(zeroTuple))
            {
                leftTile = (i, -i);
                rightTile = (-i, i);
            }
            if (!unitVectors[2].Equals(zeroTuple) || !unitVectors[6].Equals(zeroTuple))
            {
                leftTile = (0, i);
                rightTile = (0, -i);
            }
            if (!unitVectors[3].Equals(zeroTuple) || !unitVectors[7].Equals(zeroTuple))
            {
                leftTile = (-i, -i);
                rightTile = (i, i);
            }
            var newTile = GridUtil.TupleArithmetic(startPosition, leftTile, GridUtil.ArithmeticOperation.Add);
            if (newTile.HasValue && grid.IsValidPosition(newTile.Value)) widthTiles.Add(newTile.Value);
            newTile = GridUtil.TupleArithmetic(startPosition, rightTile, GridUtil.ArithmeticOperation.Add);
            if (newTile.HasValue && grid.IsValidPosition(newTile.Value)) widthTiles.Add(newTile.Value);
        }
        return widthTiles;
    }

    private (int, int)[] GetUnitVectors(DirectionType direction, DirectionFacing directionFacing=DirectionFacing.North)
    {
        var unitVectors = new (int, int)[8];
        var absoluteDirections = GetAbsoluteDirections(direction, directionFacing);
        for (var i = 0; i < 8; i++)
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

    private List<bool> GetAbsoluteDirections(DirectionType direction, DirectionFacing directionFacing=DirectionFacing.North)
    {
        var absoluteDirections = new List<bool>{false, false, false, false, false, false, false, false};
        switch (direction)
        {
            case DirectionType.Line:
                for (var i = 0; i < 8; i++) absoluteDirections[i] = true;
                break;
            case DirectionType.Diagonal:
                for (var i = 1; i < 8; i += 2) absoluteDirections[i] = true;
                break;
            case DirectionType.Straight:
                for (var i = 0; i < 8; i += 2) absoluteDirections[i] = true;
                break;
            case DirectionType.Horizontal:
                absoluteDirections[2] = true;
                absoluteDirections[6] = true;
                break;
            case DirectionType.Vertical:
                absoluteDirections[0] = true;
                absoluteDirections[4] = true;
                break;
            case DirectionType.North:
                absoluteDirections[0] = true;
                break;
            case DirectionType.NorthEast:
                absoluteDirections[1] = true;
                break;
            case DirectionType.East:
                absoluteDirections[2] = true;
                break;
            case DirectionType.SouthEast:
                absoluteDirections[3] = true;
                break;
            case DirectionType.South:
                absoluteDirections[4] = true;
                break;
            case DirectionType.SouthWest:
                absoluteDirections[5] = true;
                break;
            case DirectionType.West:
                absoluteDirections[6] = true;
                break;
            case DirectionType.NorthWest:
                absoluteDirections[7] = true;
                break;
            default:
                return absoluteDirections;
        }
        var shift = 0;
        switch (directionFacing)
        {
            case DirectionFacing.East:
                shift = 6;
                break;
            case DirectionFacing.South:
                shift = 2;
                break;
            case DirectionFacing.West:
                shift = 4;
                break;
            default:
                return absoluteDirections;
        }
        return (List<bool>)absoluteDirections.Skip(shift).Concat(absoluteDirections.Take(shift));
    }
}
