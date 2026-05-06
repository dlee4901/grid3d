using System.Collections.Generic;

public class Grid2D : INameId
{
    public GridDefinition Definition { get; }
    public GridState State { get; }

    public string Id => Definition.Id;
    public int X => Definition.X;
    public int Y => Definition.Y;
    public int MaxTeamCost => Definition.MaxTeamCost;
    public int PlayerCount => Definition.PlayerCount;
    public int Turn => State.Turn;

    private Grid2D(GridDefinition definition)
    {
        Definition = definition;
        State = new GridState(definition);
    }

    public static Grid2D Create(GridDefinition definition) => new Grid2D(definition);

    public static Grid2D Create(MapConfig config)
    {
        var def = new GridDefinition
        {
            Id = config.Id,
            X = config.X,
            Y = config.Y,
            MaxTeamCost = config.MaxTeamCost,
            PlayerCount = config.PlayerCount,
            Terrain = config.Terrain,
            PlayerStartPositions = config.PlayerStartPositions,
            EntityStartPositions = config.EntityStartPositions
        };
        def.Bake();
        return new Grid2D(def);
    }

    // ---- State pass-throughs (transitional)
    public Entity GetEntity(int position) => State.GetEntity(position);
    public Entity GetEntity(int x, int y) => State.GetEntity(x, y);
    public Entity GetEntity((int x, int y) position) => State.GetEntity(position);
    public bool TryGetEntity(int position, out Entity entity) => State.TryGetEntity(position, out entity);
    public bool TryGetEntity(int x, int y, out Entity entity) => State.TryGetEntity(x, y, out entity);
    public bool TryGetEntity((int x, int y) position, out Entity entity) => State.TryGetEntity(position, out entity);
    public bool TryGetTerrain(int position, out TerrainType terrainType) => State.TryGetTerrain(position, out terrainType);
    public HashSet<int> GetOccupiedTilesPositionSet() => State.GetOccupiedTilesPositionSet();
    public bool SetEntityPosition(int position, Entity entity) => State.SetEntityPosition(position, entity);
    public bool PerformAction(int action, int sourceTile, int targetTile) => State.PerformAction(action, sourceTile, targetTile);
    public bool PerformAction(int action, int sourceTile, List<int> targetTiles) => State.PerformAction(action, sourceTile, targetTiles);
    public bool MoveEntity(int startPosition, int targetPosition) => State.MoveEntity(startPosition, targetPosition);
    public bool IsTraversable(int position) => State.IsTraversable(position);
    public bool IsTraversable(int x, int y) => State.IsTraversable(x, y);
    public bool IsTraversable((int x, int y) position) => State.IsTraversable(position);
    public void LoadPlayerTeam(int player, TeamData teamData) => State.LoadPlayerTeam(player, teamData);
    public string PrintGrid() => State.PrintGrid();

    // ---- Definition pass-throughs (transitional)
    public int GetSize() => Definition.Size;
    public int GetSpawnPlayer(int spawn) => Definition.GetSpawnPlayer(spawn);
    public HashSet<int> GetPlayerSpawns(int player) => Definition.GetPlayerSpawnsOrNull(player);
    public bool SetSpawnPlayer(int spawn, int player) => Definition.SetSpawnPlayer(spawn, player);
    public bool SetTileTerrain(int position, TerrainType tileTerrain) => Definition.SetTileTerrain(position, tileTerrain);
    public (int, int) ToPosition2D(int position) => Definition.ToPosition2D(position);
    public int ToPosition1D(int x, int y) => Definition.ToPosition1D(x, y);
    public int ToPosition1D((int x, int y) position) => Definition.ToPosition1D(position);
    public List<int> ToPositionList(List<(int, int)> xyList) => Definition.ToPositionList(xyList);
    public bool IsValidPosition(int position) => Definition.IsValidPosition(position);
    public bool IsValidPosition(int x, int y) => Definition.IsValidPosition(x, y);
    public bool IsValidPosition((int x, int y) position) => Definition.IsValidPosition(position);
    public bool ValidateStartPositions(List<int> startPositions) => Definition.ValidateStartPositions(startPositions);
}
