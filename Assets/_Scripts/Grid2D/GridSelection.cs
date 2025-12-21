using System;
using System.Collections.Generic;
using System.Linq;

public enum Direction {North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest, Vertical, Horizontal, Diagonal, Straight, Line, Step, Stride}
// [Flags] public enum Team {Neutral=1, Ally=2, Enemy=4}

[Serializable]
public class GridSelection
{
    public Direction Direction;
    public int MaxDistance;
    public int MinDistance;
    
    public int Width;
    public bool AbsoluteDirection;
    
    public ConditionBuilder<Entity> CollideMask;
    public GridSelection Chain;

    public GridSelection(Direction direction, int maxDistance=-1, int minDistance=0, int width=0, bool absoluteDirection=false, ConditionBuilder<Entity> collideMask=null, GridSelection chain=null)
    {
        Direction = direction;
        MaxDistance = maxDistance;
        MinDistance = minDistance;
        Width = width;
        AbsoluteDirection = absoluteDirection;
        CollideMask = collideMask;
        Chain = chain;
    }
    
    public HashSet<int> GetTiles(Grid2D grid, Tuple<int, int> startPosition, Unit sourceUnit=null)
    {
        var startPositions = new HashSet<Tuple<int, int>>();
        startPositions.Add(startPosition);
        return GetTiles(grid, startPositions, sourceUnit);
    }

    public HashSet<int> GetTiles(Grid2D grid, HashSet<Tuple<int, int>> startPositions, Unit sourceUnit=null)
    {
        HashSet<Tuple<int, int>> tiles = new();
        Dictionary<Tuple<int, int>, int> tileDistances = new();
        List<Tuple<int, int>> unitVectors = GetUnitVectors(sourceUnit?.DirectionFacing ?? DirectionFacing.North);
        var collideMask = CollideMask?.Build();
        
        int maxDistance = MaxDistance;
        if (maxDistance == -1 || maxDistance > grid.X + grid.Y) maxDistance = grid.X + grid.Y;
        int minDistance = MinDistance;
        if (minDistance > grid.X + grid.Y) minDistance = grid.X + grid.Y;
        
        for (int i = 0; i < grid.GetSize(); i++) tileDistances[grid.ToPosition2D(i)] = -1;
        List<Tuple<int, int>> checkTiles = startPositions.ToList();
        
        List<Tuple<int, int>> widthTiles = new();
        for (int i = 1; i <= Width; i++)
        {
            if (!unitVectors[0].Equals(new Tuple<int, int>(0, 0)) || !unitVectors[4].Equals(new Tuple<int, int>(0, 0)))
            {
                foreach (Tuple<int, int> tile in checkTiles)
                {
                    var newTile = Util.TupleArithmetic(tile, new Tuple<int, int>(i, 0), Util.ArithmeticOperation.Add);
                    if (grid.IsValidPosition(newTile)) widthTiles.Add(newTile);
                    newTile = Util.TupleArithmetic(tile, new Tuple<int, int>(-i, 0), Util.ArithmeticOperation.Add);
                    if (grid.IsValidPosition(newTile)) widthTiles.Add(newTile);
                }
            }
            if (!unitVectors[2].Equals(new Tuple<int, int>(0, 0)) || !unitVectors[6].Equals(new Tuple<int, int>(0, 0)))
            {
                foreach (Tuple<int, int> tile in checkTiles)
                {
                    var newTile = Util.TupleArithmetic(tile, new Tuple<int, int>(0, i), Util.ArithmeticOperation.Add);
                    if (grid.IsValidPosition(newTile)) widthTiles.Add(newTile);
                    newTile = Util.TupleArithmetic(tile, new Tuple<int, int>(0, -i), Util.ArithmeticOperation.Add);
                    if (grid.IsValidPosition(newTile)) widthTiles.Add(newTile);
                }
            }
        }
        checkTiles.AddRange(widthTiles);
        
        if (Direction == Direction.Step || Direction == Direction.Stride)
        {
            for (int i = 0; i <= maxDistance; i++)
            {
                List<Tuple<int, int>> nextTiles = new();
                foreach (Tuple<int, int> tilePosition in checkTiles)
                {
                    // If tile is invalid or collided with entity, do not check further
                    if (!grid.IsValidPosition(tilePosition)) continue;
                    Entity entity;
                    if (collideMask != null && (entity = grid.GetEntity(tilePosition)) != null && collideMask(entity)) continue;
                    
                    // Update tile distance
                    if (tileDistances[tilePosition] == -1) tileDistances[tilePosition] = i;
                    else tileDistances[tilePosition] = Math.Min(tileDistances[tilePosition], i);
                    
                    // Tile checks
                    if (tileDistances[tilePosition] >= minDistance) tiles.Add(tilePosition);
                    else tiles.Remove(tilePosition);
                    
                    // Add adjacent tiles to next check list
                    for (int j = 0; j < 8; j++)
                    {
                        if (unitVectors[j].Equals(new Tuple<int, int>(0, 0))) continue;
                        Tuple<int, int> targetPosition = Util.TupleArithmetic(tilePosition, unitVectors[j], Util.ArithmeticOperation.Add);
                        nextTiles.Add(targetPosition);
                    }
                }
                checkTiles = nextTiles;
            }
        }
        else
        {
            foreach (Tuple<int, int> tilePosition in checkTiles)
            {
                for (int i = 0; i < 8; i++)
                {
                    Tuple<int, int> targetPosition = tilePosition;
                    for (int j = 0; j <= maxDistance; j++)
                    {
                        // If tile is invalid or collided with entity, do not check further
                        if (!grid.IsValidPosition(targetPosition)) break;
                        Entity entity;
                        if (collideMask != null && (entity = grid.GetEntity(tilePosition)) != null && collideMask(entity)) break;
                        
                        // Update tile distance
                        if (tileDistances[targetPosition] == -1) tileDistances[targetPosition] = j;
                        else tileDistances[targetPosition] = Math.Min(tileDistances[targetPosition], j);
                        
                        // Tile checks
                        if (tileDistances[targetPosition] >= minDistance) tiles.Add(targetPosition);
                        else tiles.Remove(targetPosition);
                        
                        // Update target position
                        targetPosition = Util.TupleArithmetic(targetPosition, unitVectors[i], Util.ArithmeticOperation.Add);
                    }
                }
            }
        }
        
        if (Chain != null)
        {
            return GetTiles(grid, tiles, sourceUnit);
        }
        return tiles.Select(t => grid.ToPosition1D(t)).ToHashSet();
    }

    public List<Tuple<int, int>> GetUnitVectors(DirectionFacing directionFacing)
    {
        List<Tuple<int, int>> unitVectors = new();
        List<bool> absoluteDirections = GetAbsoluteDirections(directionFacing);
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
            unitVectors.Add(new Tuple<int, int>(xOffset, yOffset));
        }
        return unitVectors;
    }

    public List<bool> GetAbsoluteDirections(DirectionFacing directionFacing)
    {
        List<bool> absoluteDirections = new List<bool>{false, false, false, false, false, false, false, false};
        switch (Direction)
        {
            case Direction.Stride: case Direction.Line:
                for (int i = 0; i < 8; i++) absoluteDirections[i] = true;
                break;
            case Direction.Diagonal:
                for (int i = 1; i < 8; i += 2) absoluteDirections[i] = true;
                break;
            case Direction.Step: case Direction.Straight:
                for (int i = 0; i < 8; i += 2) absoluteDirections[i] = true;
                break;
            case Direction.Horizontal:
                absoluteDirections[2] = true;
                absoluteDirections[6] = true;
                break;
            case Direction.Vertical:
                absoluteDirections[0] = true;
                absoluteDirections[4] = true;
                break;
            case Direction.North:
                absoluteDirections[0] = true;
                break;
            case Direction.NorthEast:
                absoluteDirections[1] = true;
                break;
            case Direction.East:
                absoluteDirections[2] = true;
                break;
            case Direction.SouthEast:
                absoluteDirections[3] = true;
                break;
            case Direction.South:
                absoluteDirections[4] = true;
                break;
            case Direction.SouthWest:
                absoluteDirections[5] = true;
                break;
            case Direction.West:
                absoluteDirections[6] = true;
                break;
            case Direction.NorthWest:
                absoluteDirections[7] = true;
                break;
            default:
                return absoluteDirections;
        }
        if (!AbsoluteDirection)
        {
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
        return absoluteDirections;
    }
}