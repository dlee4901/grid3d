using System;
using System.Collections.Generic;

public class TeamData : INameId
{
    public string Id { get; }
    public string MapId { get; }
    public Dictionary<int, string> UnitStartPositions { get; }

    public TeamData(string id, string mapId, Dictionary<int, string> unitStartPositions)
    {
        Id = id;
        MapId = mapId;
        UnitStartPositions = unitStartPositions;
    }
}