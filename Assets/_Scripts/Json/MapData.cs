using System;
using System.Collections.Generic;

[Serializable]
public class MapData
{
    public string Name;
    public int X;
    public int Y;
    public int UnitCostTotal;

    // 1 to n:   Entity Id
    // 0:        No Entity
    // -1 to -n: Player Start Position
    public List<int> EntityStartPositions;

    public List<TileTerrain> TileTerrain;
}