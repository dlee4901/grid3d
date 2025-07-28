using System;
using System.Collections.Generic;

[Serializable]
public class TeamData
{
    public string Name;
    public string MapName;
    public List<int> StartPositions;
    public List<int> UnitIds;

    public TeamData(string name, string mapName, List<int> startPositions, List<int> unitIds)
    {
        Name = name;
        MapName = mapName;
        StartPositions = startPositions;
        UnitIds = unitIds;
    }
}