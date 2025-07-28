using System;
using System.Collections.Generic;

[Serializable]
public class MapData
{
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

    public MapData(string name, int x, int y, int playerCount, int unitCostTotal, List<int> entityStartPositions, List<TileTerrain> tileTerrain)
    {
        Name = name;
        X = x;
        Y = y;
        PlayerCount = playerCount;
        UnitCostTotal = unitCostTotal;
        EntityStartPositions = entityStartPositions;
        TileTerrain = tileTerrain;
    }
}