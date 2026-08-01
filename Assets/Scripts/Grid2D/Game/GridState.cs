using System.Collections.Generic;

public interface IReadOnlyGridState
{
    GridDefinition Definition { get; }
    int Turn { get; }
    int ActivePlayer { get; }
    bool CanPlayerAct(int player);
    int GetMana(int player);
    int GetTimeMs(int player);

    int X { get; }
    int Y { get; }
    int Size { get; }
    //int ToPosition1D(int x, int y);
    //int ToPosition1D((int x, int y) position);
    //(int, int) ToPosition2D(int position);
    bool IsValidPosition(int position);
    bool IsValidPosition(int x, int y);
    bool IsValidPosition((int x, int y) position);
    int GetSpawnPlayer(int spawn);

    Entity GetEntity(int position);
    Entity GetEntity(int x, int y);
    Entity GetEntity((int x, int y) position);
    bool TryGetEntity(int position, out Entity entity);
    bool TryGetEntity(int x, int y, out Entity entity);
    bool TryGetEntity((int x, int y) position, out Entity entity);
    bool TryGetTerrain(int position, out TerrainType terrainType);
    bool IsTraversable(int position);
    bool IsTraversable(int x, int y);
    bool IsTraversable((int x, int y) position);
    HashSet<int> GetControllableEntities(bool available=false);
    HashSet<int> GetControllableEntities(int player, bool available=false);
    bool IsAvailableControllable(int position);
    HashSet<int> GetOccupiedTilesPositionSet();

    string PrintGrid();
}

public sealed class GridState : IReadOnlyGridState
{
    public GridDefinition Definition { get; }
    public int Turn { get; private set; } = 1;
    public int ActivePlayer { get; private set; } = 1;
    public bool CanPlayerAct(int player) => ActivePlayer == player;

    public void AdvanceTurn()
    {
        ActivePlayer = ActivePlayer % Definition.PlayerCount + 1;
        Turn++;
        RefillMana(ActivePlayer);
        TickAbilityCooldowns(ActivePlayer);
    }

    private void TickAbilityCooldowns(int player)
    {
        for (var i = 0; i < _entities.Length; i++)
        {
            var entity = _entities[i];
            if (entity == null) continue;
            if (!entity.TryGetComponent<ControlComponent>(out var control) 
                || control.PlayerController != player) continue;
            if (!entity.TryGetComponent<AbilityComponent>(out var abilities)) continue;
            foreach (var ability in abilities.List) ability.TurnUpdate();
        }
    }

    public int GetMana(int player) => (player >= 1 && player <= Definition.PlayerCount) ? _mana[player] : 0;

    public bool HasMana(int player, int cost) => GetMana(player) >= cost;

    public bool SpendMana(int player, int cost)
    {
        if (!HasMana(player, cost)) return false;
        _mana[player] -= cost;
        return true;
    }

    private void RefillMana(int player) => _mana[player] = Definition.ManaPerTurn;

    public int GetTimeMs(int player) => (player >= 1 && player <= Definition.PlayerCount) ? _timeMs[player] : 0;

    public void SpendTime(int player, int elapsedMs)
    {
        if (player < 1 || player > Definition.PlayerCount) return;
        _timeMs[player] = System.Math.Max(0, _timeMs[player] - System.Math.Max(0, elapsedMs));
    }

    public int X => Definition.X;
    public int Y => Definition.Y;
    public int Size => Definition.Size;
    public int ToPosition1D(int x, int y) => Definition.ToPosition1D(x, y);
    public int ToPosition1D((int x, int y) position) => Definition.ToPosition1D(position);
    public (int, int) ToPosition2D(int position) => Definition.ToPosition2D(position);
    public bool IsValidPosition(int position) => Definition.IsValidPosition(position);
    public bool IsValidPosition(int x, int y) => Definition.IsValidPosition(x, y);
    public bool IsValidPosition((int x, int y) position) => Definition.IsValidPosition(position);
    public int GetSpawnPlayer(int spawn) => Definition.GetSpawnPlayer(spawn);

    private readonly Entity[] _entities;
    private readonly int[] _mana;
    private readonly int[] _timeMs;
    private List<Entity> _prioritizedEntities;

    public GridState(GridDefinition definition)
    {
        Definition = definition;
        _entities = new Entity[definition.Size];
        _mana = new int[definition.PlayerCount + 1];
        RefillMana(ActivePlayer);
        _timeMs = new int[definition.PlayerCount + 1];
        for (var p = 1; p <= definition.PlayerCount; p++) _timeMs[p] = definition.PlayerStartingTimeSeconds * 1000;
        SeedStartingEntities();
    }

    private void SeedStartingEntities()
    {
        if (Definition.EntityStartPositions == null) return;
        foreach (var spec in Definition.EntityStartPositions)
        {
            if (!IdRegistry<EntityConfig>.TryGet(spec.EntityId, out var entityConfig)) continue;
            foreach (var position in Definition.ResolvePositions(spec))
            {
                if (!IsTraversable(position)) continue;
                SetEntityPosition(position, Entity.Create(entityConfig, Definition.GetSpawnPlayer(position)));
            }
        }
    }

    public void LoadPlayerTeam(int player, TeamData teamData)
    {
        if (!ValidatePlayerTeam(player, teamData)) return;
        //Debug.Log($"Loading player team {player}");

        foreach (var (position, unit) in teamData.UnitStartPositions)
        {
            //Debug.Log(player + " 1");
            if (!IdRegistry<EntityConfig>.TryGet(unit, out var entityConfig)) continue;
            //Debug.Log(player + " 2");
            var entity = Entity.Create(entityConfig, player);
            if (!entity.TryGetComponent<ControlComponent>(out var control)) continue;
            //Debug.Log(player + " 3");
            control.PlayerController = player;
            SetEntityPosition(position, entity);
        }
    }

    private bool ValidatePlayerTeam(int player, TeamData teamData)
    {
        //Debug.Log(player + " map1");
        if (teamData.MapId != Definition.Id) return false;
        //Debug.Log(player + " map2");
        foreach (var (position, unit) in teamData.UnitStartPositions)
            if (!Definition.IsValidPosition(position)
                || Definition.GetSpawnPlayer(position) != player
                || _entities[position] != null
                || !IdRegistry<EntityConfig>.TryGet(unit, out _)) return false;
        return true;
    }

    public Entity GetEntity(int position)
    {
        return Definition.IsValidPosition(position) ? _entities[position] : null;
    }

    public Entity GetEntity(int x, int y)
    {
        return GetEntity(Definition.ToPosition1D(x, y));
    }

    public Entity GetEntity((int x, int y) position)
    {
        return GetEntity(Definition.ToPosition1D(position.x, position.y));
    }

    public bool TryGetEntity(int position, out Entity entity)
    {
        if (!Definition.IsValidPosition(position) || _entities[position] == null)
        {
            entity = null;
            return false;
        }
        entity = _entities[position];
        return true;
    }

    public bool TryGetEntity(int x, int y, out Entity entity)
    {
        return TryGetEntity(Definition.ToPosition1D(x, y), out entity);
    }

    public bool TryGetEntity((int x, int y) position, out Entity entity)
    {
        return TryGetEntity(Definition.ToPosition1D(position.x, position.y), out entity);
    }

    public bool TryGetTerrain(int position, out TerrainType terrainType)
    {
        if (!Definition.IsValidPosition(position))
        {
            terrainType = TerrainType.Default;
            return false;
        }
        terrainType = Definition.TerrainMap[position];
        return true;
    }
    
    public HashSet<int> GetControllableEntities(bool available=false) => GetControllableEntities(ActivePlayer, available);
    
    public HashSet<int> GetControllableEntities(int player, bool available=false)
    {
        var controllableEntities = new HashSet<int>();
        for (var i = 0; i < _entities.Length; i++)
            if (IsControllable(i, player, available)) controllableEntities.Add(i);
        return controllableEntities;
    }

    public bool IsControllable(int position, int player, bool available = false)
    {
        if (!Definition.IsValidPosition(position)) return false;
        var entity = _entities[position];
        if (entity == null) return false;
        if (!entity.TryGetComponent<ControlComponent>(out var control)) return false;
        if (control.PlayerController != player) return false;
        // TODO: `available` — filter units that already acted / lack mana. No-op today (matches current behavior).
        return true;
    }

    public bool IsAvailableControllable(int position) => IsControllable(position, ActivePlayer, available: true);

    public HashSet<int> GetOccupiedTilesPositionSet()
    {
        HashSet<int> indices = new();
        for (var i = 0; i < Definition.Size; i++)
        {
            if (_entities[i] != null) indices.Add(i);
        }
        return indices;
    }

    public bool SetEntityPosition(int position, Entity entity)
    {
        if (!Definition.IsValidPosition(position) || entity == null) return false;
        _entities[position] = entity;
        entity.SetPosition(position);
        return true;
    }
    
    public bool ChangeEntityPosition(int startPosition, int targetPosition)
    {
        if (!Definition.IsValidPosition(startPosition) 
            || !Definition.IsValidPosition(targetPosition) 
            || !TryGetEntity(startPosition, out var entity)) return false;
        _entities[targetPosition] = entity;
        _entities[startPosition] = null;
        entity.SetPosition(targetPosition);
        return true;
    }

    public bool PerformAction(int action, int sourceTile, int targetTile)
    {
        Entity entity = GetEntity(sourceTile);
        return entity != null;
    }

    public bool PerformAction(int action, int sourceTile, List<int> targetTiles)
    {
        return false;
    }

    public bool IsTraversable(int position)
    {
        return Definition.IsValidPosition(position) && _entities[position] == null
            && Definition.TerrainMap[position] != TerrainType.Void; // TODO: Terrain check
    }

    public bool IsTraversable(int x, int y)
    {
        return IsTraversable(Definition.ToPosition1D(x, y));
    }

    public bool IsTraversable((int x, int y) position)
    {
        return IsTraversable(Definition.ToPosition1D(position.x, position.y));
    }

    public string PrintGrid()
    {
        var grid = "";

        grid += "START POSITIONS\n";
        foreach (var kvp in Definition.SpawnPlayers)
            grid += "(" + kvp.Key + "-" + kvp.Value + ") ";
        grid += "\n";

        grid += "ENTITIES\n";
        for (var i = 0; i < Definition.Size; i++)
            if (_entities[i] != null)
                grid += "(" + i + " " + _entities[i].Id + ") ";
        grid += "\n";

        return grid;
    }
}
