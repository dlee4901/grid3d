using System.Collections.Generic;
using Newtonsoft.Json;

public class UnitPlacement
{
    public int Position { get; }
    public string UnitId { get; }

    [JsonConstructor]
    public UnitPlacement(int position, string unitId)
    {
        Position = position;
        UnitId = unitId;
    }
}

public class TeamData : INameId
{
    public string Id { get; }
    public string MapId { get; }
    public IReadOnlyList<UnitPlacement> UnitStartPositions => _unitStartPositions;
  
    private readonly List<UnitPlacement> _unitStartPositions = new();
    private readonly HashSet<int> _positions = new();
    
    public TeamData(string id, string mapId, List<(int, string)> unitStartPositions)
    {
        Id = id;
        MapId = mapId;
        if (unitStartPositions == null) return;
        foreach (var (position, unitId) in unitStartPositions) Add(position, unitId);
    }
    
    [JsonConstructor]
    private TeamData(string id, string mapId, List<UnitPlacement> unitStartPositions)
    {
        Id = id;
        MapId = mapId;
        if (unitStartPositions == null) return;
        foreach (var unit in unitStartPositions) Add(unit.Position, unit.UnitId);
    }
    
    private void Add(int position, string unitId)
    {
        if (!_positions.Add(position))
        {
            GridLog.Error($"Team '{Id}' has duplicate position {position}");
            return;
        }
        _unitStartPositions.Add(new UnitPlacement(position, unitId));
    }
}