using System;
using System.Collections.Generic;

public enum TileTerrain { Void, Default }

public class Grid2D
{
    // Initial Parameters (Static)
    public string Name;
    
    public int X;
    public int Y;
    
    public int PlayerCount;
    public int MaxTeamCost;
    public List<int> EntityStartPositions;

    // State
    public List<TileTerrain> TileTerrain;
    public List<Entity> Entities;
    public List<Entity> PrioritizedEntities;
    public int Turn;

    public Grid2D(MapData mapData)
    {
        Init(mapData.Name, mapData.X, mapData.Y, mapData.PlayerCount, mapData.MaxTeamCost, mapData.EntityStartPositions, mapData.TileTerrain);
    }

    public void Init(string name, int x, int y, int playerCount, int maxTeamCost, List<int> entityStartPositions, List<TileTerrain> tileTerrain)
    {
        Name = name;
        X = x;
        Y = y;
        PlayerCount = playerCount;
        MaxTeamCost = maxTeamCost;
        EntityStartPositions = entityStartPositions;
        TileTerrain = tileTerrain;
    }

    public void LoadTeams(List<TeamData> teamData)
    {
        int[] teamCosts = new int[teamData.Count];
        for (int i = 0; i < teamData.Count; i++)
        {
            
        }
        // var unitIdStartPositions = new Dictionary<int, int>();
        // int unitCostTotal = 0;
        // for (int i = 0; i < teamData.StartPositions.Count; i++)
        // {
        //     var startPosition = teamData.StartPositions[i];
        //     var unitId = teamData.UnitIds[i];
        //     if (EntityStartPositions[startPosition] == -player)
        //     {
        //         unitIdStartPositions[startPosition] = unitId;
        //         unitCostTotal += 1;
        //     }
        // }
    }

    public int GetSize()
    {
        return X * Y;
    }

    public Entity GetEntity(int position)
    {
        if (!IsValidPosition(position))
        {
            return default;
        }
        return Entities[position];
    }

    public Entity GetEntity(int x, int y)
    {
        return GetEntity(ToPosition1D(x, y));
    }
    
    public Entity GetEntity((int x, int y) position)
    {
        return GetEntity(ToPosition1D(position.x, position.y));
    }
    
    // public Entity GetEntity(Tuple<int, int> position2D)
    // {
    //     return GetEntity(ToPosition1D(position2D));
    // }

    public HashSet<int> GetOccupiedTilesPositionSet()
    {
        HashSet<int> indices = new();
        for (int i = 0; i < GetSize(); i++)
        {
            if (Entities[i] != null)
            {
                indices.Add(i);
            }
        }
        return indices;
    }

    public bool SetTileTerrain(int position, TileTerrain tileTerrain)
    {
        if (IsValidPosition(position))
        {
            TileTerrain[position] = tileTerrain;
            return true;
        }
        return false;
    }

    public bool SetEntity(int position, Entity entity)
    {
        if (IsValidPosition(position))
        {
            Entities[position] = entity;
            return true;
        }
        return false;
    }
    
    // -1 = passive, 0 = move, 1~n = skill
    public bool PerformAction(int action, int sourceTile, int targetTile)
    {
        Entity entity = GetEntity(sourceTile);
        if (entity == null) return false;
        
        return true;
    }
    
    public bool PerformAction(int action, int sourceTile, List<int> targetTiles)
    {
        return false;
    }

    public bool MoveEntity(int startPosition, int targetPosition)
    {
        if (IsValidPosition(startPosition) && IsValidPosition(targetPosition))
        {
            return SetEntity(targetPosition, GetEntity(startPosition)) && SetEntity(startPosition, null);
        }
        return false;
    }

    // public Tuple<int, int> ToPosition2DTuple(int position1D)
    // {
    //     if (!IsValidPosition(position1D)) return null;
    //     return new Tuple<int, int>(position1D % X, position1D / X);
    // }
    
    public (int, int) ToPosition2D(int position)
    {
        if (!IsValidPosition(position)) return (-1, -1);
        return (position % X, position / X);
    }

    public int ToPosition1D(int x, int y)
    {
        int position = x * X + y;
        if (!IsValidPosition(position)) return -1;
        return position;
    }
    
    public int ToPosition1D((int x, int y) position)
    {
        int pos = position.x * X + position.y;
        if (!IsValidPosition(pos)) return -1;
        return pos;
    }
    
    // public int ToPosition1D(Tuple<int, int> position2D)
    // {
    //     return ToPosition1D(position2D.Item1, position2D.Item2);
    // }

    // public List<int> ToPosition1DList(List<Tuple<int, int>> position2DList)
    // {
    //     List<int> position1Dlist = new List<int>();
    //     foreach (Tuple<int, int> position2D in position2DList)
    //     {
    //         position1Dlist.Add(ToPosition1D(position2D));
    //     }
    //     return position1Dlist;
    // }
    
    public List<int> ToPositionList(List<(int, int)> xyList)
    {
        List<int> positionList = new List<int>();
        foreach (var (x, y) in xyList)
        {
            positionList.Add(ToPosition1D(x, y));
        }
        return positionList;
    }

    public bool IsValidPosition(int position)
    {
        return position >= 0 && position <= X * Y;
    }

    // public bool IsValidPosition(Tuple<int, int> position2D)
    // {
    //     return position2D != null && position2D.Item1 >= 0 && position2D.Item1 < X && position2D.Item2 >= 0 && position2D.Item2 < Y;
    // }
    
    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < X && y >= 0 && y < Y;
    }
    
    public bool IsValidPosition((int x, int y) position)
    {
        return position.x >= 0 && position.x < X && position.y >= 0 && position.y < Y;
    }
    
    public bool ValidateStartPositions(List<int> startPositions)
    {
        return startPositions.Count == GetSize();
    }
}