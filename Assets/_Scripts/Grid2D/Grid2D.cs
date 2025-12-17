using System;
using System.Collections.Generic;

public enum TileTerrain { Void, Default }

[Serializable]
public class Grid2D
{
    // Initial Parameters (Static)
    public string Test { get; set; }
    public string Name { get; private set; }
    
    public int UnitCostTotal { get; private set; }
    
    private int _x;
    public int X
    {
        get => _x; 
        private set => _x = Math.Clamp(value, 2, 99);
    }

    private int _y;
    public int Y
    {
        get => _y;
        private set => _y = Math.Clamp(value, 2, 99);
    }
    
    private int _playerCount;
    public int PlayerCount
    {
        get => _playerCount;
        private set => _playerCount = Math.Clamp(value, 1, 4);
    }
    
    private List<int> _entityStartPositions;
    public List<int> EntityStartPositions
    {
        get => _entityStartPositions;
        private set
        {
            _entityStartPositions = value;
            _entityStartPositions.Capacity = GetSize();
        }
    }

    // State
    private List<TileTerrain> _tileTerrain;
    public List<TileTerrain> TileTerrain
    {
        get => _tileTerrain;
        private set
        {
            _tileTerrain = value;
            _tileTerrain.Capacity = GetSize();
        }
    }
    
    private List<Entity> _entities;
    public List<Entity> Entities
    {
        get => _entities;
        private set
        {
            _entities = value;
            _entities.Capacity = GetSize();
        }
    }
    
    public int Turn { get; private set; }

    public Grid2D(MapData mapData)
    {
        Init(mapData.Name, mapData.X, mapData.Y, mapData.PlayerCount, mapData.UnitCostTotal, mapData.EntityStartPositions, mapData.TileTerrain);
    }

    public void Init(string name, int x, int y, int playerCount, int unitCostTotal, List<int> entityStartPositions, List<TileTerrain> tileTerrain)
    {
        Name = name;
        X = x;
        Y = y;
        PlayerCount = playerCount;
        UnitCostTotal = unitCostTotal;
        EntityStartPositions = entityStartPositions;
        TileTerrain = tileTerrain;
    }

    public void LoadTeam(TeamData teamData, int player)
    {
        var unitIdStartPositions = new Dictionary<int, int>();
        int unitCostTotal = 0;
        for (int i = 0; i < teamData.StartPositions.Count; i++)
        {
            var startPosition = teamData.StartPositions[i];
            var unitId = teamData.UnitIds[i];
            if (EntityStartPositions[startPosition] == -player)
            {
                unitIdStartPositions[startPosition] = unitId;
                unitCostTotal += 1;
            }
        }
    }

    public int GetSize()
    {
        return X * Y;
    }

    public Entity GetEntity(int position1D)
    {
        if (!IsValidPosition(position1D))
        {
            return default;
        }
        return Entities[position1D];
    }

    public Entity GetEntity(Tuple<int, int> position2D)
    {
        return GetEntity(ToPosition1D(position2D));
    }

    public HashSet<int> GetOccupiedTilesPosition1DSet()
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

    public bool SetTileTerrain(int position1D, TileTerrain tileTerrain)
    {
        if (IsValidPosition(position1D))
        {
            TileTerrain[position1D] = tileTerrain;
            return true;
        }
        return false;
    }

    public bool SetEntity(int position1D, Entity entity)
    {
        if (IsValidPosition(position1D))
        {
            Entities[position1D] = entity;
            return true;
        }
        return false;
    }

    public bool MoveEntity(int startPosition1D, int targetPosition1D)
    {
        if (IsValidPosition(startPosition1D) && IsValidPosition(targetPosition1D))
        {
            return SetEntity(targetPosition1D, GetEntity(startPosition1D)) && SetEntity(startPosition1D, null);
        }
        return false;
    }

    public Tuple<int, int> ToPosition2D(int position1D)
    {
        if (!IsValidPosition(position1D)) return null;
        return new Tuple<int, int>(position1D % X, position1D / X);
    }

    public int ToPosition1D(int positionX, int positionY)
    {
        int position1D = positionX * X + positionY;
        if (!IsValidPosition(position1D)) return -1;
        return position1D;
    }
    public int ToPosition1D(Tuple<int, int> position2D)
    {
        return ToPosition1D(position2D.Item1, position2D.Item2);
    }

    public List<int> ToPosition1DList(List<Tuple<int, int>> position2DList)
    {
        List<int> position1Dlist = new List<int>();
        foreach (Tuple<int, int> position2D in position2DList)
        {
            position1Dlist.Add(ToPosition1D(position2D));
        }
        return position1Dlist;
    }

    public bool IsValidPosition(int position1D)
    {
        return position1D >= 0 && position1D <= X * Y;
    }

    public bool IsValidPosition(Tuple<int, int> position2D)
    {
        return position2D != null && position2D.Item1 >= 0 && position2D.Item1 < X && position2D.Item2 >= 0 && position2D.Item2 < Y;
    }
    
    public bool ValidateStartPositions(List<int> startPositions)
    {
        return startPositions.Count == GetSize();
    }
}