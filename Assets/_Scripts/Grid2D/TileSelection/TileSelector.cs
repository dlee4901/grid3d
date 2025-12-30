using System;
using System.Collections.Generic;
using System.Linq;

public enum Direction {North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest, Vertical, Horizontal, Diagonal, Straight, Line, Step, Stride}
[Flags] public enum EntityPassthrough {None=0, Unit=1<<0, Obstacle=1<<1, Ally=Unit|1<<2, Enemy=Unit|1<<3, All=Unit|Obstacle|Ally|Enemy}

public class TileSelector
{
    public Direction Direction;
    public int Distance;
    //public int MinDistance;
    
    public int StartWidth;
    public int DeltaWidth;
    public int DeltaWidthOffset;
    
    public bool AbsoluteDirection;
    
    public EntityPassthrough Passthrough;
    public TileSelector Chain;
    public int ChainOffset; // IF (n > 0) n ~ distance ELSE maxDistReached + n ~ maxDistReached
    
    public int TotalDistance { get; private set; }
    
    public TileSelector(Direction direction, int distance=-1, int startWidth=0, int deltaWidth=0, int deltaWidthOffset=0, bool absoluteDirection=false, EntityPassthrough passthrough=EntityPassthrough.None, TileSelector chain=null, int chainOffset=0)
    {
        Direction = direction;
        Distance = distance;
        StartWidth = startWidth;
        DeltaWidth = deltaWidth;
        DeltaWidthOffset = deltaWidthOffset;
        AbsoluteDirection = absoluteDirection;
        Passthrough = passthrough;
        Chain = chain;
        ChainOffset = chainOffset;
    }
    
    public TileSelection GetTileSelection(Grid2D grid, int startPosition, Entity sourceEntity=null)
    {
        return GetTileSelection(grid, grid.ToPosition2D(startPosition), sourceEntity);
    }
    
    public TileSelection GetTileSelection(Grid2D grid, (int, int) startPosition, Entity sourceEntity=null)
    {
        var startPositions = new HashSet<(int, int)> {startPosition};
        var tileSelection = new TileSelection(grid);
        TotalDistance = 0;
        return GetTileSelection(grid, startPositions, ref tileSelection, sourceEntity);
    }

    private TileSelection GetTileSelection(Grid2D grid, HashSet<(int, int)> startPositions, ref TileSelection tileSelection, Entity sourceEntity=null, int chainDistance=0)
    {
        var directionFacing = sourceEntity?.DirectionFacing ?? DirectionFacing.North;
        var unitVectors = GetUnitVectors(Direction, directionFacing);
        //var collideMask = CollideMask?.Build();
        
        var distance = Distance;
        if (distance < 0 || distance > grid.X * grid.Y) distance = grid.X * grid.Y;
        //var minDistance = MinDistance;
        //if (minDistance > grid.X + grid.Y) minDistance = grid.X + grid.Y;
        
        var checkTiles = startPositions.ToList();
        var chainTiles = new HashSet<(int, int)>();
        
        List<(int, int)> widthTiles = new();
        for (var i = 1; i <= StartWidth; i++)
        {
            foreach (var tile in checkTiles)
            {
                widthTiles = GetWidthTiles(grid, i, tile, unitVectors);
            }
            // if (!unitVectors[0].Equals(new (int, int)(0, 0)) || !unitVectors[4].Equals(new (int, int)(0, 0)))
            // {
            //     foreach ((int, int) tile in checkTiles)
            //     {
            //         var newTile = Util.TupleArithmetic(tile, new (int, int)(i, 0), Util.ArithmeticOperation.Add);
            //         if (grid.IsValidPosition(newTile)) widthTiles.Add(newTile);
            //         newTile = Util.TupleArithmetic(tile, new (int, int)(-i, 0), Util.ArithmeticOperation.Add);
            //         if (grid.IsValidPosition(newTile)) widthTiles.Add(newTile);
            //     }
            // }
            // if (!unitVectors[2].Equals(new (int, int)(0, 0)) || !unitVectors[6].Equals(new (int, int)(0, 0)))
            // {
            //     foreach ((int, int) tile in checkTiles)
            //     {
            //         var newTile = Util.TupleArithmetic(tile, new (int, int)(0, i), Util.ArithmeticOperation.Add);
            //         if (grid.IsValidPosition(newTile)) widthTiles.Add(newTile);
            //         newTile = Util.TupleArithmetic(tile, new (int, int)(0, -i), Util.ArithmeticOperation.Add);
            //         if (grid.IsValidPosition(newTile)) widthTiles.Add(newTile);
            //     }
            // }
        }
        checkTiles.AddRange(widthTiles);
        
        if (Direction == Direction.Step || Direction == Direction.Stride)
        {
            var maxDistanceReached = 0;
            for (var i = 0; i <= distance; i++)
            {
                List<(int, int)> nextTiles = new();
                foreach (var tile in checkTiles)
                {
                    // If tile is invalid or collided with entity, do not check further
                    if (!grid.IsValidPosition(tile)) continue;
                    Entity entity;
                    if ((entity = grid.GetEntity(tile)) != null && IsColliding(entity, sourceEntity)) continue;
                    //if (collideMask != null && (entity = grid.GetEntity(tile)) != null && collideMask(entity)) continue;
                    
                    // Update tile distance
                    // if (tileDistances[tile] == -1) tileDistances[tile] = i + chainDistance;
                    // else tileDistances[tile] = Math.Min(tileDistances[tile], i + chainDistance);
                    if (maxDistanceReached < i) maxDistanceReached = i;
                    tileSelection.UpdateTileDistance(tile, i + chainDistance);
                    if (tileSelection.GetTileDistance(tile) > TotalDistance) TotalDistance = tileSelection.GetTileDistance(tile);
                    
                    // Tile checks
                    // if (tileDistances[tile] >= minDistance) tiles.Add(tile);
                    // else tiles.Remove(tile);
                    
                    // Add adjacent tiles to next check list
                    for (int j = 0; j < 8; j++)
                    {
                        if (unitVectors[j].Equals((0, 0))) continue;
                        var targetPosition = Util.TupleArithmetic(tile, unitVectors[j], Util.ArithmeticOperation.Add);
                        if (targetPosition.HasValue) nextTiles.Add(targetPosition.Value);
                    }
                }
                checkTiles = nextTiles;
            }
            if (Chain != null)
            {
                if (ChainOffset < 0) chainTiles.UnionWith(tileSelection.GetTilesInDistanceRange(maxDistanceReached + ChainOffset, maxDistanceReached));
                else chainTiles.UnionWith(tileSelection.GetTilesInDistanceRange(ChainOffset, distance));
            }
        }
        else
        {
            foreach (var tilePosition in checkTiles)
            {
                for (var i = 0; i < 8; i++)
                {
                    if (unitVectors[i].Equals((0, 0))) continue;
                    var targetPosition = tilePosition;
                    var maxDistanceReached = 0;
                    var axisTileSelection = new TileSelection(grid, new Dictionary<(int, int), int>{ { targetPosition, 0 } });
                    
                    // Width vars
                    var width = 0;
                    List<(int, int)> deltaWidthTiles = new();
                    var cacheUnitVectors = new List<(int, int)>[8];
                    
                    for (var j = 0; j <= distance; j++)
                    {
                        // If tile is invalid or collided with entity, do not check further
                        if (!grid.IsValidPosition(targetPosition)) break;
                        Entity entity;
                        if ((entity = grid.GetEntity(targetPosition)) != null && IsColliding(entity, sourceEntity)) continue;
                        //if (collideMask != null && (entity = grid.GetEntity(tilePosition)) != null && collideMask(entity)) break;
                        
                        // Update tile distance
                        // if (tileDistances[targetPosition] == -1) tileDistances[targetPosition] = j + chainDistance;
                        // else tileDistances[targetPosition] = Math.Min(tileDistances[targetPosition], j + chainDistance);
                        if (maxDistanceReached < j) maxDistanceReached = j;
                        axisTileSelection.UpdateTileDistance(targetPosition, j + chainDistance);
                        if (axisTileSelection.GetTileDistance(targetPosition) > TotalDistance) TotalDistance = axisTileSelection.GetTileDistance(targetPosition);
                        
                        // Tile checks
                        // if (tileDistances[targetPosition] >= minDistance) tiles.Add(targetPosition);
                        // else tiles.Remove(targetPosition);
                        
                        // Update target position
                        var prevPosition = targetPosition;
                        var newPosition = Util.TupleArithmetic(targetPosition, unitVectors[i], Util.ArithmeticOperation.Add);
                        if (newPosition.HasValue) targetPosition = newPosition.Value;
                        
                        //
                        // Compute DeltaWidth
                        //
                        if (DeltaWidth == 0 || axisTileSelection.GetTileDistance(prevPosition) < DeltaWidthOffset) continue;
                        
                        // Get delta width tiles
                        for (var k = 0; k < width; k++)
                        {   
                            if (cacheUnitVectors[i] == null) cacheUnitVectors[i] = GetUnitVectors((Direction)i, directionFacing);
                            deltaWidthTiles.AddRange(GetWidthTiles(grid, width, prevPosition, cacheUnitVectors[i]));
                        }
                        width += DeltaWidth;
                        
                        // Check each width tile
                        foreach (var tile in deltaWidthTiles)
                        {
                            if (!grid.IsValidPosition(tile)) continue;
                            if ((entity = grid.GetEntity(tile)) != null && IsColliding(entity, sourceEntity)) continue;
                            //if (collideMask != null && (entity = grid.GetEntity(tile)) != null && collideMask(entity)) continue;
                            
                            // if (tileDistances[tile] == -1) tileDistances[tile] = j + chainDistance;
                            // else tileDistances[tile] = Math.Min(tileDistances[tile], j + chainDistance);
                            axisTileSelection.UpdateTileDistance(tile, j + chainDistance);
                            
                            // if (tileDistances[tile] >= minDistance) tiles.Add(tile);
                            // else tiles.Remove(tile);
                        }
                    }
                    
                    tileSelection.Merge(axisTileSelection);
                    
                    if (Chain != null)
                    {
                        if (ChainOffset < 0) chainTiles.UnionWith(axisTileSelection.GetTilesInDistanceRange(maxDistanceReached + ChainOffset, maxDistanceReached, false));
                        else chainTiles.UnionWith(axisTileSelection.GetTilesInDistanceRange(ChainOffset, distance));
                    }
                }
            }
        }
        return Chain != null ? GetTileSelection(grid, chainTiles, ref tileSelection, sourceEntity, chainDistance + distance) : tileSelection;
    }
    
    private bool IsColliding(Entity targetEntity, Entity sourceEntity=null)
    {
        if (sourceEntity != null)
        {
            if (!Passthrough.HasFlag(EntityPassthrough.Enemy) && !targetEntity.HasSameController(sourceEntity)) return true;
            if (!Passthrough.HasFlag(EntityPassthrough.Ally) && targetEntity.HasSameController(sourceEntity)) return true;
        }
        if (!Passthrough.HasFlag(EntityPassthrough.Unit) && targetEntity.GetType() == typeof(Unit)) return true;
        if (!Passthrough.HasFlag(EntityPassthrough.Obstacle) && targetEntity.GetType() != typeof(Unit)) return true;
        return false;
    }
    
    private List<(int, int)> GetWidthTiles(Grid2D grid, int width, (int, int) startPosition, List<(int, int)> unitVectors)
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

    private List<(int, int)> GetUnitVectors(Direction direction, DirectionFacing directionFacing=DirectionFacing.North)
    {
        List<(int, int)> unitVectors = new();
        List<bool> absoluteDirections = GetAbsoluteDirections(direction, directionFacing);
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
            unitVectors.Add((xOffset, yOffset));
        }
        return unitVectors;
    }

    private List<bool> GetAbsoluteDirections(Direction direction, DirectionFacing directionFacing=DirectionFacing.North)
    {
        List<bool> absoluteDirections = new List<bool>{false, false, false, false, false, false, false, false};
        switch (direction)
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