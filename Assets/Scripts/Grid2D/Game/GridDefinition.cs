using System;
using System.Collections.Generic;

public enum TerrainType { Default, Void }

public class PositionConfig
{
    public List<int> PositionValues { get; set; } = new();
    public List<IntRange> PositionRanges { get; set; } = new();
}

public class TerrainConfig : PositionConfig
{
    public string TerrainType { get; set; }
}

public class EntityStartConfig : PositionConfig
{
    public string EntityId { get; set; }
}

public sealed class GridDefinition : INameId
{
    public string Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int MaxTeamCost { get; set; }
    public int PlayerCount { get; set; } = 2;
    public int ManaPerTurn { get; set; } = 3;
    public int PlayerStartingTimeSeconds { get; set; } = 600;

    public List<TerrainConfig> Terrain { get; set; }
    public List<PositionConfig> PlayerStartPositions { get; set; }
    public List<EntityStartConfig> EntityStartPositions { get; set; }

    public TerrainType[] TerrainMap { get; private set; }
    public IReadOnlyDictionary<int, int> SpawnPlayers => _spawnPlayers;
    public IReadOnlyDictionary<int, HashSet<int>> PlayerSpawns => _playerSpawns;

    private readonly Dictionary<int, int> _spawnPlayers = new();
    private readonly Dictionary<int, HashSet<int>> _playerSpawns = new();

    public int Size => X * Y;

    public void Init()
    {
        TerrainMap = new TerrainType[Size];
        if (Terrain != null)
        {
            foreach (var spec in Terrain)
            {
                if (!Enum.TryParse(spec.TerrainType, out TerrainType t) || t == TerrainType.Default) continue;
                foreach (var pos in ResolvePositions(spec))
                    if (TerrainMap[pos.Dim1] == TerrainType.Default)
                        TerrainMap[pos.Dim1] = t;
            }
        }
        
        if (PlayerStartPositions != null)
        {
            _spawnPlayers.Clear();
            _playerSpawns.Clear();
            for (var i = 0; i < PlayerStartPositions.Count; i++)
            {
                var player = i + 1;
                foreach (var pos in ResolvePositions(PlayerStartPositions[i]))
                    if (TerrainMap[pos.Dim1] != TerrainType.Void)
                        SetSpawnPlayer(pos, player);
            }
        }
    }
    
    public HashSet<GridPosition> ResolvePositions(PositionConfig config)
    {
        var positions = new HashSet<GridPosition>();
        if (config.PositionValues != null)
        {
            foreach (var position in config.PositionValues)
            {
                var gridPosition = new GridPosition(X, Y, position);
                if (gridPosition.IsValid()) positions.Add(gridPosition);
            }
        }
        if (config.PositionRanges != null)
        {
            foreach (var range in config.PositionRanges)
            {
                for (var i = range.Start; i <= range.End; i++)
                {
                    var gridPosition = new GridPosition(X, Y, i);
                    if (gridPosition.IsValid()) positions.Add(gridPosition);
                }
            }
        }
        return positions;
    }

    public int GetSpawnPlayer(GridPosition position) => _spawnPlayers.GetValueOrDefault(position.Dim1, 0);
    public HashSet<int> GetPlayerSpawnsOrNull(int player) => _playerSpawns.GetValueOrDefault(player, null);

    public bool SetSpawnPlayer(GridPosition position, int player)
    {
        if (GetSpawnPlayer(position) != 0) return false;
        _spawnPlayers[position.Dim1] = player;
        if (!_playerSpawns.TryGetValue(player, out var set))
            _playerSpawns[player] = set = new HashSet<int>();
        set.Add(position.Dim1);
        return true;
    }

    public bool SetTileTerrain(GridPosition position, TerrainType terrain)
    {
        if (!position.IsValid() || TerrainMap == null) return false;
        TerrainMap[position.Dim1] = terrain;
        return true;
    }
}
