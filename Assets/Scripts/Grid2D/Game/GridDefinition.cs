using System;
using System.Collections.Generic;

public enum TerrainType { Plain, Burning }

public class PositionConfig
{
    public List<int> PositionValues { get; init; } = new();
    public List<IntRange> PositionRanges { get; init; } = new();
}

public class TerrainPlacement : PositionConfig
{
    public string TerrainType { get; init; }
    public int Turns { get; init; }
}

public class EntityStartConfig : PositionConfig
{
    public string EntityId { get; init; }
}

public sealed class GridDefinition : INameId
{
    public string Id { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public string[] Layout { get; init; }
    public int MaxTeamCost { get; init; }
    public int PlayerCount { get; init; } = 2;
    public int ManaPerTurn { get; init; } = 3;
    public int PlayerStartingTimeSeconds { get; init; } = 600;

    public List<TerrainPlacement> Terrain { get; init; }
    public List<PositionConfig> PlayerStartPositions { get; init; }
    public List<EntityStartConfig> EntityStartPositions { get; init; }

    public GridShape Shape { get; private set; }
    public TerrainType[] TerrainMap { get; private set; }
    public int[] TerrainTurns { get; private set; }
    public IReadOnlyDictionary<int, int> SpawnPlayers => _spawnPlayers;
    public IReadOnlyDictionary<int, HashSet<int>> PlayerSpawns => _playerSpawns;

    private readonly Dictionary<int, int> _spawnPlayers = new();
    private readonly Dictionary<int, HashSet<int>> _playerSpawns = new();

    public int Size => X * Y;
    public bool IsMirrored { get; private set; }

    public int Mirror(int position) => Size - 1 - position;

    internal void Init()
    {
        Shape = new GridShape(X, Y, Layout);
        TerrainMap = new TerrainType[Size];
        TerrainTurns = new int[Size];
        if (Terrain != null)
        {
            foreach (var spec in Terrain)
            {
                if (!Enum.TryParse(spec.TerrainType, out TerrainType t) || t == TerrainType.Plain) continue;
                foreach (var pos in ResolvePositions(spec))
                {
                    if (TerrainMap[pos.Dim1] != TerrainType.Plain) continue;
                    TerrainMap[pos.Dim1] = t;
                    TerrainTurns[pos.Dim1] = spec.Turns;
                }
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
                    SetSpawnPlayer(pos, player);
            }
        }
        IsMirrored = ComputeMirrored();
    }
    
    private bool ComputeMirrored()
    {
        if (PlayerCount != 2) return false;
        for (var position = 0; position < Size; position++)
        {
            var mirror = Mirror(position);
            if (Shape.Contains(position) != Shape.Contains(mirror)) return false;
            if (TerrainMap[position] != TerrainMap[mirror]) return false;
            var spawn = _spawnPlayers.GetValueOrDefault(position, 0);
            var mirrorSpawn = _spawnPlayers.GetValueOrDefault(mirror, 0);
            if (spawn != (mirrorSpawn == 0 ? 0 : 3 - mirrorSpawn)) return false;
        }
        return true;
    }

    public HashSet<GridPosition> ResolvePositions(PositionConfig config)
    {
        var positions = new HashSet<GridPosition>();
        if (config.PositionValues != null)
        {
            foreach (var position in config.PositionValues)
            {
                var gridPosition = new GridPosition(Shape, position);
                if (gridPosition.IsValid()) positions.Add(gridPosition);
            }
        }
        if (config.PositionRanges != null)
        {
            foreach (var range in config.PositionRanges)
            {
                for (var i = range.Start; i <= range.End; i++)
                {
                    var gridPosition = new GridPosition(Shape, i);
                    if (gridPosition.IsValid()) positions.Add(gridPosition);
                }
            }
        }
        return positions;
    }

    public int GetSpawnPlayer(GridPosition position) => _spawnPlayers.GetValueOrDefault(position.Dim1, 0);
    public HashSet<int> GetPlayerSpawnsOrNull(int player) => _playerSpawns.GetValueOrDefault(player, null);

    private bool SetSpawnPlayer(GridPosition position, int player)
    {
        if (GetSpawnPlayer(position) != 0) return false;
        _spawnPlayers[position.Dim1] = player;
        if (!_playerSpawns.TryGetValue(player, out var set))
            _playerSpawns[player] = set = new HashSet<int>();
        set.Add(position.Dim1);
        return true;
    }

    private bool SetTileTerrain(GridPosition position, TerrainType terrain)
    {
        if (!position.IsValid() || TerrainMap == null) return false;
        TerrainMap[position.Dim1] = terrain;
        return true;
    }
}
