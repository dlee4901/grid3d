using System;
using System.Collections.Generic;

[Serializable]
public class TeamData
{
    public string Name;
    public int MapId;
    public List<int> StartPositions;
    public List<int> UnitIds;

    public TeamData(string name, int mapId, List<int> startPositions, List<int> unitIds)
    {
        Name = name;
        MapId = mapId;
        StartPositions = startPositions;
        UnitIds = unitIds;
    }
}