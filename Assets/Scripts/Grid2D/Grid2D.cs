using System;
using System.Collections.Generic;
using System.Linq;

public enum TileTerrain { Default, Void, Wall }

public class Grid2D : INameId
{
    // Initial Parameters (Static)
    public string Id { get; }
    public int X { get; }
    public int Y { get; }
    
    public int MaxTeamCost { get; }
    public int PlayerCount { get; }
    public int[] PlayerStartPositions { get; private set; }

    // State
    public TileTerrain[] Terrain { get; private set; }
    public Entity[] Entities { get; private set; }
    public List<Entity> PrioritizedEntities { get; private set; }
    public int Turn { get; }

    public Grid2D(string id, int x, int y, int maxTeamCost, int playerCount, (string[], int[][]) terrain, (string[], int[][]) playerStartPositions, (string[], int[][]) entityStartPositions)
    {
        Id = id;
        X = x;
        Y = y;
        MaxTeamCost = maxTeamCost;
        PlayerCount = playerCount;
        InitTypePositions(terrain, playerStartPositions, entityStartPositions);
    }
    
    public static Grid2D Create(MapConfig config)
    {
        var terrain = GetTypePositions(config.Terrain);
        var playerStartPositions = GetTypePositions(config.PlayerStartPositions);
        var entityStartPositions = GetTypePositions(config.EntityStartPositions);
        
        return new Grid2D(config.Id, config.X, config.Y, config.MaxTeamCost, config.PlayerCount, terrain, playerStartPositions, entityStartPositions);
    }
    
    private static (string[], int[][]) GetTypePositions(List<PositionRangeConfig> configs)
    {
        var positions = new List<List<int>>();
        var types = new List<string>();
        foreach (var config in configs)
        {
            types.Add(config.Type);
            var list = new List<int>();
            list.AddRange(config.Positions);
            foreach (var range in config.Ranges)
            {
                list.AddRange(Enumerable.Range(range.Start, range.End - range.Start + 1));
            }
            positions.Add(list.Distinct().ToList());
        }
        return (types.ToArray(), positions.Select(x => x.ToArray()).ToArray());
    }
    
    private void InitTypePositions((string[] types, int[][] positions) terrain, (string[] types, int[][] positions) playerStartPositions, (string[] types, int[][] positions) entityStartPositions)
    {
        Terrain = new TileTerrain[GetSize()];
        for (var i = 0; i < terrain.positions.Length; i++)
        {
            var type = terrain.types[i];
            var positions = terrain.positions[i];
            if (Enum.TryParse(type, out TileTerrain terrainType) && terrainType != TileTerrain.Default)
            {
                foreach (var position in positions) 
                    if (Terrain[position] == TileTerrain.Default) 
                        Terrain[position] = terrainType;
            }
        }
        
        PlayerStartPositions = new int[GetSize()];
        for (var i = 0; i < playerStartPositions.positions.Length; i++)
        {
            var player = i + 1;
            var positions = playerStartPositions.positions[i];
            foreach (var position in positions)
            {
                if (PlayerStartPositions[position] == 0 && Terrain[position] == TileTerrain.Default) 
                    PlayerStartPositions[position] = player;
            }
        }
        
        Entities = new Entity[GetSize()];
        for (var i = 0; i < entityStartPositions.positions.Length; i++)
        {
            var entity = Registry<Entity>.Get(entityStartPositions.types[i]);
            if (entity == null) continue;
            
            var positions = entityStartPositions.positions[i];
            foreach (var position in positions)
            {
                if (Entities[position] != null)
                {
                    Entities[position] = entity;
                    var player = PlayerStartPositions[position];
                    if (player != 0) Entities[position].Control?.SetPlayerId(player);
                }
            }
        }
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
        return !IsValidPosition(position) ? null : Entities[position];
    }

    public Entity GetEntity(int x, int y)
    {
        return GetEntity(ToPosition1D(x, y));
    }
    
    public Entity GetEntity((int x, int y) position)
    {
        return GetEntity(ToPosition1D(position.x, position.y));
    }
    
    public int[] GetPlayerStartPositions(int player)
    {
        var positions = new List<int>();
        for (int i = 0; i < PlayerStartPositions.Length; i++)
        {
            if (PlayerStartPositions[i] == player) positions.Add(i);
        }
        return positions.ToArray();
    }
    
    public TileTerrain GetTerrain(int position)
    {
        return Terrain[position];
    }

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
            Terrain[position] = tileTerrain;
            return true;
        }
        return false;
    }

    public bool SetEntity(int position, Entity entity=null)
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