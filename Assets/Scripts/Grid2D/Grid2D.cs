using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TerrainType { Default, Void }

public class Grid2D : INameId
{
    // Initial Parameters (Static)
    public string Id { get; }
    public int X { get; }
    public int Y { get; }
    
    public int MaxTeamCost { get; }
    public int PlayerCount { get; }
    public Dictionary<int, int> PlayerStartPositions { get; private set; }

    // State
    public TerrainType[] Terrain { get; private set; }
    public Entity[] Entities { get; private set; }
    public List<Entity> PrioritizedEntities { get; private set; }
    public int Turn { get; }

    private Grid2D(string id, int x, int y, int maxTeamCost, int playerCount)
    {
        Id = id;
        X = x;
        Y = y;
        MaxTeamCost = maxTeamCost;
        PlayerCount = playerCount;
    }
    
    public static Grid2D Create(MapConfig config)
    {
        var grid = new Grid2D(config.Id, config.X, config.Y, config.MaxTeamCost, config.PlayerCount);
        grid.InitPositions(config.Terrain, config.PlayerStartPositions, config.EntityStartPositions);
        return grid;
    }
    
    private void InitPositions(List<TerrainConfig> terrain, List<PositionConfig> playerStartPositions, List<EntityStartConfig> entityStartPositions)
    {
        Terrain = new TerrainType[GetSize()];
        PlayerStartPositions = new Dictionary<int, int>();
        Entities = new Entity[GetSize()];
        if (terrain != null)
        {
            foreach (var config in terrain)
            {
                if (!Enum.TryParse(config.TerrainType, out TerrainType terrainType) || terrainType == TerrainType.Default) continue;
                var positions = GetPositions(config.PositionValues, config.PositionRanges);
                foreach (var position in positions)
                    if (Terrain[position] == TerrainType.Default)
                        Terrain[position] = terrainType;
            }
        }
        if (playerStartPositions != null)
        {
            for (var i = 0; i < playerStartPositions.Count; i++)
            {
                var player = i + 1;
                var positions = GetPositions(playerStartPositions[i].PositionValues, playerStartPositions[i].PositionRanges);
                foreach (var position in positions)
                    if (IsTraversable(position)) // TODO: add Terrain mask
                        PlayerStartPositions.TryAdd(position, player);
            }
        }
        if (entityStartPositions != null)
        {
            foreach (var config in entityStartPositions)
            {
                if (!IdRegistry<EntityConfig>.TryGet(config.EntityId, out var entityConfig)) continue;
                var positions = GetPositions(config.PositionValues, config.PositionRanges);
                foreach (var position in positions)
                {
                    if (!IsTraversable(position)) continue;
                    var playerController = 0;
                    if (PlayerStartPositions.TryGetValue(position, out var player))
                        playerController = player;
                    Entities[position] = Entity.Create(entityConfig, playerController);;
                }
            }
        }
    }
    
    private int[] GetPositions(List<int> positionValues, List<IntRange> positionRanges)
    {
        var positions = new List<int>();
        positions.AddRange(positionValues);
        foreach (var range in positionRanges)
        {
            positions.AddRange(Enumerable.Range(range.Start, range.End - range.Start + 1));
        }
        return positions.Distinct().Where(position => position >= 0 && position < GetSize()).ToArray();
    }
    
    public void LoadPlayerTeam(int player, TeamData teamData)
    {
        if (!ValidatePlayerTeam(player, teamData)) return;
        
        foreach (var (position, unit) in teamData.UnitStartPositions)
        {
            if (PlayerStartPositions[position] != player) continue;
            if (!IdRegistry<EntityConfig>.TryGet(unit, out var entityConfig)) continue;
            var entity = Entity.Create(entityConfig, player);
            if (!entity.TryGetComponent<ControlComponent>(out var control)) continue;
            control.PlayerController = player;
            Entities[position] = entity;
        }
    }
    
    private bool ValidatePlayerTeam(int player, TeamData teamData)
    {
        if (teamData.MapId != Id) return false;
        foreach (var (position, unit) in teamData.UnitStartPositions)
            if (!IsValidPosition(position) || PlayerStartPositions[position] == 0 || Entities[position] != null || IdRegistry<EntityConfig>.TryGet(unit, out var entityConfig))
                return false;
        return true;
    }

    public int GetSize()
    {
        return X * Y;
    }

    public Entity GetEntity(int position)
    {
        return !IsValidPosition(position) ? null : Entities[position];
    }

    public Entity GetEntity(int x, int y)
    {
        return GetEntity(ToPosition1D(x, y));
    }
    
    public Entity GetEntity((int x, int y) position)
    {
        return GetEntity(ToPosition1D(position.x, position.y));
    }
    
    public TerrainType GetTerrain(int position)
    {
        return Terrain[position];
    }

    public HashSet<int> GetOccupiedTilesPositionSet()
    {
        HashSet<int> indices = new();
        for (int i = 0; i < GetSize(); i++)
            if (Entities[i] != null)
                indices.Add(i);
        return indices;
    }

    public bool SetTileTerrain(int position, TerrainType tileTerrain)
    {
        if (IsValidPosition(position))
        {
            Terrain[position] = tileTerrain;
            return true;
        }
        return false;
    }

    public bool SetEntity(int position, Entity entity=null)
    {
        if (IsValidPosition(position))
        {
            Entities[position] = entity;
            return true;
        }
        return false;
    }
    
    // -1 = passive, 0 = move, 1~n = skill
    public bool PerformAction(int action, int sourceTile, int targetTile)
    {
        Entity entity = GetEntity(sourceTile);
        if (entity == null)
            return false;
        return true;
    }
    
    public bool PerformAction(int action, int sourceTile, List<int> targetTiles)
    {
        return false;
    }

    public bool MoveEntity(int startPosition, int targetPosition)
    {
        if (IsValidPosition(startPosition) && IsValidPosition(targetPosition))
            return SetEntity(targetPosition, GetEntity(startPosition)) && SetEntity(startPosition, null);
        return false;
    }

    // public Tuple<int, int> ToPosition2DTuple(int position1D)
    // {
    //     if (!IsValidPosition(position1D)) return null;
    //     return new Tuple<int, int>(position1D % X, position1D / X);
    // }

    public (int, int) ToPosition2D(int position)
    {
        return !IsValidPosition(position) ? (-1, -1) : (position % X, position / X);
    }

    public int ToPosition1D(int x, int y)
    {
        var position = y * X + x;
        if (!IsValidPosition(position))
            return -1;
        return position;
    }
    
    public int ToPosition1D((int x, int y) position)
    {
        var pos = position.y * X + position.x;
        if (!IsValidPosition(pos))
            return -1;
        return pos;
    }
    
    // public int ToPosition1D(Tuple<int, int> position2D)
    // {
    //     return ToPosition1D(position2D.Item1, position2D.Item2);
    // }

    // public List<int> ToPosition1DList(List<Tuple<int, int>> position2DList)
    // {
    //     List<int> position1Dlist = new List<int>();
    //     foreach (Tuple<int, int> position2D in position2DList)
    //     {
    //         position1Dlist.Add(ToPosition1D(position2D));
    //     }
    //     return position1Dlist;
    // }
    
    public List<int> ToPositionList(List<(int, int)> xyList)
    {
        var positionList = new List<int>();
        foreach (var (x, y) in xyList)
            positionList.Add(ToPosition1D(x, y));
        return positionList;
    }

    public bool IsValidPosition(int position)
    {
        return position >= 0 && position <= X * Y;
    }

    // public bool IsValidPosition(Tuple<int, int> position2D)
    // {
    //     return position2D != null && position2D.Item1 >= 0 && position2D.Item1 < X && position2D.Item2 >= 0 && position2D.Item2 < Y;
    // }
    
    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < X && y >= 0 && y < Y;
    }
    
    public bool IsValidPosition((int x, int y) position)
    {
        return position.x >= 0 && position.x < X && position.y >= 0 && position.y < Y;
    }
    
    public bool IsTraversable(int position)
    {
        return Terrain[position] != TerrainType.Void && Entities[position] == null;
    }
    
    public bool IsTraversable(int x, int y)
    {
        return IsTraversable(ToPosition1D(x, y));
    }
    
    public bool IsTraversable((int x, int y) position)
    {
        return IsTraversable(ToPosition1D(position.x, position.y));
    }
    
    public bool ValidateStartPositions(List<int> startPositions)
    {
        return startPositions.Count == GetSize();
    }
    
    public string PrintGrid()
    {
        var grid = "";
        
        grid += "START POSITIONS\n";
        foreach (var kvp in PlayerStartPositions)
            grid += "(" + kvp.Key + "-"+ kvp.Value + ") ";
        grid += "\n";
        
        grid += "TERRAIN\n";
        for (int i = 0; i < Terrain.Length; i++)
            grid += Terrain[i] + " ";
        grid += "\n";
        
        // grid += "TERRAIN + START POSITIONS\n";
        // for (var y = Y-1; y >= 0; y--)
        // {
        //     for (var x = 0; x < X; x++)
        //     {
        //         // grid += ToPosition1D(x, y) + " ";
        //         var terrain = (int)Terrain[ToPosition1D(x, y)];
        //         var playerStartPosition = PlayerStartPositions[ToPosition1D(x, y)];
        //         grid += (playerStartPosition != 0 ? "-" + playerStartPosition + " " : terrain + "  ");
        //     }
        //     grid += "\n";
        // }
        
        grid += "ENTITIES\n";
        for (var i = 0; i < GetSize(); i++)
            if (Entities[i] != null)
                grid += "(" + i + " " + Entities[i].Id + ") ";
        grid += "\n";
        
        return grid;
    }
    
    // 8  9  10 11
    // 4  5  6  7
    // 0  1  2  3
    // GetSize() = 12
    // Y = 3
    // X = 4
    
    
}