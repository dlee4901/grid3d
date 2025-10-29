using System;
using System.Collections.Generic;

[Serializable]
public class MapData
{
    public int Id;
    public string Name;
    public int X;
    public int Y;
    public int PlayerCount;
    public int UnitCostTotal;

    // 1 to n:   Entity Id
    // 0:        No Entity
    // -1 to -n: Player Start Position
    public List<int> EntityStartPositions;

    public List<TileTerrain> TileTerrain;

    public MapData(int id, string name, int x, int y, int playerCount, int unitCostTotal, List<int> entityStartPositions, List<TileTerrain> tileTerrain)
    {
        Id = id;
        Name = name;
        X = x;
        Y = y;
        PlayerCount = playerCount;
        UnitCostTotal = unitCostTotal;
        EntityStartPositions = entityStartPositions;
        TileTerrain = tileTerrain;
    }
}