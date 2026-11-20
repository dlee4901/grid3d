using System.Collections.Generic;
using Newtonsoft.Json;

public sealed class SavedUnit
{
    public const int Unplaced = -1;

    public string UnitId { get; }
    public IReadOnlyList<int> Positions { get; }

    [JsonConstructor]
    public SavedUnit(string unitId, IReadOnlyList<int> positions)
    {
        UnitId = unitId;
        Positions = positions;
    }

    public int PositionFor(int seat) => seat >= 1 && seat <= Positions.Count ? Positions[seat - 1] : Unplaced;
}

public sealed class SavedTeam : INameId
{
    public string Id { get; }
    public string MapId { get; }
    public IReadOnlyList<SavedUnit> Units { get; }

    [JsonConstructor]
    public SavedTeam(string id, string mapId, IReadOnlyList<SavedUnit> units)
    {
        Id = id;
        MapId = mapId;
        Units = units ?? new List<SavedUnit>();
    }

    public TeamData ForSeat(int seat)
    {
        var placements = new List<(int, string)>();
        foreach (var unit in Units)
        {
            var position = unit.PositionFor(seat);
            if (position != SavedUnit.Unplaced) placements.Add((position, unit.UnitId));
        }
        return new TeamData(Id, MapId, placements);
    }

    public string Validate(GridDefinition map)
    {
        if (map == null || map.Id != MapId) return $"team is for map '{MapId}'";
        for (var seat = 1; seat <= map.PlayerCount; seat++)
        {
            foreach (var unit in Units)
                if (unit.PositionFor(seat) == SavedUnit.Unplaced) return $"{unit.UnitId} has no side {seat} square";
            var issue = MatchSetup.ValidateTeam(map, seat, ForSeat(seat));
            if (issue != null) return $"side {seat}: {issue}";
        }
        return null;
    }
}
