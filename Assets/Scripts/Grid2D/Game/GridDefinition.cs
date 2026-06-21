using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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

    public List<TerrainConfig> Terrain { get; set; }
    public List<PositionConfig> PlayerStartPositions { get; set; }
    public List<EntityStartConfig> EntityStartPositions { get; set; }

    public TerrainType[] TerrainMap { get; private set; }
    public IReadOnlyDictionary<int, int> SpawnPlayers => _spawnPlayers;
    public IReadOnlyDictionary<int, HashSet<int>> PlayerSpawns => _playerSpawns;

    private readonly Dictionary<int, int> _spawnPlayers = new();
    private readonly Dictionary<int, HashSet<int>> _playerSpawns = new();

    public int Size => X * Y;

    [OnDeserialized]
    internal void OnDeserialized(StreamingContext _) => Bake();

    public void Bake()
    {
        TerrainMap = new TerrainType[Size];
        _spawnPlayers.Clear();
        _playerSpawns.Clear();

        if (Terrain != null)
        {
            foreach (var spec in Terrain)
            {
                if (!Enum.TryParse(spec.TerrainType, out TerrainType t) || t == TerrainType.Default) continue;
                foreach (var pos in ResolvePositions(spec))
                    if (TerrainMap[pos] == TerrainType.Default)
                        TerrainMap[pos] = t;
            }
        }

        if (PlayerStartPositions != null)
        {
            for (var i = 0; i < PlayerStartPositions.Count; i++)
            {
                var player = i + 1;
                foreach (var pos in ResolvePositions(PlayerStartPositions[i]))
                    if (TerrainMap[pos] != TerrainType.Void)
                        SetSpawnPlayer(pos, player);
            }
        }
    }

    public IEnumerable<int> ResolvePositions(PositionConfig spec)
    {
        var positions = new HashSet<int>(spec.PositionValues);
        if (spec.PositionRanges != null)
            foreach (var r in spec.PositionRanges)
                for (var i = r.Start; i <= r.End; i++)
                    positions.Add(i);
        return positions.Where(p => IsValidPosition(p));
    }

    public int GetSpawnPlayer(int spawn) => _spawnPlayers.GetValueOrDefault(spawn, 0);
    public HashSet<int> GetPlayerSpawnsOrNull(int player) => _playerSpawns.GetValueOrDefault(player, null);

    public bool SetSpawnPlayer(int spawn, int player)
    {
        if (GetSpawnPlayer(spawn) != 0) return false;
        _spawnPlayers[spawn] = player;
        if (!_playerSpawns.TryGetValue(player, out var set))
            _playerSpawns[player] = set = new HashSet<int>();
        set.Add(spawn);
        return true;
    }

    public bool SetTileTerrain(int position, TerrainType terrain)
    {
        if (!IsValidPosition(position) || TerrainMap == null) return false;
        TerrainMap[position] = terrain;
        return true;
    }

    public int ToPosition1D(int x, int y)
    {
        var p = y * X + x;
        return IsValidPosition(p) ? p : -1;
    }

    public int ToPosition1D((int x, int y) p) => ToPosition1D(p.x, p.y);

    public (int, int) ToPosition2D(int p) => IsValidPosition(p) ? (p % X, p / X) : (-1, -1);

    public List<int> ToPositionList(List<(int, int)> xyList)
    {
        var positionList = new List<int>();
        foreach (var (x, y) in xyList)
            positionList.Add(ToPosition1D(x, y));
        return positionList;
    }

    public bool IsValidPosition(int position) => position >= 0 && position < X * Y;
    public bool IsValidPosition(int x, int y) => x >= 0 && x < X && y >= 0 && y < Y;
    public bool IsValidPosition((int x, int y) position) => IsValidPosition(position.x, position.y);

    public bool ValidateStartPositions(List<int> startPositions) => startPositions.Count == Size;
}
