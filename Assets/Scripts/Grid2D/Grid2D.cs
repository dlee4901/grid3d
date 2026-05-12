using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TerrainType { Default, Void }

public struct CommandContext
{
    public Grid2D Grid;
    public Entity SourceEntity;
    public int StartPosition;
}

public interface IReadOnlyGrid2D
{
    string Id { get; }
    int X { get; }
    int Y { get; }
    int MaxTeamCost { get; }
    int PlayerCount { get; }
    int GetSpawnPlayer(int spawn);
    HashSet<int> GetPlayerSpawns(int player);
}

public class Grid2D : INameId
{
    // Identifiers
    public string Id { get; }
    public int X { get; }
    public int Y { get; }
    
    public int MaxTeamCost { get; }
    public int PlayerCount { get; }
    
    private readonly Dictionary<int, int> _spawnPlayers = new();
    private readonly Dictionary<int, HashSet<int>> _playerSpawns = new();

    // State
    public int Turn { get; private set; }
    
    private TerrainType[] _terrain;
    private Entity[] _entities;
    private List<Entity> _prioritizedEntities;

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
        _terrain = new TerrainType[GetSize()];
        _entities = new Entity[GetSize()];
        if (terrain != null)
        {
            foreach (var config in terrain)
            {
                if (!Enum.TryParse(config.TerrainType, out TerrainType terrainType) || terrainType == TerrainType.Default) continue;
                var positions = GetPositions(config.PositionValues, config.PositionRanges);
                foreach (var position in positions)
                    if (_terrain[position] == TerrainType.Default)
                        _terrain[position] = terrainType;
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
                        SetSpawnPlayer(position, player);
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
                    SetEntityPosition(position, Entity.Create(entityConfig, GetSpawnPlayer(position)));
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
        Debug.Log($"Loading player team {player}");
        
        foreach (var (position, unit) in teamData.UnitStartPositions)
        {
            Debug.Log(player + " 1");
            if (!IdRegistry<EntityConfig>.TryGet(unit, out var entityConfig)) continue;
            Debug.Log(player + " 2");
            var entity = Entity.Create(entityConfig, player);
            if (!entity.TryGetComponent<ControlComponent>(out var control)) continue;
            Debug.Log(player + " 3");
            control.PlayerController = player;
            SetEntityPosition(position, entity);
        }
    }
    
    private bool ValidatePlayerTeam(int player, TeamData teamData)
    {
        Debug.Log(player + " map1");
        if (teamData.MapId != Id) return false;
        Debug.Log(player + " map2");
        foreach (var (position, unit) in teamData.UnitStartPositions)
            if (!IsValidPosition(position) || GetSpawnPlayer(position) != player || _entities[position] != null || !IdRegistry<EntityConfig>.TryGet(unit, out var entityConfig))
                return false;
        return true;
    }

    public int GetSize()
    {
        return X * Y;
    }

    public Entity GetEntity(int position)
    {
        return IsValidPosition(position) ? _entities[position] : null;
    }

    public Entity GetEntity(int x, int y)
    {
        return GetEntity(ToPosition1D(x, y));
    }
    
    public Entity GetEntity((int x, int y) position)
    {
        return GetEntity(ToPosition1D(position.x, position.y));
    }
    
    public bool TryGetEntity(int position, out Entity entity)
    {
        if (!IsValidPosition(position))
        {
            entity = null;
            return false;
        }
        entity = _entities[position];
        return true;
    }
    
    public bool TryGetEntity(int x, int y, out Entity entity)
    {
        return TryGetEntity(ToPosition1D(x, y), out entity);
    }
    
    public bool TryGetEntity((int x, int y) position, out Entity entity)
    {
        return TryGetEntity(ToPosition1D(position.x, position.y), out entity);
    }
    
    public bool TryGetTerrain(int position, out TerrainType terrainType)
    {
        if (!IsValidPosition(position))
        {
            terrainType = TerrainType.Default;
            return false;
        }
        terrainType = _terrain[position];
        return true;
    }

    public HashSet<int> GetOccupiedTilesPositionSet()
    {
        HashSet<int> indices = new();
        for (int i = 0; i < GetSize(); i++)
            if (_entities[i] != null)
                indices.Add(i);
        return indices;
    }
    
    public int GetSpawnPlayer(int spawn)
    {
        return _spawnPlayers.GetValueOrDefault(spawn, 0);
    }
    
    public HashSet<int> GetPlayerSpawns(int player)
    {
        return _playerSpawns.GetValueOrDefault(player, null);
    }
    
    public bool SetSpawnPlayer(int spawn, int player)
    {
        if (GetSpawnPlayer(spawn) != 0)
            return false;
        
        _spawnPlayers[spawn] = player;
        
        if (GetPlayerSpawns(player) == null)
            _playerSpawns.Add(player, new HashSet<int>{spawn});
        else
            _playerSpawns[player].Add(spawn);
            
        return true;
    }

    public bool SetTileTerrain(int position, TerrainType tileTerrain)
    {
        if (IsValidPosition(position))
        {
            _terrain[position] = tileTerrain;
            return true;
        }
        return false;
    }

    public bool SetEntityPosition(int position, Entity entity)
    {
        if (!IsValidPosition(position) || entity == null)
            return false;
        _entities[position] = entity;
        entity.SetPosition(position);
        return true;
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
            return SetEntityPosition(targetPosition, GetEntity(startPosition)) && SetEntityPosition(startPosition, null);
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
        return position >= 0 && position < X * Y;
    }
    
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
        return _terrain[position] != TerrainType.Void && _entities[position] == null;
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
        foreach (var kvp in _spawnPlayers)
            grid += "(" + kvp.Key + "-"+ kvp.Value + ") ";
        grid += "\n";
        
        grid += "TERRAIN\n";
        for (int i = 0; i < _terrain.Length; i++)
            grid += _terrain[i] + " ";
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
            if (_entities[i] != null)
                grid += "(" + i + " " + _entities[i].Id + ") ";
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