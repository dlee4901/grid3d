using System;
using System.Collections.Generic;
using System.Linq;

public enum Direction {N, NE, E, SE, S, SW, W, NW, step, stride, line, diagonal, straight, horizontal, vertical}
public enum DirectionCardinal {N, NE, E, SE, S, SW, W, NW}
[Flags] public enum Team {Neutral=1, Ally=2, Enemy=4}

[Serializable]
public class GridSelection
{
    public Direction Direction { get; private set; }
    public int MaxDistance { get; private set; }
    public int MinDistance { get; private set; }
    
    public int Width { get; private set; }
    public bool AbsoluteDirection { get; private set; }
    
    public int Collide { get; private set;}
    public GridSelection Chain { get; private set; }

    public GridSelection(Direction direction, int maxDistance=-1, int minDistance=0, int width=0, bool absoluteDirection=false, int collide=0, GridSelection chain=null)
    {
        Direction = direction;
        MaxDistance = maxDistance;
        MinDistance = minDistance;
        Width = width;
        AbsoluteDirection = absoluteDirection;
        Collide = collide;
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
        List<Tuple<int, int>> unitVectors = GetUnitVectors(sourceUnit?.DirectionFacing ?? DirectionFacing.N);
        
        int maxDistance = MaxDistance;
        if (maxDistance == -1 || maxDistance > grid.X + grid.Y) maxDistance = grid.X + grid.Y;
        int minDistance = MinDistance;
        if (minDistance > grid.X + grid.Y) minDistance = grid.X + grid.Y;
        
        for (int i = 0; i < grid.GetSize(); i++) tileDistances[grid.ToPosition2D(i)] = -1;
        List<Tuple<int, int>> checkTiles = startPositions.ToList();
        
        List<Tuple<int, int>> widthTiles = new();
        for (int i = 0; i < Width; i++)
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
        
        for (int i = 0; i <= maxDistance; i++)
        {
            List<Tuple<int, int>> nextTiles = new();
            foreach (Tuple<int, int> tilePosition in checkTiles)
            {
                tileDistances[tilePosition] = i;
                if (i >= minDistance && i <= maxDistance) tiles.Add(tilePosition);
                for (int j = 0; j < 8; j++)
                {
                    if (unitVectors[j].Equals(new Tuple<int, int>(0, 0))) continue;
                    Tuple<int, int> targetPosition = Util.TupleArithmetic(tilePosition, unitVectors[j], Util.ArithmeticOperation.Add);
                    if (!grid.IsValidPosition(targetPosition) || (tileDistances[targetPosition] >= 0 && tileDistances[targetPosition] <= i)) continue;
                    // Add Collide Check
                    nextTiles.Add(targetPosition);
                }
            }
            checkTiles = nextTiles;
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
                if (i > 2 && i < 6)      yOffset = 1;
                else if (i < 2 || i > 6) yOffset = -1;
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
            case Direction.stride: case Direction.line:
                for (int i = 0; i < 8; i++) absoluteDirections[i] = true;
                break;
            case Direction.diagonal:
                for (int i = 1; i < 8; i += 2) absoluteDirections[i] = true;
                break;
            case Direction.step: case Direction.straight:
                for (int i = 0; i < 8; i += 2) absoluteDirections[i] = true;
                break;
            case Direction.horizontal:
                absoluteDirections[2] = true;
                absoluteDirections[6] = true;
                break;
            case Direction.vertical:
                absoluteDirections[0] = true;
                absoluteDirections[4] = true;
                break;
            case Direction.N:
                absoluteDirections[0] = true;
                break;
            case Direction.NE:
                absoluteDirections[1] = true;
                break;
            case Direction.E:
                absoluteDirections[2] = true;
                break;
            case Direction.SE:
                absoluteDirections[3] = true;
                break;
            case Direction.S:
                absoluteDirections[4] = true;
                break;
            case Direction.SW:
                absoluteDirections[5] = true;
                break;
            case Direction.W:
                absoluteDirections[6] = true;
                break;
            case Direction.NW:
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
                case DirectionFacing.E:
                    shift = 6;
                    break;
                case DirectionFacing.S:
                    shift = 2;
                    break;
                case DirectionFacing.W:
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