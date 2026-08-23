using System.Collections.Generic;

public interface IReadOnlyGridState
{
    GridDefinition Definition { get; }
    int X { get; }
    int Y { get; }
    int Size { get; }
    int Turn { get; }
    int ActivePlayer { get; }
    IReadOnlyResolutionOrder ResolutionOrder { get; }

    bool CanPlayerAct(int player);
    int GetMana(int player);
    int GetTimeMs(int player);
    int GetSpawnPlayer(GridPosition position);
    Entity GetEntity(int position);
    Entity GetEntity(GridPosition position);
    bool TryGetEntity(GridPosition position, out Entity entity);
    bool TryGetTerrain(GridPosition position, out TerrainType terrainType);
    bool IsTraversable(GridPosition position);

    HashSet<GridPosition> GetControllableEntityPositions(bool available=false);
    HashSet<GridPosition> GetControllableEntityPositions(int player, bool available=false);
    HashSet<GridPosition> GetOccupiedEntityPositions();
    
    bool IsAvailableControllable(GridPosition position);
    
    //HashSet<int> GetOccupiedTilesPositionSet();

    string PrintGrid();
}

public sealed class GridState : IReadOnlyGridState
{
    public GridDefinition Definition { get; }
    public int Turn { get; private set; } = 1;
    public int ActivePlayer { get; private set; } = 1;
    public bool CanPlayerAct(int player) => ActivePlayer == player;

    public int X => Definition.X;
    public int Y => Definition.Y;
    public int Size => Definition.Size;
    public ResolutionOrder ResolutionOrder { get; } = new();
    IReadOnlyResolutionOrder IReadOnlyGridState.ResolutionOrder => ResolutionOrder;
    
    private readonly Entity[] _entities;
    private readonly int[] _mana;
    private readonly int[] _timeMs;
    
    private readonly TeamData[] _loadedTeams;
    
    private int PlayerSlots => Definition.PlayerCount + 1;
    
    public GridState(GridDefinition definition)
    {
        Definition = definition;
        _entities = new Entity[definition.Size];
        _mana = new int[PlayerSlots];
        _loadedTeams = new TeamData[PlayerSlots];
        
        RefillMana(ActivePlayer);
        _timeMs = new int[PlayerSlots];
        for (var p = 1; p <= Definition.PlayerCount; p++) _timeMs[p] = definition.PlayerStartingTimeSeconds * 1000;
        SeedStartingEntities();
    }
    
    public void AdvanceTurn()
    {
        ActivePlayer = ActivePlayer % Definition.PlayerCount + 1;
        Turn++;
        RefillMana(ActivePlayer);
        TickAbilityCooldowns(ActivePlayer);
    }

    public int GetMana(int player) => player < 1 || player > Definition.PlayerCount ? 0 : _mana[player];

    public bool HasMana(int player, int cost) => GetMana(player) >= cost;

    public bool SpendMana(int player, int cost)
    {
        if (!HasMana(player, cost)) return false;
        _mana[player] -= cost;
        return true;
    }

    private void RefillMana(int player) => _mana[player] = Definition.ManaPerTurn;

    public int GetTimeMs(int player) => player < 1 || player > Definition.PlayerCount ? 0 : _timeMs[player];

    public void SpendTime(int player, int elapsedMs)
    {
        if (player < 1 || player > Definition.PlayerCount) return;
        _timeMs[player] = System.Math.Max(0, _timeMs[player] - System.Math.Max(0, elapsedMs));
    }

    public int GetSpawnPlayer(GridPosition position) => Definition.GetSpawnPlayer(position);

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
    
    public void SpawnTeams()
    {
        var maxUnits = 0;
        for (var player = 1; player <= Definition.PlayerCount; player++)
        {
            var team = _loadedTeams[player];
            if (team != null && team.UnitStartPositions.Count > maxUnits) maxUnits =
                team.UnitStartPositions.Count;
        }

        for (var unit = 0; unit < maxUnits; unit++)
        for (var player = Definition.PlayerCount; player >= 1; player--)
        {
            var team = _loadedTeams[player];
            if (team == null || unit >= team.UnitStartPositions.Count) continue;

            var placement = team.UnitStartPositions[unit];
            if (!IdRegistry<EntityConfig>.TryGet(placement.UnitId, out var config)) continue;
            SetEntityPosition(new GridPosition(this, placement.Position), Entity.Create(config, player));
        }
    }
    
    public void LoadPlayerTeam(int player, TeamData teamData)
    {
        if (player < 1 || player > Definition.PlayerCount) return;
        if (!ValidatePlayerTeam(player, teamData)) return;
        _loadedTeams[player] = teamData;
    }


    private bool ValidatePlayerTeam(int player, TeamData teamData)
    {
        if (teamData == null || teamData.MapId != Definition.Id) return false;
        foreach (var unit in teamData.UnitStartPositions)
        {
            var gridPosition = new GridPosition(this, unit.Position);
            if (!gridPosition.IsValid()
                || Definition.GetSpawnPlayer(gridPosition) != player
                || _entities[gridPosition.Dim1] != null
                || !IdRegistry<EntityConfig>.TryGet(unit.UnitId, out _)) return false;
        }
        return true;
    }
    
    public Entity GetEntity(int position)
    {
        return position >= 0 && position < _entities.Length ? _entities[position] : null;
    }
    
    public Entity GetEntity(GridPosition position)
    {
        return position.IsValid() ? _entities[position.Dim1] : null;
    }

    public bool TryGetEntity(GridPosition position, out Entity entity)
    {
        if (!position.IsValid() || _entities[position.Dim1] == null)
        {
            entity = null;
            return false;
        }
        entity = _entities[position.Dim1];
        return true;
    }

    public bool TryGetTerrain(GridPosition position, out TerrainType terrainType)
    {
        if (!position.IsValid())
        {
            terrainType = TerrainType.Default;
            return false;
        }
        terrainType = Definition.TerrainMap[position.Dim1];
        return true;
    }
    
    public HashSet<GridPosition> GetControllableEntityPositions(bool available=false) => GetControllableEntityPositions(ActivePlayer, available);
    
    public HashSet<GridPosition> GetControllableEntityPositions(int player, bool available=false)
    {
        var positions = new HashSet<GridPosition>();
        for (var i = 0; i < _entities.Length; i++)
        {
            var gridPosition = new GridPosition(this, i);
            if (IsControllable(gridPosition, player, available)) positions.Add(gridPosition);
        }
        return positions;
    }
    
    public HashSet<GridPosition> GetOccupiedEntityPositions()
    {
        var positions = new HashSet<GridPosition>();
        for (var i = 0; i < _entities.Length; i++) if (_entities[i] != null) positions.Add(new GridPosition(this, i));
        return positions;
    }

    public bool IsControllable(GridPosition position, int player, bool available = false)
    {
        if (!position.IsValid()) return false;
        var entity = _entities[position.Dim1];
        if (entity == null) return false;
        if (!entity.TryGetComponent<ControlComponent>(out var control)) return false;
        if (control.PlayerController != player) return false;
        // TODO: filter units that already acted / lack mana
        return true;
    }
    
    public bool IsAvailableControllable(GridPosition position) => IsControllable(position, ActivePlayer, available: true);
    
    public HashSet<int> GetOccupiedTilesPositionSet()
    {
        HashSet<int> indices = new();
        for (var i = 0; i < Definition.Size; i++)
        {
            if (_entities[i] != null) indices.Add(i);
        }
        return indices;
    }
    
    public bool SetEntityPosition(GridPosition position, Entity entity)
    {
        if (!position.IsValid() || entity == null) return false;
        _entities[position.Dim1] = entity;
        entity.SetPosition(position);
        ResolutionOrder.Add(entity);
        return true;
    }
    
    public bool ChangeEntityPosition(GridPosition startPosition, GridPosition targetPosition)
    {
        if (!startPosition.IsValid() || !targetPosition.IsValid() || !TryGetEntity(startPosition, out var entity)) return false;
        _entities[targetPosition.Dim1] = entity;
        _entities[startPosition.Dim1] = null;
        entity.SetPosition(targetPosition);
        return true;
    }
    
    public bool RemoveEntity(GridPosition position)
    {
        if (!position.IsValid()) return false;
        var entity = _entities[position.Dim1];
        if (entity == null) return false;
        _entities[position.Dim1] = null;
        ResolutionOrder.Remove(entity);
        return true;
    }

    public bool PerformAction(int action, GridPosition sourcePosition, GridPosition targetPosition)
    {
        var entity = GetEntity(sourcePosition);
        return entity != null;
    }

    public bool PerformAction(int action, GridPosition sourcePosition, List<GridPosition> targetPosition)
    {
        return false;
    }

    // TODO: Terrain check
    public bool IsTraversable(GridPosition position)
    {
        return position.IsValid() && _entities[position.Dim1] == null && Definition.TerrainMap[position.Dim1] != TerrainType.Void; 
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
