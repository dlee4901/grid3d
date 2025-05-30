using System;
using System.Collections.Generic;

[Serializable]
public class TeamData
{
    public string Name { get; set; }
    public string MapName { get; set; }
    public Dictionary<int, int> UnitIdPositions { get; set; }

    public TeamData(string name, string mapName, Dictionary<int,int> unitIdPositions)
    {
        Name = name;
        MapName = mapName;
        UnitIdPositions = unitIdPositions;
    }
}