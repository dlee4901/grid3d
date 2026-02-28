#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

public enum DirectionType {North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest, Vertical, Horizontal, Diagonal, Straight, Line}
public enum DirectionFacing {North, East, South, West}

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

    public int MaxDistance { get; set; } = 0; // -1: inf
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
    
    public List<Step> GetSteps(Grid2D grid, (int, int) startPosition, Entity? sourceEntity=null)
    {
        return Traverse(grid, startPosition, sourceEntity).Last().ToList();
    }
    
    public HashSet<Step> GetStepsSet(Grid2D grid, (int, int) startPosition, Entity? sourceEntity=null)
    {
        return Traverse(grid, startPosition, sourceEntity).Last();
    }
    
    public List<Step> GetTraceSteps(Grid2D grid, (int, int) startPosition, Entity? sourceEntity=null)
    {
        return GetTraceSteps(grid, new Step(startPosition, 0, Direction), sourceEntity);
    }
    
    private List<Step> GetTraceSteps(Grid2D grid, Step initialStep, Entity? sourceEntity=null)
    {
        return Expand(grid, initialStep.Position, sourceEntity, initialStep.Distance, initialStep.Direction).ToList();
    }
    
    private IEnumerable<HashSet<Step>> Traverse(Grid2D grid, (int, int) startPosition, Entity? sourceEntity=null)
    {
        var steps = new HashSet<Step>();
        var queue = new Queue<Step>();
        var initialStep = new Step(startPosition, 0, Direction);
        steps.Add(initialStep);
        queue.Enqueue(initialStep);
        while (queue.Count > 0)
        {
            var currentStep = queue.Dequeue();
            foreach (var nextStep in Expand(grid, currentStep.Position, sourceEntity, currentStep.Distance + 1, currentStep.Direction))
            {
                if (!steps.Add(nextStep)) continue;
                queue.Enqueue(nextStep);
                yield return steps;
            }
        }
    }
    
    private IEnumerable<Step> Expand(Grid2D grid, (int, int) startPosition, Entity? sourceEntity, int curDistance, DirectionType direction)
    {
        var maxDistance = MaxDistance < 0 || MaxDistance > grid.X * grid.Y ? grid.X * grid.Y : MaxDistance;
        if (curDistance > maxDistance) yield break;
        
        var directionFacing = sourceEntity?.Facing ?? DirectionFacing.North;
        var unitVectors = GetUnitVectors(direction, directionFacing);
        var traverseTiles = unitVectors.Select(vec => (startPosition.Item1 + vec.Item1, startPosition.Item2 + vec.Item2)).ToList();
        
        for (var i = 0; i < traverseTiles.Count; i++)
        {
            var position = traverseTiles[i];
            if (position == startPosition) continue;
                
            // If tile is invalid or collided with entity, do not check further
            if (!grid.IsValidPosition(position)) continue;
            Entity entity;
            if ((entity = grid.GetEntity(position)) != null && IsColliding(entity, sourceEntity)) continue;
            //if (collideMask != null && (entity = grid.GetEntity(tile)) != null && collideMask(entity)) continue;
            
            yield return new Step(position, curDistance, Linear ? (DirectionType)i : direction);

            if ((StartWidth <= 0 && DeltaWidth <= 0) || DeltaWidthDistanceOffset >= curDistance) continue;
            var curWidth = StartWidth + DeltaWidth * (curDistance - DeltaWidthDistanceOffset) / DeltaWidthStep;
            var widthTiles = GetWidthTiles(grid, curWidth, position, unitVectors);
            foreach (var widthTile in widthTiles)
                yield return new Step(widthTile, curDistance, Linear ? (DirectionType)i : direction);
                
            if (Chain == null || curDistance < ChainOffset) continue;
            foreach (var step in Chain.Expand(grid, position, sourceEntity, curDistance, Chain.Direction))
                yield return step;
        }
    }
    
    private bool IsColliding(Entity targetEntity, Entity? sourceEntity=null)
    {
        Func<Entity, bool>? predicate = null;
        if (PassthroughQuery != null) predicate = PredicateFactory<Entity>.Create(PassthroughQuery);

        if (sourceEntity != null && targetEntity.Control != null)
        {
            if (!Passthrough.HasFlag(EntityPassthrough.Enemy) && !targetEntity.Control.HasSameController(sourceEntity)) return predicate?.Invoke(targetEntity) ?? true;
            if (!Passthrough.HasFlag(EntityPassthrough.Ally) && targetEntity.Control.HasSameController(sourceEntity)) return predicate?.Invoke(targetEntity) ?? true;
        }
        //if (!Passthrough.HasFlag(EntityPassthrough.Unit) && targetEntity.GetType() == typeof(Unit)) return true;
        //if (!Passthrough.HasFlag(EntityPassthrough.Obstacle) && targetEntity.GetType() != typeof(Unit)) return true;
        return false;
    }
    
    private List<(int, int)> GetWidthTiles(Grid2D grid, int width, (int, int) startPosition, (int, int)[] unitVectors)
    {
        List<(int, int)> widthTiles = new();
        var zeroTuple = (0, 0);
        var leftTile = zeroTuple;
        var rightTile = zeroTuple;
        for (var i = 1; i <= width; i++)
        {
            if (!unitVectors[0].Equals(zeroTuple) || !unitVectors[4].Equals(zeroTuple)) // N, S
            {
                leftTile = (i, 0);
                rightTile = (-i, 0);
            }
            if (!unitVectors[1].Equals(zeroTuple) || !unitVectors[5].Equals(zeroTuple)) // NE, SW
            {
                leftTile = (i, -i);
                rightTile = (-i, i);
            }
            if (!unitVectors[2].Equals(zeroTuple) || !unitVectors[6].Equals(zeroTuple)) // E, W
            {
                leftTile = (0, i);
                rightTile = (0, -i);
            }
            if (!unitVectors[3].Equals(zeroTuple) || !unitVectors[7].Equals(zeroTuple)) // SE, NW
            {
                leftTile = (-i, -i);
                rightTile = (i, i);
            }
            var newTile = Util.TupleArithmetic(startPosition, leftTile, Util.ArithmeticOperation.Add);
            if (newTile.HasValue && grid.IsValidPosition(newTile.Value)) widthTiles.Add(newTile.Value);
            newTile = Util.TupleArithmetic(startPosition, rightTile, Util.ArithmeticOperation.Add);
            if (newTile.HasValue && grid.IsValidPosition(newTile.Value)) widthTiles.Add(newTile.Value);
        }
        return widthTiles;
    }

    private (int, int)[] GetUnitVectors(DirectionType direction, DirectionFacing directionFacing=DirectionFacing.North)
    {
        var unitVectors = new (int, int)[8];
        var absoluteDirections = GetAbsoluteDirections(direction, directionFacing);
        for (int i = 0; i < 8; i++)
        {
            int xOffset = 0;
            int yOffset = 0;
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
                for (int i = 0; i < 8; i++) absoluteDirections[i] = true;
                break;
            case DirectionType.Diagonal:
                for (int i = 1; i < 8; i += 2) absoluteDirections[i] = true;
                break;
            case DirectionType.Straight:
                for (int i = 0; i < 8; i += 2) absoluteDirections[i] = true;
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
        int shift = 0;
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